using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 4. Hafta: C tuşu ile 3 farklı kamera modu arasında geçiş yapar:
/// 1. Kuşbakışı: Labirentin ve yarışın tepeden görünümü.
/// 2. Oyuncu_CC Takip: Kullanıcının WASD ile yönettiği karakteri arkadan takip eden mod.
/// 3. Yapay Zeka Takip: Kendi A* yol bulucumuz (Oyuncu_AStar) veya NavAgent'ı takip eden mod.
/// </summary>
public class CameraModes : MonoBehaviour
{
    public enum CameraMode
    {
        KusBakisi = 0,
        OyuncuCCTakip = 1,
        YapayZekaTakip = 2
    }

    [Header("Kamera Modu")]
    public CameraMode currentMode = CameraMode.OyuncuCCTakip;

    [Header("Hedefler")]
    public Transform oyuncuCC;
    public Transform targetAStar;
    public MazeGenerator maze;

    [Header("Takip Mesafeleri")]
    public Vector3 ccFollowOffset = new(0f, 5f, -7f);
    public Vector3 astarFollowOffset = new(0f, 9f, -7f);
    public float smoothing = 6f;
    public float topHeight = 70f;

    // Geriye dönük uyumluluk özellikleri
    public Transform target
    {
        get => targetAStar;
        set => targetAStar = value;
    }
    public Vector3 followOffset
    {
        get => astarFollowOffset;
        set => astarFollowOffset = value;
    }
    public bool top
    {
        get => currentMode == CameraMode.KusBakisi;
        set => currentMode = value ? CameraMode.KusBakisi : CameraMode.OyuncuCCTakip;
    }

    void Awake()
    {
        var oldTk = GetComponent<TakipKamerasi>();
        if (oldTk != null) oldTk.enabled = false;
    }

    void Start()
    {
        FindReferences();
        SnapToCurrentMode();
    }

    public void FindReferences()
    {
        if (maze == null) maze = Object.FindFirstObjectByType<MazeGenerator>();

        if (oyuncuCC == null)
        {
            var ccObj = GameObject.Find("Oyuncu_CC");
            if (ccObj != null) oyuncuCC = ccObj.transform;
        }

        if (targetAStar == null)
        {
            var astarObj = GameObject.Find("Oyuncu_AStar");
            if (astarObj != null) targetAStar = astarObj.transform;
            else
            {
                var navObj = GameObject.Find("NavAgent");
                if (navObj != null) targetAStar = navObj.transform;
            }
        }
    }

    public void SnapToCurrentMode()
    {
        FindReferences();

        Vector3 wantedPos;
        Quaternion wantedRot;
        GetTargetPose(out wantedPos, out wantedRot);

        transform.position = wantedPos;
        transform.rotation = wantedRot;
    }

    void LateUpdate()
    {
        FindReferences();

        // [C] Tuşu ile 3 kamera modu arasında döngüsel geçiş
        bool cPressed = false;
        if (Keyboard.current != null)
        {
            cPressed = Keyboard.current.cKey.wasPressedThisFrame;
        }

        if (cPressed)
        {
            currentMode = (CameraMode)(((int)currentMode + 1) % 3);
            Debug.Log($"📷 Kamera Modu Değişti: {currentMode}");
        }

        Vector3 wantedPos;
        Quaternion wantedRot;
        GetTargetPose(out wantedPos, out wantedRot);

        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, wantedPos, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, wantedRot, t);
    }

    void GetTargetPose(out Vector3 wantedPos, out Quaternion wantedRot)
    {
        switch (currentMode)
        {
            case CameraMode.KusBakisi:
            default:
                Vector2Int center = (maze != null) ? new Vector2Int(maze.width / 2, maze.depth / 2) : new Vector2Int(15, 15);
                Vector3 centerPos = (maze != null) ? maze.CellToWorld(center) : transform.position;
                wantedPos = centerPos + Vector3.up * topHeight;
                wantedRot = Quaternion.Euler(90f, 0f, 0f);
                break;

            case CameraMode.OyuncuCCTakip:
                if (oyuncuCC != null)
                {
                    wantedPos = oyuncuCC.position + ccFollowOffset;
                    wantedRot = Quaternion.LookRotation((oyuncuCC.position + Vector3.up * 1.5f) - wantedPos);
                }
                else
                {
                    wantedPos = transform.position;
                    wantedRot = transform.rotation;
                }
                break;

            case CameraMode.YapayZekaTakip:
                if (targetAStar != null)
                {
                    wantedPos = targetAStar.position + astarFollowOffset;
                    wantedRot = Quaternion.LookRotation((targetAStar.position + Vector3.up * 1f) - wantedPos);
                }
                else
                {
                    wantedPos = transform.position;
                    wantedRot = transform.rotation;
                }
                break;
        }
    }
}
