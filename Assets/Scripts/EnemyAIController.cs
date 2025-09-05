// File: EnemyAIController.cs
using UnityEngine;
using System.Collections;

public class EnemyAIController : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    public float chaseSpeed = 1f;
    public float chaseRange = 5f;
    public float chaseTimeout = 3f;
    public float attackDelay = 0.5f;

    [HideInInspector] public float chaseTimer;
    [HideInInspector] public int currentPatrolIndex = 0;
    [HideInInspector] public IceGolemAnimator animator;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public bool justKilledPlayer = false;
    [HideInInspector] public bool isAttacking = false;

    public Rigidbody2D rb;
    public StateMachine stateMachine;

    private void Awake()
    {
        animator = GetComponent<IceGolemAnimator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        PatrolState patrol = new PatrolState(this);
        stateMachine.ChangeState(patrol);
    }

    public void FaceDirection(Vector2 direction)
    {
        if (isAttacking) return;
        spriteRenderer.flipX = direction.x < 0;
    }

    public bool CanSeePlayer()
    {
        if (player == null || justKilledPlayer)
            return false;

        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, chaseRange, LayerMask.GetMask("Player", "Ground"));

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    public void Die()
    {
        stateMachine.ChangeState(new DeathState(this));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 toPlayer = (collision.transform.position - transform.position).normalized;
            if (!isAttacking) FaceDirection(toPlayer);

            animator.TriggerAttack();

            HealthManager health = collision.gameObject.GetComponent<HealthManager>();
            if (health != null)
            {
                rb.velocity = Vector2.zero; // Stop all movement during attack
                rb.constraints = RigidbodyConstraints2D.FreezeAll; // Freeze position and rotation

                StartCoroutine(DelayedDamage(health));
                justKilledPlayer = true;
                isAttacking = true;
                StartCoroutine(WaitThenReturnToPost());
            }
        }
    }

    private IEnumerator DelayedDamage(HealthManager health)
    {
        yield return new WaitForSeconds(attackDelay);
        health.TakeDamage(100);
    }

    private IEnumerator WaitThenReturnToPost()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Unfreeze movement
        stateMachine.ChangeState(new ReturnToPostState(this));
    }
}
