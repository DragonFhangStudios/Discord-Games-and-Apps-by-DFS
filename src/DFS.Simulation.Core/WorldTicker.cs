namespace DFS.Simulation.Core;

public class WorldTicker(SimulationGrid<Cell> grid)
{
    private readonly SimulationGrid<Cell> _grid = grid;
    private readonly Cell[] _buffer = new Cell[grid.Width * grid.Height];

    public void Tick()
    {
        int width = _grid.Width;
        int height = _grid.Height;

        // Iterate and calculate new state into buffer
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = _grid[x, y];

                float totalRad = cell.Radiation;
                float totalTemp = cell.Temperature;
                int count = 1;

                // Up
                if (y > 0)
                {
                    var n = _grid[x, y - 1];
                    totalRad += n.Radiation;
                    totalTemp += n.Temperature;
                    count++;
                }

                // Down
                if (y < height - 1)
                {
                    var n = _grid[x, y + 1];
                    totalRad += n.Radiation;
                    totalTemp += n.Temperature;
                    count++;
                }

                // Left
                if (x > 0)
                {
                    var n = _grid[x - 1, y];
                    totalRad += n.Radiation;
                    totalTemp += n.Temperature;
                    count++;
                }

                // Right
                if (x < width - 1)
                {
                    var n = _grid[x + 1, y];
                    totalRad += n.Radiation;
                    totalTemp += n.Temperature;
                    count++;
                }

                var newCell = cell;
                newCell.Radiation = totalRad / count;
                newCell.Temperature = totalTemp / count;
                _buffer[y * width + x] = newCell;
            }
        }

        // Apply buffer back to grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _grid[x, y] = _buffer[y * width + x];
            }
        }
    }
}
