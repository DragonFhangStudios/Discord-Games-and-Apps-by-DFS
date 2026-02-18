using DFS.Simulation.Core;
using Xunit;

namespace DFS.Simulation.Tests;

public class DisasterManagerTests
{
    [Fact]
    public void TriggerRandomEvent_ModifiesGridCell()
    {
        // Arrange
        var grid = new SimulationGrid<Cell>(10, 10);
        var manager = new DisasterManager(grid);

        // Act
        manager.TriggerRandomEvent();

        // Assert
        bool modified = false;
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                var cell = grid[x, y];
                if (cell.Radiation == 500f || cell.Temperature == -50f)
                {
                    modified = true;
                    break;
                }
            }
            if (modified) break;
        }

        Assert.True(modified, "Expected at least one cell to be modified with high radiation or low temperature.");
    }
}
