using UnityEngine;

public class MouseController : MonoBehaviour
{
    [Header("Referencias")]
    public FormationManager miGestor; // Arrastra tu Gestor_Escuadron aquí
    public Agent liderFormacion;      // El Agent de tu Gestor_Escuadron

    [Header("Configuración Raycast")]
    public LayerMask capaSuelo; // Para diferenciar el suelo de los tanques

    void Update()
    {
// --- CLIC IZQUIERDO: AÑADIR/QUITAR DE FORMACIÓN ---
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ¡EL CHIVATO! Esto nos dirá qué objeto exacto está tocando el ratón
                Debug.Log("Clic izquierdo ha tocado el objeto: " + hit.collider.gameObject.name);

                AgentNPC tanqueTocado = hit.collider.GetComponentInParent<AgentNPC>();
                
                if (tanqueTocado != null)
                {
                    if (miGestor.IsInFormation(tanqueTocado))
                    {
                        miGestor.RemoveCharacter(tanqueTocado);
                        Debug.Log("Tanque sacado de la formación: " + tanqueTocado.name);
                    }
                    else
                    {
                        miGestor.AddCharacter(tanqueTocado);
                        Debug.Log("Tanque añadido a la formación: " + tanqueTocado.name);
                    }
                }
            }
        }

        // CLIC DERECHO MOVER LA FORMACIÓN 
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            // Aquí usamos capaSuelo para asegurarnos de que solo hacemos clic en el terreno
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, capaSuelo))
            {
                if (liderFormacion != null)
                {
                    // Movemos al Líder a ese punto del mapa

                    liderFormacion.Position = hit.point; 
                    Debug.Log("Moviendo formación a: " + hit.point);
                }
            }
        }
    }
}