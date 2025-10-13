namespace GreatVoidBattle.Core.Domains;

public class MovementPath
{
    public Guid ObjectId { get; private set; }
    public Position StartPosition { get; private set; } 

    public Guid? TargetId { get; private set; }
    public Position TargetPosition { get; private set; }

    public List<Position> Path { get; private set; } = [];

    public MovementPath(Guid objectId, Position startPosition, Position targetPosition)
    {
        ObjectId = objectId;
        StartPosition = startPosition;
        TargetPosition = targetPosition;
    }

    public MovementPath(Guid objectId, Position startPosition, Guid targetId, Position targetPosition) : this(objectId, startPosition, targetPosition)
    {
        TargetId = targetId;
    }

    //Wygeneruj mi metode ktora wyznacza sciezke z StartPosition do TargetPosition i zapisuje ja w Path
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
            }
            else if (!moveHorizontally && currentY != targetY)
            {
                currentY += currentY < targetY ? 1 : -1;
            }
            // Only add if position changed
            Path.Add(new Position(currentX, currentY));
            moveHorizontally = !moveHorizontally;
        }
    }
}
