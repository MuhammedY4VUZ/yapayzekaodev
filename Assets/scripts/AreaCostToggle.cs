using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// 6. Hafta: [M] tuşu ile çamur maliyetini (1 -> 2 -> 5 -> 10),
/// [J] tuşu ile atlama bağlantısını (JumpLink) açıp kapatan deney kontrolcüsü.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AreaCostToggle : MonoBehaviour
{
    public NavMeshLink jumpLink;
    public string mudAreaName = "Camur";
    public float[] mudCosts = { 1f, 2f, 5f, 10f };

    public float CurrentMudCost => (costIndex >= 0 && costIndex < mudCosts.Length) ? mudCosts[costIndex] : 1f;
    public bool IsLinkActive => (jumpLink != null) && jumpLink.activated;

    NavMeshAgent agent;
    InputAction cycleMud, toggleLink;
    int mudArea = -1;
    int costIndex;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (InputSystem.actions != null)
        {
            cycleMud = InputSystem.actions.FindAction("CycleMudCost");
            toggleLink = InputSystem.actions.FindAction("ToggleLink");
        }
    }

    void Start()
    {
        if (jumpLink == null) jumpLink = Object.FindFirstObjectByType<NavMeshLink>();

        mudArea = NavMesh.GetAreaFromName(mudAreaName);
        if (mudArea < 0)
        {
            // Eğer "Camur" adlı alan bulunamadıysa User 3 alanını dene
            mudArea = 3;
        }

        ApplyMudCost();
    }

    void Update()
    {
        if (jumpLink == null) jumpLink = Object.FindFirstObjectByType<NavMeshLink>();

        // [M] Tuşu: Çamur maliyetini değiştir
        bool mPressed = (cycleMud != null && cycleMud.WasPressedThisFrame()) ||
                        (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame);

        if (mPressed)
        {
            costIndex = (costIndex + 1) % mudCosts.Length;
            ApplyMudCost();
        }

        // [J] Tuşu: Atlama bağlantısını aç / kapat
        bool jPressed = (toggleLink != null && toggleLink.WasPressedThisFrame()) ||
                        (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame);

        if (jPressed && jumpLink != null)
        {
            jumpLink.activated = !jumpLink.activated;
            Debug.Log($"🦘 Atlama Bağlantısı (JumpLink): {(jumpLink.activated ? "AÇIK" : "KAPALI")}");
        }
    }

    void ApplyMudCost()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null && mudArea >= 0)
        {
            float cost = mudCosts[costIndex];
            agent.SetAreaCost(mudArea, cost);
            Debug.Log($"🌊 Çamur Alanı Maliyeti Değişti: {cost} (Rota buna göre güncelleniyor)");
        }
    }
}
