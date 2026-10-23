using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.SceneManagement;

/// <summary>
/// 4. Hafta: Sahne Play modunda açıldığında eksik nesne varsa anında kuran,
/// Oyuncu_CC kontrolünü, 3 kameralı sistemi ve hem NavMesh hem A* yarışını tam başlatan motor.
/// </summary>
public class Hafta4RuntimeInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureHafta4Ready()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Hafta4") && !sceneName.Contains("Hafta_4")) return;

        // 1. Hafta 3'ten kalan çakışan nesneleri kapat, Oyuncu_CC'yi AKTİF tut
        GameObject gozcu = GameObject.Find("Gozcu");
        if (gozcu != null) gozcu.SetActive(false);
        GameObject h3hud = GameObject.Find("Hafta3HUD");
        if (h3hud != null) h3hud.SetActive(false);
        GameObject sonda = GameObject.Find("AraziSondasi");
        if (sonda != null) sonda.SetActive(false);
        GameObject orb = GameObject.Find("Oyuncu_RB");
        if (orb != null) orb.SetActive(false);

        // 2. Maze nesnesi
        GameObject mazeObj = GameObject.Find("Maze");
        MazeGenerator maze = null;
        if (mazeObj == null)
        {
            mazeObj = new GameObject("Maze");
            mazeObj.transform.position = new Vector3(-31f, 0f, -31f);
            maze = mazeObj.AddComponent<MazeGenerator>();
        }
        else
        {
            maze = mazeObj.GetComponent<MazeGenerator>();
        }

        maze.width = 31;
        maze.depth = 31;
        maze.cellSize = 2f;
        maze.wallHeight = 2f;
        maze.wallSink = 1f;
        if (maze.terrain == null) maze.terrain = Terrain.activeTerrain;
        maze.Generate(); // Haritayı kesin olarak üret

        Vector3 startWorld = maze.CellToWorld(maze.StartCell);
        Vector3 goalWorld = maze.CellToWorld(maze.GoalCell);

        // 3. GoalFlag (Kırmızı Hedef Silindiri - Collider'sız)
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
        if (goalCol != null) Object.Destroy(goalCol);
        goalFlag.transform.position = goalWorld;

        // 4. NavAgent (Mavi NavMesh Kapsülü)
        GameObject navAgentObj = GameObject.Find("NavAgent");
        NavMeshAgent agent = null;
        if (navAgentObj == null)
        {
            navAgentObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            navAgentObj.name = "NavAgent";
            var col = navAgentObj.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            var ren = navAgentObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.cyan;

            agent = navAgentObj.AddComponent<NavMeshAgent>();
            agent.speed = 4f;
            agent.angularSpeed = 720f;
            agent.acceleration = 20f;
            agent.stoppingDistance = 0.2f;
            agent.radius = 0.4f;
            agent.height = 1.8f;

            var navDebug = navAgentObj.AddComponent<NavPathDebug>();
            navDebug.navAgent = agent;
        }
        else
        {
            agent = navAgentObj.GetComponent<NavMeshAgent>();
        }
        navAgentObj.transform.position = startWorld;

        // 5. NavSurface & NavMeshRefresher (Dinamik NavMesh Pişirici)
        int gLayer = LayerMask.NameToLayer("Ground");
        int wLayer = LayerMask.NameToLayer("Wall");
        int surfaceMask = ((gLayer >= 0) ? (1 << gLayer) : 1) | ((wLayer >= 0) ? (1 << wLayer) : 0);

        GameObject navSurfaceObj = GameObject.Find("NavSurface");
        if (navSurfaceObj == null) navSurfaceObj = new GameObject("NavSurface");
        var surface = navSurfaceObj.GetComponent<NavMeshSurface>() ?? navSurfaceObj.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Volume;
        surface.layerMask = surfaceMask;

        var refresher = navSurfaceObj.GetComponent<NavMeshRefresher>() ?? navSurfaceObj.AddComponent<NavMeshRefresher>();
        refresher.surface = surface;
        refresher.maze = maze;
        refresher.navAgent = agent;
        refresher.Rebuild(); // NavMesh'i anında pişir!

        // 6. Pathfinder & PathLine
        GameObject pathfinderObj = GameObject.Find("Pathfinder");
        GridPathfinder pathfinder = null;
        if (pathfinderObj == null)
        {
            pathfinderObj = new GameObject("Pathfinder");
            pathfinder = pathfinderObj.AddComponent<GridPathfinder>();
        }
        else
        {
            pathfinder = pathfinderObj.GetComponent<GridPathfinder>();
        }
        pathfinder.gridSource = maze;
        pathfinder.algorithm = GridPathfinder.Algorithm.AStar;
        pathfinder.heuristic = GridPathfinder.Heuristic.Euclidean;

        GameObject pathLineObj = GameObject.Find("PathLine");
        if (pathLineObj == null)
        {
            pathLineObj = new GameObject("PathLine");
            var lineRen = pathLineObj.AddComponent<LineRenderer>();
            lineRen.startWidth = 0.35f;
            lineRen.endWidth = 0.35f;
            lineRen.material = new Material(Shader.Find("Sprites/Default"));
            lineRen.startColor = Color.yellow;
            lineRen.endColor = Color.green;

            var pathLine = pathLineObj.AddComponent<PathLine>();
            pathLine.pathfinder = pathfinder;
            pathLine.gridSource = maze;
        }

        // 7. Oyuncu_AStar (Kendi A* Ajanımız - Sarı Kapsül)
        GameObject charObj = GameObject.Find("Oyuncu_AStar");
        PathFollower follower = null;
        if (charObj == null)
        {
            charObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            charObj.name = "Oyuncu_AStar";
            var ren = charObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.yellow;

            var cc = charObj.AddComponent<CharacterController>();
            cc.radius = 0.4f;
            cc.height = 1.8f;

            follower = charObj.AddComponent<PathFollower>();
        }
        else
        {
            follower = charObj.GetComponent<PathFollower>();
        }

        charObj.transform.position = startWorld + Vector3.up * 1f;
        follower.pathfinder = pathfinder;
        follower.gridSource = maze;
        follower.Teleport(maze.StartCell);

        // 8. Oyuncu_CC (Kullanıcının bizzat yönettiği karakter)
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
                occ.transform.position = startWorld + Vector3.back * 3f + Vector3.up * 1f;
                cc.enabled = true;
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

        // 10. Kamera Modları (3 Kamera: Kuşbakışı, Oyuncu_CC, Yapay Zeka)
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
            // Kullanıcı bizzat Oyuncu_CC'yi oynadığı için kamera Oyuncu_CC takibinde başlasın
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
    }
}
