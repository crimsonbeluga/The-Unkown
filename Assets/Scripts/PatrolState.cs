using UnityEngine;

public class PatrolState : State
{
    private EnemyAIController ai;

    public PatrolState(EnemyAIController ai)
    {
        this.ai = ai;
    }

    public override void Enter()
    {
        ai.animator.SetWalking(true);
        ai.chaseTimer = 0f;
        Debug.Log("Patrol points count: " + ai.patrolPoints.Length);
    }

    public override void Update()
    {
        if (!ai.justKilledPlayer && ai.CanSeePlayer())
        {
            Debug.Log("PATROL: Player spotted. Switching to ChaseState.");
            stateMachine.ChangeState(new ChaseState(ai));
            return;
        }

        Transform target = ai.patrolPoints[ai.currentPatrolIndex];
        Vector2 direction = (target.position - ai.transform.position).normalized;
        ai.FaceDirection(direction);
        ai.rb.velocity = direction * ai.moveSpeed;

        Debug.DrawLine(ai.transform.position, target.position, Color.red);

        float distance = Vector2.Distance(ai.transform.position, target.position);

        if (distance < 1f)
        {
            ai.currentPatrolIndex = (ai.currentPatrolIndex + 1) % ai.patrolPoints.Length;
            Debug.Log("Reached patrol point. Switching to index: " + ai.currentPatrolIndex);

            // Reset guard once enemy has returned to post
            ai.justKilledPlayer = false;
        }
    }

    public override void Exit()
    {
        ai.animator.SetWalking(false);
        ai.rb.velocity = Vector2.zero;
    }
}
