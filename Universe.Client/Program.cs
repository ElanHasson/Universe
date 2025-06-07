using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using System.CommandLine;
using System.Numerics;
using Universe.Abstractions.Grains;
using Universe.Abstractions.Physics;
using Universe.Client;

var rootCommand = new RootCommand("Universe Quark Simulator Client");

// Build Orleans client
var hostBuilder = new HostBuilder()
    .UseOrleansClient(client =>
    {
        client.UseLocalhostClustering()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "Universe";
            });
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<SimulationCommands>();
    });

using var host = hostBuilder.Build();
await host.StartAsync();

var clusterClient = host.Services.GetRequiredService<IClusterClient>();
var simulationCommands = host.Services.GetRequiredService<SimulationCommands>();

// Define commands
var createProtonCommand = new Command("create-proton", "Create a proton (uud)")
{
    new Option<float>("--x", () => 0f, "X position"),
    new Option<float>("--y", () => 0f, "Y position"),
    new Option<float>("--z", () => 0f, "Z position")
};
createProtonCommand.SetHandler(async (float x, float y, float z) =>
{
    await simulationCommands.CreateProton(new Vector3(x, y, z));
}, 
createProtonCommand.Options.OfType<Option<float>>().ElementAt(0),
createProtonCommand.Options.OfType<Option<float>>().ElementAt(1),
createProtonCommand.Options.OfType<Option<float>>().ElementAt(2));

var createNeutronCommand = new Command("create-neutron", "Create a neutron (udd)")
{
    new Option<float>("--x", () => 0f, "X position"),
    new Option<float>("--y", () => 0f, "Y position"),
    new Option<float>("--z", () => 0f, "Z position")
};
createNeutronCommand.SetHandler(async (float x, float y, float z) =>
{
    await simulationCommands.CreateNeutron(new Vector3(x, y, z));
}, 
createNeutronCommand.Options.OfType<Option<float>>().ElementAt(0),
createNeutronCommand.Options.OfType<Option<float>>().ElementAt(1),
createNeutronCommand.Options.OfType<Option<float>>().ElementAt(2));

var createPionCommand = new Command("create-pion", "Create a pion")
{
    new Option<string>("--type", () => "neutral", "Pion type: positive, negative, or neutral"),
    new Option<float>("--x", () => 0f, "X position"),
    new Option<float>("--y", () => 0f, "Y position"),
    new Option<float>("--z", () => 0f, "Z position")
};
createPionCommand.SetHandler(async (string type, float x, float y, float z) =>
{
    await simulationCommands.CreatePion(type, new Vector3(x, y, z));
}, 
createPionCommand.Options.OfType<Option<string>>().ElementAt(0),
createPionCommand.Options.OfType<Option<float>>().ElementAt(0),
createPionCommand.Options.OfType<Option<float>>().ElementAt(1),
createPionCommand.Options.OfType<Option<float>>().ElementAt(2));

var statsCommand = new Command("stats", "Display simulation statistics");
statsCommand.SetHandler(async () =>
{
    await simulationCommands.ShowStats();
});

var startCommand = new Command("start", "Start simulation");
startCommand.SetHandler(async () =>
{
    await simulationCommands.Start();
});

var pauseCommand = new Command("pause", "Pause simulation");
pauseCommand.SetHandler(async () =>
{
    await simulationCommands.Pause();
});

var resetCommand = new Command("reset", "Reset simulation");
resetCommand.SetHandler(async () =>
{
    await simulationCommands.Reset();
});

var listQuarksCommand = new Command("list-quarks", "List all quarks in the simulation");
listQuarksCommand.SetHandler(async () =>
{
    await simulationCommands.ListQuarks();
});

var visualizeCommand = new Command("visualize", "Show ASCII visualization of the simulation");
visualizeCommand.SetHandler(async () =>
{
    await simulationCommands.Visualize();
});

var monitorCommand = new Command("monitor", "Monitor simulation in real-time")
{
    new Option<int>("--interval", () => 1, "Update interval in seconds")
};
monitorCommand.SetHandler(async (int interval) =>
{
    await simulationCommands.Monitor(interval);
}, monitorCommand.Options.OfType<Option<int>>().First());

var saveSnapshotCommand = new Command("save-snapshot", "Save simulation snapshot to file")
{
    new Option<string>("--file", () => $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.json", "Output filename")
};
saveSnapshotCommand.SetHandler(async (string file) =>
{
    await simulationCommands.SaveSnapshot(file);
}, saveSnapshotCommand.Options.OfType<Option<string>>().First());

// Add commands to root
rootCommand.AddCommand(createProtonCommand);
rootCommand.AddCommand(createNeutronCommand);
rootCommand.AddCommand(createPionCommand);
rootCommand.AddCommand(statsCommand);
rootCommand.AddCommand(startCommand);
rootCommand.AddCommand(pauseCommand);
rootCommand.AddCommand(resetCommand);
rootCommand.AddCommand(listQuarksCommand);
rootCommand.AddCommand(visualizeCommand);
rootCommand.AddCommand(monitorCommand);
rootCommand.AddCommand(saveSnapshotCommand);

// Run the command line app
return await rootCommand.InvokeAsync(args);