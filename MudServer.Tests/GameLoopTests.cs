using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MudServer.Server.Models;
using Moq;
using MudServer.Server.Services;

namespace MudServer.Tests
{
    public class GameLoopTests
    {
        public GameLoop GameLoop { get; }

        public GameLoopTests()
        {
            // Arrange
            var actionManager = new Mock<IActionManager>();
            var gameStateManager = new Mock<IGameStateManager>();
            this.GameLoop = new GameLoop(actionManager.Object, gameStateManager.Object);
        }


        [Fact]
        public void InitialPhase_ShouldBeTurnStart()
        {
            // Act
            var initialPhase = this.GameLoop.CurrentPhase;

            // Assert
            Assert.Equal(GameLoop.Phase.TurnStart, initialPhase);
        }

        [Fact]
        public void InitialTurnNumber_ShouldBeOne()
        {
            // Act
            var initialTurnNumber = this.GameLoop.TurnNumber;

            // Assert
            Assert.Equal(1, initialTurnNumber);
        }

        [Fact]
        public async Task StartAsync_ShouldTransitionPhases()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var phaseChangedCount = 0;

            this.GameLoop.OnPhaseChanged += (phase, turnNumber) =>
            {
                phaseChangedCount++;
            };

            // Act
            var task = this.GameLoop.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(5000); // Allow some time for phases to change
            cancellationTokenSource.Cancel(); // Stop the game loop

            // Assert
            Assert.True(phaseChangedCount > 0);
        }

        [Fact]
        public async Task StartAsync_ShouldIncrementTurnNumber()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var task = this.GameLoop.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(5000); // Allow some time for the game loop to run
            cancellationTokenSource.Cancel(); // Stop the game loop

            // Assert
            Assert.True(this.GameLoop.TurnNumber > 1);
        }
    }
}