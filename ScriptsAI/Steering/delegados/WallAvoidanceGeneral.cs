using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WallAvoidanceGeneral : WallAvoidance
{
    public int numRays = 5;
    public float archWidth = 180f;

    protected override Collision CalculateCollision()
    {
        return CollisionDetector.GetNRaysCollision(characterDummyPos, ray, numRays, archWidth);
    }

    protected override void DrawGizmos()
    {
        if (numRays < 1) return;

        float step = archWidth / (numRays - 1);
        float angle = -archWidth / 2;

        for (int i = 0; i < numRays; i++)
        {
            Vector3 whisker = Quaternion.Euler(0, angle, 0) * ray;
            Gizmos.DrawLine(characterDummyPos, characterDummyPos + whisker);
            angle += step;
        }
    }
}
