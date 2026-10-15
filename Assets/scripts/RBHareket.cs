using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3. Hafta: Rigidbody ile fizik motoruna dayalı hareket betiği.
/// İvme sınırı (maksIvme), SphereCast ile zemin kontrolü, FixedUpdate senkronizasyonu.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RBHareket : MonoBehaviour
{
    public Transform kamera;
    public float yurumeHizi = 5f;
    public float kosmaHizi = 9f;
    public float maksIvme = 25f;
    public float ziplamaYuksekligi = 1.5f;
    public float donusHizi = 720f;
    public LayerMask zemin = 1;

    Rigidbody rb;
    InputAction move, jump, sprint;
    Vector3 istenenYon;
    float hiz;
    bool ziplaIstegi;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        move = InputSystem.actions.FindAction("Player/Move");
        jump = InputSystem.actions.FindAction("Player/Jump");
        sprint = InputSystem.actions.FindAction("Player/Sprint");
    }

    void Update()
    {
        Vector2 girdi = move != null ? move.ReadValue<Vector2>() : Vector2.zero;
        istenenYon = Hareket.IstenenYon(girdi, kamera);
        hiz = (sprint != null && sprint.IsPressed()) ? kosmaHizi : yurumeHizi;
        if (jump != null && jump.WasPressedThisFrame()) ziplaIstegi = true;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 v = rb.linearVelocity;
        Vector3 yatay = new Vector3(v.x, 0f, v.z);
        Vector3 fark = istenenYon * hiz - yatay;

        rb.AddForce(Vector3.ClampMagnitude(fark, maksIvme * dt), ForceMode.VelocityChange);

        if (ziplaIstegi && YerdeMi())
        {
            float v0 = Mathf.Sqrt(2f * -Physics.gravity.y * ziplamaYuksekligi);
            rb.AddForce(Vector3.up * (v0 - v.y), ForceMode.VelocityChange);
        }
        ziplaIstegi = false;

        if (istenenYon.sqrMagnitude > 0.001f)
        {
            Quaternion hedef = Quaternion.LookRotation(istenenYon);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, hedef, donusHizi * dt));
        }
    }

    bool YerdeMi() =>
        Physics.SphereCast(transform.position, 0.45f, Vector3.down, out _, 0.6f, zemin, QueryTriggerInteraction.Ignore);
}
