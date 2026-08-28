namespace RxFlow.Contracts.Orders;

public sealed record CreateOrderRequest(
    string PatientId,
    decimal Sphere,
    decimal Cylinder,
    int Axis,
    string FrameCode,
    string LensMaterial,
    bool Expedited);