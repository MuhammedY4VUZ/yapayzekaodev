using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 4. Hafta: Izgara üzerinde A*, Dijkstra, BFS ve DFS algoritmalarıyla yol bulan çekirdek bileşen.
/// </summary>
public class GridPathfinder : MonoBehaviour
{
    public enum Algorithm { DFS, BFS, Dijkstra, AStar }
    public enum Heuristic { Manhattan, Euclidean }

    [Header("Kaynak ve Algoritma")]
    public MonoBehaviour gridSource;
    public Algorithm algorithm = Algorithm.AStar;
    public Heuristic heuristic = Heuristic.Manhattan;

    [Header("Görselleştirme ve Hız")]
    [Range(0f, 0.05f)] public float stepDelay = 0f;
    public float markerLift = 0.15f;
    public bool drawMarkers = false;

    public event Action<List<Vector2Int>> OnPathFound;
    public List<Vector2Int> Path { get; private set; } = new();

    ISearchGrid grid;
    Coroutine running;

    void Awake()
    {
        grid = gridSource as ISearchGrid;
    }

    void Update()
    {
        bool spacePressed = false;
        if (Keyboard.current != null)
            spacePressed = Keyboard.current.spaceKey.wasPressedThisFrame;

        if (spacePressed)
        {
            if (grid == null) grid = gridSource as ISearchGrid;
            if (grid != null) FindPath(grid.StartCell, grid.GoalCell);
        }
    }

    public void FindPath(Vector2Int start, Vector2Int goal)
    {
        if (grid == null) grid = gridSource as ISearchGrid;
        if (grid == null) return;

        Clear();
        running = StartCoroutine(Search(start, goal));
    }

    public void Clear()
    {
        if (running != null) StopCoroutine(running);
        running = null;
        Path.Clear();
    }

    IEnumerator Search(Vector2Int start, Vector2Int goal)
    {
        if (!grid.IsWalkable(start) || !grid.IsWalkable(goal))
        {
            Debug.LogWarning("Başlangıç ya da hedef yürünebilir değil!");
            yield break;
        }

        var open = new List<Node>();
        var openDict = new Dictionary<Vector2Int, Node>();
        var closed = new HashSet<Vector2Int>();

        Node startNode = new Node(start, 0f, GetH(start, goal), null);
        open.Add(startNode);
        openDict[start] = startNode;

        Vector2Int[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
        Node found = null;
        int expanded = 0;

        while (open.Count > 0)
        {
            // En düşük F (veya DFS/BFS sırasına göre) seç
            int bestIdx = 0;
            if (algorithm == Algorithm.DFS)
            {
                bestIdx = open.Count - 1; // LIFO
            }
            else if (algorithm == Algorithm.BFS)
            {
                bestIdx = 0; // FIFO
            }
            else // Dijkstra & A*
            {
                for (int i = 1; i < open.Count; i++)
                {
                    if (open[i].F < open[bestIdx].F || (Mathf.Approximately(open[i].F, open[bestIdx].F) && open[i].h < open[bestIdx].h))
                        bestIdx = i;
                }
            }

            Node cur = open[bestIdx];
            open.RemoveAt(bestIdx);
            openDict.Remove(cur.pos);
            closed.Add(cur.pos);
            expanded++;

            if (cur.pos == goal)
            {
                found = cur;
                break;
            }

            foreach (var d in dirs)
            {
                Vector2Int nPos = cur.pos + d;
                if (!grid.IsWalkable(nPos) || closed.Contains(nPos)) continue;

                float stepCost = grid.CellCost(cur.pos, nPos);
                float tentativeG = cur.g + stepCost;

                if (openDict.TryGetValue(nPos, out Node existing))
                {
                    if (tentativeG < existing.g)
                    {
                        existing.g = tentativeG;
                        existing.parent = cur;
                    }
                }
                else
                {
                    float h = (algorithm == Algorithm.AStar) ? GetH(nPos, goal) : 0f;
                    Node nNode = new Node(nPos, tentativeG, h, cur);
                    open.Add(nNode);
                    openDict[nPos] = nNode;
                }
            }

            if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
        }

        if (found != null)
        {
            BuildPath(found);
            Debug.Log($"✅ {algorithm} Yolu Buldu: {Path.Count} adım, {expanded} düğüm genişletildi.");
            OnPathFound?.Invoke(Path);
        }
        else
        {
            Debug.LogWarning($"❌ {algorithm}: Yol bulunamadı! {expanded} düğüm gezildi.");
        }
    }

    void BuildPath(Node end)
    {
        Path.Clear();
        Node c = end;
        while (c != null)
        {
            Path.Add(c.pos);
            c = c.parent;
        }
        Path.Reverse();
    }

    float GetH(Vector2Int a, Vector2Int b)
    {
        if (heuristic == Heuristic.Manhattan)
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        else
            return Vector2Int.Distance(a, b);
    }

    class Node
    {
        public Vector2Int pos;
        public float g, h;
        public Node parent;
        public float F => g + h;

        public Node(Vector2Int pos, float g, float h, Node parent)
        {
            this.pos = pos;
            this.g = g;
            this.h = h;
            this.parent = parent;
        }
    }
}
