using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Moq;

using MudServer.Server.Models;
using MudServer.Server.Services;

using Xunit;

namespace MudServer.Tests
{
    public class GameLoopTests
    {
        public GameLoop GameLoop { get; }

        public GameLoopTests()
        {
            // Arrange
            var actionManager = new Mock<IActionManager>();
            actionManager
                .Setup(am => am.CollectActionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<GameAction>());
            var gameStateManager = new Mock<IGameStateManager>();
            gameStateManager
                .Setup(gsm => gsm.StartTurn()).Returns(new Turn(1));
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
            var cancellationTokenSource = new CancellationTokenSource(10);
            var phaseChangedCount = 0;

            this.GameLoop.OnPhaseChanged += (phase, turnNumber) =>
            {
                phaseChangedCount++;
            };

            // Act
            var task = this.GameLoop.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(5); // Allow some time for phases to change
            cancellationTokenSource.Cancel(); // Stop the game loop

            // Assert
            Assert.True(phaseChangedCount > 0);
        }

        [Fact]
        public async Task StartAsync_ShouldIncrementTurnNumber()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource(10);

            // Act
            var task = this.GameLoop.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(5); // Allow some time for phases to change
            if (task.Exception != null) throw task.Exception;
            cancellationTokenSource.Cancel(); // Stop the game loop

            Assert.True(this.GameLoop.TurnNumber > 1);
        }
    }
}