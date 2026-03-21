using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [Header("Referencias")]
    public FormationManager miGestor;
    public Agent liderFormacion;
    public Agent destinoLider;

    [Header("Configuración Raycast")]
    public LayerMask capaSuelo;

    public Grid grid; // Referencia al Grid para convertir posiciones a celdas

    [Header("Lista de Seleccionados")]
    public List<AgentNPC> soldadosSeleccionados = new List<AgentNPC>();

    void Update()
    {
        // ---------------------------------------------------------
        // 1. DESCARTAR A TODOS (Atajo de teclado: Espacio)
        // ---------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Space))
        {
            soldadosSeleccionados.Clear();
            Debug.Log("Selección vaciada.");
        }

        // ---------------------------------------------------------
        // 2. SELECCIONAR / DESCARTAR INDIVIDUAL (Clic Izquierdo)
        // ---------------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                AgentNPC soldadoTocado = hit.collider.GetComponentInParent<AgentNPC>();

                // SOLO hacemos cosas si hemos tocado un soldado. 
                // Si tocamos el suelo o cualquier otra cosa, el código no hace nada.
                if (soldadoTocado != null)
                {
                    // Si NO pulsamos Control: Vaciamos y seleccionamos solo a este
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        soldadosSeleccionados.Clear();
                        soldadosSeleccionados.Add(soldadoTocado);
                        Debug.Log("Seleccionado ÚNICO: " + soldadoTocado.name);
                    }
                    // Si SÍ pulsamos Control: Lo añadimos o quitamos de la lista sin tocar a los demás
                    else
                    {
                        if (soldadosSeleccionados.Contains(soldadoTocado))
                        {
                            soldadosSeleccionados.Remove(soldadoTocado);
                            Debug.Log("Descartado: " + soldadoTocado.name);
                        }
                        else
                        {
                            soldadosSeleccionados.Add(soldadoTocado);
                            Debug.Log("Añadido: " + soldadoTocado.name);
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // 3. ORDEN: UNIRSE A LA FORMACIÓN (Tecla 'F') - Apartado H
        // ---------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.F) && soldadosSeleccionados.Count > 0)
        {
            foreach (AgentNPC soldado in soldadosSeleccionados)
            {
                // Evitamos meter al líder (él ya es el dueño de la formación)
                if (soldado.gameObject != liderFormacion.gameObject)
                {
                    // Si no está ya en la formación, lo añadimos
                    if (!miGestor.IsInFormation(soldado))
                    {
                        miGestor.AddCharacter(soldado);
                        Debug.Log(soldado.name + " recibe la orden de FORMACIÓN.");
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // 4. ORDEN: MOVER (Clic Derecho) - Apartado C
        // ---------------------------------------------------------
        if (Input.GetMouseButtonDown(1) && soldadosSeleccionados.Count > 0)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, capaSuelo))
            {
                bool liderEstaSeleccionado = false;
                List<AgentNPC> subordinados = new List<AgentNPC>();

                foreach (AgentNPC soldado in soldadosSeleccionados)
                {
                    if (soldado.gameObject == liderFormacion.gameObject)
                    {
                        liderEstaSeleccionado = true;
                    }
                    else
                    {
                        subordinados.Add(soldado);
                    }
                }

                // Movemos al líder (y a su formación) si estaba seleccionado
                if (liderEstaSeleccionado && destinoLider != null)
                {
                    destinoLider.Position = hit.point;
                    liderFormacion.GetComponentInParent<PathFindingNPCFollow>().SetUpObjective();
                    Debug.Log("Orden de mover al Líder (y su formación).");
                }

                // Los subordinados rompen filas y van al punto por libre formando un círculo
                if (subordinados.Count > 0)
                {
                    for (int i = 0; i < subordinados.Count; i++)
                    {
                        AgentNPC soldadoLibre = subordinados[i];

                        // Lo sacamos de la formación si estaba en ella
                        if (miGestor.IsInFormation(soldadoLibre))
                        {
                            miGestor.RemoveCharacter(soldadoLibre);
                        }

                        // Calculamos su posición en el círculo alrededor del clic
                        Vector3 destinoFinal = hit.point;
                        if (subordinados.Count > 1)
                        {
                            float angulo = i * (Mathf.PI * 2 / subordinados.Count);
                            float radio = 2f;
                            destinoFinal += new Vector3(Mathf.Cos(angulo) * radio, 0, Mathf.Sin(angulo) * radio);
                        }

                        // Asignamos el Arrive a ese punto
                        Arrive arrive = soldadoLibre.GetComponent<Arrive>();
                        if (arrive != null)
                        {
                            if (arrive.target == null)
                            {
                                GameObject obj = new GameObject("DestinoLibre_" + soldadoLibre.name);
                                arrive.target = obj.AddComponent<Agent>();
                            }
                            arrive.target.Position = destinoFinal;
                            arrive.enabled = true;

                            // Asignamos el Face para que mire hacia donde va
                            Face face = soldadoLibre.GetComponent<Face>();
                            if (face != null) { face.target = arrive.target; face.enabled = true; }
                        }
                    }
                }
            }
        }
    }
}