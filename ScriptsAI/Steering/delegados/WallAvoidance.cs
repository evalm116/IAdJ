using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAvoidance : Seek
{
    // Altura a la que se lanzan los rayos para detectar paredes
    private static float HEIGHTRAY_Y = 0.5f; 

    public float avoidDistance = 5f;
    public float lookahead = 10f;

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
    }

    private Vector3 ray;
    private Vector3 characterDummyPos;
    public override Steering GetSteering(AgentNPC character)
    {
         ray = character.Velocity.normalized * lookahead;

        characterDummyPos = new Vector3(character.Position.x, HEIGHTRAY_Y, character.Position.z);

        Collision collision = CollisionDetector.getCollision(characterDummyPos, ray);

        if (collision == null)
            return new Steering();

        target.Position = collision.Position + collision.Normal * avoidDistance;
        return base.GetSteering(character);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(characterDummyPos, characterDummyPos + ray);
    }


}
