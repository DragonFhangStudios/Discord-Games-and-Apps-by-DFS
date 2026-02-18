using DFS.Simulation.Core;
using Xunit;

namespace DFS.Simulation.Tests;

public class WorldTickerTests
{
    [Fact]
    public void Tick_SpreadsRadiation()
    {
        // Arrange
        var grid = new SimulationGrid<Cell>(3, 3);

        var centerCell = grid[1, 1];
        centerCell.Radiation = 100f;
        grid[1, 1] = centerCell;

        var ticker = new WorldTicker(grid);

        // Act
        ticker.Tick();

        // Assert
        // Center should be (100 + 0 + 0 + 0 + 0) / 5 = 20
        Assert.Equal(20f, grid[1, 1].Radiation, 0.01f);

        // Neighbors should be (0 + 100 + 0 + 0) / 4 = 25 (since they are on edge, 3 neighbors + self)
        Assert.Equal(25f, grid[0, 1].Radiation, 0.01f);
        Assert.Equal(25f, grid[2, 1].Radiation, 0.01f);
        Assert.Equal(25f, grid[1, 0].Radiation, 0.01f);
        Assert.Equal(25f, grid[1, 2].Radiation, 0.01f);

        // Corners should be 0 as they don't touch center
        Assert.Equal(0f, grid[0, 0].Radiation);
        Assert.Equal(0f, grid[2, 0].Radiation);
        Assert.Equal(0f, grid[0, 2].Radiation);
        Assert.Equal(0f, grid[2, 2].Radiation);
    }
}
