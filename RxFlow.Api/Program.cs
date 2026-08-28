using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RxFlow.Api.Auth;
using RxFlow.Api.Background;
using RxFlow.Api.Endpoints;
using RxFlow.Application.Abstractions;
using RxFlow.Application.Labs;
using RxFlow.Application.Pricing;
using RxFlow.Application.Orders;
using RxFlow.Contracts.Orders;
using RxFlow.Infrastructure;
using RxFlow.Infrastructure.Persistence;
using RxFlow.Infrastructure.Reporting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IOrderJobScheduler, ApiOrderJobScheduler>();
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Postgres"))));
builder.Services.AddHangfireServer();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("RxFlow.Api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/jobs");

app.MapPost("/auth/token", ([FromBody] string user) =>
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user),
        new Claim(ClaimTypes.Role, "optician")
    };

    var creds = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
        SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        jwtSettings.Issuer,
        jwtSettings.Audience,
        claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);

    return Results.Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token) });
});

var ordersGroup = app.MapGroup("/orders").RequireAuthorization();

ordersGroup.MapPost(string.Empty, async (
    [FromBody] CreateOrderRequestModel request,
    IOrderSubmissionService orderSubmissionService,
    CancellationToken cancellationToken) =>
{
    var validationContext = new ValidationContext(request);
    var results = new List<ValidationResult>();
    if (!Validator.TryValidateObject(request, validationContext, results, true))
    {
        return Results.ValidationProblem(results
            .GroupBy(x => x.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(x => x.Key, x => x.Select(v => v.ErrorMessage ?? "Invalid").ToArray()));
    }

    var response = await orderSubmissionService.SubmitAsync(
        new CreateOrderRequest(
            request.PatientId,
            request.Sphere,
            request.Cylinder,
            request.Axis,
            request.FrameCode,
            request.LensMaterial,
            request.Expedited),
        cancellationToken);

    return Results.Ok(response);
});

ordersGroup.MapPost("/price-preview", async (
    [FromBody] PricePreviewRequestModel request,
    ComplexPricingEngine pricingEngine,
    ILabRoutingService labRoutingService,
    CancellationToken cancellationToken) =>
{
    var validationContext = new ValidationContext(request);
    var results = new List<ValidationResult>();
    if (!Validator.TryValidateObject(request, validationContext, results, true))
    {
        return Results.ValidationProblem(results
            .GroupBy(x => x.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(x => x.Key, x => x.Select(v => v.ErrorMessage ?? "Invalid").ToArray()));
    }

    var quotedPrice = await pricingEngine.CalculateAsync(
        request.PatientId,
        request.LensMaterial,
        request.Expedited,
        cancellationToken);
    var routedLabCode = await labRoutingService.PickLabCodeAsync(request.LensMaterial, cancellationToken);

    return Results.Ok(new PricePreviewResponse(quotedPrice, routedLabCode));
});
app.MapPost("/orders/lab-override", async (
    [FromBody] LabOverrideRequest request,
    RxFlowDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
    if (order is null)
    {
        return Results.NotFound();
    }

    order.RoutedLabCode = request.LabCode;
    order.Status = "LabOverride";
    await dbContext.SaveChangesAsync(cancellationToken);
    return Results.Ok();
});

ordersGroup.MapGet("/{orderId:guid}", async (Guid orderId, RxFlowDbContext dbContext, CancellationToken cancellationToken) =>
{
    var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapGet("/reports/adhoc", async (string q, [FromServices] RawSqlOrderReportQuery rawSql, CancellationToken cancellationToken) =>
{
    var rows = await rawSql.SearchByPatientAsync(q, cancellationToken);
    return Results.Ok(rows);
}).RequireAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RxFlowDbContext>();
    db.Database.Migrate();
}

app.Run();
