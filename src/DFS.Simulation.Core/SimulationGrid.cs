namespace DFS.Simulation.Core;

public class SimulationGrid<T>(int width, int height)
{
    private readonly T[] _grid = new T[width * height];

    public int Width { get; } = width;
    public int Height { get; } = height;

    public T this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException($"Coordinates ({x}, {y}) are out of bounds. Grid size: {Width}x{Height}.");
            }
            return _grid[y * Width + x];
        }
        set
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new IndexOutOfRangeException($"Coordinates ({x}, {y}) are out of bounds. Grid size: {Width}x{Height}.");
            }
            _grid[y * Width + x] = value;
        }
    }
}
