using UnityEngine;

public class MouseController : MonoBehaviour
{
    [Header("Referencias")]
    public FormationManager miGestor; // Arrastra tu Gestor_Escuadron aquí
    public Agent liderFormacion;      // El Agent de tu Gestor_Escuadron

    [Header("Configuración Raycast")]
    public LayerMask capaSuelo; // Para diferenciar el suelo de los tanques

    public Agent destinoLider; // Este es el "fantasma" que el líder seguirá, y la formación con él

    void Update()
    {
        // --- CLIC IZQUIERDO: AÑADIR/QUITAR DE FORMACIÓN ---
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Esto nos dirá qué objeto exacto está tocando el ratón
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
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, capaSuelo))
            {
                if (destinoLider != null)
                {
                    // Movemos la "zanahoria" al punto del clic. 
                    // El Líder usará su Arrive para ir hacia ella, y la formación le seguirá.
                    destinoLider.Position = hit.point; 
                }
            }
        }
    }
}