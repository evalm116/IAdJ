using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollowing : Seek
{
    public Transform[] pathNodes;
    private int currentNode = 0;
    public override Steering GetSteering(AgentNPC character)
    {
        if ((character.Position - pathNodes[currentNode].position).magnitude < 1f) currentNode = (currentNode + 1) % pathNodes.Length;
        target.Position = pathNodes[currentNode].position;
        return base.GetSteering(character);
    }
}

