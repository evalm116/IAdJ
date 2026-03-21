using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : Face
{
    public float wanderRadius = 3f;
    public float wanderOffset = 4f;
    public float wanderRate = 10f; 
    public float maxAcceleration = 5f;

    private Agent wanderTarget;
    
    // El punto de destino del Wander se mueve dentro de un círculo imaginario delante del agente. 
    // Este vector almacena la posición local de ese punto, que luego convertiremos a coordenadas globales.
    private Vector3 targetLocalPosition;

    public bool isWandering = true; // Interruptor para activar/desactivar el Wander 
    
    void Awake()
    {
        this.nameSteering = "Wander";

        GameObject tempObj = new GameObject("Dummy_Wander");
        tempObj.transform.parent = this.transform; 
        wanderTarget = tempObj.AddComponent<Agent>();
        
        // Inicializamos el punto en un lugar aleatorio del borde del círculo
        Vector2 rand = Random.insideUnitCircle.normalized * wanderRadius;
        targetLocalPosition = new Vector3(rand.x, 0, rand.y);
    }

    void OnDestroy()
    {
        if (wanderTarget != null) Destroy(wanderTarget.gameObject);
    }

    public override Steering GetSteering(AgentNPC character)
    {
        // Si el interruptor está apagado, devolvemos 0 fuerza
        if (!isWandering || wanderTarget == null) 
        {
            Steering stopSteer = new Steering();
            stopSteer.linear = Vector3.zero;
            stopSteer.angular = 0f;
            return stopSteer;
        }

        // MOVER EL PUNTO ALEATORIAMENTE (Jitter)
        // Le sumamos un vector aleatorio 3D pequeño cada frame
        targetLocalPosition += new Vector3(
            Random.Range(-1f, 1f), 
            0, 
            Random.Range(-1f, 1f)
        ) * wanderRate * Time.deltaTime;

        // Al normalizarlo y multiplicar por el radio, lo obligamos a no salirse de la línea verde
        targetLocalPosition = targetLocalPosition.normalized * wanderRadius;

        // EMPUJARLO HACIA ADELANTE (Offset)
        // Le sumamos un vector hacia adelante para que el círculo no esté centrado en el personaje, sino delante de él
        Vector3 localTargetWithOffset = targetLocalPosition + new Vector3(0, 0, wanderOffset);

        // Convertimos la posición local del punto a global para que el Face pueda usarlo como target
        wanderTarget.Position = character.transform.TransformPoint(localTargetWithOffset);
        this.target = wanderTarget;
        
        Steering steer = base.GetSteering(character);

        // El steering de Face nos da la rotación necesaria para mirar al punto, 
        // pero el Wander también necesita una fuerza lineal que empuje al personaje hacia ese punto.
        Vector3 directionToTarget = (wanderTarget.Position - character.Position).normalized;
        steer.linear = directionToTarget * character.MaxAcceleration;

        return steer;
    }

    void OnDrawGizmos()
    {
        if (wanderTarget != null)
        {
            AgentNPC agent = GetComponent<AgentNPC>();
            if (agent != null)
            {
                Vector3 circleCenter = agent.Position + (agent.transform.forward * wanderOffset);
                Gizmos.color = Color.green;
                // Dibujamos el círculo en el que vive el punto
                Gizmos.DrawWireSphere(circleCenter, wanderRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(wanderTarget.Position, 0.2f);
            }
        }
    }
}