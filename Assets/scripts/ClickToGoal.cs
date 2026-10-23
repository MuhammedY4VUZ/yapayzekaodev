using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// 4. Hafta: Sol tıklandığında hem NavMesh ajanına hem de kendi A* yol bulucumuza hedef veren bileşen.
/// Tıklanan yer duvara veya araziye denk gelse bile en yakın açık koridoru bulup ajanları oraya koşturur.
/// </summary>
public class ClickToGoal : MonoBehaviour
{
    public Camera cam;
    public MazeGenerator maze;
    public Transform goalFlag;
    public LayerMask groundMask;
    public NavMeshAgent navAgent;
    public GridPathfinder pathfinder;
    public Transform agent;

    void Update()
    {
        bool clicked = false;
        Vector2 mousePos = Vector2.zero;

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                clicked = true;
                mousePos = Mouse.current.position.ReadValue();
            }
        }

        if (!clicked) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (maze == null) maze = Object.FindFirstObjectByType<MazeGenerator>();
        if (maze == null) return;

        if (goalFlag == null)
        {
            var gf = GameObject.Find("GoalFlag");
            if (gf != null) goalFlag = gf.transform;
        }
        if (navAgent == null)
        {
            var naObj = GameObject.Find("NavAgent");
            if (naObj != null) navAgent = naObj.GetComponent<NavMeshAgent>();
        }
        if (pathfinder == null) pathfinder = Object.FindFirstObjectByType<GridPathfinder>();
        if (agent == null)
        {
            var aObj = GameObject.Find("Oyuncu_AStar");
            if (aObj != null) agent = aObj.transform;
        }

        int mask = groundMask.value;
        if (mask == 0)
        {
            int gLayer = LayerMask.NameToLayer("Ground");
            mask = (gLayer >= 0) ? (1 << gLayer) : ~0;
        }

        Ray ray = cam.ScreenPointToRay(mousePos);
        RaycastHit hit;
        bool hasHit = Physics.Raycast(ray, out hit, 1000f, mask);
        if (!hasHit)
        {
            hasHit = Physics.Raycast(ray, out hit, 1000f);
        }
        if (!hasHit) return;

        Vector2Int rawCell = maze.WorldToCell(hit.point);
        Vector2Int goal = maze.IsWalkable(rawCell) ? rawCell : FindNearestWalkableCell(maze, rawCell);

        Vector3 goalWorld = maze.CellToWorld(goal);
        if (goalFlag != null)
        {
            goalFlag.position = goalWorld;
            var col = goalFlag.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        // 1. NavMesh Ajanına Hedef Ver
        if (navAgent != null)
        {
            if (!navAgent.isOnNavMesh)
            {
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(navAgent.transform.position, out navHit, 15f, NavMesh.AllAreas))
                {
                    navAgent.Warp(navHit.position);
                }
            }

            if (navAgent.isOnNavMesh)
            {
                navAgent.isStopped = false;
                navAgent.SetDestination(goalWorld);
            }
        }

        // 2. Kendi A* Yol Bulucumuza Hedef Ver
        if (pathfinder != null)
        {
            Vector2Int start = (agent != null) ? maze.WorldToCell(agent.position) : maze.StartCell;
            if (!maze.IsWalkable(start))
            {
                start = FindNearestWalkableCell(maze, start);
            }
            pathfinder.FindPath(start, goal);
        }

        Debug.Log($"🎯 Hedefe Tıklandı! Hücre: {goal} (Dünya: {goalWorld})");
    }

    public static Vector2Int FindNearestWalkableCell(MazeGenerator maze, Vector2Int center)
    {
        if (maze.IsWalkable(center)) return center;

        // Spiral tarama: 1'den 12 hücre mesafeye kadar en yakın yürünebilir koridoru bul
        for (int r = 1; r <= 12; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                    Vector2Int candidate = new Vector2Int(center.x + dx, center.y + dy);
                    if (maze.IsWalkable(candidate)) return candidate;
                }
            }
        }
        return maze.GoalCell;
    }
}
