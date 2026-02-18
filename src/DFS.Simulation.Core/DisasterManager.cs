using System;

namespace DFS.Simulation.Core;

public class DisasterManager(SimulationGrid<Cell> grid)
{
    private readonly Random _random = new();

    public void TriggerRandomEvent()
    {
        int x = _random.Next(grid.Width);
        int y = _random.Next(grid.Height);

        var cell = grid[x, y];
        if (_random.Next(2) == 0)
        {
            cell.Radiation = 500f;
        }
        else
        {
            cell.Temperature = -50f;
        }
        grid[x, y] = cell;
    }
}
