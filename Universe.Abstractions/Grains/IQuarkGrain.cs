using Universe.Abstractions.Physics;
using System.Numerics;

namespace Universe.Abstractions.Grains;

public interface IQuarkGrain : IGrainWithGuidKey
{
    Task<QuarkState> GetState();
    Task Initialize(QuarkFlavor flavor, ColorCharge color, bool isAntiParticle, Vector3 position, Vector3 momentum, double spinZ);
    Task UpdatePosition(Vector3 newPosition);
    Task UpdateMomentum(Vector3 newMomentum);
    Task UpdateEnergy(double newEnergy);
    Task UpdateSpin(double newSpinZ);
    Task ApplyForce(Vector3 force, double deltaTime);
    Task<StrongForceInteraction> CalculateInteractionWith(Guid otherQuarkId);
    Task FormBoundState(List<Guid> partnerQuarkIds);
    Task<bool> IsConfined();
    Task<double> GetBindingEnergy();
    Task Evolve(double deltaTime);
}