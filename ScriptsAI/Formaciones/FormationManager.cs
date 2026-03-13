using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    // Lista de soldados y los huecos que ocupan
    public List<SlotAssignment> slotAssignments = new List<SlotAssignment>();
    
    // El desfase del centro de masas (para evitar derrapar)
    private Vector3 driftOffset;
    
    // El patrón que estamos usando (Línea, V, etc.)
    public FormationPattern pattern;

    // Referencia al mapa 
    public Grid gridManager;

    [Header("Ajuste de Obstáculos")]
    public LayerMask capaObstaculos; // Para detectar las pirámides y columnas
    public float distanciaOffsetPared = 1.5f; // Cuánto se separa el fantasma de la pared

    // Método para saber si un tanque ya está en esta formación
    public bool IsInFormation(AgentNPC character)
    {
        foreach (SlotAssignment slot in slotAssignments)
        {
            if (slot.character == character) return true;
        }
        return false;
    }


    private void UpdateSlotAssignments()
    {
        for (int i = 0; i < slotAssignments.Count; i++)
        {

            SlotAssignment temp = slotAssignments[i];
            temp.slotNumber = i; 
            slotAssignments[i] = temp; 
        }

        // Actualizamos el drift
        if (pattern != null)
        {
            driftOffset = pattern.GetDriftOffset(slotAssignments);
        }
    }

    public bool AddCharacter(AgentNPC character)
    {
        int occupiedSlots = slotAssignments.Count;

        // Comprobamos si en esta formación cabe uno más
        if (pattern != null && pattern.SupportsSlots(occupiedSlots + 1))
        {
            // Creamos el nuevo assignment y lo metemos en la lista
            SlotAssignment newAssignment = new SlotAssignment();
            newAssignment.character = character;

            // gestión del dummy target
            GameObject tempObj = new GameObject("Dummy_Formacion_" + character.name);
            Agent dummy = tempObj.AddComponent<Agent>();
            newAssignment.dummyTarget = dummy;

            // Buscamos el Arrive del soldado, le asignamos el fantasma y lo encendemos para no crear y destruir componentes cada vez que entran o salen de la formación
            Arrive arriveScript = character.GetComponent<Arrive>();
            if (arriveScript != null) {
                arriveScript.target = dummy;
                arriveScript.enabled = true; 
            }
            // le asignamos el fantasma al Face para que mire hacia él mientras está en formación
            Face faceScript = character.GetComponent<Face>();
            if (faceScript != null)
            {
                faceScript.target = dummy;
                faceScript.enabled = true;
            }

            slotAssignments.Add(newAssignment);
            
            // Recalculamos los números de hueco de todos
            UpdateSlotAssignments();
            return true;
        }
        return false; // No cabe o no hay patrón
    }


    public bool RemoveCharacter(AgentNPC character)
    {
        // 1. Buscamos en qué hueco está este personaje
        int indexToRemove = -1;     
        for (int i = 0; i < slotAssignments.Count; i++)
        {
            if (slotAssignments[i].character == character)
            {
                indexToRemove = i;
                break; 
            }
        }
        // si lo encontramos, lo quitamos y actualizamos los huecos de los demás
        if (indexToRemove != -1)
        {
            // gestion del Arrive y el dummy target al salir de la formación
            AgentNPC npcToLeave = slotAssignments[indexToRemove].character;
            Arrive arriveScript = npcToLeave.GetComponent<Arrive>();
            
            // Apagamos el Arrive y le quitamos el target
            if (arriveScript != null)
            {
                arriveScript.target = null;
                arriveScript.enabled = false; 
            }
            // Apagamos el Face y le quitamos el target
            Face faceScript = npcToLeave.GetComponent<Face>();
            if (faceScript != null)
            {
                faceScript.target = null;
                faceScript.enabled = false;
            }

            // Destruimos el fantasma para no dejar basura en la escena
            if (slotAssignments[indexToRemove].dummyTarget != null)
            {
                Destroy(slotAssignments[indexToRemove].dummyTarget.gameObject);
            }
            
            slotAssignments.RemoveAt(indexToRemove); 
            UpdateSlotAssignments(); 

            return true; 
        }
        return false; 
    }

    void Start()
    {
        pattern = new PatternV(); 

        // lider el hueco 0, el resto se asigna al añadirlos a la formación
        AgentNPC miPropioNPC = GetComponent<AgentNPC>();
        if (miPropioNPC != null)
        {
            SlotAssignment leaderSlot = new SlotAssignment();
            leaderSlot.character = miPropioNPC;
            leaderSlot.slotNumber = 0;
            // Al líder NO le creamos dummyTarget, porque él no persigue a nadie de la formación.
            slotAssignments.Add(leaderSlot);
        }
    }


    void Update()
    {
        // Si no hay patrón, no hacemos matemáticas
        if (pattern == null) return;

        // Recorremos cada soldado en la formación
        for (int i = 0; i < slotAssignments.Count; i++)
        {
            // Pedimos la posición local al patrón
            Vector3 relativeLoc = pattern.GetSlotLocation(slotAssignments[i].slotNumber);

            // TransformPoint: Convierte esa coordenada local a mundo real usando
            //    la rotación y posición del Líder (este GameObject llamado "anchor")
            Vector3 targetPosition = transform.TransformPoint(relativeLoc);

            // Calculamos la dirección y distancia desde el Líder hasta el hueco
            Vector3 direccionAlTarget = targetPosition - transform.position;
            float distanciaAlTarget = direccionAlTarget.magnitude;

            // Lanzamos un rayo desde el Líder hacia el hueco. 
            // Si se choca con un muro antes de llegar al sitio asignado...
            if (Physics.Raycast(transform.position, direccionAlTarget.normalized, out RaycastHit hit, distanciaAlTarget, capaObstaculos))
            {
                // Cogemos el punto exacto donde chocó el rayo con la pared,
                // y usamos hit.normal (que apunta hacia afuera de la pared) para aplicarle el Offset.
                targetPosition = hit.point + (hit.normal * distanciaOffsetPared);
            }

            // 4. Movemos el fantasma (Dummy) a esa coordenada real
            if (slotAssignments[i].dummyTarget != null)
            {
                slotAssignments[i].dummyTarget.Position = targetPosition;
            }

            // CONEXIÓN CON EL GRID 
            if (gridManager != null)
            {
                // Usamos el método para saber qué casilla es (X, Z)
                Vector2Int posGrid = gridManager.GetGridPosition(targetPosition);

                //Comprobamos que la coordenada está dentro de los límites del mapa 
                if (posGrid.x >= 0 && posGrid.x < gridManager.columnas && 
                    posGrid.y >= 0 && posGrid.y < gridManager.filas)
                {
                    // Obtenemos esa celda exacta de la matriz
                    GridCell celda = gridManager.gridArray[posGrid.x, posGrid.y];
                    
                    // Asignar el NPC a la celda
                    celda.isOccupied = true;
                    celda.occupant = slotAssignments[i].character.gameObject;
                }
            }

        }
    }
}