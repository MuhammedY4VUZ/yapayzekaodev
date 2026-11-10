using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 6. Hafta: Kapı durumu, Çamur maliyeti, Atlama bağlantısı ve yol ölçümlerini ekranda gösteren HUD.
/// </summary>
public class Hafta6HUD : MonoBehaviour
{
    public AreaCostToggle costToggle;
    public SlidingDoor door;
    public PathDebugDraw pathDebug;
    public NavMeshAgent agent;

    void Start()
    {
        FindReferences();
    }

    void FindReferences()
    {
        if (costToggle == null) costToggle = Object.FindFirstObjectByType<AreaCostToggle>();
        if (door == null) door = Object.FindFirstObjectByType<SlidingDoor>();
        if (pathDebug == null) pathDebug = Object.FindFirstObjectByType<PathDebugDraw>();
        if (agent == null)
        {
            var npc = GameObject.Find("Npc_NavChase");
            if (npc != null) agent = npc.GetComponent<NavMeshAgent>();
        }
    }

    void OnGUI()
    {
        FindReferences();

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 13;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(15, 15, 450, 270), boxStyle);
        GUILayout.Label("<b>HAFTA 6: NAVMESH, ALAN MALİYETLERİ & CARVING</b>");
        GUILayout.Label("--------------------------------------------------");
        GUILayout.Label("• <b>WASD / Ok Tuşları:</b> Oyuncu_CC ile yürüyün (NPC sizi kovalar)");
        GUILayout.Label($"• <b>[K] Tuşu - Kapı Durumu:</b> <b>{(door != null && door.isOpen ? "<color=lime>AÇIK</color>" : "<color=red>KAPALI (Carve Edildi)</color>")}</b>");
        GUILayout.Label($"• <b>[M] Tuşu - Çamur Maliyeti:</b> <b><color=yellow>{(costToggle != null ? costToggle.CurrentMudCost : 1f)}x</color></b> (1x, 2x, 5x, 10x)");
        GUILayout.Label($"• <b>[J] Tuşu - Atlama Bağlantısı:</b> <b>{(costToggle != null && costToggle.IsLinkActive ? "<color=lime>AÇIK (Parabolik Atlayış)</color>" : "<color=red>KAPALI</color>")}</b>");
        GUILayout.Space(5);

        GUILayout.Label("<b>Yol Ölçümleri & Ajan Durumu:</b>");
        if (pathDebug != null)
        {
            GUILayout.Label($"• Köşe Sayısı: <b>{pathDebug.cornerCount}</b> | Yol Uzunluğu: <b>{pathDebug.length:F2} m</b>");
            GUILayout.Label($"• Ağırlıklı Toplam Maliyet: <b><color=cyan>{pathDebug.cost:F2}</color></b>");
        }

        if (agent != null)
        {
            GUILayout.Label($"• NavMesh Yol Durumu: <b>{agent.pathStatus}</b>");
        }

        GUILayout.EndArea();
    }
}
