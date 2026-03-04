using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionDetector 
{
    public static Collision getCollision(Vector3 position, Vector3 movement)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, movement, out hit, movement.magnitude))
        {
            return new Collision(hit);
        }
        return null;
    }
}
