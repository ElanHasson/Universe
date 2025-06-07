using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System.Numerics;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;

namespace Universe.Grains;

public class HadronGrain : Grain, IHadronGrain
{
    private readonly IPersistentState<HadronState> _state;
    private readonly ILogger<HadronGrain> _logger;

    public HadronGrain(
        [PersistentState("hadron", "hadrons")] IPersistentState<HadronState> state,
        ILogger<HadronGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public Task<HadronState> GetState() => Task.FromResult(_state.State);

    public async Task Initialize(List<Guid> constituentQuarkIds, HadronType type)
    {
        // Validate hadron composition
        if (!ValidateHadronComposition(constituentQuarkIds, type))
        {
            throw new ArgumentException("Invalid hadron composition");
        }

        var quarks = await GetQuarkStates(constituentQuarkIds);
        
        // Calculate hadron properties
        var centerOfMass = CalculateCenterOfMass(quarks);
        var totalMomentum = quarks.Aggregate(Vector3.Zero, (sum, q) => sum + q.Momentum);
        var invariantMass = CalculateInvariantMass(quarks);
        var bindingEnergy = await CalculateBindingEnergy(quarks);
        
        _state.State = new HadronState
        {
            HadronId = this.GetPrimaryKey(),
            Type = type,
            ConstituentQuarkIds = constituentQuarkIds,
            Position = centerOfMass,
            Momentum = totalMomentum,
            Mass = invariantMass,
            BindingEnergy = bindingEnergy,
            IsStable = DetermineStability(quarks, type),
            Lifetime = DetermineLifetime(quarks, type),
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        await _state.WriteStateAsync();
        
        // Update quarks to know they're bound
        foreach (var quarkId in constituentQuarkIds)
        {
            var quark = GrainFactory.GetGrain<IQuarkGrain>(quarkId);
            await quark.FormBoundState(constituentQuarkIds.Where(id => id != quarkId).ToList());
        }
        
        _logger.LogInformation("Created {Type} hadron with quarks {Quarks}", 
            type, string.Join(", ", constituentQuarkIds));
    }

    public async Task<Vector3> GetCenterOfMass()
    {
        var quarks = await GetConstituentQuarks();
        return CalculateCenterOfMass(quarks);
    }

    public Task<double> GetInvariantMass() => Task.FromResult(_state.State.Mass);

    public Task<bool> IsStable() => Task.FromResult(_state.State.IsStable);

    public async Task<List<QuarkState>> GetConstituentQuarks()
    {
        return await GetQuarkStates(_state.State.ConstituentQuarkIds);
    }

    public async Task Decay()
    {
        if (_state.State.IsStable)
        {
            _logger.LogWarning("Attempted to decay stable hadron {HadronId}", _state.State.HadronId);
            return;
        }

        // Implement decay products based on hadron type
        // This is simplified - real decay involves complex quantum mechanics
        _logger.LogInformation("Hadron {HadronId} decaying", _state.State.HadronId);
        
        // Release quarks from bound state
        foreach (var quarkId in _state.State.ConstituentQuarkIds)
        {
            var quark = GrainFactory.GetGrain<IQuarkGrain>(quarkId);
            await quark.FormBoundState(new List<Guid>());
        }
        
        // In a real simulation, we would create decay products here
        await _state.ClearStateAsync();
    }

    public async Task Evolve(double deltaTime)
    {
        // Check for decay
        if (!_state.State.IsStable)
        {
            var age = DateTimeOffset.UtcNow - _state.State.CreatedAt;
            var decayProbability = 1 - Math.Exp(-age.TotalSeconds / _state.State.Lifetime);
            
            if (Random.Shared.NextDouble() < decayProbability * deltaTime)
            {
                await Decay();
                return;
            }
        }

        // Update position based on momentum
        var quarks = await GetConstituentQuarks();
        var totalMass = quarks.Sum(q => q.Mass);
        var velocity = _state.State.Momentum / (float)totalMass;
        
        _state.State = _state.State with
        {
            Position = _state.State.Position + velocity * (float)deltaTime
        };
        
        await _state.WriteStateAsync();
        
        // Let constituent quarks evolve
        foreach (var quarkId in _state.State.ConstituentQuarkIds)
        {
            var quark = GrainFactory.GetGrain<IQuarkGrain>(quarkId);
            await quark.Evolve(deltaTime);
        }
    }

    private bool ValidateHadronComposition(List<Guid> quarks, HadronType type)
    {
        return type switch
        {
            HadronType.Meson => quarks.Count == 2,
            HadronType.Baryon or HadronType.AntiBaryon => quarks.Count == 3,
            _ => false
        };
    }

    private async Task<List<QuarkState>> GetQuarkStates(List<Guid> quarkIds)
    {
        var tasks = quarkIds.Select(id => 
            GrainFactory.GetGrain<IQuarkGrain>(id).GetState()).ToList();
        return (await Task.WhenAll(tasks)).ToList();
    }

    private Vector3 CalculateCenterOfMass(List<QuarkState> quarks)
    {
        var totalMass = quarks.Sum(q => q.Mass);
        var weightedPosition = quarks.Aggregate(Vector3.Zero, 
            (sum, q) => sum + q.Position * (float)q.Mass);
        return weightedPosition / (float)totalMass;
    }

    private double CalculateInvariantMass(List<QuarkState> quarks)
    {
        var totalEnergy = quarks.Sum(q => q.Energy);
        var totalMomentum = quarks.Aggregate(Vector3.Zero, (sum, q) => sum + q.Momentum);
        var p = totalMomentum.Length();
        
        // M² = E² - p²
        return Math.Sqrt(totalEnergy * totalEnergy - p * p);
    }

    private Task<double> CalculateBindingEnergy(List<QuarkState> quarks)
    {
        double bindingEnergy = 0;
        
        for (int i = 0; i < quarks.Count; i++)
        {
            for (int j = i + 1; j < quarks.Count; j++)
            {
                var interaction = StrongForceCalculator.CalculateInteraction(quarks[i], quarks[j]);
                bindingEnergy += interaction.PotentialEnergy;
            }
        }
        
        return Task.FromResult(bindingEnergy);
    }

    private bool DetermineStability(List<QuarkState> quarks, HadronType type)
    {
        // Check color neutrality
        var colors = quarks.Select(q => q.Color).ToArray();
        if (!ColorChargeExtensions.IsColorNeutral(colors))
            return false;

        // Simplified stability rules
        if (type == HadronType.Meson)
        {
            // Pions (up-antiup, down-antidown) are relatively stable
            var q1 = quarks[0];
            var q2 = quarks[1];
            if (q1.Flavor == q2.Flavor && q1.IsAntiParticle != q2.IsAntiParticle)
                return true;
        }
        else if (type == HadronType.Baryon)
        {
            // Protons (uud) and neutrons (udd) are stable
            var flavors = quarks.Select(q => q.Flavor).OrderBy(f => f).ToList();
            if (flavors.SequenceEqual(new[] { QuarkFlavor.Up, QuarkFlavor.Up, QuarkFlavor.Down }) ||
                flavors.SequenceEqual(new[] { QuarkFlavor.Up, QuarkFlavor.Down, QuarkFlavor.Down }))
                return true;
        }
        
        return false;
    }

    private double DetermineLifetime(List<QuarkState> quarks, HadronType type)
    {
        // Simplified lifetime calculation
        if (DetermineStability(quarks, type))
            return double.PositiveInfinity;
        
        // Default unstable particle lifetime (in seconds)
        return 1e-10; // 100 picoseconds
    }
}