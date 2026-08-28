using System.ComponentModel.DataAnnotations;

namespace RxFlow.Api.Endpoints;

public sealed class PricePreviewRequestModel
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string PatientId { get; init; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 2)]
    public string LensMaterial { get; init; } = string.Empty;

    public bool Expedited { get; init; }
}
