using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MeleeUnit
{
    // Start is called before the first frame update
    void Awake()
    {
        this.type = Type.Light;
    }

    protected override void CheckGround()
    {
        throw new System.NotImplementedException();
    }

    
}
