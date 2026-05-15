using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranger : RangeUnit
{
    void Awake()
    {
        this.type = Type.Ranger;
    }

    protected override void CheckGround()
    {
        throw new System.NotImplementedException();
    }

}
