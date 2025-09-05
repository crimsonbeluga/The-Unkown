using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private State currentState;

    public void ChangeState(State newState)
    {
        if (currentState != null)
        {
            Debug.Log("Exiting state: " + currentState.GetType().Name);
            currentState.Exit();
        }

        currentState = newState;

        if (currentState != null)
        {
            Debug.Log("Entering state: " + currentState.GetType().Name);
            currentState.SetStateMachine(this);
            currentState.Enter();
        }
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }
}
