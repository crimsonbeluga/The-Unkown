using UnityEngine;

// Base class for all states (abstract means it must be inherited, not used directly)
//abstract  means This class cannot be instantiated directly. It's meant to be inherited from.
public abstract class State
{
    // Reference to the parent state machine, so states can call ChangeState()
    //protected means Only this class and classes that inherit from it can access this.
    protected StateMachine stateMachine;

    // Called once when state is assigned to a state machine
    public void SetStateMachine(StateMachine machine)
    {
        stateMachine = machine;
    }

    // Called when this state is entered
    //virtual means subclasses can override it to add logic.
    public virtual void Enter() { }

    // Called every frame this state is active
    public virtual void Update() { }

    // Called when this state is exited
    public virtual void Exit() { }
}
