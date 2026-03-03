using UnityEngine;

public class Arrive : SteeringBehaviour
{

    void Start()
    {
        this.nameSteering = "Arrive";
    }

    // 2. Aquí usamos AgentNPC (ahora coincidirá con el padre)
    public override Steering GetSteering(AgentNPC agent)
    {
        Steering steer = new Steering();

        if (target == null) return steer;

        Vector3 direction = target.Position - agent.Position;
        float distance = direction.magnitude;

        if (distance < agent.InteriorRadius)
        {
            agent.Velocity = Vector3.zero;
            return steer;
        }

        float targetSpeed;
        if (distance > agent.ArrivalRadius)
            targetSpeed = agent.MaxSpeed;
        else
            targetSpeed = agent.MaxSpeed * (distance / agent.ArrivalRadius);

        Vector3 desiredVelocity = direction.normalized * targetSpeed;
        steer.linear = desiredVelocity - agent.Velocity;
        steer.angular = 0;

        return steer;
    }
}