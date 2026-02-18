namespace DFS.Simulation.Core;

public interface IAgent
{
    Guid Id { get; }
    Vector2Int Position { get; }
    void Tick(float deltaTime);
}
