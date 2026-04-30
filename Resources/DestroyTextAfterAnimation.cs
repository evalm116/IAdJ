using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTextAfterAnimation : MonoBehaviour
{
    public void DestroyParent()
    {
        GameObject parent = gameObject.transform.parent.gameObject;
        Destroy(parent);
    }

}
