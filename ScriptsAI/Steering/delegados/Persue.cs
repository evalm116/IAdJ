using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pursue : Seek
{
    public float maxPrediction = 1f;
    public Agent Target;
    public override Steering GetSteering(AgentNPC character)
    {
        var direction = Target.transform.position - character.Position;
        var distance = direction.magnitude;
        var speed = character.Velocity.magnitude;
        var prediction = (speed < distance / maxPrediction) ? maxPrediction : distance / speed;
        Target.transform.position = Target.transform.position + Target.Velocity * prediction;  // Delega implícito a Seek
        return base.GetSteering(character);
    }
}

