using UnityEngine;

public class ChaseState : State
{
    private EnemyAIController ai;
    private PlayerDeathHandler playerDeathHandler;

    public ChaseState(EnemyAIController ai)
    {
        this.ai = ai;
    }

    public override void Enter()
    {
        ai.animator.SetRuning(true);
        ai.chaseTimer = 0f;
        Debug.Log("CHASE: Entered");

        var player = GameObject.FindWithTag("Player");
        playerDeathHandler = player?.GetComponent<PlayerDeathHandler>();
    }

    public override void Update()
    {
        Debug.Log("CHASE: Update running");

        if (ai.justKilledPlayer)
        {
            Debug.Log("CHASE: Aborting chase — player was just killed.");
            stateMachine.ChangeState(new ReturnToPostState(ai));
            return;
        }

        if (!ai.CanSeePlayer())
        {
            ai.chaseTimer += Time.deltaTime;
            Debug.Log("CHASE: Lost sight. Timer = " + ai.chaseTimer);

            if (ai.chaseTimer >= ai.chaseTimeout || (playerDeathHandler != null && playerDeathHandler.isDead))
            {
                Debug.Log("CHASE: Timeout exceeded or Player dead. Returning to post.");
                stateMachine.ChangeState(new ReturnToPostState(ai));
                return;
            }
        }

        Vector2 direction = (ai.player.position - ai.transform.position).normalized;
        ai.FaceDirection(direction);

        // 🔧 New movement system using MovePosition to prevent slowdown
        Vector2 currentPos = ai.rb.position;
        Vector2 targetPos = Vector2.MoveTowards(currentPos, ai.player.position, ai.chaseSpeed * Time.fixedDeltaTime);
        ai.rb.MovePosition(targetPos);

        Debug.DrawLine(ai.transform.position, ai.player.position, Color.green);
    }

    public override void Exit()
    {
        ai.animator.SetRuning(false);
        Debug.Log("CHASE: Exit");
        // No need to reset velocity if using MovePosition
    }
}
