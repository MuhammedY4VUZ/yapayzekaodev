using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3. Hafta: Ekranda kontrolleri, Gözcü durumunu (Gördü / Gizlendi)
/// ve CC vs RB karakter geçişini ([T] tuşu) sağlayan bilgilendirici arayüz.
/// </summary>
public class Hafta3HUD : MonoBehaviour
{
    public Gozcu gozcu;
    public GameObject oyuncuCC;
    public GameObject oyuncuRB;
    public TakipKamerasi takipKamerasi;

    bool ccAktif = true;

    void Update()
    {
        // [T] Tuşu: CharacterController ile Rigidbody karakterleri arasında geçiş yap
        bool tPressed = false;
        if (Keyboard.current != null)
            tPressed = Keyboard.current.tKey.wasPressedThisFrame;

        if (tPressed)
        {
            ccAktif = !ccAktif;
            if (oyuncuCC != null) oyuncuCC.SetActive(ccAktif);
            if (oyuncuRB != null) oyuncuRB.SetActive(!ccAktif);

            Transform aktifOyuncu = ccAktif ? oyuncuCC.transform : oyuncuRB.transform;
            if (takipKamerasi != null) takipKamerasi.hedef = aktifOyuncu;
            if (gozcu != null) gozcu.oyuncu = aktifOyuncu;
        }
    }

    void OnGUI()
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 13;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(15, 15, 420, 240), boxStyle);
        GUILayout.Label("<b>HAFTA 3: 3B DÜNYA, ARAZİ & GÖZCÜ YZ</b>");
        GUILayout.Label("--------------------------------------------------");
        GUILayout.Label("• <b>WASD / Yön Tuşları:</b> Karakteri yürüt");
        GUILayout.Label("• <b>Sol Shift:</b> Koş  |  <b>Boşluk (Space):</b> Zıpla");
        GUILayout.Label("• <b>Fare Hareketi:</b> Kamerayı döndür");
        GUILayout.Label($"• <b>[T] Tuşu:</b> Karakteri Değiştir: <b>{(ccAktif ? "CharacterController" : "Rigidbody")}</b>");
        GUILayout.Space(5);

        if (gozcu != null && gozcu.oyuncu != null)
        {
            Vector3 goz = gozcu.transform.position + Vector3.up * gozcu.gozYuksekligi;
            Vector3 hedef = gozcu.oyuncu.position + Vector3.up * 0.8f;
            bool konide = gozcu.GorusKonisinde(goz, hedef);
            bool hatAcik = gozcu.GorusHattiAcik(goz, hedef);
            bool goruyor = konide && hatAcik;
            float mesafe = Vector3.Distance(goz, hedef);

            string durumRenk = goruyor ? "<color=red><b>GÖRDÜ (KIRMIZI)</b></color>" : "<color=green><b>GİZLENDİ / GÖRMÜYOR (YEŞİL)</b></color>";
            GUILayout.Label($"<b>Gözcü Durumu:</b> {durumRenk}");
            GUILayout.Label($"• Mesafe: <b>{mesafe:F1} m</b> / Menzil: {gozcu.menzil} m");
            GUILayout.Label($"• Görüş Konisinde mi?: <b>{(konide ? "EVET" : "HAYIR")}</b>");
            GUILayout.Label($"• Görüş Hattı (Raycast) Açık mı?: <b>{(hatAcik ? "AÇIK (Engel Yok)" : "ENGEL VAR (Tepe/Ağaç Arkasında)")}</b>");

            Vector3 hedefYon = (gozcu.oyuncu.position - gozcu.transform.position).normalized;
            hedefYon.y = 0;
            float caprazY = Vector3.Cross(gozcu.transform.forward, hedefYon).y;
            string yon = caprazY >= 0 ? "SAĞINDA" : "SOLUNDA";
            GUILayout.Label($"• Oyuncu Gözcünün <b>{yon}</b> (Cross Product)");
        }

        GUILayout.EndArea();
    }
}
