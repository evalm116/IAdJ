using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : MeleeUnit
{
    // Start is called before the first frame update
    void Awake()
    {
        this.type = Type.Rogue;
    }

    protected override void CheckGround()
    {
        throw new System.NotImplementedException();
    }

    
}
