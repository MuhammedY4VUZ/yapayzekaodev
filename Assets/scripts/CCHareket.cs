using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3. Hafta: CharacterController ile hareket eden karakter betiği.
/// Eğim sınırı (Slope Limit), zıplama, yerçekimi ve kameraya göre yönelim.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CCHareket : MonoBehaviour
{
    public Transform kamera;
    public float yurumeHizi = 5f;
    public float kosmaHizi = 9f;
    public float ziplamaYuksekligi = 1.5f;
    public float yercekimi = -20f;
    public float donusHizi = 720f;

    CharacterController cc;
    InputAction move, jump, sprint;
    float dikeyHiz;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        move = InputSystem.actions.FindAction("Player/Move");
        jump = InputSystem.actions.FindAction("Player/Jump");
        sprint = InputSystem.actions.FindAction("Player/Sprint");
    }

    void Update()
    {
        float dt = Time.deltaTime;
        Vector2 girdi = move != null ? move.ReadValue<Vector2>() : Vector2.zero;
        Vector3 istenenYon = Hareket.IstenenYon(girdi, kamera);

        float hiz = (sprint != null && sprint.IsPressed()) ? kosmaHizi : yurumeHizi;
        Vector3 yatayHareket = istenenYon * hiz;

        if (cc.isGrounded)
        {
            dikeyHiz = -2f; // Zemine yapışması için hafif negatif hız
            if (jump != null && jump.WasPressedThisFrame())
                dikeyHiz = Mathf.Sqrt(2f * -yercekimi * ziplamaYuksekligi);
        }
        else
        {
            dikeyHiz += yercekimi * dt;
        }

        Vector3 toplamHareket = yatayHareket + Vector3.up * dikeyHiz;
        cc.Move(toplamHareket * dt);

        if (istenenYon.sqrMagnitude > 0.001f)
        {
            Quaternion hedef = Quaternion.LookRotation(istenenYon);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, hedef, donusHizi * dt);
        }
    }
}
