using UnityEngine;

public class ReturnToPostState : State
{
    private EnemyAIController ai;

    public ReturnToPostState(EnemyAIController ai)
    {
        this.ai = ai;
    }

    public override void Enter()
    {
        ai.animator.SetWalking(true);
        ai.rb.velocity = Vector2.zero;
    }

    public override void Update()
    {
        // 🚫 DO NOT chase again if we just killed the player
        if (!ai.justKilledPlayer && ai.CanSeePlayer())
        {
            stateMachine.ChangeState(new ChaseState(ai));
            return;
        }

        Transform target = ai.patrolPoints[ai.currentPatrolIndex];
        Vector2 direction = (target.position - ai.transform.position).normalized;
        ai.FaceDirection(direction);
        ai.rb.velocity = direction * ai.moveSpeed;

        float distance = Vector2.Distance(ai.transform.position, target.position);

        if (distance < 1f)
        {
            ai.currentPatrolIndex = (ai.currentPatrolIndex + 1) % ai.patrolPoints.Length;

            // ✅ Reset justKilledPlayer after returning to post
            ai.justKilledPlayer = false;

            stateMachine.ChangeState(new PatrolState(ai));
        }
    }

    public override void Exit()
    {
        ai.rb.velocity = Vector2.zero;
        ai.animator.SetWalking(false);
    }
}
