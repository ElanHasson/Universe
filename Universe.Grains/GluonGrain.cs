using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System.Numerics;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;

namespace Universe.Grains;

public class GluonGrain : Grain, IGluonGrain
{
    private readonly IPersistentState<GluonState> _state;
    private readonly ILogger<GluonGrain> _logger;

    public GluonGrain(
        [PersistentState("gluon", "gluons")] IPersistentState<GluonState> state,
        ILogger<GluonGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public Task<GluonState> GetState() => Task.FromResult(_state.State);

    public async Task Initialize(ColorCharge color1, ColorCharge color2, Vector3 position, 
        Vector3 momentum, Guid sourceQuarkId, Guid targetQuarkId)
    {
        var energy = momentum.Length(); // Massless gluon: E = |p|
        
        _state.State = new GluonState
        {
            GluonId = this.GetPrimaryKey(),
            ColorCharge1 = color1,
            ColorCharge2 = color2,
            Position = position,
            Momentum = momentum,
            Energy = energy,
            Polarization = Random.Shared.NextDouble() * 2 * Math.PI,
            SourceQuarkId = sourceQuarkId,
            TargetQuarkId = targetQuarkId,
            CreatedAt = DateTimeOffset.UtcNow,
            PropagationTime = 0
        };
        
        await _state.WriteStateAsync();
        _logger.LogDebug("Created gluon {GluonId} with colors {Color1}-{Color2}", 
            _state.State.GluonId, color1, color2);
    }

    public async Task Propagate(double deltaTime)
    {
        // Gluons are massless and travel at speed of light
        var velocity = _state.State.Momentum / (float)_state.State.Energy;
        var newPosition = _state.State.Position + velocity * (float)deltaTime;
        
        _state.State = _state.State with
        {
            Position = newPosition,
            PropagationTime = _state.State.PropagationTime + deltaTime
        };
        
        await _state.WriteStateAsync();
        
        // Check if gluon has exceeded virtual particle lifetime
        if (_state.State.PropagationTime > 1e-23)
        {
            _logger.LogDebug("Gluon {GluonId} transitioning from virtual to real", _state.State.GluonId);
        }
    }

    public Task<bool> IsVirtual()
    {
        return Task.FromResult(_state.State.IsVirtual);
    }

    public Task<double> GetFieldStrength(Vector3 position)
    {
        return Task.FromResult(_state.State.GetFieldStrength(position));
    }

    public async Task Interact(IGluonGrain otherGluon)
    {
        var otherState = await otherGluon.GetState();
        
        // Gluon self-interaction via three-gluon and four-gluon vertices
        var separation = otherState.Position - _state.State.Position;
        var distance = separation.Length();
        
        if (distance > 1e-10) // Only interact at very close range
            return;
        
        // Calculate interaction strength
        var energyScale = (_state.State.Energy + otherState.Energy) / 2;
        var coupling = QuantumChromodynamics.GetGluonSelfCoupling(energyScale);
        
        // Momentum exchange
        var momentumTransfer = separation * (float)(coupling / distance);
        
        _state.State = _state.State with
        {
            Momentum = _state.State.Momentum + momentumTransfer,
            Energy = (_state.State.Momentum + momentumTransfer).Length()
        };
        
        await _state.WriteStateAsync();
        
        _logger.LogDebug("Gluon {Gluon1} interacted with {Gluon2}", 
            _state.State.GluonId, otherState.GluonId);
    }

    public Task<GluonFieldTensor> GetFieldTensor(Vector3 position)
    {
        var direction = position - _state.State.Position;
        var distance = direction.Length();
        
        if (distance < 1e-15f)
        {
            return Task.FromResult(new GluonFieldTensor
            {
                ElectricField = Vector3.Zero,
                MagneticField = Vector3.Zero
            });
        }
        
        var fieldStrength = _state.State.GetFieldStrength(position);
        var unitDirection = direction / distance;
        
        // Simplified chromoelectric and chromomagnetic fields
        var electricField = unitDirection * (float)fieldStrength;
        var magneticField = Vector3.Cross(_state.State.Momentum, unitDirection) * (float)(fieldStrength / _state.State.Energy);
        
        // Color matrix would involve SU(3) generators - simplified here
        var colorMatrix = new double[8, 8];
        
        return Task.FromResult(new GluonFieldTensor
        {
            ElectricField = electricField,
            MagneticField = magneticField,
            ColorMatrix = colorMatrix
        });
    }
}