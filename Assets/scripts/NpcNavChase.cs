using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 6. Hafta: Karakteri kovalayan, köşeleri LineRenderer ile çizen
/// ve yol durumunu (Complete, Partial, Invalid) izleyen NavMesh ajanı.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(LineRenderer))]
public class NpcNavChase : MonoBehaviour
{
    public Transform target;
    public float repathInterval = 0.25f;

    NavMeshAgent agent;
    LineRenderer line;
    float timer;
    NavMeshPathStatus lastStatus = NavMeshPathStatus.PathComplete;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        line = GetComponent<LineRenderer>();
    }

    void Start()
    {
        if (target == null)
        {
            var pObj = GameObject.Find("Oyuncu_CC");
            if (pObj != null) target = pObj.transform;
        }

        if (line != null)
        {
            line.startWidth = 0.3f;
            line.endWidth = 0.3f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.cyan;
            line.endColor = Color.blue;
        }
    }

    void Update()
    {
        if (target == null)
        {
            var pObj = GameObject.Find("Oyuncu_CC");
            if (pObj != null) target = pObj.transform;
            if (target == null) return;
        }

        // 1. Hedef hareket ediyor: atlarken yeni yol isteme
        timer -= Time.deltaTime;
        if (timer <= 0f && !agent.isOnOffMeshLink && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
            timer = repathInterval;
        }

        // 2. Yolun durumu değişince konsola yaz
        if (!agent.pathPending && agent.pathStatus != lastStatus)
        {
            lastStatus = agent.pathStatus;
            Debug.Log($"🗺️ NavMesh Yol Durumu: {lastStatus}");
        }

        // 3. Yolu göster: NavMesh'in bulduğu köşe noktaları
        if (line != null && agent.hasPath)
        {
            Vector3[] corners = agent.path.corners;
            line.positionCount = corners.Length;
            for (int i = 0; i < corners.Length; i++)
            {
                line.SetPosition(i, corners[i] + Vector3.up * 0.25f);
            }
        }
        else if (line != null)
        {
            line.positionCount = 0;
        }
    }

    void OnDisable()
    {
        if (line != null) line.positionCount = 0;
    }
}
