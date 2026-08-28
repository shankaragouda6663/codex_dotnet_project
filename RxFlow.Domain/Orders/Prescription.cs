namespace RxFlow.Domain.Orders;

public sealed class Prescription
{
    public Prescription(decimal sphere, decimal cylinder, int axis)
    {
        Sphere = sphere;
        Cylinder = cylinder;
        Axis = axis;
    }

    public decimal Sphere { get; private set; }
    public decimal Cylinder { get; private set; }
    public int Axis { get; private set; }
}