using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 6. Hafta: NPC'nin kalan yolunu ölçen bileşen: köşe sayısı, toplam uzunluk ve alan maliyetli toplam.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PathDebugDraw : MonoBehaviour
{
    public NavMeshLink jumpLink;
    public float sampleStep = 0.25f;

    NavMeshAgent agent;
    public float length { get; private set; }
    public float cost { get; private set; }
    public int cornerCount { get; private set; }

    void Awake() => agent = GetComponent<NavMeshAgent>();

    void Start()
    {
        if (jumpLink == null) jumpLink = Object.FindFirstObjectByType<NavMeshLink>();
    }

    void Update()
    {
        if (jumpLink == null) jumpLink = Object.FindFirstObjectByType<NavMeshLink>();
        if (agent == null || agent.pathPending || !agent.hasPath) return;

        Vector3[] c = agent.path.corners;
        cornerCount = c.Length;
        length = 0f;
        cost = 0f;

        for (int i = 0; i < c.Length - 1; i++)
        {
            float segLen = Vector3.Distance(c[i], c[i + 1]);
            length += segLen;
            cost += SegmentCost(c[i], c[i + 1], segLen);
        }
    }

    float SegmentCost(Vector3 a, Vector3 b, float segLen)
    {
        if (IsLink(a, b) && jumpLink != null)
            return segLen * agent.GetAreaCost(jumpLink.area);

        int steps = Mathf.Max(1, Mathf.CeilToInt(segLen / sampleStep));
        float stepLen = segLen / steps;
        float total = 0f;

        for (int s = 0; s < steps; s++)
        {
            Vector3 p = Vector3.Lerp(a, b, (s + 0.5f) / steps);
            int area = 0;
            if (NavMesh.SamplePosition(p, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
                area = AreaIndex(hit.mask);
            total += stepLen * agent.GetAreaCost(area);
        }

        return total;
    }

    bool IsLink(Vector3 a, Vector3 b)
    {
        if (jumpLink == null || !jumpLink.activated) return false;
        Vector3 s = jumpLink.transform.TransformPoint(jumpLink.startPoint);
        Vector3 e = jumpLink.transform.TransformPoint(jumpLink.endPoint);
        return (Near(a, s) && Near(b, e)) || (Near(a, e) && Near(b, s));
    }

    static bool Near(Vector3 p, Vector3 q)
    {
        p.y = 0f;
        q.y = 0f;
        return Vector3.Distance(p, q) < 0.4f;
    }

    static int AreaIndex(int mask)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0) return i;
        }
        return 0;
    }

    void OnDrawGizmos()
    {
        if (agent == null || !agent.hasPath) return;
        Gizmos.color = Color.yellow;
        foreach (Vector3 p in agent.path.corners)
            Gizmos.DrawSphere(p, 0.2f);
    }
}
