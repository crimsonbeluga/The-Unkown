// We need UnityEngine for MonoBehaviour, Animator, Debug, etc.
using UnityEngine;

// This class controls animation parameters for the Ice Golem enemy.
// It connects script logic (e.g., Patrol, Attack, Death) to Unity’s Animator system.
public class IceGolemAnimator : MonoBehaviour
{
    // ----------------------- //
    //    Inspector Fields     //
    // ----------------------- //

    [Header("Animator Reference")] // This adds a labeled section in the Inspector.
    public Animator animator;      // Reference to the Animator component on the Ice Golem.
                                   // Animator is what actually plays animation clips in Unity.

    // Animator parameter names — these MUST match the exact names inside the Animator Controller.
    public string walkParam = "IsWalking";  // A bool parameter to toggle walk animation.
    public string runParam = "IsRunning";   // A bool parameter to toggle run animation.
    public string attackTrigger = "Attack"; // A trigger parameter to fire the attack animation.
    public string deathTrigger = "Die";     // A trigger parameter to fire the death animation.

    // ----------------------- //
    //    Public API Methods   //
    // ----------------------- //

    // Call this to start/stop the walk animation.
    // Example: SetWalking(true) turns on the walk animation.
    public void SetWalking(bool value)
    {
        SafeSetBool(walkParam, value);
    }

    // Same as above but for running. Note the typo: "SetRuning" should be "SetRunning".
    public void SetRuning(bool value)
    {
        SafeSetBool(runParam, value);
    }

    // Triggers the attack animation ONCE. It doesn't toggle — it just fires like a button press.
    public void TriggerAttack()
    {
        SafeTrigger(attackTrigger);
    }

    // Triggers the death animation.
    public void TriggerDeath()
    {
        SafeTrigger(deathTrigger);
    }

    // ----------------------- //
    //  Internal Safe Helpers  //
    // ----------------------- //

    // Ensures the trigger parameter exists before trying to activate it.
    // Prevents runtime errors if the Animator is missing a parameter.
    private void SafeTrigger(string param)
    {
        // Check if the Animator has this trigger parameter defined.
        if (HasParameter(param, AnimatorControllerParameterType.Trigger))
        {
            // Valid trigger found; activate it.
            animator.SetTrigger(param);
        }
        else
        {
            // Otherwise warn the developer in the console.
            Debug.LogWarning($"[IceGolemAnimator] Missing Trigger parameter: {param}");
        }
    }

    // Ensures the boolean parameter exists before setting it.
    private void SafeSetBool(string param, bool value)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Bool))
        {
            // Valid bool param; set it to true or false.
            animator.SetBool(param, value);
        }
        else
        {
            // Warn if parameter not found — this saves hours of debugging bad param names.
            Debug.LogWarning($"[IceGolemAnimator] Missing Bool parameter: {param}");
        }
    }

    // Checks if a parameter exists on the attached Animator controller.
    private bool HasParameter(string name, AnimatorControllerParameterType type)
    {
        // Loop through each parameter defined in the Animator.
        foreach (var p in animator.parameters)
        {
            // If we find a match for name and type, return true.
            if (p.name == name && p.type == type)
                return true;
        }

        // No match found, return false.
        return false;
    }
}
