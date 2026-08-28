using System.ComponentModel.DataAnnotations;

namespace RxFlow.Api.Endpoints;

public sealed class CreateOrderRequestModel
{
    [Required]
    [MaxLength(64)]
    public string PatientId { get; set; } = string.Empty;

    [Range(-20, 20)]
    public decimal Sphere { get; set; }

    [Range(-30, 30)]
    public decimal Cylinder { get; set; }

    [Range(-360, 360)]
    public int Axis { get; set; }

    [Required]
    [MaxLength(64)]
    public string FrameCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string LensMaterial { get; set; } = string.Empty;

    public bool Expedited { get; set; }
}