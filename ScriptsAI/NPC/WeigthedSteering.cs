using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeigthedSteering
{
    public List<(SteeringBehaviour behaviour, float weight)> behaviours;

    public WeigthedSteering()
    {
        behaviours = new List<(SteeringBehaviour behaviour, float weight)>();
    }

    public Steering GetSteering(AgentNPC character)
    {
        Steering result = new Steering();
        foreach (var item in behaviours)
        {
            var steering = item.behaviour.GetSteering(character);
            result.linear += steering.linear * item.weight;
            result.angular += steering.angular * item.weight;
        }

        result.linear = Vector3.ClampMagnitude(result.linear, character.MaxAcceleration);
        result.angular = Mathf.Clamp(result.angular, -character.MaxRotation, character.MaxRotation);

        return result;
    }

    public bool AddBehaviour(SteeringBehaviour behaviour, float weight)
    {
        foreach (var item in behaviours)
        {
            if (item.behaviour.name == behaviour.name)
                return false;
        }
        behaviours.Add((behaviour, weight));
        return true;
    }

    public bool RemoveBehaviour(string name)
    {
        foreach (var item in behaviours)
        {
            if (item.behaviour.name == name)
            {
                behaviours.Remove(item);
                return true;
            }
        }
        return false;
    }
}
