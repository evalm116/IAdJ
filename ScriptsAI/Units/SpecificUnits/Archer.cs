using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : RangeUnit
{
    void Awake()
    {
        this.type = Type.Archer;
    }

    protected override void CheckGround()
    {
        throw new System.NotImplementedException();
    }

}
