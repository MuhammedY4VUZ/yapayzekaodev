using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3. ve 4. Hafta: CharacterController ile hareket eden karakter betiği.
/// Eğim sınırı (Slope Limit), zıplama, yerçekimi ve kameraya göre yönelim.
/// Kuşbakışı ve takip modlarında kusursuz 4 yönlü WASD kontrolü sağlar.
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
        if (InputSystem.actions != null)
        {
            move = InputSystem.actions.FindAction("Player/Move");
            jump = InputSystem.actions.FindAction("Player/Jump");
            sprint = InputSystem.actions.FindAction("Player/Sprint");
        }
    }

    void Start()
    {
        if (kamera == null && Camera.main != null)
            kamera = Camera.main.transform;
    }

    void Update()
    {
        if (kamera == null && Camera.main != null)
            kamera = Camera.main.transform;

        float dt = Time.deltaTime;

        // 1. Girdi Okuma (InputAction ve doğrudan klavye desteği)
        Vector2 girdi = Vector2.zero;
        if (move != null) girdi = move.ReadValue<Vector2>();

        if (girdi == Vector2.zero && Keyboard.current != null)
        {
            float x = 0f;
            float y = 0f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            girdi = new Vector2(x, y);
        }

        Vector3 istenenYon = Hareket.IstenenYon(girdi, kamera);

        // 2. Koşma & Yürüme
        bool isSprinting = (sprint != null && sprint.IsPressed()) ||
                           (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed);
        float hiz = isSprinting ? kosmaHizi : yurumeHizi;
        Vector3 yatayHareket = istenenYon * hiz;

        // 3. Yerçekimi & Zıplama
        bool isJumpPressed = (jump != null && jump.WasPressedThisFrame()) ||
                             (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

        if (cc.isGrounded)
        {
            dikeyHiz = -2f; // Zemine yapışması için hafif negatif hız
            if (isJumpPressed)
                dikeyHiz = Mathf.Sqrt(2f * -yercekimi * ziplamaYuksekligi);
        }
        else
        {
            dikeyHiz += yercekimi * dt;
        }

        Vector3 toplamHareket = yatayHareket + Vector3.up * dikeyHiz;
        cc.Move(toplamHareket * dt);

        // 4. Dönüş
        if (istenenYon.sqrMagnitude > 0.001f)
        {
            Quaternion hedef = Quaternion.LookRotation(istenenYon);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, hedef, donusHizi * dt);
        }
    }
}
