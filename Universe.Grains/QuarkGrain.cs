using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System.Numerics;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;

namespace Universe.Grains;

public class QuarkGrain : Grain, IQuarkGrain
{
    private readonly IPersistentState<QuarkState> _state;
    private readonly ILogger<QuarkGrain> _logger;

    public QuarkGrain(
        [PersistentState("quark", "quarks")] IPersistentState<QuarkState> state,
        ILogger<QuarkGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public Task<QuarkState> GetState() => Task.FromResult(_state.State);

    public async Task Initialize(QuarkFlavor flavor, ColorCharge color, bool isAntiParticle, 
        Vector3 position, Vector3 momentum, double spinZ)
    {
        _state.State = new QuarkState
        {
            QuarkId = this.GetPrimaryKey(),
            Flavor = flavor,
            Color = color,
            IsAntiParticle = isAntiParticle,
            Position = position,
            Momentum = momentum,
            SpinZ = spinZ,
            Energy = CalculateTotalEnergy(momentum, flavor.GetMass()),
            LastUpdated = DateTimeOffset.UtcNow,
            BoundPartners = new List<Guid>()
        };
        
        await _state.WriteStateAsync();
        _logger.LogInformation("Initialized {Flavor} quark with color {Color} at position {Position}", 
            flavor, color, position);
    }

    public async Task UpdatePosition(Vector3 newPosition)
    {
        _state.State = _state.State with 
        { 
            Position = newPosition,
            LastUpdated = DateTimeOffset.UtcNow
        };
        await _state.WriteStateAsync();
    }

    public async Task UpdateMomentum(Vector3 newMomentum)
    {
        _state.State = _state.State with 
        { 
            Momentum = newMomentum,
            Energy = CalculateTotalEnergy(newMomentum, _state.State.Mass),
            LastUpdated = DateTimeOffset.UtcNow
        };
        await _state.WriteStateAsync();
    }

    public async Task UpdateEnergy(double newEnergy)
    {
        // Adjust momentum to match the new energy while preserving direction
        var currentMomentumMagnitude = _state.State.Momentum.Length();
        var momentumDirection = currentMomentumMagnitude > 0 
            ? _state.State.Momentum / currentMomentumMagnitude 
            : Vector3.UnitX;
        
        // E² = (pc)² + (mc²)² => p = sqrt(E² - (mc²)²) / c
        var massTerm = _state.State.Mass * _state.State.Mass;
        var newMomentumSquared = Math.Max(0, newEnergy * newEnergy - massTerm);
        var newMomentumMagnitude = Math.Sqrt(newMomentumSquared);
        
        _state.State = _state.State with 
        { 
            Energy = newEnergy,
            Momentum = momentumDirection * (float)newMomentumMagnitude,
            LastUpdated = DateTimeOffset.UtcNow
        };
        await _state.WriteStateAsync();
    }

    public async Task UpdateSpin(double newSpinZ)
    {
        if (Math.Abs(newSpinZ) != 0.5)
        {
            throw new ArgumentException("Quarks must have spin ±1/2");
        }
        
        _state.State = _state.State with 
        { 
            SpinZ = newSpinZ,
            LastUpdated = DateTimeOffset.UtcNow
        };
        await _state.WriteStateAsync();
    }

    public async Task ApplyForce(Vector3 force, double deltaTime)
    {
        // F = dp/dt, so dp = F * dt
        var momentumChange = force * (float)deltaTime;
        var newMomentum = _state.State.Momentum + momentumChange;
        
        // Update position using current velocity
        var velocity = _state.State.GetVelocity();
        var newPosition = _state.State.Position + velocity * (float)deltaTime;
        
        _state.State = _state.State with
        {
            Position = newPosition,
            Momentum = newMomentum,
            Energy = CalculateTotalEnergy(newMomentum, _state.State.Mass),
            LastUpdated = DateTimeOffset.UtcNow
        };
        
        await _state.WriteStateAsync();
    }

    public async Task<StrongForceInteraction> CalculateInteractionWith(Guid otherQuarkId)
    {
        var otherQuark = GrainFactory.GetGrain<IQuarkGrain>(otherQuarkId);
        var otherState = await otherQuark.GetState();
        
        var interaction = StrongForceCalculator.CalculateInteraction(_state.State, otherState);
        
        // Create actual gluon for non-perturbative interactions
        if (!interaction.IsPerturbative && StrongForceCalculator.ShouldExchangeGluon(_state.State, otherState, 0.001))
        {
            var gluonId = Guid.NewGuid();
            var gluon = GrainFactory.GetGrain<IGluonGrain>(gluonId);
            
            var momentum = interaction.ForceVector * 0.1f; // Simplified gluon momentum
            await gluon.Initialize(
                interaction.GluonColor1, 
                interaction.GluonColor2,
                _state.State.Position,
                momentum,
                _state.State.QuarkId,
                otherQuarkId
            );
            
            interaction = interaction with { GluonId = gluonId };
            
            _logger.LogDebug("Created gluon {GluonId} for interaction between {Quark1} and {Quark2}",
                gluonId, _state.State.QuarkId, otherQuarkId);
        }
        
        return interaction;
    }

    public async Task FormBoundState(List<Guid> partnerQuarkIds)
    {
        _state.State = _state.State with 
        { 
            BoundPartners = partnerQuarkIds,
            LastUpdated = DateTimeOffset.UtcNow
        };
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Quark {QuarkId} formed bound state with {Partners}", 
            _state.State.QuarkId, string.Join(", ", partnerQuarkIds));
    }

    public Task<bool> IsConfined()
    {
        // A quark is confined if it has bound partners
        return Task.FromResult(_state.State.BoundPartners.Any());
    }

    public async Task<double> GetBindingEnergy()
    {
        if (!_state.State.BoundPartners.Any())
            return 0;

        double totalBindingEnergy = 0;
        
        foreach (var partnerId in _state.State.BoundPartners)
        {
            var interaction = await CalculateInteractionWith(partnerId);
            totalBindingEnergy += interaction.PotentialEnergy;
        }
        
        return totalBindingEnergy;
    }

    public async Task Evolve(double deltaTime)
    {
        // Apply interactions with bound partners
        if (_state.State.BoundPartners.Any())
        {
            var netForce = Vector3.Zero;
            
            foreach (var partnerId in _state.State.BoundPartners)
            {
                var interaction = await CalculateInteractionWith(partnerId);
                netForce += interaction.ForceVector;
            }
            
            await ApplyForce(netForce, deltaTime);
        }
        else
        {
            // Free quark evolution (shouldn't happen in nature due to confinement)
            _logger.LogWarning("Free quark {QuarkId} detected - this violates confinement!", 
                _state.State.QuarkId);
        }
    }

    private static double CalculateTotalEnergy(Vector3 momentum, double mass)
    {
        var p = momentum.Length();
        // Relativistic energy: E² = (pc)² + (mc²)²
        return Math.Sqrt(p * p + mass * mass);
    }
}