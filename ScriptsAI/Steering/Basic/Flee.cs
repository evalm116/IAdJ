using UnityEngine;

public class Flee : SteeringBehaviour
{
    // Variable pública para asignar el objetivo del que huir

    void Start()
    {
        this.nameSteering = "Flee";
    }

    // Usamos AgentNPC
    public override Steering GetSteering(AgentNPC agent)
    {
        Steering steer = new Steering();

        if (target == null) return steer;

        // Huir: Vector DESDE el objetivo HACIA mí
        Vector3 direction = agent.Position - target.Position;

        direction.Normalize();
        steer.linear = direction * agent.MaxAcceleration;
        steer.angular = 0;

        return steer;
    }
}