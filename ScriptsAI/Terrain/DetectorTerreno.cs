using UnityEngine;

public class DetectorTerreno : MonoBehaviour
{
    public static TipoTerreno DetectarTerreno(Transform transform, float distanciaDeteccion, LayerMask capasTerreno)
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, distanciaDeteccion, capasTerreno))
        {
            TerrenoInfo nuevoTerreno = hit.collider.GetComponent<TerrenoInfo>();

            if (nuevoTerreno != null)
            {
                return nuevoTerreno.TerrainType;
            }
        }

        return TipoTerreno.Plain; // Fallback a terreno plano si no se detecta nada
    }
}