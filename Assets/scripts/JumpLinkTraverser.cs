using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 6. Hafta: NavMeshLink üzerinden geçerken NPC'yi duvarın üstünden parabol çizerek atlatan bileşen.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class JumpLinkTraverser : MonoBehaviour
{
    public float arcHeight = 2f;
    public float duration = 0.7f;

    NavMeshAgent agent;
    bool jumping;
    float t;
    Vector3 a, b;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false; // Geçişi biz yöneteceğiz
    }

    void Update()
    {
        // 1. Bağlantının başına geldik: atlayışı hazırla
        if (agent.isOnOffMeshLink && !jumping)
        {
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            a = transform.position;
            b = data.endPos + Vector3.up * agent.baseOffset;
            t = 0f;
            jumping = true;
            Debug.Log("🦘 NPC Atlama Bağlantısını (JumpLink) kullanarak duvarın üstünden atlıyor!");
        }

        if (!jumping) return;

        // 2. Parabol: düz çizgi + 4h * t(1 - t) kadar yukarı
        t += Time.deltaTime / duration;
        float k = Mathf.Clamp01(t);
        transform.position = Vector3.Lerp(a, b, k) + Vector3.up * (4f * arcHeight * k * (1f - k));

        // 3. Vardık: ajana bağlantının bittiğini bildir
        if (t >= 1f)
        {
            agent.CompleteOffMeshLink();
            jumping = false;
        }
    }
}
