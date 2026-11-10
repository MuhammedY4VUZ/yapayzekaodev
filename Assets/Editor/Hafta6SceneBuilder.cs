#if UNITY_EDITOR
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 6. Hafta: Hafta_3 sahnesini kopyalayarak Hafta6.unity sahnesini oluşturan,
/// Çamur alanını (NavMeshModifierVolume), Kayan kapıyı (Carving),
/// Atlama bağlantısını (NavMeshLink) ve kovalayan NPC'yi kuran editör aracı.
/// </summary>
public static class Hafta6SceneBuilder
{
    [MenuItem("Yapay Zeka/Hafta 6 Sahnesini Eksiksiz Kur")]
    public static void BuildScene()
    {
        string h3Path = "Assets/Scenes/Hafta_3.unity";
        string h6Path = "Assets/Scenes/Hafta6.unity";

        if (!System.IO.File.Exists(h6Path) && System.IO.File.Exists(h3Path))
        {
            AssetDatabase.CopyAsset(h3Path, h6Path);
            AssetDatabase.Refresh();
        }

        var scene = EditorSceneManager.OpenScene(h6Path);

        // 1. Önceki haftaların çakışan nesnelerini kapat
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

        // 2. Çamur Alanı (Mud Area)
        GameObject camurObj = GameObject.Find("Camur");
        if (camurObj == null)
        {
            camurObj = new GameObject("Camur");
            camurObj.transform.position = new Vector3(10f, 0f, 17.5f);

            var modVol = camurObj.AddComponent<NavMeshModifierVolume>();
            modVol.size = new Vector3(6f, 2f, 5f);
            modVol.center = Vector3.zero;
            int camurArea = NavMesh.GetAreaFromName("Camur");
            modVol.area = (camurArea >= 0) ? camurArea : 3;

            GameObject camurGorsel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            camurGorsel.name = "CamurGorsel";
            camurGorsel.transform.SetParent(camurObj.transform);
            camurGorsel.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            camurGorsel.transform.localScale = new Vector3(6f, 0.04f, 5f);
            var col = camurGorsel.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var ren = camurGorsel.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = new Color(0.35f, 0.2f, 0.05f);
        }

        // 3. Kayan Kapı (Sliding Door with NavMeshObstacle Carving)
        GameObject kapiObj = GameObject.Find("Kapi");
        SlidingDoor door = null;
        if (kapiObj == null)
        {
            kapiObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kapiObj.name = "Kapi";
            kapiObj.transform.position = new Vector3(9.5f, 1f, 2.5f);
            kapiObj.transform.localScale = new Vector3(1f, 2f, 5f);

            var mod = kapiObj.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;

            var obstacle = kapiObj.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            obstacle.carveOnlyStationary = true;
            obstacle.carvingTimeToStationary = 0.5f;

            door = kapiObj.AddComponent<SlidingDoor>();
            door.closedY = 1f;
            door.openY = -1.5f;
            door.startOpen = true;

            var ren = kapiObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = new Color(0.7f, 0.2f, 0.2f);
        }
        else
        {
            door = kapiObj.GetComponent<SlidingDoor>();
        }

        // 4. Atlama Bağlantısı (NavMeshLink)
        GameObject linkObj = GameObject.Find("AtlamaBaglantisi");
        NavMeshLink link = null;
        if (linkObj == null)
        {
            linkObj = new GameObject("AtlamaBaglantisi");
            linkObj.transform.position = new Vector3(12.5f, 0f, 9.5f);

            link = linkObj.AddComponent<NavMeshLink>();
            link.startPoint = new Vector3(-1.5f, 0f, 0f);
            link.endPoint = new Vector3(1.5f, 0f, 0f);
            link.width = 1f;
            link.bidirectional = true;
            link.activated = true;
            int jumpArea = NavMesh.GetAreaFromName("Jump");
            link.area = (jumpArea >= 0) ? jumpArea : 2;
        }
        else
        {
            link = linkObj.GetComponent<NavMeshLink>();
        }

        // 5. NavMeshSurface
        GameObject navSurfaceObj = GameObject.Find("NavSurface_Hafta6");
        if (navSurfaceObj == null) navSurfaceObj = new GameObject("NavSurface_Hafta6");
        var surface = navSurfaceObj.GetComponent<NavMeshSurface>() ?? navSurfaceObj.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();

        // 6. Oyuncu_CC
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

        // 7. Kovalayan NPC (Npc_NavChase)
        GameObject npcObj = GameObject.Find("Npc_NavChase");
        NavMeshAgent agent = null;
        AreaCostToggle costToggle = null;
        PathDebugDraw pathDebug = null;

        if (npcObj == null)
        {
            npcObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npcObj.name = "Npc_NavChase";
            npcObj.transform.position = new Vector3(6.5f, 0.5f, 13.5f);

            var ren = npcObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.red;

            agent = npcObj.AddComponent<NavMeshAgent>();
            agent.speed = 4f;
            agent.angularSpeed = 720f;
            agent.acceleration = 20f;
            agent.stoppingDistance = 0.5f;
            agent.radius = 0.4f;
            agent.height = 1.8f;

            var chase = npcObj.AddComponent<NpcNavChase>();
            chase.target = (occ != null) ? occ.transform : null;

            var traverser = npcObj.AddComponent<JumpLinkTraverser>();
            traverser.arcHeight = 2.5f;

            costToggle = npcObj.AddComponent<AreaCostToggle>();
            costToggle.jumpLink = link;

            pathDebug = npcObj.AddComponent<PathDebugDraw>();
            pathDebug.jumpLink = link;
        }
        else
        {
            agent = npcObj.GetComponent<NavMeshAgent>();
            costToggle = npcObj.GetComponent<AreaCostToggle>();
            pathDebug = npcObj.GetComponent<PathDebugDraw>();
        }

        // 8. Kamera
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

        // 9. Hafta6HUD
        GameObject hudObj = GameObject.Find("Hafta6HUD");
        if (hudObj == null) hudObj = new GameObject("Hafta6HUD");
        var hud = hudObj.GetComponent<Hafta6HUD>() ?? hudObj.AddComponent<Hafta6HUD>();
        hud.costToggle = costToggle;
        hud.door = door;
        hud.pathDebug = pathDebug;
        hud.agent = agent;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("🎉 HAFTA 6: NavMesh, Çamur Alanı, Kayan Kapı ve Atlama Bağlantısı Başarıyla Kuruldu!");
    }
}
#endif
