using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    // VARIABLES DE LA FORMACIÓN 
    public List<SlotAssignment> slotAssignments = new List<SlotAssignment>();
    private Vector3 driftOffset;
    public FormationPattern pattern;
    public LayerMask capaObstaculos; 
    public float distanciaOffsetPared = 2f; 

    [Header("Tiempos de Formación")]
    public float tiempoParaFormar = 0.8f; 
    public float tiempoParaRomper = 0.5f; 

    private float tiempoQuieto = 0f;
    private float tiempoMoviendose = 0f;
    private bool enFormacion = true; 

    // VARIABLES DEL LÍDER
    [Header("Tiempos del Líder")]
    public float tiempoCaminandoWander = 5f; 
    public float tiempoParadoWander = 20f; 
    public float tiempoInactivoParaWander = 20f; 

    private float cronometroWander = 0f;
    private bool liderDePaseo = true;
    
    private bool modoManual = false; 
    private float cronometroManual = 0f;

    private AgentNPC agenteLider;
    private Wander wanderLider;
    private Arrive arriveLider;
    private Face faceLider;
    private Agent destinoOriginal;

    // Variable para controlar a los muros 
    private WallAvoidanceThreeRays wallAvoidanceLider;
    private float pesoOriginalMuros = 10f;


    // GESTIÓN DE LA FORMACIÓN

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

        if (pattern != null) driftOffset = pattern.GetDriftOffset(slotAssignments);
    }

    public bool AddCharacter(AgentNPC character)
    {
        int occupiedSlots = slotAssignments.Count;

        if (pattern != null && pattern.SupportsSlots(occupiedSlots + 1))
        {
            SlotAssignment newAssignment = new SlotAssignment();
            newAssignment.character = character;

            GameObject tempObj = new GameObject("Dummy_Formacion_" + character.name);
            Agent dummy = tempObj.AddComponent<Agent>();
            newAssignment.dummyTarget = dummy;

            Arrive arriveScript = character.GetComponent<Arrive>();
            if (arriveScript != null) { arriveScript.target = dummy; arriveScript.enabled = true; }
            
            Face faceScript = character.GetComponent<Face>();
            if (faceScript != null) { faceScript.target = dummy; faceScript.enabled = true; }

            foreach (Align a in character.GetComponents<Align>())
            {
                if (a.GetType() == typeof(Align)) { a.target = dummy; a.enabled = false; }
            }

            slotAssignments.Add(newAssignment);
            UpdateSlotAssignments();
            return true;
        }
        return false; 
    }

    public bool RemoveCharacter(AgentNPC character)
    {
        if (character == this.GetComponent<AgentNPC>()) return false;
        // Buscamos al personaje en las asignaciones
        int indexToRemove = -1;     
        for (int i = 0; i < slotAssignments.Count; i++)
        {
            if (slotAssignments[i].character == character) { indexToRemove = i; break; }
        }
        // Si lo encontramos, lo eliminamos y actualizamos las asignaciones
        if (indexToRemove != -1)
        {
            AgentNPC npcToLeave = slotAssignments[indexToRemove].character;
            Arrive arriveScript = npcToLeave.GetComponent<Arrive>();
            if (arriveScript != null) { arriveScript.target = null; arriveScript.enabled = false; }
            
            Face faceScript = npcToLeave.GetComponent<Face>();
            if (faceScript != null) { faceScript.target = null; faceScript.enabled = false; }

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


    // LÓGICA DE MOVIMIENTO

    void Start()
    {
        // elegimos el patrón
        pattern = new PatternMediaLuna(); 

        agenteLider = GetComponent<AgentNPC>();
        arriveLider = GetComponent<Arrive>();
        wallAvoidanceLider = GetComponent<WallAvoidanceThreeRays>(); // Buscamos el script de muros

        Face[] todosLosFaces = GetComponents<Face>();
        foreach (Face f in todosLosFaces)
        {
            if (f.GetType() == typeof(Face)) faceLider = f;
            if (f.GetType() == typeof(Wander)) wanderLider = (Wander)f;
        }
        // Asignamos el líder al slot 0 de la formación
        if (agenteLider != null)
        {
            SlotAssignment leaderSlot = new SlotAssignment();
            leaderSlot.character = agenteLider;
            leaderSlot.slotNumber = 0;
            slotAssignments.Add(leaderSlot);
        }
        // Guardamos el destino original del Arrive y el peso original de los muros para poder restaurarlos después
        if (arriveLider != null) destinoOriginal = arriveLider.target;
        if (wallAvoidanceLider != null) pesoOriginalMuros = wallAvoidanceLider.weight;

        // El AgentNPC se queda siempre encendido (Buena Práctica)
        if (agenteLider != null) agenteLider.enabled = true;

        ApagarManual();
        if (wanderLider != null) wanderLider.isWandering = true;
    }

    void Update()
    {
        if (pattern == null) return;

        // Clic Derecho para activar el control manual del líder
        if (Input.GetMouseButtonDown(1)) 
        {
            modoManual = true;
            cronometroManual = 0f; 
            
            // Le devolvemos el instinto de esquivar muros por si estaba apagado
            if (wallAvoidanceLider != null) wallAvoidanceLider.weight = pesoOriginalMuros;

            if (wanderLider != null) wanderLider.isWandering = false;
            ActivarManual();
        }

        // CONTROL DEL MOVIMIENTO DEL LÍDER

        if (modoManual)
        {
            if (arriveLider != null && arriveLider.target != null)
            {
                float distanciaAlDestino = Vector3.Distance(transform.position, arriveLider.target.Position);

                if (distanciaAlDestino < 1.0f) 
                {
                    // Ha llegado a la marca: Mantenemos velocidad a cero y apagamos los muros temporalmente para que no le arrastre 
                    if (agenteLider != null)
                    {
                        agenteLider.Velocity = Vector3.zero;
                        agenteLider.Rotation = 0f;
                    }
                    if (wallAvoidanceLider != null) wallAvoidanceLider.weight = 0f;
                    // Empezamos a contar el tiempo que lleva parado para volver al Wander
                    cronometroManual += Time.deltaTime;
                    
                    if (cronometroManual >= tiempoInactivoParaWander)
                    {
                        modoManual = false;
                        liderDePaseo = true; 
                        cronometroWander = 0f;

                        // Toca volver al Wander: Le devolvemos el instinto de los muros
                        if (wallAvoidanceLider != null) wallAvoidanceLider.weight = pesoOriginalMuros;
                        ApagarManual();
                        if (wanderLider != null) wanderLider.isWandering = true;
                    }
                }
                else
                {
                    cronometroManual = 0f; 
                }
            }
        }
        else
        {
            // MODO AUTOMÁTICO 
            cronometroWander += Time.deltaTime;

            if (liderDePaseo)
            {
                if (cronometroWander >= tiempoCaminandoWander)
                {
                    if (wanderLider != null) wanderLider.isWandering = false;

                    // Toca pararse: Clavamos frenos y apagamos los muros temporalmente
                    if (agenteLider != null)
                    {
                        agenteLider.Velocity = Vector3.zero; 
                        agenteLider.Rotation = 0f; 
                    }
                    if (wallAvoidanceLider != null) wallAvoidanceLider.weight = 0f;
                    
                    liderDePaseo = false;
                    cronometroWander = 0f;
                }
            }
            else
            {
                // Mantenemos los frenos pisados durante todo el descanso
                if (agenteLider != null)
                {
                    agenteLider.Velocity = Vector3.zero; 
                    agenteLider.Rotation = 0f; 
                }

                if (cronometroWander >= tiempoParadoWander)
                {
                    if (wanderLider != null) wanderLider.isWandering = true;
                    
                    // Toca caminar: Le devolvemos el instinto de los muros
                    if (wallAvoidanceLider != null) wallAvoidanceLider.weight = pesoOriginalMuros;
                    
                    liderDePaseo = true;
                    cronometroWander = 0f;
                }
            }
        }

        // LIDERFOLLOWING
        float velocidadLider = (agenteLider != null) ? agenteLider.Velocity.magnitude : 0f;

        // Si el líder se está moviendo, vamos acumulando tiempo moviéndonos. Si se para, reseteamos ese tiempo y empezamos a acumular tiempo quietos.
        if (velocidadLider > 0.1f)
        {
            tiempoQuieto = 0f; 
            tiempoMoviendose += Time.deltaTime; 

            if (tiempoMoviendose >= tiempoParaRomper) enFormacion = false; 
        }
        else
        {
            tiempoMoviendose = 0f; 
            tiempoQuieto += Time.deltaTime;

            if (tiempoQuieto >= tiempoParaFormar) enFormacion = true; 
        }
        // Si estamos en formación, actualizamos las posiciones de los soldados según el patrón. Si no, los juntamos todos en el punto del pelotón detrás del líder.
        if (enFormacion)
        {
            for (int i = 0; i < slotAssignments.Count; i++)
            {
                if (slotAssignments[i].dummyTarget == null) continue;

                SlotTransform slotData = pattern.GetSlotLocation(slotAssignments[i].slotNumber);
                Vector3 relativeLoc = slotData.position;
                
                float orientacionFinal = slotData.orientation + transform.eulerAngles.y;
                slotAssignments[i].dummyTarget.Orientation = orientacionFinal;
                slotAssignments[i].dummyTarget.transform.rotation = Quaternion.Euler(0, orientacionFinal, 0);

                Vector3 targetPosition = transform.TransformPoint(relativeLoc);
                Vector3 direccionAlTarget = targetPosition - transform.position;
                float distanciaAlTarget = direccionAlTarget.magnitude;

                if (Physics.Raycast(transform.position, direccionAlTarget.normalized, out RaycastHit hit, distanciaAlTarget, capaObstaculos))
                {
                    // empujamos hacia afuera de la pared
                    targetPosition = hit.point + (hit.normal * distanciaOffsetPared);

                    // Si el líder está justo al otro lado de la pared, no queremos empujar al soldado contra el líder, así que comprobamos la distancia al líder
                    float distanciaAlLider = Vector3.Distance(transform.position, targetPosition);
                    float radioSeguridadLider = 2.0f; // 2 metros de espacio personal 

                    // Si al empujarlo del muro, se nos ha quedado muy pegado al líder
                    if (distanciaAlLider < radioSeguridadLider)
                    {
                        // Calculamos la dirección desde el líder hacia el soldado
                        Vector3 direccionDesdeLider = (targetPosition - transform.position).normalized;
                        
                        // Lo mantenemos exactamente en el borde de la burbuja de seguridad
                        targetPosition = transform.position + (direccionDesdeLider * radioSeguridadLider);
                    }
                }

                slotAssignments[i].dummyTarget.Position = targetPosition;
                slotAssignments[i].dummyTarget.transform.position = targetPosition;
            }
        }
        else
        {   // Fuera de formación, todos los soldados se amontonan en el punto del pelotón detrás del líder
            Vector3 puntoPeloton = transform.position - (transform.forward * 2f);

            for (int i = 0; i < slotAssignments.Count; i++)
            {
                if (slotAssignments[i].dummyTarget == null) continue;

                slotAssignments[i].dummyTarget.Position = puntoPeloton;
                slotAssignments[i].dummyTarget.transform.position = puntoPeloton;
                slotAssignments[i].dummyTarget.Orientation = transform.eulerAngles.y;
            }
        }
    }


    // FUNCIONES DE CONTROL MANUAL

    // Al activar el control manual, le asignamos al Arrive y al Face del líder el destino original que tenía antes, 
    // para que se mueva hacia donde el jugador le indique. 
    private void ActivarManual()
    {
        if (arriveLider != null) { arriveLider.enabled = true; arriveLider.weight = 1f; arriveLider.target = destinoOriginal; }
        if (faceLider != null) { faceLider.enabled = true; faceLider.weight = 1f; faceLider.target = destinoOriginal; }
    }

    // Al apagar el control manual, le quitamos el destino al Arrive y al Face del líder y los apagamos para que no interfieran con el Wander.  
    private void ApagarManual()
    {
        if (arriveLider != null) { arriveLider.enabled = false; arriveLider.weight = 0f; arriveLider.target = null; }
        if (faceLider != null) { faceLider.enabled = false; faceLider.weight = 0f; faceLider.target = null; }
    }
}