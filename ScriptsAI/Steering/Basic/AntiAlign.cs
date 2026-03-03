using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiAlign : SteeringBehaviour
{

    // Configuración para frenar el giro suavemente
    public float slowAngle = 20f;   // Empieza a frenar cuando falten 20 grados
    public float targetAngle = 1f;  // Se considera "alineado" si el error es menor a 1 grado
    public float timeToTarget = 0.1f; // Tiempo para alcanzar la rotación (suavizado)

    void Start()
    {
        this.nameSteering = "Anti-Align";
    }

    public override Steering GetSteering(AgentNPC agent)
    {
        Steering steer = new Steering();
        if (target == null) return steer;

        // 1. Calculamos la orientación que queremos (la del target)
        float rotation = (target.Orientation + 180) - agent.Orientation;

        // 2. Mapeamos el ángulo a (-180, 180) para girar por el lado más corto
        // Usamos la función MapToRange que ya tienes en Bodi.cs
        rotation = Bodi.MapToRange(rotation, Range.NegPiToPi);
        // Nota: Si en Bodi usas radianes, usa NegPiToPi. Si usas grados, usa una versión de (-180, 180).
        // Si tu Bodi trabaja en grados (que parece que sí), mejor hacemos esto manual para asegurar:
        while (rotation > 180) rotation -= 360;
        while (rotation < -180) rotation += 360;

        float rotationSize = Mathf.Abs(rotation);

        // 3. Si ya estamos alineados, paramos
        if (rotationSize < targetAngle)
        {
            agent.Rotation = 0; // Forzamos parada angular
            return steer;
        }

        // 4. Calculamos la rotación deseada (velocidad angular)
        float targetRotation;
        if (rotationSize > slowAngle)
        {
            targetRotation = agent.MaxRotation;
        }
        else
        {
            targetRotation = agent.MaxRotation * (rotationSize / slowAngle);
        }

        // Recuperamos el signo del giro
        targetRotation *= rotation / rotationSize;

        // 5. Calculamos la aceleración angular necesaria
        steer.angular = targetRotation - agent.Rotation;
        steer.angular /= timeToTarget;

        // 6. Limitamos la aceleración angular máxima
        float angularAcc = Mathf.Abs(steer.angular);
        if (angularAcc > agent.MaxAngularAcc)
        {
            steer.angular /= angularAcc;
            steer.angular *= agent.MaxAngularAcc;
        }

        steer.linear = Vector3.zero; // No nos movemos, solo rotamos
        return steer;
    }
}