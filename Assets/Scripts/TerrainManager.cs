using UnityEngine;

public class TerrainManager
{
    private const byte threshold = 10; // Für Farb-Vergleich
    private TerrainType[,] logicGrid;
    private Color32[] visualBuffer;
    private Texture2D displayTexture;
    private int width, height;
    private bool isDirty; // Flag: Muss die Textur diesen Frame aktualisiert werden?

    public void LoadLevel(GameObject go, Level level)
    {
        width = level.Width;
        height = level.Height;
        logicGrid = new TerrainType[width, height];
        visualBuffer = new Color32[width * height];

        // 1. Die Anzeige-Textur erstellen (RAM-Instanz)
        displayTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        displayTexture.filterMode = FilterMode.Point;

        // 2. Pixels aus der VISUAL Texture holen (für die Optik)
        Color32[] originalVisual = level.VisualTexture.GetPixels32();
        System.Array.Copy(originalVisual, visualBuffer, originalVisual.Length);

        // 3. Pixels aus der LOGIC Mask holen (für die Physik)
        Color32[] maskPixels = level.LogicMask.GetPixels32();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color32 m = maskPixels[index];

                // Mapping auf das interne Physik-Grid
                if (CompareColor(m, Level.ColorDirt)) logicGrid[x, y] = TerrainType.Dirt;
                else if (CompareColor(m, Level.ColorSteel)) logicGrid[x, y] = TerrainType.Steel;
                else if (CompareColor(m, Level.ColorFire)) logicGrid[x, y] = TerrainType.Fire;
                else if (CompareColor(m, Level.ColorWater)) logicGrid[x, y] = TerrainType.Water;
                else if (CompareColor(m, Level.ColorOneWayLeft)) logicGrid[x, y] = TerrainType.OneWayLeft;
                else if (CompareColor(m, Level.ColorOneWayRight)) logicGrid[x, y] = TerrainType.OneWayRight;
                else logicGrid[x, y] = TerrainType.Empty;
            }
        }

        // 4. Anzeige-Textur füllen
        displayTexture.SetPixels32(visualBuffer);
        displayTexture.Apply();

        // 5. SpriteRenderer Setup
        SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = go.AddComponent<SpriteRenderer>();

        spriteRenderer.sprite = Sprite.Create(displayTexture,
            new Rect(0, 0, width, height), new Vector2(0f, 0f), 1);
    }

    // Die "Sonde" für die Lemming-KI
    public TerrainType GetTypeAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return TerrainType.Empty;
        return logicGrid[x, y];
    }

    public bool IsOutOfBounds(float x, float y)
    {
        return (x < 0 || x >= width || y < 0 || y >= height);
    }

    // Im TerrainManager.cs
    public TerrainType GetHighestSolidPixelInColumn(float worldX, float minY, float maxY, out float foundWorldY)
    {
        int x = Mathf.FloorToInt(worldX);
        int yStart = Mathf.FloorToInt(maxY);
        int yEnd = Mathf.FloorToInt(minY);

        for (int y = yStart; y >= yEnd; y--)
        {
            TerrainType t = GetTypeAt(x, y);
            if (t != TerrainType.Empty)
            {
                foundWorldY = y;
                return t;
            }
        }

        foundWorldY = 0;
        return TerrainType.Empty;
    }

    public bool DestroyTerrain(int x, int y, int direction = 0)
    {
        //Debug.Log($"DestroyTerrain({x}, {y})");
        if (x < 0 || x >= width || y < 0 || y >= height) return false;

        if (!IsTerrainDestructable(logicGrid[x, y], direction)) return false;

        logicGrid[x, y] = TerrainType.Empty;
        visualBuffer[y * width + x] = Color.black;
        isDirty = true;
        return true;
    }
    public bool BuildStair(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;

        if (logicGrid[x, y] != TerrainType.Empty) return false;

        logicGrid[x, y] = TerrainType.Stairs;
        visualBuffer[y * width + x] = Color.darkSlateGray;
        isDirty = true;
        return true;
    }

    // Wird vom GameManager am Ende jedes Frames aufgerufen
    public void RenderUpdate()
    {
        if (isDirty)
        {
            displayTexture.SetPixels32(visualBuffer);
            displayTexture.Apply();
            isDirty = false;
        }
    }
    private bool CompareColor(Color32 a, Color32 b)
    {
        // Kleiner Schwellenwert (Threshold), falls die vgmaps leichte Farbvariationen haben
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    public static bool IsTerrainPassable(TerrainType t)
    {
        return t == TerrainType.Empty
            || t == TerrainType.Stairs
            || t == TerrainType.Fire
            || t == TerrainType.Water
            || t == TerrainType.Bolt;
    }

    public static bool IsTerrainDestructable(TerrainType t, int direction = 0)
    {
        return t == TerrainType.Dirt
            || t == TerrainType.Stairs
            || (t == TerrainType.OneWayLeft && direction < 0)
            || (t == TerrainType.OneWayRight && direction > 0);
    }
    public static bool IsTerrainHazard(TerrainType t)
    {
        return t == TerrainType.Fire
            || t == TerrainType.Water
            || t == TerrainType.Bolt;
    }
}