using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 5. Hafta: Genel Waypoint grafı üzerinde A* ile en kısa yolu bulan sınıf.
/// </summary>
public class GraphAStar
{
    class Node
    {
        public Waypoint wp;
        public float g, h;
        public Node parent;
    }

    public bool useHeuristic = true;
    public int Expanded { get; private set; }

    public List<Waypoint> FindPath(Waypoint start, Waypoint goal)
    {
        if (start == null || goal == null) return null;
        if (start == goal) return new List<Waypoint> { start };

        var open = new List<Node>();
        var openByWp = new Dictionary<Waypoint, Node>();
        var closed = new HashSet<Waypoint>();

        Expanded = 0;
        var first = new Node { wp = start, g = 0f, h = H(start, goal) };
        open.Add(first);
        openByWp[start] = first;

        while (open.Count > 0)
        {
            Node current = open[0];
            foreach (Node n in open)
            {
                if (F(n) < F(current) || (Mathf.Approximately(F(n), F(current)) && n.h < current.h))
                    current = n;
            }

            open.Remove(current);
            openByWp.Remove(current.wp);
            closed.Add(current.wp);
            Expanded++;

            if (current.wp == goal)
                return Retrace(current);

            foreach (Waypoint next in current.wp.neighbors)
            {
                if (next == null || closed.Contains(next)) continue;

                float newG = current.g + Vector3.Distance(current.wp.transform.position, next.transform.position);
                if (!openByWp.TryGetValue(next, out Node node))
                {
                    node = new Node { wp = next, g = newG, h = H(next, goal), parent = current };
                    open.Add(node);
                    openByWp[next] = node;
                }
                else if (newG < node.g)
                {
                    node.g = newG;
                    node.parent = current;
                }
            }
        }

        return null;
    }

    float F(Node n) => n.g + n.h;
    float H(Waypoint a, Waypoint b) => useHeuristic ? Vector3.Distance(a.transform.position, b.transform.position) : 0f;

    List<Waypoint> Retrace(Node end)
    {
        var path = new List<Waypoint>();
        for (Node n = end; n != null; n = n.parent)
            path.Add(n.wp);
        path.Reverse();
        return path;
    }
}
