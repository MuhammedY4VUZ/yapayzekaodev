using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 4. Hafta: Ekranda kontrolleri, NavMesh Ajanı vs Kendi A* Ajanımızın yarış durumunu
/// ve 3 kamera modunu gösteren bilgilendirici HUD.
/// </summary>
public class Hafta4HUD : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public PathFollower ourAgent;
    public GridPathfinder pathfinder;
    public CameraModes camModes;

    void Start()
    {
        FindReferences();
    }

    void FindReferences()
    {
        if (camModes == null && Camera.main != null) camModes = Camera.main.GetComponent<CameraModes>();
        if (navAgent == null)
        {
            var na = GameObject.Find("NavAgent");
            if (na != null) navAgent = na.GetComponent<NavMeshAgent>();
        }
        if (ourAgent == null)
        {
            var oa = GameObject.Find("Oyuncu_AStar");
            if (oa != null) ourAgent = oa.GetComponent<PathFollower>();
        }
        if (pathfinder == null) pathfinder = Object.FindFirstObjectByType<GridPathfinder>();
    }

    void OnGUI()
    {
        FindReferences();

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 13;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(15, 15, 450, 290), boxStyle);
        GUILayout.Label("<b>HAFTA 4: ARAZİDE LABİRENT & A* YOL BULMA</b>");
        GUILayout.Label("--------------------------------------------------");
        GUILayout.Label("• <b>WASD / Ok Tuşları + Boşluk:</b> Oyuncu_CC karakterinizi kontrol edin");
        GUILayout.Label("• <b>Sol Tık:</b> Hedef seç (NavMesh ve A* ajanları yarışır)");
        GUILayout.Label("• <b>Sağ Tık:</b> Duvar ekle / kaldır (Replanner yolu anında yeniler)");
        GUILayout.Label("• <b>[R] Tuşu:</b> Yeni rastgele DFS labirenti üret");

        string camModeName = "1/3 Kuşbakışı";
        if (camModes != null)
        {
            switch (camModes.currentMode)
            {
                case CameraModes.CameraMode.KusBakisi:
                    camModeName = "1/3 Kuşbakışı (Labirent Tepeden)";
                    break;
                case CameraModes.CameraMode.OyuncuCCTakip:
                    camModeName = "2/3 Oyuncu_CC Takip (Siz)";
                    break;
                case CameraModes.CameraMode.YapayZekaTakip:
                    camModeName = "3/3 Yapay Zeka Takip (A* Karakteri)";
                    break;
            }
        }
        GUILayout.Label($"• <b>[C] Tuşu:</b> Kamera Modu: <b><color=lime>{camModeName}</color></b>");
        GUILayout.Space(5);

        GUILayout.Label("<b>Ajanların Durumu:</b>");
        if (navAgent != null)
        {
            string navDurum = navAgent.hasPath ? $"Hedefe Koşuyor (Kalan: {navAgent.remainingDistance:F1} m)" : "Beklemede / Ulaştı";
            GUILayout.Label($"• <color=cyan><b>Mavi Kapsül (NavMesh):</b></color> {navDurum}");
        }

        if (ourAgent != null)
        {
            string ourDurum = ourAgent.IsMoving ? $"A* Yolunda Koşuyor (Adım: {ourAgent.Index}/{ourAgent.Route.Count})" : "Beklemede / Ulaştı";
            GUILayout.Label($"• <color=yellow><b>Sarı Kapsül (Kendi A*'ımız):</b></color> {ourDurum}");
        }

        if (pathfinder != null)
        {
            GUILayout.Label($"• Algoritma: <b>{pathfinder.algorithm}</b> | Sezgisel: <b>{pathfinder.heuristic}</b>");
        }

        GUILayout.EndArea();
    }
}
