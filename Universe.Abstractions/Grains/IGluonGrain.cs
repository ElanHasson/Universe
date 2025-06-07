using Universe.Abstractions.Physics;
using System.Numerics;

namespace Universe.Abstractions.Grains;

public interface IGluonGrain : IGrainWithGuidKey
{
    Task<GluonState> GetState();
    Task Initialize(ColorCharge color1, ColorCharge color2, Vector3 position, Vector3 momentum, 
        Guid sourceQuarkId, Guid targetQuarkId);
    Task Propagate(double deltaTime);
    Task<bool> IsVirtual();
    Task<double> GetFieldStrength(Vector3 position);
    Task Interact(IGluonGrain otherGluon); // Gluon self-interaction
    Task<GluonFieldTensor> GetFieldTensor(Vector3 position);
}