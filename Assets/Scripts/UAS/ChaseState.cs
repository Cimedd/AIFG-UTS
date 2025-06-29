using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChaseState : EnemyState
{
    public ChaseState(WeepingAngel angel) : base(angel)
    {
    }

    public override void Collision()
    {
        Stage2Manager.Instance.GameOver("The angel caught you");
    }

    public override void Enter()
    {
        Debug.Log("Chase Up");
        //ChangeSprite
        //Play Sound
    }

    public override void Exit()
    {
        Debug.Log("Chase Out");
        angel.StopMoving();
    }

    public override void Update()
    {
        if (!angel.isMoving && !angel.isFrozen)
        {
            angel.GetPath(angel.currentPos.position, angel.targetPos.position);
            angel.StartMoving(angel.finalPath);
            angel.fov.setMaterial("Chase");
        }
        
        if (CheckFacing()) 
        {
            if (!angel.isFrozen) 
            {
                angel.StopMoving();
                angel.isFrozen = true;
            }
        }
        else
            { 
            if (angel.isFrozen) 
            {
                angel.isFrozen = false;
            }

            if (!CanSeePlayer())
            {
                angel.ChangeState(angel.wanderState);
            }
        }
    }


    private bool CheckFacing()
    {
        var target = angel.targetPos.GetComponent<Player>();
        var playerDir = target.facingDirection;
        var angelDir = angel.facingDirection;
        return (playerDir == FacingDirection.Direction.Left && angelDir == FacingDirection.Direction.Right) ||
           (playerDir == FacingDirection.Direction.Right && angelDir == FacingDirection.Direction.Left) ||
           (playerDir == FacingDirection.Direction.Up && angelDir == FacingDirection.Direction.Down) ||
           (playerDir == FacingDirection.Direction.Down && angelDir == FacingDirection.Direction.Up);
    }

}
