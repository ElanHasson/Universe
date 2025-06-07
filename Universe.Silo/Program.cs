using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.UseLocalhostClustering()
            .Configure<Orleans.Configuration.ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "Universe";
            })
            .AddMemoryGrainStorage("simulations")
            .AddMemoryGrainStorage("quarks")
            .AddMemoryGrainStorage("hadrons")
            .AddMemoryGrainStorage("gluons")
            .ConfigureLogging(logging => logging.AddConsole())
            .UseDashboard(x => x.HostSelf = true); // Orleans Dashboard on http://localhost:8080
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    });

using var host = builder.Build();

Console.WriteLine("Starting Orleans Silo...");
await host.RunAsync();
Console.WriteLine("Orleans Silo stopped.");
