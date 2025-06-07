using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System.Numerics;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;

namespace Universe.Grains;

public class SimulationOrchestratorGrain : Grain, ISimulationOrchestratorGrain
{
    private readonly IPersistentState<SimulationState> _state;
    private readonly ILogger<SimulationOrchestratorGrain> _logger;
    private IGrainTimer? _simulationTimer;

    public SimulationOrchestratorGrain(
        [PersistentState("simulation", "simulations")] IPersistentState<SimulationState> state,
        ILogger<SimulationOrchestratorGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _state.State ??= new SimulationState();
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Guid> CreateQuark(QuarkFlavor flavor, ColorCharge color, bool isAntiParticle, 
        Vector3 position, Vector3 momentum, double spinZ)
    {
        var quarkId = Guid.NewGuid();
        var quark = GrainFactory.GetGrain<IQuarkGrain>(quarkId);
        await quark.Initialize(flavor, color, isAntiParticle, position, momentum, spinZ);
        
        _state.State.QuarkIds.Add(quarkId);
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Created quark {QuarkId} with flavor {Flavor}", quarkId, flavor);
        return quarkId;
    }

    public async Task<List<Guid>> CreateQuarkPair(QuarkFlavor flavor, Vector3 position)
    {
        // Create matter-antimatter pair with opposite colors
        var color = (ColorCharge)Random.Shared.Next(3); // Red, Green, or Blue
        var antiColor = color.GetAntiColor();
        
        // Give them opposite momenta (momentum conservation)
        var momentum = new Vector3(
            (float)(Random.Shared.NextDouble() - 0.5),
            (float)(Random.Shared.NextDouble() - 0.5),
            (float)(Random.Shared.NextDouble() - 0.5)
        ) * 0.1f; // Small initial momentum
        
        var quarkId = await CreateQuark(flavor, color, false, position, momentum, 0.5);
        var antiQuarkId = await CreateQuark(flavor, antiColor, true, position, -momentum, -0.5);
        
        return new List<Guid> { quarkId, antiQuarkId };
    }

    public async Task<List<Guid>> CreateMeson(QuarkFlavor quark1Flavor, QuarkFlavor quark2Flavor, Vector3 position)
    {
        // Create quark-antiquark pair
        var color = (ColorCharge)Random.Shared.Next(3);
        var antiColor = color.GetAntiColor();
        
        var quarkIds = new List<Guid>();
        quarkIds.Add(await CreateQuark(quark1Flavor, color, false, position, Vector3.Zero, 0.5));
        quarkIds.Add(await CreateQuark(quark2Flavor, antiColor, true, position, Vector3.Zero, -0.5));
        
        // Create hadron grain to manage the meson
        var hadronId = Guid.NewGuid();
        var hadron = GrainFactory.GetGrain<IHadronGrain>(hadronId);
        await hadron.Initialize(quarkIds, HadronType.Meson);
        
        _state.State.HadronIds.Add(hadronId);
        await _state.WriteStateAsync();
        
        return quarkIds;
    }

    public async Task<List<Guid>> CreateBaryon(QuarkFlavor[] flavors, Vector3 position)
    {
        if (flavors.Length != 3)
            throw new ArgumentException("Baryons must have exactly 3 quarks");

        // Ensure color neutrality (red + green + blue)
        var colors = new[] { ColorCharge.Red, ColorCharge.Green, ColorCharge.Blue };
        var quarkIds = new List<Guid>();
        
        // Place quarks in a small triangle formation around the center position
        var offsets = new[]
        {
            new Vector3(0.1f, 0, 0),      // Red quark slightly to the right
            new Vector3(-0.05f, 0.087f, 0), // Green quark upper left
            new Vector3(-0.05f, -0.087f, 0) // Blue quark lower left
        };
        
        for (int i = 0; i < 3; i++)
        {
            var spin = i == 0 ? 0.5 : -0.5; // Simplified spin assignment
            var quarkPosition = position + offsets[i];
            quarkIds.Add(await CreateQuark(flavors[i], colors[i], false, quarkPosition, Vector3.Zero, spin));
        }
        
        // Create hadron grain to manage the baryon
        var hadronId = Guid.NewGuid();
        var hadron = GrainFactory.GetGrain<IHadronGrain>(hadronId);
        await hadron.Initialize(quarkIds, HadronType.Baryon);
        
        _state.State.HadronIds.Add(hadronId);
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Created baryon with quarks {Flavors}", string.Join(", ", flavors));
        return quarkIds;
    }

    public async Task<List<QuarkState>> GetAllQuarks()
    {
        var tasks = _state.State.QuarkIds.Select(id => 
            GrainFactory.GetGrain<IQuarkGrain>(id).GetState()).ToList();
        return (await Task.WhenAll(tasks)).ToList();
    }

    public async Task<List<StrongForceInteraction>> GetActiveInteractions()
    {
        var interactions = new List<StrongForceInteraction>();
        var quarks = await GetAllQuarks();
        
        // Calculate all pairwise interactions
        for (int i = 0; i < quarks.Count; i++)
        {
            for (int j = i + 1; j < quarks.Count; j++)
            {
                var interaction = StrongForceCalculator.CalculateInteraction(quarks[i], quarks[j]);
                interactions.Add(interaction);
            }
        }
        
        return interactions;
    }

    public async Task RunSimulationStep(double deltaTime)
    {
        if (!_state.State.IsRunning)
            return;

        // Update all hadrons (which will update their constituent quarks)
        var hadronTasks = _state.State.HadronIds.Select(id =>
            GrainFactory.GetGrain<IHadronGrain>(id).Evolve(deltaTime)).ToList();
        await Task.WhenAll(hadronTasks);
        
        // Update any free quarks (shouldn't exist due to confinement)
        var allQuarks = await GetAllQuarks();
        var freeQuarkIds = allQuarks
            .Where(q => !q.BoundPartners.Any())
            .Select(q => q.QuarkId)
            .ToList();
            
        if (freeQuarkIds.Any())
        {
            _logger.LogWarning("Found {Count} free quarks - attempting confinement", freeQuarkIds.Count);
            // In a real simulation, we would create new quark-antiquark pairs to ensure confinement
        }
        
        _state.State.SimulationTime += deltaTime;
        _state.State.StepCount++;
        await _state.WriteStateAsync();
    }

    public async Task<SimulationStats> GetSimulationStats()
    {
        var quarks = await GetAllQuarks();
        var freeQuarks = quarks.Where(q => !q.BoundPartners.Any()).ToList();
        var confinedQuarks = quarks.Where(q => q.BoundPartners.Any()).ToList();
        
        var hadronStates = new List<HadronState>();
        foreach (var hadronId in _state.State.HadronIds)
        {
            var hadron = GrainFactory.GetGrain<IHadronGrain>(hadronId);
            hadronStates.Add(await hadron.GetState());
        }
        
        var mesons = hadronStates.Count(h => h.Type == HadronType.Meson);
        var baryons = hadronStates.Count(h => h.Type == HadronType.Baryon || h.Type == HadronType.AntiBaryon);
        
        var totalEnergy = quarks.Sum(q => q.Energy);
        var totalMomentum = quarks.Aggregate(Vector3.Zero, (sum, q) => sum + q.Momentum).Length();
        
        return new SimulationStats
        {
            TotalQuarks = quarks.Count,
            FreeeQuarks = freeQuarks.Count,
            ConfinedQuarks = confinedQuarks.Count,
            Mesons = mesons,
            Baryons = baryons,
            TotalEnergy = totalEnergy,
            TotalMomentum = totalMomentum,
            SimulationTime = _state.State.SimulationTime,
            InteractionCount = quarks.Count * (quarks.Count - 1) / 2
        };
    }

    public async Task Reset()
    {
        _simulationTimer?.Dispose();
        _simulationTimer = null;
        
        _state.State = new SimulationState();
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Simulation reset");
    }

    public async Task Pause()
    {
        _state.State.IsRunning = false;
        _simulationTimer?.Dispose();
        _simulationTimer = null;
        await _state.WriteStateAsync();
        
        _logger.LogInformation("Simulation paused");
    }

    public async Task Resume()
    {
        _state.State.IsRunning = true;
        await _state.WriteStateAsync();
        
        // Start simulation timer
        _simulationTimer = this.RegisterGrainTimer(
            async () => await RunSimulationStep(0.001), // 1ms time steps
            new GrainTimerCreationOptions
            {
                DueTime = TimeSpan.Zero,
                Period = TimeSpan.FromMilliseconds(10),
                Interleave = true
            });
        
        _logger.LogInformation("Simulation resumed");
    }
}

[GenerateSerializer]
public class SimulationState
{
    [Id(0)] public List<Guid> QuarkIds { get; set; } = new();
    [Id(1)] public List<Guid> HadronIds { get; set; } = new();
    [Id(2)] public double SimulationTime { get; set; }
    [Id(3)] public long StepCount { get; set; }
    [Id(4)] public bool IsRunning { get; set; }
}