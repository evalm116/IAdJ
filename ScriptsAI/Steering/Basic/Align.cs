using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align : SteeringBehaviour
{

    // Configuración (valores recomendados basados en tus imágenes)
    public float timeToTarget = 0.1f;// Tiempo para alcanzar la velocidad deseada

    void Start()
    {
        this.nameSteering = "Align";
    }

    public override Steering GetSteering(AgentNPC agent)
    {
        Steering steer = new Steering();
        if (target == null) return steer;

        // --- PASO 1: Calcular la dirección "ingenua" (Naive direction) ---
        // Slide Imagen 1: rotation = target.orientation - character.orientation
        float rotation = target.Orientation - agent.Orientation;

        // --- PASO 2: Mapear al intervalo (-pi, pi) o (-180, 180) ---
        // Slide Imagen 1: rotation = mapToRange(rotation)
        while (rotation > 180) rotation -= 360;
        while (rotation < -180) rotation += 360;

        float rotationSize = Mathf.Abs(rotation);

        // --- PASO 3: Comprobar si hemos llegado (Check if we are there) ---
        // Slide Imagen 1: if rotationSize < targetRadius return None
        if (rotationSize < agent.InteriorRadius)
        {
            // Opcional: Matar la velocidad residual para que no vibre
            agent.Rotation = 0;
            return steer; // Devuelve steering con todo a 0
        }

        // --- PASO 4: Calcular velocidad deseada (Target Rotation Speed) ---
        float targetRotationSpeed;

        // Slide Imagen 2: if rotationSize > slowRadius -> maxRotation
        if (rotationSize > agent.ArrivalRadius)
        {
            targetRotationSpeed = agent.MaxRotation;
        }
        else
        {
            // Slide Imagen 2: else -> maxRotation * rotationSize / slowRadius
            // (Regla de tres para frenar poco a poco)
            targetRotationSpeed = agent.MaxRotation * (rotationSize / agent.ArrivalRadius);
        }

        // --- PASO 5: Restaurar el signo ---
        // Slide Imagen 2: targetRotation *= rotation / rotationSize
        targetRotationSpeed *= rotation / rotationSize;

        // --- PASO 6: Calcular Aceleración Angular ---
        // Slide Imagen 2: steering.angular = targetRotation - character.rotation
        // (Aquí character.rotation es tu velocidad angular actual)
        steer.angular = targetRotationSpeed - agent.Rotation;

        // Slide Imagen 2: steering.angular /= timeToTarget
        steer.angular /= timeToTarget;

        // --- PASO 7: Limitar Aceleración (Clamp) ---
        float angularAcceleration = Mathf.Abs(steer.angular);

        // Slide Imagen 2: if angularAcceleration > maxAngularAcceleration...
        if (angularAcceleration > agent.MaxAngularAcc)
        {
            steer.angular /= angularAcceleration;
            steer.angular *= agent.MaxAngularAcc;
        }

        // --- PASO 8: Salida ---
        // Slide Imagen 2: steering.linear = 0
        steer.linear = Vector3.zero;

        return steer;
    }
}