using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MudServer.Models;

namespace MudServer.Tests
{
    public class GameLoopTests
    {
        [Fact]
        public void InitialPhase_ShouldBeTurnStart()
        {
            // Arrange
            var gameLoop = new GameLoop();

            // Act
            var initialPhase = gameLoop.CurrentPhase;

            // Assert
            Assert.Equal(GameLoop.Phase.TurnStart, initialPhase);
        }

        [Fact]
        public void InitialTurnNumber_ShouldBeOne()
        {
            // Arrange
            var gameLoop = new GameLoop();

            // Act
            var initialTurnNumber = gameLoop.TurnNumber;

            // Assert
            Assert.Equal(1, initialTurnNumber);
        }

        [Fact]
        public async Task StartAsync_ShouldTransitionPhases()
        {
            // Arrange
            var gameLoop = new GameLoop();
            var cancellationTokenSource = new CancellationTokenSource();
            var phaseChangedCount = 0;

            gameLoop.OnPhaseChanged += (phase, turnNumber) =>
            {
                phaseChangedCount++;
            };

            // Act
            var task = gameLoop.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(5000); // Allow some time for phases to change
            cancellationTokenSource.Cancel(); // Stop the game loop

            // Assert
            Assert.True(phaseChangedCount > 0);
        }

        [Fact]
        public async Task StartAsync_ShouldIncrementTurnNumber()
        {
            // Arrange
            var gameLoop = new GameLoop();
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = gameLoop.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(5000); // Allow some time for the game loop to run
            cancellationTokenSource.Cancel(); // Stop the game loop

            // Assert
            Assert.True(gameLoop.TurnNumber > 1);
        }
    }
}