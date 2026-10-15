using UnityEngine;

/// <summary>
/// 3. Hafta: Sahne Play modunda açıldığında eksik nesne varsa
/// (Arazi, Ağaçlar, Oyuncu_CC, Gözcü, Huni, HUD) anında kuran çalışma zamanı motoru.
/// </summary>
public class Hafta3RuntimeInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void EnsureHafta3Ready()
    {
        // 1. Zemin / Arazi Kontrolü
        GameObject terrainObj = GameObject.Find("Terrain");
        Terrain terrain = terrainObj != null ? terrainObj.GetComponent<Terrain>() : Terrain.activeTerrain;

        if (terrain == null && GameObject.Find("Hafta3_Zemin") == null)
        {
            GameObject zemin = GameObject.CreatePrimitive(PrimitiveType.Plane);
            zemin.name = "Hafta3_Zemin";
            zemin.transform.position = Vector3.zero;
            zemin.transform.localScale = new Vector3(20, 1, 20); // 200x200m
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0) zemin.layer = groundLayer;

            // Zemin materyali (yeşil çimen tonu)
            var ren = zemin.GetComponent<Renderer>();
            ren.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            ren.material.color = new Color(0.25f, 0.55f, 0.2f);
        }

        // 2. Ağaçlar (15 Adet)
        GameObject agaclar = GameObject.Find("Agaclar");
        if (agaclar == null)
        {
            agaclar = new GameObject("Agaclar");
            Vector3[] poslar = new Vector3[]
            {
                new Vector3(-12, 0, 6), new Vector3(-8, 0, 14), new Vector3(-3, 0, 18),
                new Vector3(6, 0, 15), new Vector3(14, 0, 9), new Vector3(-16, 0, -4),
                new Vector3(-7, 0, -11), new Vector3(2, 0, -17), new Vector3(13, 0, -8),
                new Vector3(-22, 0, 8), new Vector3(18, 0, 14), new Vector3(22, 0, -4),
                new Vector3(-10, 0, 20), new Vector3(7, 0, 22), new Vector3(-1, 0, 7)
            };

            foreach (var pos in poslar)
            {
                GameObject tree = new GameObject("Agac");
                tree.transform.SetParent(agaclar.transform);
                tree.transform.position = pos;

                // Gövde
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Govde";
                trunk.transform.SetParent(tree.transform);
                trunk.transform.localPosition = new Vector3(0, 1.5f, 0);
                trunk.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
                var tRen = trunk.GetComponent<Renderer>();
                tRen.material.color = new Color(0.4f, 0.22f, 0.08f);

                // Yapraklar
                GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaves.name = "Yapraklar";
                leaves.transform.SetParent(tree.transform);
                leaves.transform.localPosition = new Vector3(0, 3.2f, 0);
                leaves.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
                var lRen = leaves.GetComponent<Renderer>();
                lRen.material.color = new Color(0.12f, 0.55f, 0.15f);
            }
        }

        // 3. Saklanma Bloğu ve Basamaklar
        if (GameObject.Find("SaklanmaKupu") == null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "SaklanmaKupu";
            cube.transform.position = new Vector3(0, 1.5f, 3f);
            cube.transform.localScale = new Vector3(4f, 3f, 1.5f);
            var ren = cube.GetComponent<Renderer>();
            ren.material.color = new Color(0.6f, 0.6f, 0.65f);
        }

        // 4. Oyuncu CC
        int playerLayer = LayerMask.NameToLayer("Player");
        GameObject ccObj = GameObject.Find("Oyuncu_CC");
        if (ccObj == null)
        {
            ccObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            ccObj.name = "Oyuncu_CC";
            Destroy(ccObj.GetComponent<CapsuleCollider>());

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

        // 5. Oyuncu RB
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
        rbObj.SetActive(false);

        // 6. Main Camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            var tk = cam.gameObject.GetComponent<TakipKamerasi>() ?? cam.gameObject.AddComponent<TakipKamerasi>();
            tk.hedef = ccObj.transform;

            var ccScript = ccObj.GetComponent<CCHareket>();
            if (ccScript != null) ccScript.kamera = cam.transform;

            var rbScript = rbObj.GetComponent<RBHareket>();
            if (rbScript != null) rbScript.kamera = cam.transform;
        }

        // 7. Gözcü ve Burun
        GameObject gozcuObj = GameObject.Find("Gozcu");
        if (gozcuObj == null)
        {
            gozcuObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            gozcuObj.name = "Gozcu";

            GameObject burun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            burun.name = "Burun";
            burun.transform.SetParent(gozcuObj.transform);
            burun.transform.localPosition = new Vector3(0, 0.4f, 0.5f);
            burun.transform.localScale = new Vector3(0.25f, 0.25f, 0.35f);
            Destroy(burun.GetComponent<Collider>());
            burun.GetComponent<Renderer>().material.color = Color.black;
        }
        gozcuObj.transform.position = new Vector3(0, 1f, 10f);
        gozcuObj.transform.rotation = Quaternion.Euler(0, 180, 0);

        var gozcu = gozcuObj.GetComponent<Gozcu>() ?? gozcuObj.AddComponent<Gozcu>();
        gozcu.oyuncu = ccObj.transform;
        gozcu.menzil = 18f;
        gozcu.gorusAcisi = 90f;
        gozcu.maksDikeyAci = 30f;
        gozcu.engeller = ~LayerMask.GetMask("Player");

        // 8. HUD
        GameObject hudObj = GameObject.Find("Hafta3HUD");
        if (hudObj == null) hudObj = new GameObject("Hafta3HUD");
        var hud = hudObj.GetComponent<Hafta3HUD>() ?? hudObj.AddComponent<Hafta3HUD>();
        hud.gozcu = gozcu;
        hud.oyuncuCC = ccObj;
        hud.oyuncuRB = rbObj;
        if (cam != null) hud.takipKamerasi = cam.GetComponent<TakipKamerasi>();
    }
}
