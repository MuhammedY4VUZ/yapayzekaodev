using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 4. Hafta: A* tarafından bulunan yolu LineRenderer ile arazi üstünde kesintisiz çizen bileşen.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PathLine : MonoBehaviour
{
    public GridPathfinder pathfinder
    {
        get => _pathfinder;
        set
        {
            if (_pathfinder != null) _pathfinder.OnPathFound -= Draw;
            _pathfinder = value;
            if (_pathfinder != null) _pathfinder.OnPathFound += Draw;
        }
    }
    [SerializeField] GridPathfinder _pathfinder;

    public MonoBehaviour gridSource;
    public float lift = 0.3f;

    LineRenderer line;
    ISearchGrid grid;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
    }

    void OnEnable()
    {
        BindReferences();
    }

    void Start()
    {
        BindReferences();
    }

    public void BindReferences()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (gridSource == null) gridSource = Object.FindFirstObjectByType<MazeGenerator>();
        grid = gridSource as ISearchGrid;

        if (_pathfinder == null) _pathfinder = Object.FindFirstObjectByType<GridPathfinder>();
        if (_pathfinder != null)
        {
            _pathfinder.OnPathFound -= Draw;
            _pathfinder.OnPathFound += Draw;
        }

        if (gridSource is MazeGenerator maze)
        {
            maze.OnRegenerated -= Hide;
            maze.OnRegenerated += Hide;
        }
    }

    void Draw(List<Vector2Int> path)
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (grid == null) grid = gridSource as ISearchGrid;
        if (grid == null || path == null || line == null) return;

        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, grid.CellToWorld(path[i]) + Vector3.up * lift);
        }
    }

    public void Hide()
    {
        if (line != null) line.positionCount = 0;
    }
}
