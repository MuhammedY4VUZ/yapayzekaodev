using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 5. Hafta: Sahne Play modunda açıldığında 9 Waypoint'lik grafı, U engellerini,
/// Muhafız NPC'yi ve Oyuncu_CC kontrolünü tam otomatik kuran motor.
/// </summary>
public class Hafta5RuntimeInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureHafta5Ready()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Hafta5") && !sceneName.Contains("Hafta_5")) return;

        // 1. Çakışan nesneleri devredışı bırak
        GameObject maze = GameObject.Find("Maze");
        if (maze != null) maze.SetActive(false);
        GameObject navSurface = GameObject.Find("NavSurface");
        if (navSurface != null) navSurface.SetActive(false);
        GameObject goalFlag = GameObject.Find("GoalFlag");
        if (goalFlag != null) goalFlag.SetActive(false);
        GameObject navAgent = GameObject.Find("NavAgent");
        if (navAgent != null) navAgent.SetActive(false);
        GameObject astarChar = GameObject.Find("Oyuncu_AStar");
        if (astarChar != null) astarChar.SetActive(false);
        GameObject h3hud = GameObject.Find("Hafta3HUD");
        if (h3hud != null) h3hud.SetActive(false);
        GameObject h4hud = GameObject.Find("Hafta4HUD");
        if (h4hud != null) h4hud.SetActive(false);

        // 2. U-Şeklinde Engel Küpleri (Görüşü ve düz yürüyüşü kesen engeller)
        GameObject engeller = GameObject.Find("Engeller_Hafta5");
        if (engeller == null)
        {
            engeller = new GameObject("Engeller_Hafta5");
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");

            // Sol duvar, arka duvar, sağ duvar (U şeklinde)
            CreateObstacleCube(engeller, "Engel_Sol", new Vector3(7f, 1f, 10f), new Vector3(1f, 2f, 6f), obstacleLayer);
            CreateObstacleCube(engeller, "Engel_Arka", new Vector3(10f, 1f, 13f), new Vector3(6f, 2f, 1f), obstacleLayer);
            CreateObstacleCube(engeller, "Engel_Sag", new Vector3(13f, 1f, 10f), new Vector3(1f, 2f, 6f), obstacleLayer);
        }

        // 3. Waypoint Nesneleri (W0 .. W8)
        GameObject wpRoot = GameObject.Find("Waypoints");
        WaypointGraph graph = null;
        if (wpRoot == null)
        {
            wpRoot = new GameObject("Waypoints");
            Vector3[] wpPositions = new Vector3[]
            {
                new Vector3(3f, 0.5f, 3f),   // W0
                new Vector3(10f, 0.5f, 3f),  // W1
                new Vector3(16f, 0.5f, 3f),  // W2
                new Vector3(16f, 0.5f, 10f), // W3
                new Vector3(16f, 0.5f, 17f), // W4
                new Vector3(10f, 0.5f, 17f), // W5
                new Vector3(3f, 0.5f, 17f),  // W6
                new Vector3(3f, 0.5f, 10f),  // W7
                new Vector3(9f, 0.5f, 10f)   // W8 (İç Düğüm)
            };

            for (int i = 0; i < wpPositions.Length; i++)
            {
                GameObject wpObj = new GameObject($"W{i}");
                wpObj.transform.SetParent(wpRoot.transform);
                wpObj.transform.position = wpPositions[i];
                wpObj.AddComponent<Waypoint>();
            }

            graph = wpRoot.AddComponent<WaypointGraph>();
            graph.maxEdgeLength = 14f;
            graph.BuildEdges();
        }
        else
        {
            graph = wpRoot.GetComponent<WaypointGraph>() ?? wpRoot.AddComponent<WaypointGraph>();
            graph.BuildEdges();
        }

        // 4. Oyuncu_CC
        GameObject occ = GameObject.Find("Oyuncu_CC");
        if (occ != null)
        {
            occ.SetActive(true);
            var ccHareket = occ.GetComponent<CCHareket>() ?? occ.AddComponent<CCHareket>();
            ccHareket.enabled = true;
            if (Camera.main != null) ccHareket.kamera = Camera.main.transform;

            var cc = occ.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                occ.transform.position = new Vector3(3f, 1f, 3f);
                cc.enabled = true;
            }
        }

        // 5. Muhafız NPC (Kırmızı Küre / Kapsül)
        GameObject npc = GameObject.Find("Npc_Guard");
        NpcWaypointGuard guard = null;
        if (npc == null)
        {
            npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = "Npc_Guard";
            npc.transform.position = new Vector3(6.5f, 1f, 13.5f);

            var ren = npc.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.red;

            guard = npc.AddComponent<NpcWaypointGuard>();
            guard.graph = graph;
            guard.player = (occ != null) ? occ.transform : null;
        }
        else
        {
            guard = npc.GetComponent<NpcWaypointGuard>() ?? npc.AddComponent<NpcWaypointGuard>();
            guard.graph = graph;
            guard.player = (occ != null) ? occ.transform : null;
        }

        // 6. Takip Kamerası
        Camera cam = Camera.main;
        if (cam != null && occ != null)
        {
            var tk = cam.GetComponent<TakipKamerasi>();
            if (tk != null)
            {
                tk.enabled = true;
                tk.hedef = occ.transform;
            }
            else
            {
                var cm = cam.GetComponent<CameraModes>();
                if (cm != null)
                {
                    cm.oyuncuCC = occ.transform;
                    cm.currentMode = CameraModes.CameraMode.OyuncuCCTakip;
                    cm.SnapToCurrentMode();
                }
            }
        }

        // 7. Hafta5HUD
        GameObject hudObj = GameObject.Find("Hafta5HUD");
        if (hudObj == null) hudObj = new GameObject("Hafta5HUD");
        var hud = hudObj.GetComponent<Hafta5HUD>() ?? hudObj.AddComponent<Hafta5HUD>();
        hud.guard = guard;
    }

    static void CreateObstacleCube(GameObject parent, string name, Vector3 pos, Vector3 size, int layer)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent.transform);
        cube.transform.position = pos;
        cube.transform.localScale = size;
        if (layer >= 0) cube.layer = layer;

        var ren = cube.GetComponent<Renderer>();
        ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        ren.material.color = new Color(0.45f, 0.45f, 0.5f);
    }
}
