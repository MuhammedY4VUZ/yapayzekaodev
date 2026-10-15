using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3. Hafta: Kare hızından bağımsız yumuşatma (t = 1 - e^(-k*dt)) kullanan 3. kişi takip kamerası.
/// </summary>
public class TakipKamerasi : MonoBehaviour
{
    public Transform hedef;
    public float mesafe = 7f;
    public float odakYuksekligi = 0.8f;
    public float hassasiyet = 0.15f;
    public float minPitch = -20f, maksPitch = 60f;
    public float keskinlik = 10f;

    InputAction look;
    float yaw, pitch = 20f;

    void Awake()
    {
        look = InputSystem.actions.FindAction("Player/Look");
        if (hedef != null) yaw = hedef.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (hedef == null) return;

        Vector2 fark = look != null ? look.ReadValue<Vector2>() : Vector2.zero;
        yaw += fark.x * hassasiyet;
        pitch = Mathf.Clamp(pitch - fark.y * hassasiyet, minPitch, maksPitch);

        Quaternion donus = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 odak = hedef.position + Vector3.up * odakYuksekligi;
        Vector3 istenen = odak - donus * Vector3.forward * mesafe;

        float t = 1f - Mathf.Exp(-keskinlik * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, istenen, t);
        transform.LookAt(odak);
    }
}
