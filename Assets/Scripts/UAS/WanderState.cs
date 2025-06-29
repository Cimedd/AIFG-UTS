using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WanderState : EnemyState
{
    public WanderState(WeepingAngel angel) : base(angel)
    {
    }

    public override void Collision()
    {
        //
    }

    public override void Enter()
    {
        Debug.Log("Wander Up");
        angel.StartSensor();
        //Change Sprite
    }

    public override void Exit()
    {
        angel.StopMoving();
    }

    public override void Update()
    {
        if (!angel.isMoving)
        {
            var wander = angel.wanderSpots[Random.Range(0, angel.wanderSpots.Length)];
            angel.GetPath(angel.currentPos.position, wander.position);
            angel.StartMoving(angel.finalPath);
            Debug.Log("Moving to Point");
            angel.fov.setMaterial("Wander");
        }

        
        if (CanSeePlayer())
        {
            angel.ChangeState(angel.chaseState);
        }
    }

}
