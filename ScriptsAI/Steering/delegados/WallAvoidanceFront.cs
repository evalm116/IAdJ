using UnityEngine;

public class WallAvoidanceFront : WallAvoidance
{
    protected override Collision CalculateCollision()
    {
        return CollisionDetector.GetCollision(characterDummyPos, ray);
    }

    protected override void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(characterDummyPos, characterDummyPos + ray);
    }
}
