using UnityEngine;

/// <summary>
/// 4. Hafta: Yol bulucuların harita ile iletişim kurduğu ortak arayüz.
/// </summary>
public interface ISearchGrid
{
    int Width { get; }
    int Depth { get; }
    float CellSize { get; }
    Vector2Int StartCell { get; }
    Vector2Int GoalCell { get; }
    bool IsWalkable(Vector2Int cell);
    Vector3 CellToWorld(Vector2Int cell);
    Vector2Int WorldToCell(Vector3 worldPos);
    float CellCost(Vector2Int from, Vector2Int to);
    float MinCellCost { get; }
}
