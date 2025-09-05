using UnityEngine;
using UnityEngine.Playables;

public class TimelineAnimatorOverride : MonoBehaviour
{
    [Tooltip("The PlayableDirector controlling this cutscene")]
    public PlayableDirector director;

    [Tooltip("The Animator to disable during timeline playback")]
    public Animator animator;

    private RuntimeAnimatorController originalController;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (director == null) director = GetComponent<PlayableDirector>();

        if (animator != null)
            originalController = animator.runtimeAnimatorController;

        if (director != null)
        {
            director.played += OnTimelinePlay;
            director.stopped += OnTimelineStop;
        }
    }

    void OnTimelinePlay(PlayableDirector dir)
    {
        if (animator != null)
            animator.runtimeAnimatorController = null;
    }

    void OnTimelineStop(PlayableDirector dir)
    {
        if (animator != null)
            animator.runtimeAnimatorController = originalController;
    }
}
