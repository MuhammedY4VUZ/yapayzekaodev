using UnityEngine;

/// <summary>
/// 3. Hafta: Arazi yüksekliğini, yüzey normalini ve eğim açısını sorgulayan sonda.
/// </summary>
public class AraziSondasi : MonoBehaviour
{
    public float yurunebilirEgim = 45f;
    Terrain arazi;

    void Start() => arazi = Terrain.activeTerrain;

    public float ZeminYuksekligi(Vector3 p) =>
        arazi != null ? arazi.SampleHeight(p) + arazi.GetPosition().y : p.y;

    public Vector3 ZeminNormali(Vector3 p)
    {
        if (arazi == null) return Vector3.up;
        Vector3 yerel = p - arazi.GetPosition();
        Vector3 boyut = arazi.terrainData.size;
        return arazi.terrainData.GetInterpolatedNormal(yerel.x / boyut.x, yerel.z / boyut.z);
    }

    public bool Yurunebilir(Vector3 p)
    {
        float egim = Mathf.Acos(Mathf.Clamp01(ZeminNormali(p).y)) * Mathf.Rad2Deg;
        return egim <= yurunebilirEgim;
    }

    void OnDrawGizmos()
    {
        if (arazi == null) arazi = Terrain.activeTerrain;
        if (arazi == null) return;

        Vector3 p = transform.position;
        p.y = ZeminYuksekligi(p);
        Gizmos.color = Yurunebilir(p) ? Color.green : Color.red;
        Gizmos.DrawLine(p, p + ZeminNormali(p) * 3f);
    }
}
