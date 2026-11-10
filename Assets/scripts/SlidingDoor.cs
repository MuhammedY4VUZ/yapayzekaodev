using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// 6. Hafta: [K] tuşu ile açılıp kapanan sürgülü kapı.
/// Açıkken zemine gömülür ve NavMesh'i oymayı bırakır (Carve pasif).
/// </summary>
[RequireComponent(typeof(NavMeshObstacle))]
public class SlidingDoor : MonoBehaviour
{
    public float closedY = 1f;
    public float openY = -1.5f;
    public float speed = 3f;
    public bool startOpen = true;

    NavMeshObstacle obstacle;
    InputAction toggle;
    public bool isOpen { get; private set; }

    void Awake()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.carving = true;
        if (InputSystem.actions != null)
            toggle = InputSystem.actions.FindAction("ToggleDoor");

        isOpen = startOpen;
        SetY(isOpen ? openY : closedY);
        obstacle.enabled = !isOpen;
    }

    void Update()
    {
        bool kPressed = (toggle != null && toggle.WasPressedThisFrame()) ||
                        (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame);

        if (kPressed)
        {
            isOpen = !isOpen;
            obstacle.enabled = !isOpen;
            Debug.Log($"🚪 Kapı Durumu Değişti: {(isOpen ? "AÇIK (Geçilebilir)" : "KAPALI (Engellendi)")}");
        }

        float targetY = isOpen ? openY : closedY;
        SetY(Mathf.MoveTowards(transform.position.y, targetY, speed * Time.deltaTime));
    }

    void SetY(float y)
    {
        Vector3 p = transform.position;
        p.y = y;
        transform.position = p;
    }
}
