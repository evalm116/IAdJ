using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision 
{
    private Vector3 position;
    private Vector3 normal;

    public Collision(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
    }

    public Collision(RaycastHit hit)
    {
        this.position = new Vector3(hit.point.x, 0, hit.point.z);
        this.normal = new Vector3(hit.normal.x, 0, hit.normal.z);
    }

    public Vector3 Position
    {
        get { return new Vector3(position.x, position.y, position.z); }
        set { position = new Vector3(value.x, value.y, value.z); } // Copia
    }
    public Vector3 Normal
    {
        get { return new Vector3 (normal.x, normal.y, normal.z); }
        set { normal = new Vector3(value.x, value.y, value.z); } // Copia el vector
    }
}
