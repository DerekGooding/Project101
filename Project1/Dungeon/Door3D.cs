
namespace Project1.Dungeon;

public class Door3D(Point tileA, Point tileB, int orientation, bool isLocked = false, string? keyId = null)
{
    // Orientation values
    public const int NORTH_SOUTH = 0; // Door aligned along north-south axis
    public const int EAST_WEST = 1;   // Door aligned along east-west axis

    // Store the two adjacent tile positions
    public Point TileA { get; } = tileA;
    public Point TileB { get; } = tileB;
    public int Orientation { get; } = orientation;
    public bool IsLocked { get; private set; } = isLocked;
    public string? KeyId { get; } = keyId;

    // Get the world position for rendering - center between the two tiles
    public Vector3 GetWorldPosition(float tileSize) => new(
            (TileA.X + TileB.X) * tileSize / 2,
            0,
            (TileA.Y + TileB.Y) * tileSize / 2);

    public Matrix GetWorldMatrix(float tileSize)
    {
        var position = GetWorldPosition(tileSize);

        // Rotate the door based on orientation
        if (Orientation == EAST_WEST)
        {
            // Door aligned along east-west axis (rotate 90 degrees)
            return Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(position);
        }
        else
        {
            // Door aligned along north-south axis (default orientation)
            return Matrix.CreateTranslation(position);
        }
    }

    internal void SetLocked(bool lockStatus) => IsLocked = lockStatus;

    public static VertexPositionTexture[] CreateDoorVertices(float size)
    {
        var s = size / 2f;
        const float thickness = 0.1f;

        // Create a thin vertical door panel
        return [
            // Front face
            new VertexPositionTexture(new Vector3(-s, -s, -thickness), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(s, -s, -thickness), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(s, s, -thickness), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-s, s, -thickness), new Vector2(0, 0)),

            // Back face
            new VertexPositionTexture(new Vector3(s, -s, thickness), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-s, -s, thickness), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-s, s, thickness), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(s, s, thickness), new Vector2(0, 0)),

            // Edges (to give it some thickness)
            // Top edge
            new VertexPositionTexture(new Vector3(-s, s, -thickness), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(s, s, -thickness), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(s, s, thickness), new Vector2(1, 0.1f)),
            new VertexPositionTexture(new Vector3(-s, s, thickness), new Vector2(0, 0.1f)),

            // Bottom edge
            new VertexPositionTexture(new Vector3(-s, -s, thickness), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(s, -s, thickness), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(s, -s, -thickness), new Vector2(1, 0.1f)),
            new VertexPositionTexture(new Vector3(-s, -s, -thickness), new Vector2(0, 0.1f)),

            // Left edge
            new VertexPositionTexture(new Vector3(-s, -s, thickness), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-s, -s, -thickness), new Vector2(0.1f, 1)),
            new VertexPositionTexture(new Vector3(-s, s, -thickness), new Vector2(0.1f, 0)),
            new VertexPositionTexture(new Vector3(-s, s, thickness), new Vector2(0, 0)),

            // Right edge
            new VertexPositionTexture(new Vector3(s, -s, -thickness), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(s, -s, thickness), new Vector2(0.1f, 1)),
            new VertexPositionTexture(new Vector3(s, s, thickness), new Vector2(0.1f, 0)),
            new VertexPositionTexture(new Vector3(s, s, -thickness), new Vector2(0, 0))
        ];
    }

    public static short[] GetDoorIndices() =>
    [
        0, 1, 2, 0, 2, 3,       // Front face
        4, 5, 6, 4, 6, 7,       // Back face
        8, 9, 10, 8, 10, 11,    // Top edge
        12, 13, 14, 12, 14, 15, // Bottom edge
        16, 17, 18, 16, 18, 19, // Left edge
        20, 21, 22, 20, 22, 23  // Right edge
    ];
}