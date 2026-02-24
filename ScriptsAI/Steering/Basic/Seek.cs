using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : SteeringBehaviour
{

    public Agent target;

    
    void Start()
    {
        this.nameSteering = "Seek";
    }


    public override Steering GetSteering(AgentNPC agent)
    {
        Steering steer = new Steering();

        // 1. Vector desde agent hasta target
        Vector3 direction = target.Position - agent.Position;

        // 2. Normalizar el vector y multiplicarlo por la máxima velocidad del agente
        direction = direction.normalized * agent.MaxAcceleration;

        steer.linear = direction;
        steer.angular = 0;

        // Retornamos el resultado final.
        return steer;
    }
}