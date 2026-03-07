using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionDetector 
{
    public static Collision GetCollision(Vector3 position, Vector3 movement)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, movement, out hit, movement.magnitude))
        {
            return new Collision(hit);
        }
        return null;
    }

    internal static Collision GetWhiskersCollision(Vector3 position, Vector3 movement, int degreesWhiskers)
    {
        Collision closestCollision = null;
        float closestDistance = float.MaxValue;
        Collision collision;

        // Comprobar si el rayo izquierdo colisiona
        Vector3 leftWhisker = Quaternion.Euler(0, -degreesWhiskers, 0) * movement;
        if ((collision = GetCollision(position, leftWhisker)) != null)
        {
                closestCollision = collision;
                closestDistance = Vector3.Distance(position, collision.Position);
        }

        // Comprobar si el rayo derecho colisiona
        Vector3 rightWhisker = Quaternion.Euler(0, degreesWhiskers, 0) * movement;
        if ((collision = GetCollision(position, rightWhisker)) != null &&
            closestDistance > Vector3.Distance(position, collision.Position))
        {
            closestCollision = collision;
            // Nota: No es necesario actualizar closestDistance aquí porque
            // no se volverá a comparar con ningún otro rayo
        }
        // Si ninguno colisiona, closestCollision seguirá siendo null
        return closestCollision;
    }

    public static Collision GetNRaysCollision( Vector3 position, Vector3 movement, int numRays, float archWidth)
    {
        Collision closestCollision = null;
        float closestDistance = float.MaxValue;

        if (numRays < 1)
            return null;

        float angleStep = archWidth / (numRays - 1);

        float angle = -archWidth / 2;

        for (int i = 0; i < numRays; i++)
        {
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * movement;
            Collision collision = GetCollision(position, rayDirection);
            if (collision != null)
            {
                float distance = Vector3.Distance(position, collision.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollision = collision;
                }
            }

            angle += angleStep;
        }
        return closestCollision;
    }
}
