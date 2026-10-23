using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 4. Hafta: NavMesh'i koddan labirente göre dinamik pişiren ve duvar değiştikçe güncelleyen bileşen.
/// </summary>
public class NavMeshRefresher : MonoBehaviour
{
    public NavMeshSurface surface;
    public MazeGenerator maze;
    public NavMeshAgent navAgent;

    void Awake()
    {
        if (surface == null) surface = GetComponent<NavMeshSurface>();
        if (maze != null)
        {
            maze.OnRegenerated += Rebuild;
            maze.OnCellChanged += _ => Refresh();
        }
    }

    void Start() => Rebuild();

    public void Rebuild()
    {
        if (surface == null || maze == null) return;
        FitVolumeToMaze();
        surface.BuildNavMesh();
        if (navAgent != null) navAgent.Warp(maze.CellToWorld(maze.StartCell));
    }

    void Refresh()
    {
        if (surface == null || surface.navMeshData == null) return;
        AsyncOperation op = surface.UpdateNavMesh(surface.navMeshData);
        op.completed += _ =>
        {
            if (navAgent != null && navAgent.hasPath)
                navAgent.SetDestination(navAgent.destination);
        };
    }

    void FitVolumeToMaze()
    {
        surface.collectObjects = CollectObjects.Volume;
        surface.transform.SetPositionAndRotation(
            maze.CellToWorld(new Vector2Int(maze.width / 2, maze.depth / 2)), Quaternion.identity);
        surface.center = Vector3.zero;
        surface.size = new Vector3(maze.width * maze.cellSize + 4f, 80f, maze.depth * maze.cellSize + 4f);
    }
}
