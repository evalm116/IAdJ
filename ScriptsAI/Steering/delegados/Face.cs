using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : Align
{

    public override Steering GetSteering(AgentNPC character)
    {
        var direction = target.transform.position - character.Position;
        if (direction.magnitude == 0)
            return new Steering();

        target.Orientation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;        

        return base.GetSteering(character);
    }
}

