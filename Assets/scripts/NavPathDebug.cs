using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 4. Hafta: NavMesh ajanının hesaplanan yolunu Scene görünümünde çizgilerle çizen hata ayıklayıcı.
/// </summary>
public class NavPathDebug : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public int cornerCount;

    void Awake()
    {
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (navAgent == null || !navAgent.hasPath)
        {
            cornerCount = 0;
            return;
        }

        Vector3[] c = navAgent.path.corners;
        cornerCount = c.Length;
        for (int i = 0; i < c.Length - 1; i++)
            Debug.DrawLine(c[i] + Vector3.up * 0.3f, c[i + 1] + Vector3.up * 0.3f, Color.cyan);
    }
}
