#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 3. Hafta: Hocanın ders notundaki tüm adımları (Arazi, Tepe/Düzlük/Yamaç, Ağaçlar,
/// Oyuncu_CC, Oyuncu_RB, Takip Kamerası, Arazi Sondası, Gözcü ve Görüş Hunisi)
/// tek tıkla kurup sahneyi kaydeden profesyonel editör aracı.
/// </summary>
[InitializeOnLoad]
public static class Hafta3SceneBuilder
{
    static Hafta3SceneBuilder()
    {
        EditorApplication.delayCall += CheckAndBuild;
    }

    static void CheckAndBuild()
    {
        if (GameObject.Find("Gozcu") == null)
        {
            BuildScene();
        }
    }

    [MenuItem("Yapay Zeka/Hafta 3 Sahnesini Eksiksiz Kur")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.GetActiveScene();

        // 1. Tag & Layers kontrolü
        int playerLayer = LayerMask.NameToLayer("Player");
        int groundLayer = LayerMask.NameToLayer("Ground");

        // 2. Basit Ağaç Prefab'ı Oluştur (Silindir Gövde + Küre Yapraklar)
        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabs/Agac.prefab");
        if (treePrefab == null)
        {
            GameObject treeRoot = new GameObject("Agac");

            // Gövde (Silindir + Collider)
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Govde";
            trunk.transform.SetParent(treeRoot.transform);
            trunk.transform.localPosition = new Vector3(0, 1.5f, 0);
            trunk.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
            var trunkRen = trunk.GetComponent<Renderer>();
            trunkRen.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            trunkRen.material.color = new Color(0.4f, 0.22f, 0.08f); // Kahverengi

            // Yapraklar (Küre)
            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Yapraklar";
            leaves.transform.SetParent(treeRoot.transform);
            leaves.transform.localPosition = new Vector3(0, 3.2f, 0);
            leaves.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
            var leavesRen = leaves.GetComponent<Renderer>();
            leavesRen.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            leavesRen.material.color = new Color(0.12f, 0.55f, 0.15f); // Koyu yeşil

            if (!AssetDatabase.IsValidFolder("Assets/prefabs"))
                AssetDatabase.CreateFolder("Assets", "prefabs");

            treePrefab = PrefabUtility.SaveAsPrefabAsset(treeRoot, "Assets/prefabs/Agac.prefab");
            Object.DestroyImmediate(treeRoot);
        }

        // 3. Arazi (Terrain) Oluştur ve Şekillendir
        GameObject terrainObj = GameObject.Find("Terrain");
        Terrain terrain = null;
        if (terrainObj != null) terrain = terrainObj.GetComponent<Terrain>();

        if (terrain == null)
        {
            TerrainData tData = new TerrainData();
            tData.name = "TerrainData_H3";
            tData.heightmapResolution = 257;
            tData.size = new Vector3(200, 50, 200);

            // Tepe, Düzlük ve Dik Yamaç Şekillendirme
            int res = tData.heightmapResolution;
            float[,] heights = new float[res, res];

            for (int x = 0; x < res; x++)
            {
                for (int y = 0; y < res; y++)
                {
                    float u = (float)x / res;
                    float v = (float)y / res;

                    // Tepe (Merkeze yakın sağ taraf)
                    float distTepe = Vector2.Distance(new Vector2(u, v), new Vector2(0.65f, 0.65f));
                    float tepe = Mathf.Max(0, 1f - distTepe / 0.2f) * 0.35f;

                    // Dik Yamaç (> 45 derece)
                    float distYamac = Vector2.Distance(new Vector2(u, v), new Vector2(0.35f, 0.65f));
                    float yamac = Mathf.Max(0, 1f - distYamac / 0.1f) * 0.45f;

                    heights[x, y] = Mathf.Max(tepe, yamac);
                }
            }
            tData.SetHeights(0, 0, heights);

            AssetDatabase.CreateAsset(tData, "Assets/TerrainData_H3.asset");

            terrainObj = Terrain.CreateTerrainGameObject(tData);
            terrainObj.name = "Terrain";
            terrainObj.transform.position = new Vector3(-100, 0, -100);
            if (groundLayer >= 0) terrainObj.layer = groundLayer;
            terrain = terrainObj.GetComponent<Terrain>();
        }

        // 4. Ağaçları Araziye Dik (15 Adet)
        GameObject treesRoot = GameObject.Find("Agaclar");
        if (treesRoot == null)
        {
            treesRoot = new GameObject("Agaclar");
            Vector3[] treePositions = new Vector3[]
            {
                new Vector3(-15, 0, 5), new Vector3(-10, 0, 12), new Vector3(-5, 0, 18),
                new Vector3(5, 0, 16), new Vector3(12, 0, 8), new Vector3(-18, 0, -5),
                new Vector3(-8, 0, -12), new Vector3(0, 0, -18), new Vector3(15, 0, -10),
                new Vector3(-25, 0, 10), new Vector3(20, 0, 15), new Vector3(25, 0, -5),
                new Vector3(-12, 0, 22), new Vector3(8, 0, 25), new Vector3(-2, 0, 8)
            };

            foreach (var pos in treePositions)
            {
                float y = terrain != null ? terrain.SampleHeight(pos) : 0;
                GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab, treesRoot.transform);
                tree.transform.position = new Vector3(pos.x, y, pos.z);
            }
        }

        // 5. Engel Küpü ve Basamaklar (0.25 m ve 0.5 m)
        GameObject engeller = GameObject.Find("EngelNesneleri");
        if (engeller == null)
        {
            engeller = new GameObject("EngelNesneleri");

            // Saklanma Küpü
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "SaklanmaKupu";
            cube.transform.SetParent(engeller.transform);
            cube.transform.position = new Vector3(0, 1.5f, 3f);
            cube.transform.localScale = new Vector3(4f, 3f, 1.5f);

            // Basamak 1 (0.25 m)
            GameObject b1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b1.name = "Basamak_025m";
            b1.transform.SetParent(engeller.transform);
            b1.transform.position = new Vector3(-6, 0.125f, -5);
            b1.transform.localScale = new Vector3(2, 0.25f, 2);

            // Basamak 2 (0.5 m)
            GameObject b2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b2.name = "Basamak_050m";
            b2.transform.SetParent(engeller.transform);
            b2.transform.position = new Vector3(-6, 0.25f, -2);
            b2.transform.localScale = new Vector3(2, 0.5f, 2);
        }

        // 6. Oyuncu CC (CharacterController)
        GameObject ccObj = GameObject.Find("Oyuncu_CC");
        if (ccObj == null)
        {
            ccObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            ccObj.name = "Oyuncu_CC";
            Object.DestroyImmediate(ccObj.GetComponent<CapsuleCollider>()); // CC kendi colliderını getirir

            var cc = ccObj.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 2f;
            cc.slopeLimit = 45f;

            ccObj.AddComponent<CCHareket>();
            var ren = ccObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.cyan;
        }
        ccObj.transform.position = new Vector3(0, 1f, -10f);
        if (playerLayer >= 0) ccObj.layer = playerLayer;

        // 7. Oyuncu RB (Rigidbody)
        GameObject rbObj = GameObject.Find("Oyuncu_RB");
        if (rbObj == null)
        {
            rbObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rbObj.name = "Oyuncu_RB";

            var rb = rbObj.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            rbObj.AddComponent<RBHareket>();
            var ren = rbObj.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = Color.yellow;
        }
        rbObj.transform.position = new Vector3(3, 1f, -10f);
        if (playerLayer >= 0) rbObj.layer = playerLayer;
        rbObj.SetActive(false); // Başlangıçta CC aktif, [T] ile RB'ye geçilir

        // 8. Main Camera & Takip Kamerası
        Camera cam = Camera.main;
        if (cam != null)
        {
            var tk = cam.gameObject.GetComponent<TakipKamerasi>() ?? cam.gameObject.AddComponent<TakipKamerasi>();
            tk.hedef = ccObj.transform;
            tk.mesafe = 7f;

            var ccScript = ccObj.GetComponent<CCHareket>();
            if (ccScript != null) ccScript.kamera = cam.transform;

            var rbScript = rbObj.GetComponent<RBHareket>();
            if (rbScript != null) rbScript.kamera = cam.transform;
        }

        // 9. Gözcü (Gozcu Kapsülü + Burun + Görüş Hunisi)
        GameObject gozcuObj = GameObject.Find("Gozcu");
        if (gozcuObj == null)
        {
            gozcuObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            gozcuObj.name = "Gozcu";

            // Burun (Gözcünün baktığı yönü gösterir)
            GameObject burun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            burun.name = "Burun";
            burun.transform.SetParent(gozcuObj.transform);
            burun.transform.localPosition = new Vector3(0, 0.4f, 0.5f);
            burun.transform.localScale = new Vector3(0.25f, 0.25f, 0.35f);
            Object.DestroyImmediate(burun.GetComponent<Collider>());
            var bRen = burun.GetComponent<Renderer>();
            bRen.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            bRen.material.color = Color.black;
        }
        gozcuObj.transform.position = new Vector3(0, 1f, 10f);
        gozcuObj.transform.rotation = Quaternion.Euler(0, 180, 0); // Oyuncuya doğru baksın

        var gozcu = gozcuObj.GetComponent<Gozcu>() ?? gozcuObj.AddComponent<Gozcu>();
        gozcu.oyuncu = ccObj.transform;
        gozcu.menzil = 18f;
        gozcu.gorusAcisi = 90f;
        gozcu.maksDikeyAci = 30f;
        gozcu.engeller = ~LayerMask.GetMask("Player");

        // 10. Arazi Sondası
        GameObject sondaObj = GameObject.Find("AraziSondasi");
        if (sondaObj == null)
        {
            sondaObj = new GameObject("AraziSondasi");
            sondaObj.AddComponent<AraziSondasi>();
        }
        sondaObj.transform.position = new Vector3(5, 0, 5);

        // 11. HUD Bilgi Paneli
        GameObject hudObj = GameObject.Find("Hafta3HUD");
        if (hudObj == null) hudObj = new GameObject("Hafta3HUD");
        var hud = hudObj.GetComponent<Hafta3HUD>() ?? hudObj.AddComponent<Hafta3HUD>();
        hud.gozcu = gozcu;
        hud.oyuncuCC = ccObj;
        hud.oyuncuRB = rbObj;
        if (cam != null) hud.takipKamerasi = cam.GetComponent<TakipKamerasi>();

        // 12. Sahneyi Kaydet (Hem Hafta_3 hem de açık olan sahne)
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Ayrıca Assets/Scenes/Hafta_3.unity olarak da kaydet
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Hafta_3.unity", true);

        Debug.Log("🎉 HAFTA 3: Arazi, Ağaçlar, Karakterler ve Görüş Hunili Gözcü Başarıyla Kuruldu!");
    }
}
#endif
