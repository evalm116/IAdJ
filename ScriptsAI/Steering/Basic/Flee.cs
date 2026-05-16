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

        Vector3 direction = agent.Position - target.Position;
        float distance = direction.magnitude;

        if (distance < agent.InteriorRadius)
        {
            agent.Velocity = Vector3.zero;
            return steer;
        }

        float targetSpeed;
        if (distance <= agent.InteriorRadius)
            targetSpeed = agent.MaxSpeed;
        else if (distance <= agent.ArrivalRadius)
            targetSpeed = agent.MaxSpeed * (distance / agent.InteriorRadius);
        else
            targetSpeed = 0;

            Vector3 desiredVelocity = direction.normalized * targetSpeed;
        steer.linear = desiredVelocity - agent.Velocity;
        steer.angular = 0;

        return steer;
    }
}