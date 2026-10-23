using UnityEngine;

/// <summary>
/// 3. ve 4. Hafta: İstenen yön hesabını yapan ortak statik yardımcı sınıf.
/// Giriş vektörünün boyunu 1 ile sınırlar ve kameranın yatay yönüne göre yönü hesaplar.
/// Kamera tam tepeden (kuşbakışı) baktığında da W/S (ileri/geri) hareketinin kaybolmasını engeller.
/// </summary>
public static class Hareket
{
    public static Vector3 IstenenYon(Vector2 girdi, Transform kamera)
    {
        Vector2 normalizeGirdi = Vector2.ClampMagnitude(girdi, 1f);
        if (kamera == null)
            return new Vector3(normalizeGirdi.x, 0f, normalizeGirdi.y);

        Vector3 ileri = kamera.forward;
        Vector3 sag = kamera.right;
        ileri.y = 0f;
        sag.y = 0f;

        // Kamera tam tepeden bakıyorsa (pitch ~90 derece) forward.y = -1 olur ve yatay düzlemde ileri (0,0,0) kalır.
        // Bu durumda kameranın yukarı vektörü (kamera.up) ekrandaki ileri yönümüzdür (+Z).
        if (ileri.sqrMagnitude < 0.01f)
        {
            ileri = kamera.up;
            ileri.y = 0f;
            if (ileri.sqrMagnitude < 0.01f) ileri = Vector3.forward;
        }

        if (sag.sqrMagnitude < 0.01f)
        {
            sag = Vector3.right;
        }

        ileri.Normalize();
        sag.Normalize();

        return ileri * normalizeGirdi.y + sag * normalizeGirdi.x;
    }
}
