using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 4. Hafta: Sağ tıklandığında duvara veya zemine ışın atıp duvar ekleyen veya kaldıran editör.
/// </summary>
public class WallEditor : MonoBehaviour
{
    public Camera cam;
    public MazeGenerator maze;
    public Transform agent;
    public Transform navAgent;
    public LayerMask editMask;

    void Update()
    {
        bool rightClicked = false;
        Vector2 mousePos = Vector2.zero;

        if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                rightClicked = true;
                mousePos = Mouse.current.position.ReadValue();
            }
        }

        if (!rightClicked) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (maze == null) maze = Object.FindFirstObjectByType<MazeGenerator>();
        if (maze == null) return;

        if (agent == null)
        {
            var aObj = GameObject.Find("Oyuncu_AStar");
            if (aObj != null) agent = aObj.transform;
        }
        if (navAgent == null)
        {
            var naObj = GameObject.Find("NavAgent");
            if (naObj != null) navAgent = naObj.transform;
        }

        int mask = editMask.value;
        if (mask == 0)
        {
            int gLayer = LayerMask.NameToLayer("Ground");
            int wLayer = LayerMask.NameToLayer("Wall");
            mask = ((gLayer >= 0) ? (1 << gLayer) : 0) | ((wLayer >= 0) ? (1 << wLayer) : 0);
            if (mask == 0) mask = ~0;
        }

        Ray ray = cam.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, mask)) return;

        bool hitWall = hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall");
        Vector3 p = hitWall ? hit.collider.transform.position : hit.point;
        Vector2Int cell = maze.WorldToCell(p);

        // Ajanların tam üstüne duvar örmeyi engelle
        if (agent != null && cell == maze.WorldToCell(agent.position)) return;
        if (navAgent != null && cell == maze.WorldToCell(navAgent.position)) return;

        maze.ToggleWall(cell);
    }
}
