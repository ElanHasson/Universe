namespace Universe.Abstractions.Visualization;

public interface IVisualizationService
{
    Task<SimulationSnapshot> CaptureSnapshot();
    Task<string> GenerateASCIIVisualization(SimulationSnapshot snapshot);
    Task<string> GenerateJsonVisualization(SimulationSnapshot snapshot);
    Task SaveSnapshotToFile(SimulationSnapshot snapshot, string filePath);
    Task<List<SimulationSnapshot>> GetRecentSnapshots(int count);
}