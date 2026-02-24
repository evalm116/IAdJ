using UnityEngine;

public class Arrive : SteeringBehaviour
{
    // 1. Variable pública para que salga en Unity
    public Agent target; 
    
    public float slowRadius = 10f;
    public float targetRadius = 2f;

    void Start()
    {
        this.nameSteering = "Arrive";
    }

    // 2. Aquí usamos AgentNPC (ahora coincidirá con el padre)
    public override Steering GetSteering(AgentNPC agent)
    {
        Steering steer = new Steering();
        
        if(target == null) return steer;

        Vector3 direction = target.Position - agent.Position;
        float distance = direction.magnitude;

        if (distance < targetRadius)
        {
            agent.Velocity = Vector3.zero; 
            return steer;
        }

        float targetSpeed;
        if (distance > slowRadius)
            targetSpeed = agent.MaxSpeed;
        else
            targetSpeed = agent.MaxSpeed * (distance / slowRadius);

        Vector3 desiredVelocity = direction.normalized * targetSpeed;
        steer.linear = desiredVelocity - agent.Velocity;
        steer.angular = 0;

        return steer;
    }
}