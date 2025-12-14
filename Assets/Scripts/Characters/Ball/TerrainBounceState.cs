using System.Collections.Generic;
using UnityEngine;

public class TerrainBounceState : BounceState
{
    public override void Enter(Dictionary<string, object> msg = null)
    {
       if (echo.playerControlled)
        {
            echo.staminaComponent.EnableForesight();
        }
        base.Enter(msg);
    }
 
}
