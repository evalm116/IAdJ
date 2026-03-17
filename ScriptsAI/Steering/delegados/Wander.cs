using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : Face
{
    public float wanderRadius = 3f;
    public float wanderOffset = 4f;
    // Ahora wanderRate es "cuántos metros se mueve el punto por segundo"
    public float wanderRate = 10f; 
    public float maxAcceleration = 5f;

    private Agent wanderTarget;
    
    // GUARDAMOS LA POSICIÓN LOCAL DEL PUNTO EN EL CÍRCULO
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

        // 1. MOVER EL PUNTO ALEATORIAMENTE (Jitter)
        // Le sumamos un vector aleatorio 3D pequeño cada frame
        targetLocalPosition += new Vector3(
            Random.Range(-1f, 1f), 
            0, 
            Random.Range(-1f, 1f)
        ) * wanderRate * Time.deltaTime;

        // 2. DEVOLVERLO AL BORDE DEL CÍRCULO
        // Al normalizarlo y multiplicar por el radio, lo obligamos a no salirse de la línea verde
        targetLocalPosition = targetLocalPosition.normalized * wanderRadius;

        // 3. EMPUJARLO HACIA ADELANTE (El palo de la zanahoria)
        Vector3 localTargetWithOffset = targetLocalPosition + new Vector3(0, 0, wanderOffset);

        // 4. CONVERTIR AL MUNDO REAL Y DELEGAR EN FACE
        wanderTarget.Position = character.transform.TransformPoint(localTargetWithOffset);
        this.target = wanderTarget;
        
        Steering steer = base.GetSteering(character);

        // 5. ACELERAR HACIA EL PUNTO
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