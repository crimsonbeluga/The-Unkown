// File: AttackState.cs
using UnityEngine;

public class AttackState : State
{
    private EnemyAIController ai;
    private PlayerDeathHandler playerDeathHandler;

    public AttackState(EnemyAIController ai)
    {
        this.ai = ai;
    }

    public override void Enter()
    {
        ai.rb.velocity = Vector2.zero;
        ai.isAttacking = true;
        ai.rb.constraints = RigidbodyConstraints2D.FreezeAll;

        var player = GameObject.FindGameObjectWithTag("Player");
        playerDeathHandler = player?.GetComponent<PlayerDeathHandler>();
    }

    public override void Update()
    {
        if (playerDeathHandler != null && playerDeathHandler.isDead)
        {
            ai.isAttacking = false;
            ai.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            stateMachine.ChangeState(new ReturnToPostState(ai));
        }
    }

    public override void Exit()
    {
        ai.rb.velocity = Vector2.zero;
        ai.isAttacking = false;
        ai.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
