using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 4. Hafta: Araziden yükseklik okuyan, duvarları eğime göre batıran dinamik labirent üretici.
/// ISearchGrid arayüzünü uygular. Duvar ekleme/kaldırma (ToggleWall) ve OnCellChanged olayını barındırır.
/// </summary>
public class MazeGenerator : MonoBehaviour, ISearchGrid
{
    public const int WALL = 1;
    public const int FLOOR = 0;

    [Header("Labirent Boyutları")]
    public int width = 31;
    public int depth = 31;
    public float cellSize = 2f;
    public int seed = 2026;
    [Range(0f, 0.2f)] public float loopChance = 0.08f;

    [Header("Görseller ve Prefab")]
    public GameObject wallPrefab;
    public Transform floor;

    [Header("Arazi Entegrasyonu")]
    public Terrain terrain;
    public float wallHeight = 2f;
    public float wallSink = 1f;

    public event Action OnRegenerated;
    public event Action<Vector2Int> OnCellChanged;

    public int Width => width;
    public int Depth => depth;
    public float CellSize => cellSize;
    public int[,] Map { get; private set; }
    public Vector2Int StartCell { get; private set; }
    public Vector2Int GoalCell { get; private set; }
    public float MinCellCost => 1f;

    readonly Dictionary<Vector2Int, GameObject> wallObjects = new();

    void Awake()
    {
        if (terrain == null) terrain = Terrain.activeTerrain;
        Generate();
    }

    void Update()
    {
        bool rPressed = false;
        if (Keyboard.current != null)
            rPressed = Keyboard.current.rKey.wasPressedThisFrame;

        if (rPressed)
        {
            seed = UnityEngine.Random.Range(1000, 9999);
            Generate();
        }
    }

    [ContextMenu("Yeniden Üret")]
    public void Generate()
    {
        UnityEngine.Random.InitState(seed);
        Map = new int[width, depth];

        // 1. Her yeri duvar yap
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                Map[x, z] = WALL;

        // 2. DFS ile yollar aç (koridor üretimi)
        StartCell = new Vector2Int(1, 1);
        GoalCell = new Vector2Int(width - 2, depth - 2);

        Stack<Vector2Int> stack = new();
        Map[StartCell.x, StartCell.y] = FLOOR;
        stack.Push(StartCell);

        Vector2Int[] dirs = { new(0, 2), new(0, -2), new(2, 0), new(-2, 0) };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> unvisited = new();

            foreach (var d in dirs)
            {
                Vector2Int n = current + d;
                if (InBounds(n) && Map[n.x, n.y] == WALL)
                    unvisited.Add(d);
            }

            if (unvisited.Count > 0)
            {
                Vector2Int chosenDir = unvisited[UnityEngine.Random.Range(0, unvisited.Count)];
                Vector2Int wallBetween = current + chosenDir / 2;
                Vector2Int nextCell = current + chosenDir;

                Map[wallBetween.x, wallBetween.y] = FLOOR;
                Map[nextCell.x, nextCell.y] = FLOOR;
                stack.Push(nextCell);
            }
            else
            {
                stack.Pop();
            }
        }

        // 3. Döngü şansı ile bazı duvarları kaldırarak alternatif yollar aç
        for (int x = 1; x < width - 1; x++)
        {
            for (int z = 1; z < depth - 1; z++)
            {
                if (Map[x, z] == WALL && UnityEngine.Random.value < loopChance)
                {
                    bool yatayGecis = (Map[x - 1, z] == FLOOR && Map[x + 1, z] == FLOOR);
                    bool dikeyGecis = (Map[x, z - 1] == FLOOR && Map[x, z + 1] == FLOOR);
                    if (yatayGecis || dikeyGecis) Map[x, z] = FLOOR;
                }
            }
        }

        Build();
        OnRegenerated?.Invoke();
    }

    public void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject old = transform.GetChild(i).gameObject;
            old.SetActive(false);
            Destroy(old);
        }
        wallObjects.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (Map[x, z] == WALL)
                {
                    var c = new Vector2Int(x, z);
                    wallObjects[c] = SpawnWall(c);
                }
            }
        }

        if (floor != null)
        {
            floor.position = transform.position +
                new Vector3((width - 1) * cellSize * 0.5f, 0f, (depth - 1) * cellSize * 0.5f);
            floor.localScale = new Vector3(width * cellSize / 10f, 1f, depth * cellSize / 10f);
        }
    }

    GameObject SpawnWall(Vector2Int c)
    {
        float h = wallHeight + wallSink;
        Vector3 pos = CellToWorld(c) + Vector3.up * ((wallHeight - wallSink) * 0.5f);
        GameObject w;
        if (wallPrefab != null)
        {
            w = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
        }
        else
        {
            w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = $"Wall_{c.x}_{c.y}";
            w.transform.SetParent(transform);
            w.transform.position = pos;
        }
        w.transform.localScale = new Vector3(cellSize, h, cellSize);
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer != -1) w.layer = wallLayer;
        return w;
    }

    public bool ToggleWall(Vector2Int c)
    {
        bool border = c.x <= 0 || c.y <= 0 || c.x >= width - 1 || c.y >= depth - 1;
        if (!InBounds(c) || border) return false;

        if (Map[c.x, c.y] == WALL)
        {
            Map[c.x, c.y] = FLOOR;
            if (wallObjects.TryGetValue(c, out GameObject w))
            {
                if (w != null)
                {
                    w.SetActive(false);
                    Destroy(w);
                }
                wallObjects.Remove(c);
            }
        }
        else
        {
            Map[c.x, c.y] = WALL;
            wallObjects[c] = SpawnWall(c);
        }

        OnCellChanged?.Invoke(c);
        return true;
    }

    public bool IsWalkable(Vector2Int c) => InBounds(c) && Map[c.x, c.y] == FLOOR;

    public bool InBounds(Vector2Int c) =>
        c.x >= 0 && c.x < width && c.y >= 0 && c.y < depth;

    public Vector3 CellToWorld(Vector2Int c)
    {
        Vector3 p = transform.position + new Vector3(c.x * cellSize, 0f, c.y * cellSize);
        if (terrain != null)
            p.y = terrain.transform.position.y + terrain.SampleHeight(p);
        return p;
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - transform.position;
        int x = Mathf.RoundToInt(local.x / cellSize);
        int z = Mathf.RoundToInt(local.z / cellSize);
        return new Vector2Int(x, z);
    }

    public float CellCost(Vector2Int from, Vector2Int to) => 1f;
}
