using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChaseState : EnemyState
{
    public ChaseState(WeepingAngel angel) : base(angel)
    {
    }

    public override void Collision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (angel.isFrozen)
            {
                angel.ChangeState(angel.dormantState);
            }
            else
            {
                Stage2Manager.Instance.GameOver("The angel caught you");
            }

        }

        if (collision.gameObject.CompareTag("dummy"))
        {
            if (angel.targetPos != null && collision.transform == angel.targetPos)
            {
                angel.StopMoving();
                angel.targetPos = null;
                angel.targetInRange = false;
                collision.gameObject.SetActive(false);
                Object.Destroy(collision.gameObject, 0.5f);
            }
        }
       
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
        angel.targetPos = null;
        angel.targetCooldownTimer = angel.targetCooldownDuration;
    }

    public override void Update()
    {
        if(angel.targetPos == null)
        {
            angel.ChangeState(angel.wanderState);
            return; 
        }

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
        if (angel.targetPos.CompareTag("dummy"))
            return false;

        var target = angel.targetPos.GetComponent<Player>();
        if (target == null)
            return false;

        var playerDir = target.facingDirection;
        var angelDir = angel.facingDirection;
        return (playerDir == FacingDirection.Direction.Left && angelDir == FacingDirection.Direction.Right) ||
           (playerDir == FacingDirection.Direction.Right && angelDir == FacingDirection.Direction.Left) ||
           (playerDir == FacingDirection.Direction.Up && angelDir == FacingDirection.Direction.Down) ||
           (playerDir == FacingDirection.Direction.Down && angelDir == FacingDirection.Direction.Up);
    }

}
