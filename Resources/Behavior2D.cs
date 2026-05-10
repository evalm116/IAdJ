using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior2D : MonoBehaviour
{
    private void LateUpdate()
    {
        // Mira a la camara
        transform.forward = Camera.main.transform.forward;
    }
}
