using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class WallAvoidanceThreeRays : WallAvoidance
{
    protected Vector3 whiskersRay;
    public float whiskersLookahead = 2.5f;
    public int degreesWhiskers = 30;

    protected override Collision CalculateCollision()
    {
        // Comprobar colisión con el rayo central
        Collision collision = CollisionDetector.GetCollision(characterDummyPos, ray);
        if (collision != null)
            return collision;
        // Comprobar colisión con los rayos laterales (whiskers)
        whiskersRay = ray.normalized * whiskersLookahead;
        return CollisionDetector.GetWhiskersCollision(characterDummyPos,
                                                        whiskersRay,
                                                        degreesWhiskers);
    }

    protected override void DrawGizmos()
    {       
        Vector3 leftWhisker = Quaternion.Euler(0, -degreesWhiskers, 0) * whiskersRay;
        Vector3 rightWhisker = Quaternion.Euler(0, degreesWhiskers, 0) * whiskersRay;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(characterDummyPos, characterDummyPos + ray);
        Gizmos.DrawLine(characterDummyPos, characterDummyPos + leftWhisker);
        Gizmos.DrawLine(characterDummyPos, characterDummyPos + rightWhisker);
    }
}
