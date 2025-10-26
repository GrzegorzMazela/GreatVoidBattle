using GreatVoidBattle.Core.Domains;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains;

public class MovementPathTests
{
    [Fact]
    public void GeneratePath_CreatesCorrectPath_FromStartToTarget()
    {
        // Arrange
        var start = new Position(0, 0);
        var target = new Position(2, 2);
        var movementPath = new MovementPath(0, start, target);

        // Act
        movementPath.GeneratePath();

        // Assert
        movementPath.Path.ShouldNotBeNull();
        movementPath.Path.Count.ShouldBe(4); // (1,0), (1,1), (2,1), (2,2)
        movementPath.Path[0].ShouldBe(new Position(1, 0));
        movementPath.Path[1].ShouldBe(new Position(1, 1));
        movementPath.Path[2].ShouldBe(new Position(2, 1));
        movementPath.Path[3].ShouldBe(new Position(2, 2));
    }

    [Fact]
    public void GeneratePath_EmptyPath_WhenStartEqualsTarget()
    {
        // Arrange
        var pos = new Position(5, 5);
        var movementPath = new MovementPath(0, pos, pos);

        // Act
        movementPath.GeneratePath();

        // Assert
        movementPath.Path.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0, 0, 0, 3, 3)]
    [InlineData(2, 2, 5, 2, 3)]
    [InlineData(1, 1, 1, 5, 4)]
    [InlineData(1, 1, 5, 5, 8)]
    public void GeneratePath_CorrectPathLength(int startX, int startY, int targetX, int targetY, int expectedLength)
    {
        // Arrange
        var movementPath = new MovementPath(0, new Position(startX, startY), new Position(targetX, targetY));

        // Act
        movementPath.GeneratePath();

        // Assert
        movementPath.Path.Count.ShouldBe(expectedLength);
        movementPath.Path.Last().ShouldBe(new Position(targetX, targetY));
    }

    [Theory]
    [InlineData(6, 0, 10, 4)]
    [InlineData(2, 10, 0, 8)]
    [InlineData(6, 5, 0, 0)]
    [InlineData(6, 5, 5, 4)]
    public void MoveOneStep_ResultAfterFirstMove(int speed, int targetX, int targetY, int expectedLength)
    {
        // Arrange
        var movementPath = new MovementPath(speed, new Position(0, 0), new Position(targetX, targetY));

        // Act
        movementPath.GeneratePath();
        movementPath.MoveOneStep();

        // Assert
        movementPath.Path.Count.ShouldBe(expectedLength);
        if (expectedLength == 0)
            movementPath.IsCompleted.ShouldBeTrue();
        else
            movementPath.IsCompleted.ShouldBeFalse();
    }

    [Theory]
    [InlineData(10, 0, 4, 0)]
    [InlineData(0, 10, 0, 4)]
    [InlineData(10, 10, 2, 2)]
    public void MoveOneStep_StartingPositionAfterMove(int targetX, int targetY, int newStartX, int newStartY)
    {
        // Arrange
        var movementPath = new MovementPath(4, new Position(0, 0), new Position(targetX, targetY));

        // Act
        movementPath.GeneratePath();
        movementPath.MoveOneStep();

        // Assert
        movementPath.StartPosition.ShouldBe(new Position(newStartX, newStartY));
    }

    [Fact]
    public void MoveOneStep_MoveToEnd()
    {
        var movementPath = new MovementPath(4, new Position(0, 0), new Position(0, 10));

        // Act
        movementPath.GeneratePath();
        movementPath.MoveOneStep();

        // Assert
        movementPath.Path.Count.ShouldBe(6);
        movementPath.IsCompleted.ShouldBeFalse();

        // Act
        movementPath.MoveOneStep();

        // Assert
        movementPath.Path.Count.ShouldBe(2);
        movementPath.IsCompleted.ShouldBeFalse();

        // Act
        movementPath.MoveOneStep();
        // Assert
        movementPath.Path.Count.ShouldBe(0);
        movementPath.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public void GeneratePath_NewStartPosition()
    {
        // Arrange
        var movementPath = new MovementPath(4, new Position(0, 0), new Position(0, 10));

        // Act
        movementPath.GeneratePath();
        movementPath.NewStartPosition(new Position(0, 5));
        movementPath.GeneratePath();

        // Assert
        movementPath.Path.Count.ShouldBe(5);
        movementPath.Path.First().ShouldBe(new Position(0, 6));
        movementPath.Path.Last().ShouldBe(new Position(0, 10));
    }

    [Fact]
    public void GeneratePath_NewTargetPosition()
    {
        // Arrange
        var movementPath = new MovementPath(4, new Position(0, 0), new Position(0, 10));

        // Act
        movementPath.GeneratePath();
        movementPath.NewTargetPosition(new Position(0, 5));
        movementPath.GeneratePath();

        // Assert
        movementPath.Path.Count.ShouldBe(5);
        movementPath.Path.First().ShouldBe(new Position(0, 1));
        movementPath.Path.Last().ShouldBe(new Position(0, 5));
    }
}