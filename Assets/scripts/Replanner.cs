using UnityEngine;

/// <summary>
/// 4. Hafta: Labirentte bir duvar örüldüğünde yolun kapanıp kapanmadığını denetleyen
/// ve gerekiyorsa otomatik olarak A* ile yeni yol hesaplatan yeniden planlayıcı.
/// </summary>
public class Replanner : MonoBehaviour
{
    public MazeGenerator maze;
    public GridPathfinder pathfinder;
    public PathFollower follower;
    public bool replanOnAnyChange = false;

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
        if (maze == null) maze = Object.FindFirstObjectByType<MazeGenerator>();
        if (pathfinder == null) pathfinder = Object.FindFirstObjectByType<GridPathfinder>();
        if (follower == null) follower = Object.FindFirstObjectByType<PathFollower>();

        if (maze != null)
        {
            maze.OnCellChanged -= OnCellChanged;
            maze.OnCellChanged += OnCellChanged;
        }
    }

    void OnCellChanged(Vector2Int cell)
    {
        if (follower == null || !follower.IsMoving) return;
        if (follower.Route == null) return;

        bool blocked = false;
        for (int i = follower.Index; i < follower.Route.Count; i++)
        {
            if (follower.Route[i] == cell)
            {
                blocked = true;
                break;
            }
        }

        if (!blocked && !replanOnAnyChange) return;

        Vector2Int goal = follower.Route[follower.Route.Count - 1];
        Vector2Int here = maze.WorldToCell(follower.transform.position);

        follower.Stop();
        if (pathfinder != null)
            pathfinder.FindPath(here, goal);

        Debug.Log(blocked ? $"🚨 Yol {cell} hücresinde kapandı, A* ile yeniden planlanıyor..."
                          : $"🔄 {cell} hücresi değişti, kestirme yol aranıyor...");
    }
}
