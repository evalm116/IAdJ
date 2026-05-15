using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : RangeUnit
{
    void Awake()
    {
        this.type = Type.Wizard;
    }

    protected override void CheckGround()
    {
        throw new System.NotImplementedException();
    }

}
