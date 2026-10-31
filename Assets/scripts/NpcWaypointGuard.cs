using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 5. Hafta: Waypoint'ler arasında devriye gezen, oyuncuyu görünce A* ile kovalayan,
/// gözden kaybedince 3 saniye sonra en yakın devriye noktasına dönen Muhafız NPC.
/// </summary>
public class NpcWaypointGuard : MonoBehaviour
{
    public enum State { Patrol, Chase, Return }

    [Header("Referanslar")]
    public WaypointGraph graph;
    public Transform player;

    [Header("Devriye")]
    public Waypoint[] patrolRoute;
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public float arriveRadius = 0.3f;

    [Header("Görüş Parametreleri")]
    public float viewDistance = 14f;
    public float viewAngle = 120f;
    public float loseTime = 3f;

    public State CurrentState => state;

    State state = State.Patrol;
    int patrolIndex;
    Waypoint heading;
    readonly GraphAStar astar = new();
    readonly List<Vector3> path = new();
    readonly List<Waypoint> pathNodes = new();
    int pathIndex;
    float lastSeenTime = -999f;
    float replanTimer;

    void Start()
    {
        FindReferences();
    }

    void FindReferences()
    {
        if (graph == null) graph = Object.FindFirstObjectByType<WaypointGraph>();
        if (player == null)
        {
            var pObj = GameObject.Find("Oyuncu_CC");
            if (pObj != null) player = pObj.transform;
        }

        if ((patrolRoute == null || patrolRoute.Length == 0) && graph != null)
        {
            var allNodes = graph.GetComponentsInChildren<Waypoint>();
            if (allNodes != null && allNodes.Length > 0)
            {
                // W0 - W7 arasındaki dış döngüyü devriye olarak al
                int count = Mathf.Min(8, allNodes.Length);
                patrolRoute = new Waypoint[count];
                for (int i = 0; i < count; i++) patrolRoute[i] = allNodes[i];
            }
        }
    }

    void Update()
    {
        FindReferences();
        if (player == null || graph == null || patrolRoute == null || patrolRoute.Length == 0) return;

        bool sees = CanSeePlayer();
        if (sees) lastSeenTime = Time.time;

        if (state == State.Patrol)
        {
            if (sees)
            {
                state = State.Chase;
                replanTimer = 0f;
                return;
            }

            heading = patrolRoute[patrolIndex];
            if (heading != null && MoveTo(heading.transform.position, patrolSpeed))
            {
                patrolIndex = (patrolIndex + 1) % patrolRoute.Length;
            }
        }
        else if (state == State.Chase)
        {
            if (Time.time - lastSeenTime > loseTime)
            {
                StartReturn();
                return;
            }

            replanTimer -= Time.deltaTime;
            if (replanTimer <= 0f)
            {
                PlanTo(player.position);
                replanTimer = 0.5f;
            }

            FollowPath(chaseSpeed);
        }
        else // State.Return
        {
            if (sees)
            {
                state = State.Chase;
                replanTimer = 0f;
                return;
            }

            if (FollowPath(patrolSpeed))
            {
                state = State.Patrol;
            }
        }
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector3 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, toPlayer) > viewAngle / 2f) return false;
        return graph != null && graph.CanSee(transform.position + Vector3.up * 0.5f, player.position + Vector3.up * 0.5f);
    }

    void PlanTo(Vector3 target)
    {
        path.Clear();
        pathNodes.Clear();
        pathIndex = 0;

        if (!graph.CanSee(transform.position + Vector3.up * 0.5f, target + Vector3.up * 0.5f))
        {
            Waypoint start = (heading != null) ? heading : graph.Nearest(transform.position);
            Waypoint goal = graph.Nearest(target);

            List<Waypoint> nodes = (start != null && goal != null) ? astar.FindPath(start, goal) : null;
            if (nodes != null)
            {
                foreach (Waypoint w in nodes)
                {
                    path.Add(w.transform.position);
                    pathNodes.Add(w);
                }
            }
        }

        path.Add(target);
        pathNodes.Add(null);
        heading = pathNodes[0];
    }

    void StartReturn()
    {
        int best = 0;
        for (int i = 1; i < patrolRoute.Length; i++)
        {
            if (Dist(patrolRoute[i]) < Dist(patrolRoute[best]))
                best = i;
        }

        patrolIndex = best;
        if (patrolRoute[best] != null)
        {
            PlanTo(patrolRoute[best].transform.position);
        }
        state = State.Return;
    }

    float Dist(Waypoint w) => (w != null) ? Vector3.Distance(transform.position, w.transform.position) : float.MaxValue;

    bool FollowPath(float speed)
    {
        if (pathIndex >= path.Count) return true;
        if (MoveTo(path[pathIndex], speed))
        {
            pathIndex++;
            heading = (pathIndex < pathNodes.Count) ? pathNodes[pathIndex] : null;
        }
        return pathIndex >= path.Count;
    }

    bool MoveTo(Vector3 target, float speed)
    {
        target.y = transform.position.y;
        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        return Vector3.Distance(transform.position, target) < arriveRadius;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = (state == State.Chase) ? Color.red : (state == State.Return) ? Color.yellow : Color.green;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * viewDistance);

        if (state == State.Patrol && heading != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, heading.transform.position);
        }

        Gizmos.color = Color.magenta;
        for (int i = pathIndex; i < path.Count; i++)
        {
            Gizmos.DrawLine((i == pathIndex) ? transform.position : path[i - 1], path[i]);
        }
    }
}
