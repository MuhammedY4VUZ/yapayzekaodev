#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 5. Hafta: Hafta_3 sahnesini kopyalayarak Hafta5.unity sahnesini oluşturan,
/// 9 Waypoint'lik grafı, U engellerini, devriye gezen ve A* ile kovalayan Muhafız NPC'yi kuran editör aracı.
/// </summary>
public static class Hafta5SceneBuilder
{
    [MenuItem("Yapay Zeka/Hafta 5 Sahnesini Eksiksiz Kur")]
    public static void BuildScene()
    {
        string h3Path = "Assets/Scenes/Hafta_3.unity";
        string h5Path = "Assets/Scenes/Hafta5.unity";

        if (!System.IO.File.Exists(h5Path) && System.IO.File.Exists(h3Path))
        {
            AssetDatabase.CopyAsset(h3Path, h5Path);
            AssetDatabase.Refresh();
        }

        var scene = EditorSceneManager.OpenScene(h5Path);

        // 1. Çakışan nesneleri kapat
        GameObject gozcu = GameObject.Find("Gozcu");
        if (gozcu != null) gozcu.SetActive(false);
        GameObject h3hud = GameObject.Find("Hafta3HUD");
        if (h3hud != null) h3hud.SetActive(false);
        GameObject orb = GameObject.Find("Oyuncu_RB");
        if (orb != null) orb.SetActive(false);
        GameObject sonda = GameObject.Find("AraziSondasi");
        if (sonda != null) sonda.SetActive(false);
        GameObject maze = GameObject.Find("Maze");
        if (maze != null) maze.SetActive(false);

        // 2. Obstacle Katmanı
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (obstacleLayer < 0) obstacleLayer = 0;

        // 3. U-Şeklinde Engeller
        GameObject engeller = GameObject.Find("Engeller_Hafta5");
        if (engeller == null)
        {
            engeller = new GameObject("Engeller_Hafta5");
            CreateObstacleCube(engeller, "Engel_Sol", new Vector3(7f, 1f, 10f), new Vector3(1f, 2f, 6f), obstacleLayer);
            CreateObstacleCube(engeller, "Engel_Arka", new Vector3(10f, 1f, 13f), new Vector3(6f, 2f, 1f), obstacleLayer);
            CreateObstacleCube(engeller, "Engel_Sag", new Vector3(13f, 1f, 10f), new Vector3(1f, 2f, 6f), obstacleLayer);
        }

        // 4. Waypoints (W0 .. W8)
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

        // 5. Oyuncu_CC
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

        // 6. Muhafız NPC (Kırmızı Kapsül)
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

        // 7. Kamera
        Camera cam = Camera.main;
        if (cam != null && occ != null)
        {
            var tk = cam.GetComponent<TakipKamerasi>();
            if (tk != null)
            {
                tk.enabled = true;
                tk.hedef = occ.transform;
            }
        }

        // 8. Hafta5HUD
        GameObject hudObj = GameObject.Find("Hafta5HUD");
        if (hudObj == null) hudObj = new GameObject("Hafta5HUD");
        var hud = hudObj.GetComponent<Hafta5HUD>() ?? hudObj.AddComponent<Hafta5HUD>();
        hud.guard = guard;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("🎉 HAFTA 5: Waypoint Grafı, Muhafız NPC ve A* Kovalama Başarıyla Kuruldu!");
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
#endif
