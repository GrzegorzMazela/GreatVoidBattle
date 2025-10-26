namespace GreatVoidBattle.Core.Domains;

public class MovementPath
{
    public int Speed { get; private set; }

    public Position StartPosition { get; private set; }

    public Position TargetPosition { get; private set; }

    public bool IsCompleted => !Path.Any();

    public List<Position> Path { get; private set; } = [];

    public MovementPath(int speed, Position startPosition, Position targetPosition)
    {
        Speed = speed;
        StartPosition = startPosition;
        TargetPosition = targetPosition;
    }

    public void GeneratePath()
    {
        Path.Clear();
        int currentX = (int)Math.Round(StartPosition.X);
        int currentY = (int)Math.Round(StartPosition.Y);
        int targetX = (int)Math.Round(TargetPosition.X);
        int targetY = (int)Math.Round(TargetPosition.Y);

        bool moveHorizontally = true;
        while (currentX != targetX || currentY != targetY)
        {
            if (moveHorizontally && currentX != targetX)
            {
                currentX += currentX < targetX ? 1 : -1;
                Path.Add(new Position(currentX, currentY));
            }
            else if (!moveHorizontally && currentY != targetY)
            {
                currentY += currentY < targetY ? 1 : -1;
                Path.Add(new Position(currentX, currentY));
            }
            moveHorizontally = !moveHorizontally;
        }
    }

    public void MoveOneStep()
    {
        if (Path.Any())
        {
            for (int i = 0; i < Speed && Path.Any(); i++)
            {
                NewStartPosition(Path[0]);
                Path.RemoveAt(0);
            }
        }
    }

    public void NewStartPosition(Position newStartPosition)
    {
        StartPosition = newStartPosition;
    }

    public void NewTargetPosition(Position newTargetPosition)
    {
        TargetPosition = newTargetPosition;
    }
}