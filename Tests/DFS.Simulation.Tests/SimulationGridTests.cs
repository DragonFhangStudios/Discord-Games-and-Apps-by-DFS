using DFS.Simulation.Core;
using Xunit;

namespace DFS.Simulation.Tests;

public class SimulationGridTests
{
    [Fact]
    public void Indexer_ThrowsIndexOutOfRangeException_WhenOutOfBounds()
    {
        // Arrange
        var grid = new SimulationGrid<int>(10, 10);

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => grid[-1, 0]);
        Assert.Throws<IndexOutOfRangeException>(() => grid[0, -1]);
        Assert.Throws<IndexOutOfRangeException>(() => grid[10, 0]);
        Assert.Throws<IndexOutOfRangeException>(() => grid[0, 10]);
        Assert.Throws<IndexOutOfRangeException>(() => grid[100, 100]);
    }

    [Fact]
    public void Indexer_ReturnsCorrectValue_WhenInBounds()
    {
        // Arrange
        var grid = new SimulationGrid<int>(5, 5);
        grid[2, 2] = 42;

        // Act
        var result = grid[2, 2];

        // Assert
        Assert.Equal(42, result);
    }
}
