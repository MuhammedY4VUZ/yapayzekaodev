using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 5. Hafta: Grafın bir düğümünü (Waypoint) temsil eden bileşen.
/// Komşuluk listesi tutar ve Gizmos ile görselleştirir.
/// </summary>
public class Waypoint : MonoBehaviour
{
    public List<Waypoint> neighbors = new();

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.35f);

        Gizmos.color = Color.white;
        foreach (Waypoint n in neighbors)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}
