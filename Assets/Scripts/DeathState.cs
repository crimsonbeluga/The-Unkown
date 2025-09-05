using Unity.VisualScripting;
using UnityEngine;

public class DeathState : State
{
    private EnemyAIController ai;

    public DeathState(EnemyAIController ai)
    {
        this.ai = ai;
    }


    public override void Enter()
    {
        ai.animator.TriggerDeath();
        ai.rb.velocity = Vector2.zero;

        // disable collision so it doesnt block anything 
        if(ai.TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }
        // Optional: play death animation or destroy after delay
        Debug.Log("Enemy has died.");



        
    }

    public override void Update()
    {
        // No update needed — enemy is dead
    }

    public override void Exit()
    {
        // Nothing to clean up — dead enemies don't come back
    }
}
