using UnityEngine;

public class DetectorTerreno : MonoBehaviour
{
    [Header("Configuraci�n")]
    public float distanciaDeteccion = 1f;
    public LayerMask capasTerreno; // filtra solo los terrenos

    private TerrenoInfo terrenoActual;

    void Update()
    {
        DetectarTerreno();
    }

    void DetectarTerreno()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, distanciaDeteccion, capasTerreno))
        {
            TerrenoInfo nuevoTerreno = hit.collider.GetComponent<TerrenoInfo>();

            if (nuevoTerreno != null && nuevoTerreno != terrenoActual)
            {
                terrenoActual = nuevoTerreno;
                OnCambioTerreno(terrenoActual);
            }
        }
    }

    void OnCambioTerreno(TerrenoInfo terreno)
    {
        Debug.Log($"Cambiaste a terreno: {terreno.TerrainType}");
    }

    public TerrenoInfo GetTerrenoActual()
    {
        return terrenoActual;
    }

    public TipoTerreno GetTipoTerrenoActual()
    {
        return terrenoActual != null ? terrenoActual.TerrainType : TipoTerreno.Plain;
    }
}