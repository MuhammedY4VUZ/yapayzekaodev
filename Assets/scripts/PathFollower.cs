using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 4. Hafta: Karakteri bulunan A* yolundaki hücreler boyunca CharacterController ile yürüten bileşen.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PathFollower : MonoBehaviour
{
    public GridPathfinder pathfinder
    {
        get => _pathfinder;
        set
        {
            if (_pathfinder != null) _pathfinder.OnPathFound -= Follow;
            _pathfinder = value;
            if (_pathfinder != null) _pathfinder.OnPathFound += Follow;
        }
    }
    [SerializeField] GridPathfinder _pathfinder;

    public MonoBehaviour gridSource;
    public float speed = 4.5f;
    public float turnSpeed = 10f;
    public float arriveRadius = 0.4f;
    public float gravity = -20f;

    public bool IsMoving => route != null && index < route.Count;
    public IReadOnlyList<Vector2Int> Route => route;
    public int Index => index;

    CharacterController cc;
    ISearchGrid grid;
    List<Vector2Int> route;
    int index;
    float verticalSpeed;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        grid = gridSource as ISearchGrid;
    }

    void OnEnable()
    {
        BindReferences();
    }

    void Start()
    {
        BindReferences();
        if (grid != null) Teleport(grid.StartCell);
    }

    public void BindReferences()
    {
        if (cc == null) cc = GetComponent<CharacterController>();
        if (gridSource == null) gridSource = Object.FindFirstObjectByType<MazeGenerator>();
        grid = gridSource as ISearchGrid;

        if (_pathfinder == null) _pathfinder = Object.FindFirstObjectByType<GridPathfinder>();
        if (_pathfinder != null)
        {
            _pathfinder.OnPathFound -= Follow;
            _pathfinder.OnPathFound += Follow;
        }

        if (gridSource is MazeGenerator maze)
        {
            maze.OnRegenerated -= HandleRegenerate;
            maze.OnRegenerated += HandleRegenerate;
        }
    }

    void HandleRegenerate()
    {
        if (grid == null) grid = gridSource as ISearchGrid;
        if (grid != null) Teleport(grid.StartCell);
    }

    public void Teleport(Vector2Int cell)
    {
        if (grid == null) grid = gridSource as ISearchGrid;
        if (grid == null || cc == null) return;

        cc.enabled = false;
        float up = cc.height * 0.5f - cc.center.y + 0.1f;
        transform.position = grid.CellToWorld(cell) + Vector3.up * up;
        cc.enabled = true;
        route = null;
        verticalSpeed = 0f;
    }

    public void Follow(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0) return;
        route = new List<Vector2Int>(path);
        index = 0;
        Debug.Log($"🚶 Oyuncu_AStar yola koyuldu! Adım sayısı: {route.Count}");
    }

    public void Stop() => route = null;

    void Update()
    {
        Vector3 horizontal = Vector3.zero;

        if (IsMoving)
        {
            if (grid == null) grid = gridSource as ISearchGrid;
            if (grid != null)
            {
                Vector3 target = grid.CellToWorld(route[index]);
                Vector3 toTarget = target - transform.position;
                toTarget.y = 0f;

                if (toTarget.magnitude <= arriveRadius)
                {
                    index++;
                }
                else
                {
                    Vector3 dir = toTarget.normalized;
                    float t = 1f - Mathf.Exp(-turnSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), t);
                    horizontal = dir * speed;
                }
            }
        }

        if (cc != null)
        {
            if (cc.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed += gravity * Time.deltaTime;
            cc.Move((horizontal + Vector3.up * verticalSpeed) * Time.deltaTime);
        }
    }
}
