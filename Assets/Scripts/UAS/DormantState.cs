using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DormantState : EnemyState
{
    public DormantState(WeepingAngel angel) : base(angel)
    {

    }

    public override void Collision()
    {
        //
    }

    public override void Enter()
    {
        angel.StopSennsor();
        //Change Sprite
    }

    public override void Exit()
    {
      //Change Sprite
    }

    public override void Update()
    {
        
        //Play Sound at Random Interval;
    }
}
