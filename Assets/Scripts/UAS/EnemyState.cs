using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class EnemyState
{
    protected WeepingAngel angel;

    public EnemyState(WeepingAngel angel)
    {
        this.angel = angel;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
    public abstract void Collision();

    protected bool CanSeePlayer()
    {
        return angel.targetInRange;
    }
}
