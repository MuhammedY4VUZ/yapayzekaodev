using UnityEngine;

/// <summary>
/// 3. Hafta: İstenen yön hesabını yapan ortak statik yardımcı sınıf.
/// Giriş vektörünün boyunu 1 ile sınırlar ve kameranın yatay yönüne göre yönü hesaplar.
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
        ileri.Normalize();
        sag.Normalize();

        return ileri * normalizeGirdi.y + sag * normalizeGirdi.x;
    }
}
