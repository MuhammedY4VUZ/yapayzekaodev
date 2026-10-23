#if UNITY_EDITOR
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 4. Hafta: Hafta 3 sahnesini kopyalayarak Hafta4_Uygulama sahnesini oluşturan,
/// Oyuncu_CC kontrolünü, 3 kameralı sistemi ve hem NavMesh hem A* yarışını tam kuran Editör aracı.
/// </summary>
public static class Hafta4SceneBuilder
{
    [MenuItem("Yapay Zeka/Hafta 4 Sahnesini Eksiksiz Kur")]
    public static void BuildScene()
    {
        string h3Path = "Assets/Scenes/Hafta_3.unity";
        string h4Path = "Assets/Scenes/Hafta4_Uygulama.unity";

        if (!System.IO.File.Exists(h4Path) && System.IO.File.Exists(h3Path))
        {
            AssetDatabase.CopyAsset(h3Path, h4Path);
            AssetDatabase.Refresh();
        }

        var scene = EditorSceneManager.OpenScene(h4Path);

        int groundLayer = LayerMask.NameToLayer("Ground");
        int wallLayer = LayerMask.NameToLayer("Wall");

        // 1. Duvar Prefab'ı
        GameObject wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabs/WallPrefab.prefab");
        if (wallPrefab == null)
        {
            GameObject tempWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tempWall.name = "WallPrefab";
            if (wallLayer >= 0) tempWall.layer = wallLayer;

            var r = tempWall.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            r.material.color = new Color(0.7f, 0.7f, 0.75f);

            if (!AssetDatabase.IsValidFolder("Assets/prefabs"))
                AssetDatabase.CreateFolder("Assets", "prefabs");

            wallPrefab = PrefabUtility.SaveAsPrefabAsset(tempWall, "Assets/prefabs/WallPrefab.prefab");
            Object.DestroyImmediate(tempWall);
        }

        // 2. Maze Nesnesi
        GameObject mazeObj = GameObject.Find("Maze");
        if (mazeObj == null)
        {
            mazeObj = new GameObject("Maze");
            mazeObj.transform.position = new Vector3(-31f, 0f, -31f);
        }
        var maze = mazeObj.GetComponent<MazeGenerator>() ?? mazeObj.AddComponent<MazeGenerator>();
        maze.width = 31;
        maze.depth = 31;
        maze.cellSize = 2f;
        maze.wallPrefab = wallPrefab;
        maze.terrain = Terrain.activeTerrain;
        maze.wallHeight = 2f;
        maze.wallSink = 1f;
        maze.Generate();

        Vector3 startWorld = maze.CellToWorld(maze.StartCell);
        Vector3 goalWorld = maze.CellToWorld(maze.GoalCell);

        // 3. GoalFlag Nesnesi (Kırmızı Hedef Silindiri - Collider'sız)
        GameObject goalFlag = GameObject.Find("GoalFlag");
        if (goalFlag == null)
        {
            goalFlag = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goalFlag.name = "GoalFlag";
            goalFlag.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
            var ren = goalFlag.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.red;
        }
        var goalCol = goalFlag.GetComponent<Collider>();
        if (goalCol != null) Object.DestroyImmediate(goalCol);
        goalFlag.transform.position = goalWorld;

        // 4. NavAgent (Mavi NavMesh Kapsülü)
        GameObject navAgentObj = GameObject.Find("NavAgent");
        if (navAgentObj == null)
        {
            navAgentObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            navAgentObj.name = "NavAgent";
            var ren = navAgentObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.cyan;
        }
        var navCol = navAgentObj.GetComponent<Collider>();
        if (navCol != null) Object.DestroyImmediate(navCol);
        navAgentObj.transform.position = startWorld;

        var agent = navAgentObj.GetComponent<NavMeshAgent>() ?? navAgentObj.AddComponent<NavMeshAgent>();
        agent.speed = 4f;
        agent.angularSpeed = 720f;
        agent.acceleration = 20f;
        agent.stoppingDistance = 0.2f;
        agent.radius = 0.4f;
        agent.height = 1.8f;
        var navDebug = navAgentObj.GetComponent<NavPathDebug>() ?? navAgentObj.AddComponent<NavPathDebug>();
        navDebug.navAgent = agent;

        // 5. NavSurface & NavMeshRefresher
        int surfaceMask = (1 << (groundLayer >= 0 ? groundLayer : 0)) | (1 << (wallLayer >= 0 ? wallLayer : 0));
        GameObject navSurfaceObj = GameObject.Find("NavSurface");
        if (navSurfaceObj == null) navSurfaceObj = new GameObject("NavSurface");
        var surface = navSurfaceObj.GetComponent<NavMeshSurface>() ?? navSurfaceObj.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Volume;
        surface.layerMask = surfaceMask;

        var refresher = navSurfaceObj.GetComponent<NavMeshRefresher>() ?? navSurfaceObj.AddComponent<NavMeshRefresher>();
        refresher.surface = surface;
        refresher.maze = maze;
        refresher.navAgent = agent;
        refresher.Rebuild();

        // 6. Pathfinder & PathLine
        GameObject pathfinderObj = GameObject.Find("Pathfinder");
        if (pathfinderObj == null) pathfinderObj = new GameObject("Pathfinder");
        var pathfinder = pathfinderObj.GetComponent<GridPathfinder>() ?? pathfinderObj.AddComponent<GridPathfinder>();
        pathfinder.gridSource = maze;
        pathfinder.algorithm = GridPathfinder.Algorithm.AStar;
        pathfinder.heuristic = GridPathfinder.Heuristic.Euclidean;

        GameObject pathLineObj = GameObject.Find("PathLine");
        if (pathLineObj == null) pathLineObj = new GameObject("PathLine");
        var lineRen = pathLineObj.GetComponent<LineRenderer>() ?? pathLineObj.AddComponent<LineRenderer>();
        lineRen.startWidth = 0.35f;
        lineRen.endWidth = 0.35f;
        lineRen.material = new Material(Shader.Find("Sprites/Default"));
        lineRen.startColor = Color.yellow;
        lineRen.endColor = Color.green;

        var pathLine = pathLineObj.GetComponent<PathLine>() ?? pathLineObj.AddComponent<PathLine>();
        pathLine.pathfinder = pathfinder;
        pathLine.gridSource = maze;

        // 7. Oyuncu_AStar (Kendi A* Ajanımız - Sarı Kapsül)
        GameObject charObj = GameObject.Find("Oyuncu_AStar");
        if (charObj == null)
        {
            charObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            charObj.name = "Oyuncu_AStar";
            var ren = charObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.yellow;
        }
        charObj.transform.position = startWorld + Vector3.up * 1f;
        var cc = charObj.GetComponent<CharacterController>() ?? charObj.AddComponent<CharacterController>();
        cc.radius = 0.4f;
        cc.height = 1.8f;
        var follower = charObj.GetComponent<PathFollower>() ?? charObj.AddComponent<PathFollower>();
        follower.pathfinder = pathfinder;
        follower.gridSource = maze;
        follower.speed = 4f;

        // 8. Oyuncu_CC (Kullanıcının bizzat oynadığı karakter)
        GameObject occ = GameObject.Find("Oyuncu_CC");
        if (occ != null)
        {
            occ.SetActive(true);
            var ccHareket = occ.GetComponent<CCHareket>() ?? occ.AddComponent<CCHareket>();
            ccHareket.enabled = true;
            if (Camera.main != null) ccHareket.kamera = Camera.main.transform;

            var occCC = occ.GetComponent<CharacterController>();
            if (occCC != null)
            {
                occCC.enabled = false;
                occ.transform.position = startWorld + Vector3.back * 3f + Vector3.up * 1f;
                occCC.enabled = true;
            }
        }

        // 9. Input & WallEditor & Replanner
        GameObject inputObj = GameObject.Find("InputManager");
        if (inputObj == null) inputObj = new GameObject("InputManager");
        var click = inputObj.GetComponent<ClickToGoal>() ?? inputObj.AddComponent<ClickToGoal>();
        click.maze = maze;
        click.goalFlag = goalFlag.transform;
        click.groundMask = surfaceMask;
        click.navAgent = agent;
        click.pathfinder = pathfinder;
        click.agent = charObj.transform;

        var editor = inputObj.GetComponent<WallEditor>() ?? inputObj.AddComponent<WallEditor>();
        editor.maze = maze;
        editor.agent = charObj.transform;
        editor.navAgent = navAgentObj.transform;
        editor.editMask = surfaceMask;

        GameObject replannerObj = GameObject.Find("Replanner");
        if (replannerObj == null) replannerObj = new GameObject("Replanner");
        var replanner = replannerObj.GetComponent<Replanner>() ?? replannerObj.AddComponent<Replanner>();
        replanner.maze = maze;
        replanner.pathfinder = pathfinder;
        replanner.follower = follower;

        // 10. Kamera (3 Mod: Kuşbakışı, Oyuncu_CC, Yapay Zeka)
        Camera cam = Camera.main;
        CameraModes camModes = null;
        if (cam != null)
        {
            var oldTk = cam.GetComponent<TakipKamerasi>();
            if (oldTk != null) oldTk.enabled = false;

            camModes = cam.GetComponent<CameraModes>() ?? cam.gameObject.AddComponent<CameraModes>();
            camModes.maze = maze;
            camModes.oyuncuCC = (occ != null) ? occ.transform : null;
            camModes.targetAStar = charObj.transform;
            camModes.currentMode = CameraModes.CameraMode.OyuncuCCTakip;
            camModes.SnapToCurrentMode();
        }

        // 11. Hafta4HUD
        GameObject hudObj = GameObject.Find("Hafta4HUD");
        if (hudObj == null) hudObj = new GameObject("Hafta4HUD");
        var hud = hudObj.GetComponent<Hafta4HUD>() ?? hudObj.AddComponent<Hafta4HUD>();
        hud.navAgent = agent;
        hud.ourAgent = follower;
        hud.pathfinder = pathfinder;
        hud.camModes = camModes;

        // 12. Diğer çakışan nesneleri kapat
        GameObject gozcu = GameObject.Find("Gozcu");
        if (gozcu != null) gozcu.SetActive(false);
        GameObject h3hud = GameObject.Find("Hafta3HUD");
        if (h3hud != null) h3hud.SetActive(false);
        GameObject orb = GameObject.Find("Oyuncu_RB");
        if (orb != null) orb.SetActive(false);
        GameObject sonda = GameObject.Find("AraziSondasi");
        if (sonda != null) sonda.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("🎉 HAFTA 4: 3 Kameralı Sistem, Oyuncu_CC ve A* & NavMesh Yarışı Başarıyla Kuruldu!");
    }
}
#endif
