using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityMatching : SteeringBehaviour
{
    // Variable para el objetivo (target)

    // Variable timeToTarget = 0.1 (como en la imagen)
    public float timeToTarget = 0.1f;

    void Start()
    {
        this.nameSteering = "Velocity Matching";
    }

    public override Steering GetSteering(AgentNPC agent)
    {
        // # Create the structure to hold our output
        Steering steer = new Steering();

        if (target == null) return steer;

        // --- PASO 1: Calcular Aceleración ---
        // Imagen: steering.linear = target.velocity - character.velocity
        steer.linear = target.Velocity - agent.Velocity;

        // Imagen: steering.linear /= timeToTarget
        steer.linear /= timeToTarget;

        // --- PASO 2: Limitar Aceleración ---
        // Imagen: if steering.linear.length() > maxAcceleration
        if (steer.linear.magnitude > agent.MaxAcceleration)
        {
            // Imagen: steering.linear.normalize()
            // Imagen: steering.linear *= maxAcceleration
            steer.linear = steer.linear.normalized * agent.MaxAcceleration;
        }

        // --- PASO 3: Salida ---
        // Imagen: steering.angular = 0
        steer.angular = 0;

        return steer;
    }
}