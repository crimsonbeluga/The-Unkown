// File: PlayerAnimator.cs
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Animation Parameters")]
    public string speedParam = "Speed";
    public string groundedParam = "isGrounded";
    public string crawlParam = "isCrawling";
    public string runParam = "isRunning";
    public string jumpTrigger = "Jump";
    public string doubleJumpTrigger = "DoubleJump";
    public string rollTrigger = "Roll";
    public string forwardJumpTrigger = "ForwardJump";
    public string mantleTrigger = "Mantle";
    public string wallJumpTrigger = "WallJump";
    public string fallingFromJumpParam = "Falling From Jump";
    public string crawlSpeedParam = "CrawlSpeed";
    public string climbParam = "isClimbing";
    public string climbDownParam = "isClimbingDown";
    public string pushParam = "Pushing";
    public string pullParam = "Pulling";
    public string slidingParam = "isSliding";

    void Start()
    {
        Debug.Log("[PlayerAnimator] Start() ran. Checking Animator parameters...");
        ValidateParameters();
        Debug.Log("[PlayerAnimator] All Animator parameters checked.");
    }

    void ValidateParameters()
    {
        string[] boolParams = {
            groundedParam, crawlParam, runParam,
            fallingFromJumpParam, climbParam, climbDownParam,
            pushParam, pullParam, slidingParam
        };
        string[] floatParams = { speedParam, crawlSpeedParam };
        string[] triggerParams = {
            jumpTrigger, doubleJumpTrigger, rollTrigger,
            forwardJumpTrigger, mantleTrigger, wallJumpTrigger
        };

        foreach (var name in boolParams)
            SafeSetBool(name, false);
        foreach (var name in floatParams)
            SafeSetFloat(name, 0f);
        foreach (var name in triggerParams)
            SafeTrigger(name);
    }

    public void UpdateAnimation(float speed, bool isGrounded, bool isCrawling, bool isSliding, bool isFalling)
    {
        SafeSetFloat(speedParam, speed);
        SafeSetBool(groundedParam, isGrounded);
        SafeSetBool(crawlParam, isCrawling);
        SafeSetBool(slidingParam, isSliding);
        SafeSetBool(fallingFromJumpParam, isFalling);
        animator.speed = isCrawling && speed < 0.05f ? 0f : 1f;
    }

    public void SetClimbing(bool value)
    {
        SafeSetBool(climbParam, value);
    }

    public void SetClimbingDirection(bool goingUp)
    {
        SafeSetBool(climbDownParam, !goingUp);
    }

    public void SetBool(string paramName, bool value)
    {
        SafeSetBool(paramName, value);
    }

    public void TriggerJump() => SafeTrigger(jumpTrigger);
    public void TriggerDoubleJump() => SafeTrigger(doubleJumpTrigger);
    public void TriggerRoll() => SafeTrigger(rollTrigger);
    public void TriggerForwardJump() => SafeTrigger(forwardJumpTrigger);
    public void TriggerMantle() => SafeTrigger(mantleTrigger);

    public void TriggerWallJump()
    {
        Debug.Log("[PlayerAnimator] TriggerWallJump called");
        SafeTrigger(wallJumpTrigger);
    }

    public void SetCrawlSpeed(float value)
    {
        SafeSetFloat(crawlSpeedParam, value);
    }

    public void SetPushing(bool value)
    {
        SafeSetBool(pushParam, value);
    }

    public void SetPulling(bool value)
    {
        SafeSetBool(pullParam, value);
    }

    public void ResetAllTriggers()
    {
        animator.ResetTrigger(jumpTrigger);
        animator.ResetTrigger(doubleJumpTrigger);
        animator.ResetTrigger(rollTrigger);
        animator.ResetTrigger(forwardJumpTrigger);
        animator.ResetTrigger(mantleTrigger);
        animator.ResetTrigger(wallJumpTrigger);
    }

    public void ResumeAnimator() => animator.speed = 1f;

    void SafeSetBool(string param, bool value)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Bool))
            animator.SetBool(param, value);
    }

    void SafeSetFloat(string param, float value)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Float))
            animator.SetFloat(param, value);
    }

    void SafeTrigger(string param)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Trigger))
        {
            Debug.Log($"[Animator] Triggering animation: {param}");
            ResumeAnimator();
            ResetAllTriggers();
            animator.SetTrigger(param);
        }
    }

    bool HasParameter(string name, AnimatorControllerParameterType type)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == name && param.type == type)
                return true;
        }
        Debug.LogWarning($"[Animator] Missing parameter: '{name}' of type {type}");
        return false;
    }
    public void SetIdle()
    {
        SafeSetFloat(speedParam, 0f);
        SafeSetBool(crawlParam, false);
        SafeSetBool(slidingParam, false);
        SafeSetBool(runParam, false);
        SafeSetBool(fallingFromJumpParam, false);
        SafeSetBool(pushParam, false);
        SafeSetBool(pullParam, false);
        ResumeAnimator();
    }

}
