using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAvoidanceWhiskers : WallAvoidance
{
    public int degreesWhiskers = 30;

    protected override Collision CalculateCollision()
    {
        return CollisionDetector.GetWhiskersCollision(characterDummyPos, ray, degreesWhiskers); //TO-DO
    }

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 leftWhisker = Quaternion.Euler(0, -degreesWhiskers, 0) * ray;
        Vector3 rightWhisker = Quaternion.Euler(0, degreesWhiskers, 0) * ray;
        Gizmos.DrawLine(characterDummyPos, characterDummyPos + leftWhisker);
        Gizmos.DrawLine(characterDummyPos, characterDummyPos + rightWhisker);
    }
}
