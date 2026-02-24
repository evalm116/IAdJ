using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : Face
{
    public float wanderRadius = 5f;
    public float wanderOffset = 5f;
    public float wanderRate = 1f;
    public float wanderOrientation = 0f;
    public float maxAcceleration = 5f;

    private Agent wanderTarget;

    void Start()
    {
        // Configuramos el nombre para el debug
        this.nameSteering = "Wander";

        // 1. CREAMOS EL FANTASMA (DUMMY)
        // Como tu Face modifica el target, necesitamos que el target sea este fantasma
        // para no fastidiar a nadie más.
        GameObject tempObj = new GameObject("Dummy_Wander");
        tempObj.transform.parent = this.transform; 
        wanderTarget = tempObj.AddComponent<Agent>();
        
        // Configuración inicial segura del fantasma
        wanderTarget.Position = transform.position;
        wanderTarget.Orientation = 0;
        
        // Le damos un ángulo aleatorio inicial
        wanderOrientation = Random.Range(0f, 360f);
    }

    void OnDestroy()
    {
        if (wanderTarget != null) Destroy(wanderTarget.gameObject);
    }

    public override Steering GetSteering(AgentNPC character)
    {
        // 1. CALCULAMOS EL DESPLAZAMIENTO ALEATORIO (Jitter)
        float randomJitter = Random.Range(-1f, 1f) * wanderRate * Time.deltaTime;
        wanderOrientation += randomJitter;

        // 2. CALCULAMOS EL CENTRO DEL CÍRCULO
        // El círculo está 'wanderOffset' metros delante del personaje
        Vector3 circleCenter = character.Position + (character.transform.forward * wanderOffset);

        // 3. CALCULAMOS LA POSICIÓN EN EL BORDE DEL CÍRCULO
        // Convertimos el ángulo a coordenadas (X, Z)
        // Sumamos la orientación del personaje para que el círculo gire con él
        float targetAngle = character.Orientation + wanderOrientation;
        
        Vector3 direction = new Vector3(Mathf.Sin(targetAngle * Mathf.Deg2Rad), 0, Mathf.Cos(targetAngle * Mathf.Deg2Rad));
        
        // 4. MOVEMOS NUESTRO FANTASMA A ESA POSICIÓN
        wanderTarget.Position = circleCenter + (direction * wanderRadius);

        // Tu script Face calculará la rotación necesaria para mirar a este fantasma
        // y se la aplicará a la orientación del fantasma (lo cual nos da igual) 
        // y luego llamará a Align.
        this.target = wanderTarget;

        // 6. LLAMAMOS A FACE 
        Steering steer = base.GetSteering(character);

        this.target = wanderTarget;
        
        // 7. AÑADIMOS MOVIMIENTO HACIA ADELANTE
        steer.linear = character.transform.forward * character.MaxAcceleration;

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
                Gizmos.DrawWireSphere(circleCenter, wanderRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(wanderTarget.Position, 0.2f);
            }
        }
    }
}


