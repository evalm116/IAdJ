using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Arrivev2 : SteeringBehaviour
{
    [Tooltip("Mostrar gizmos de depuración")]
    [SerializeField] protected bool _showDebugGizmos = true;

    // Declara las variables que necesites para este SteeringBehaviour
    public float timeToTarget = 0.1f;
    public float slowRadius = 5f;



    void Start()
    {
        this.nameSteering = "Arrive";
    }

    // Helper functions
    private Vector3 CalculateVelocity(Vector3 v, Agent agent)
    {
        /// Depending on distance and radius
        float targetSpeed = 0.0f;
        float distance = v.magnitude;

        if (distance < agent.ArrivalRadius)
        {

            // Stops, when enters in the inner radius 
            return new Vector3(0, 0, 0);    // return
        }

        if (distance > slowRadius)
            targetSpeed = agent.MaxSpeed;
        else
        {
            // as we get closer, speed decreases
            targetSpeed = agent.MaxSpeed * distance / agent.ArrivalRadius;
        }

        // creates the vector3 Velocity based on speed expected
        Vector3 targetVelocity = v;
        targetVelocity = targetVelocity.normalized;
        targetVelocity *= targetSpeed;

        return targetVelocity;
    }

    public override Steering GetSteering(AgentNPC agent)
    {
        Steering steer = new Steering();

        // First, gets the vector
        Vector3 v = target.Position - agent.Position;

        // Calculates the agent velocity
        Vector3 targetVelocity = CalculateVelocity(v, agent);

        steer.linear = targetVelocity - agent.Velocity;
        steer.linear /= timeToTarget;

        // Clips
        if (steer.linear.magnitude > agent.MaxAcceleration)
        {
            steer.linear = steer.linear.normalized;
            steer.linear *= agent.MaxAcceleration;
        }

        steer.angular = 0.0f;

        return steer;
    }

    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos) return;

        // Radio interior (rojo)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.Position, slowRadius);

    }
}