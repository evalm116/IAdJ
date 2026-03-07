using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WallAvoidance : Seek
{
    public bool drawGizmos = false;
    // Altura a la que se lanzan los rayos para detectar paredes
    protected static float HEIGHTRAY_Y = 0.5f;

    public float offset = 1.5f; // Distancia adicional para evitar colisiones
    protected float avoidDistance;
    public float lookahead = 2f;


    public void Awake()
    {
        this.nameSteering = "WallAvoidance";
    }
    // Start is called before the first frame update
    void Start()
    {
        if (target == null)
        {
            GameObject dummy = new GameObject("DummyWallAvoidanceTarget");
            target = dummy.AddComponent<Agent>();
        }
        avoidDistance = GetAvoidDistance(GetComponent<Collider>());
    }

    protected Vector3 ray;
    protected Vector3 characterDummyPos;
    public override Steering GetSteering(AgentNPC character)
    {
        ray = character.Velocity.normalized * lookahead;

        characterDummyPos = new Vector3(character.Position.x, HEIGHTRAY_Y, character.Position.z);

        Collision collision = this.CalculateCollision();

        if (collision == null)
            return new Steering();

        target.Position = collision.Position + collision.Normal * (avoidDistance + offset);

        Steering steer = base.GetSteering(character);
        steer.linear *= 2.0f;
        return steer;
        //return base.GetSteering(character);
    }

    protected abstract Collision CalculateCollision();

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        DrawGizmos();
    }

    protected abstract void DrawGizmos();

    private static float GetAvoidDistance(Collider collider)
    {
        float size;
        switch (collider)
        {
            case SphereCollider sphereCollider:
                size = sphereCollider.radius;
                break;
            case CapsuleCollider capsuleCollider:
                size = capsuleCollider.radius;
                break;
            case BoxCollider boxCollider:
                size = boxCollider.size.x / 2f;
                break;
            default:
                // Fallback for unknown collider types
                size = collider.bounds.size.x / 2f;
                break;
        }
        return size;

    }

}
