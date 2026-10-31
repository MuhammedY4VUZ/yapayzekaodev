using UnityEngine;

/// <summary>
/// 5. Hafta: Devriye ve Kovalama durumunu, görüş bilgisini ekranda gösteren HUD.
/// </summary>
public class Hafta5HUD : MonoBehaviour
{
    public NpcWaypointGuard guard;

    void Start()
    {
        if (guard == null) guard = Object.FindFirstObjectByType<NpcWaypointGuard>();
    }

    void OnGUI()
    {
        if (guard == null) guard = Object.FindFirstObjectByType<NpcWaypointGuard>();

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 13;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(15, 15, 450, 240), boxStyle);
        GUILayout.Label("<b>HAFTA 5: WAYPOINT GRAFI & DEVRİYE / KOVALAMA</b>");
        GUILayout.Label("--------------------------------------------------");
        GUILayout.Label("• <b>WASD / Ok Tuşları:</b> Oyuncu_CC ile arazide serbestçe yürüyün");
        GUILayout.Label("• <b>Kırmızı Kapsül (Muhafız):</b> Waypoint'ler arasında devriye gezer.");
        GUILayout.Label("• <b>Görüş:</b> Sizi görünce A* ile kovalama durumuna geçer.");
        GUILayout.Label("• <b>Gizlenme:</b> Engellerin arkasına saklanıp 3 sn görünmezseniz devriyesine döner.");
        GUILayout.Space(5);

        if (guard != null)
        {
            string durumText = guard.CurrentState.ToString();
            string colorTag = "lime";
            if (guard.CurrentState == NpcWaypointGuard.State.Chase) { durumText = "Kovalıyor! (Chase)"; colorTag = "red"; }
            else if (guard.CurrentState == NpcWaypointGuard.State.Return) { durumText = "Devriyeye Dönüyor (Return)"; colorTag = "yellow"; }
            else { durumText = "Devriye Geziyor (Patrol)"; colorTag = "lime"; }

            GUILayout.Label($"• Muhafız Durumu: <b><color={colorTag}>{durumText}</color></b>");
            GUILayout.Label($"• Oyuncuyu Görüyor mu? <b>{(guard.CanSeePlayer() ? "<color=red>EVET (GÖRDÜ)</color>" : "<color=lime>HAYIR (GİZLİ)</color>")}</b>");
        }

        GUILayout.EndArea();
    }
}
