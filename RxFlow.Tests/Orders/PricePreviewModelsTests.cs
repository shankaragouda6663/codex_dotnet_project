using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using RxFlow.Api.Endpoints;
using RxFlow.Contracts.Orders;

namespace RxFlow.Tests.Orders;

public sealed class PricePreviewModelsTests
{
    [Fact]
    public void PricePreviewRequestModel_WithRequiredFields_IsValid()
    {
        var model = new PricePreviewRequestModel
        {
            PatientId = "PAT-12345",
            LensMaterial = "polycarbonate",
            Expedited = true
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, context, results, true);

        valid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void PricePreviewRequestModel_WithoutRequiredFields_FailsValidation()
    {
        var model = new PricePreviewRequestModel
        {
            PatientId = string.Empty,
            LensMaterial = string.Empty
        };

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, context, results, true);

        valid.Should().BeFalse();
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void PricePreviewResponse_StoresQuotedPriceAndLabCode()
    {
        var response = new PricePreviewResponse(245.50m, "LAB-EAST");

        response.QuotedPrice.Should().Be(245.50m);
        response.RoutedLabCode.Should().Be("LAB-EAST");
    }
}
