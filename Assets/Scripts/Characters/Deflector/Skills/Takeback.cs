using System.Collections.Generic;
using UnityEngine;

public class Takeback : BaseSkill
{
    enum TakebackState
    {
        Catching,
        Holding,
        Throwing,
        YoYo,
    }

    TakebackState currentState = TakebackState.Catching;

    public override void Enter(Dictionary<string, object> msg = null)
    {
        currentState = TakebackState.Catching;
    }


}
