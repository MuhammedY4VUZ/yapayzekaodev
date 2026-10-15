using UnityEngine;

/// <summary>
/// 3. Hafta Ödevi: Gözcü Yapay Zekası.
/// - 3B Görüş Konisi (Nokta Çarpım)
/// - Görüş Hattı Engelleme (Raycast / Linecast ile Arazi, Ağaç ve Nesneler)
/// - Görüş Alanını Gösteren Şeffaf Huni (FOV Cone Mesh)
/// - Oyuncunun Sağda / Solda Olma Durumu (Vektörel Çapraz Çarpım)
/// - Ek Görevler (+10 Puan):
///   1. Tarama (+4): ±60° sağa-sola devriye dönüşü (30°/s)
///   2. Yükseklik Tuzağı (+3): Yatay 90°, Dikey ±30° açı kontrolü
///   3. Takip (+3): Oyuncuyu görünce takip etme, 2 sn sonra taramaya dönme
/// </summary>
public class Gozcu : MonoBehaviour
{
    [Header("Hedef ve Algılama")]
    public Transform oyuncu;
    public float menzil = 15f;
    public float gorusAcisi = 90f;        // Yatay görüş açısı
    public float maksDikeyAci = 30f;      // Yükseklik tuzağı (+3 puan)
    public float gozYuksekligi = 0.6f;
    public LayerMask engeller = ~0;       // Arazi, ağaçlar, duvarlar (Player hariç)

    [Header("Ek Puan Özellikleri (+10 Puan)")]
    public bool taramaAktif = true;       // Tarama (+4 puan)
    public float taramaAcisi = 60f;
    public float taramaHizi = 30f;
    public float takipHizi = 60f;         // Takip (+3 puan)
    public float beklemeSuresi = 2f;

    [Header("Görsel Huni (FOV Cone)")]
    public bool huniGoster = true;
    public Material huniMateryali;

    Renderer govde;
    Quaternion baslangicRotasyonu;
    float sonGorulmeZamani = -10f;
    bool sagaDon = true;
    MeshFilter huniFilter;
    MeshRenderer huniRenderer;
    Mesh huniMesh;

    void Awake()
    {
        govde = GetComponent<Renderer>();
        baslangicRotasyonu = transform.rotation;
        if (engeller == ~0)
            engeller = ~LayerMask.GetMask("Player");

        if (huniGoster)
            OlusturGorusHunisi();
    }

    void Update()
    {
        Vector3 goz = transform.position + Vector3.up * gozYuksekligi;
        bool goruyor = false;

        if (oyuncu != null)
        {
            Vector3 hedefNokta = oyuncu.position + Vector3.up * 0.8f;
            goruyor = GorusKonisinde(goz, hedefNokta) && GorusHattiAcik(goz, hedefNokta);
        }

        // Renk geri bildirimi: Görmüyorsa Yeşil, Görüyorsa Kırmızı
        if (govde != null)
            govde.material.color = goruyor ? Color.red : Color.green;

        // Görsel Huninin rengini güncelle
        if (huniRenderer != null && huniRenderer.material != null)
        {
            Color c = goruyor ? new Color(1f, 0f, 0f, 0.35f) : new Color(0f, 1f, 0f, 0.25f);
            huniRenderer.material.color = c;
        }

        // Ek Görev Davranışları (Tarama & Takip)
        if (goruyor)
        {
            sonGorulmeZamani = Time.time;

            // Sağ / Sol hesabı (Çapraz çarpım - 1. Hafta formülü)
            Vector3 hedefYon = (oyuncu.position - transform.position).normalized;
            hedefYon.y = 0f;
            float caprazY = Vector3.Cross(transform.forward, hedefYon).y;
            string taraf = caprazY >= 0 ? "SAĞINDA" : "SOLUNDA";

            if (Time.frameCount % 60 == 0)
                Debug.Log($"🚨 Gözcü oyuncuyu gördü! Konum: Gözcünün {taraf} (Çapraz Y: {caprazY:F2})");

            // Takip (+3): Oyuncuya doğru dön
            if (hedefYon.sqrMagnitude > 0.001f)
            {
                Quaternion hedefRot = Quaternion.LookRotation(hedefYon);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, hedefRot, takipHizi * Time.deltaTime);
            }
        }
        else
        {
            // 2 saniye göremediyse başlangıç rotasyonuna göre ±60° tarama yap
            if (taramaAktif && (Time.time - sonGorulmeZamani > beklemeSuresi))
            {
                float hedefAci = sagaDon ? taramaAcisi : -taramaAcisi;
                Quaternion hedefRot = baslangicRotasyonu * Quaternion.Euler(0, hedefAci, 0);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, hedefRot, taramaHizi * Time.deltaTime);

                if (Quaternion.Angle(transform.rotation, hedefRot) < 1f)
                    sagaDon = !sagaDon;
            }
        }
    }

    /// <summary>
    /// Görüş konisi kontrolü: Mesafe, Yatay Açı (Nokta Çarpım) ve Dikey Açı (Yükseklik Tuzağı)
    /// </summary>
    public bool GorusKonisinde(Vector3 goz, Vector3 hedef)
    {
        Vector3 fark = hedef - goz;
        float mesafe = fark.magnitude;

        // 1. Menzil kontrolü
        if (mesafe > this.menzil || mesafe < 0.001f) return false;

        // 2. Yatay açı kontrolü (Nokta çarpım ile cos karşılaştırması)
        Vector3 yatayFark = new Vector3(fark.x, 0f, fark.z).normalized;
        Vector3 yatayIleri = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        float nokta = Vector3.Dot(yatayIleri, yatayFark);
        float esikCos = Mathf.Cos(gorusAcisi * 0.5f * Mathf.Deg2Rad);

        if (nokta < esikCos) return false;

        // 3. Yükseklik Tuzağı (+3 Puan): Dikey açı kontrolü
        float dikeyAci = Mathf.Atan2(Mathf.Abs(fark.y), new Vector2(fark.x, fark.z).magnitude) * Mathf.Rad2Deg;
        if (dikeyAci > maksDikeyAci) return false;

        return true;
    }

    /// <summary>
    /// Görüş hattı kontrolü: Gözden hedefe Raycast atılır; arazi veya ağaç çarparsa false döner.
    /// </summary>
    public bool GorusHattiAcik(Vector3 goz, Vector3 hedef)
    {
        Vector3 yon = hedef - goz;
        float mesafe = yon.magnitude;

        if (Physics.Raycast(goz, yon.normalized, out RaycastHit hit, mesafe, engeller, QueryTriggerInteraction.Ignore))
        {
            // Eğer ışın bir engele çarptıysa ve çarptığı nesne oyuncu değilse görüş hattı kapalıdır
            if (oyuncu != null && hit.transform != oyuncu && !hit.transform.IsChildOf(oyuncu))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Görüş konisini (huniyi) Game penceresinde görünür kılmak için prosedürel huni mesh'i oluşturur.
    /// </summary>
    void OlusturGorusHunisi()
    {
        GameObject huniObj = new GameObject("GorusHunisi");
        huniObj.transform.SetParent(transform);
        huniObj.transform.localPosition = Vector3.up * gozYuksekligi;
        huniObj.transform.localRotation = Quaternion.identity;

        huniFilter = huniObj.AddComponent<MeshFilter>();
        huniRenderer = huniObj.AddComponent<MeshRenderer>();

        // Standart URP Transparent Shader veya Sprites Shader
        Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        Material mat = new Material(s);
        mat.color = new Color(0f, 1f, 0f, 0.25f);
        huniRenderer.material = mat;

        huniMesh = new Mesh();
        huniMesh.name = "HuniMesh";

        int dilim = 24;
        Vector3[] vertices = new Vector3[dilim + 2];
        int[] triangles = new int[dilim * 3];

        vertices[0] = Vector3.zero; // Tepe noktası (göz)
        float yarimAci = gorusAcisi * 0.5f;

        for (int i = 0; i <= dilim; i++)
        {
            float oran = (float)i / dilim;
            float aci = Mathf.Lerp(-yarimAci, yarimAci, oran);
            Quaternion rot = Quaternion.Euler(0, aci, 0);
            vertices[i + 1] = rot * Vector3.forward * menzil;
        }

        for (int i = 0; i < dilim; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        huniMesh.vertices = vertices;
        huniMesh.triangles = triangles;
        huniMesh.RecalculateNormals();
        huniFilter.mesh = huniMesh;
    }

    void OnDrawGizmos()
    {
        Vector3 goz = transform.position + Vector3.up * gozYuksekligi;

        // 1. Menzil küresi
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawWireSphere(goz, menzil);

        // 2. Koni sınırları
        Gizmos.color = Color.yellow;
        float yarimAci = gorusAcisi * 0.5f;
        Vector3 solYon = Quaternion.AngleAxis(-yarimAci, Vector3.up) * transform.forward;
        Vector3 sagYon = Quaternion.AngleAxis(yarimAci, Vector3.up) * transform.forward;

        Gizmos.DrawLine(goz, goz + solYon * menzil);
        Gizmos.DrawLine(goz, goz + sagYon * menzil);
        Gizmos.DrawLine(goz, goz + transform.forward * menzil);

        // 3. Görüş hattı ışını
        if (oyuncu != null)
        {
            Vector3 hedef = oyuncu.position + Vector3.up * 0.8f;
            bool goruyor = GorusKonisinde(goz, hedef) && GorusHattiAcik(goz, hedef);
            Gizmos.color = goruyor ? Color.red : Color.gray;
            Gizmos.DrawLine(goz, hedef);
        }
    }
}
