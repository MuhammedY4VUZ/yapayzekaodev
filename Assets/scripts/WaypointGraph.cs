using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 5. Hafta: Waypoint'ler arasındaki kenarları otomatik kuran (CanSee / Linecast)
/// ve en yakın görünen düğümü bulan graf yöneticisi.
/// </summary>
public class WaypointGraph : MonoBehaviour
{
    public LayerMask obstacleMask;
    public float maxEdgeLength = 14f;
    [HideInInspector] public Waypoint[] nodes;

    void Awake() => BuildEdges();

    [ContextMenu("Kenarları Kur")]
    public void BuildEdges()
    {
        nodes = GetComponentsInChildren<Waypoint>();
        if (nodes == null || nodes.Length == 0) return;

        foreach (Waypoint a in nodes)
        {
            a.neighbors.Clear();
            foreach (Waypoint b in nodes)
            {
                if (a == b) continue;
                float dist = Vector3.Distance(a.transform.position, b.transform.position);
                if (dist <= maxEdgeLength && CanSee(a.transform.position, b.transform.position))
                {
                    a.neighbors.Add(b);
                }
            }
        }
    }

    public bool CanSee(Vector3 from, Vector3 to)
    {
        int mask = obstacleMask.value;
        if (mask == 0)
        {
            int oLayer = LayerMask.NameToLayer("Obstacle");
            int wLayer = LayerMask.NameToLayer("Wall");
            mask = ((oLayer >= 0) ? (1 << oLayer) : 0) | ((wLayer >= 0) ? (1 << wLayer) : 0);
        }
        if (mask != 0)
            return !Physics.Linecast(from, to, mask);
        return true;
    }

    public Waypoint Nearest(Vector3 pos)
    {
        if (nodes == null || nodes.Length == 0)
            nodes = GetComponentsInChildren<Waypoint>();

        Waypoint best = null;
        float bestDist = float.MaxValue;

        if (nodes != null)
        {
            foreach (Waypoint w in nodes)
            {
                if (w == null) continue;
                float d = Vector3.Distance(pos, w.transform.position);
                if (d < bestDist && CanSee(pos, w.transform.position))
                {
                    best = w;
                    bestDist = d;
                }
            }

            if (best == null)
            {
                foreach (Waypoint w in nodes)
                {
                    if (w == null) continue;
                    float d = Vector3.Distance(pos, w.transform.position);
                    if (d < bestDist)
                    {
                        best = w;
                        bestDist = d;
                    }
                }
            }
        }

        return best;
    }
}
