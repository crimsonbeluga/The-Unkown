using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private PlayerController playerController;
    private Camera mainCamera;
    private Coroutine currentTransition;
    private CameraMode currentMode;
    private Transform followTarget;
    private bool isTransitioning = false;
    private GameObject zoneToDestroyAfterTransition;
    private bool inputWasLocked = false;
    private bool resumeOnlyOnInput = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetCameraMode(CameraMode newMode, GameObject triggerZone = null)
    {
        Vector3 targetPosition = newMode.modeType == CameraModeType.FollowPlayer && newMode.target != null
            ? newMode.target.position + newMode.offset
            : newMode.position;
        targetPosition.z = mainCamera.transform.position.z;

        bool cameraIsAlreadyAtTarget =
            Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.01f &&
            Mathf.Abs(mainCamera.orthographicSize - newMode.zoom) < 0.01f;

        if (cameraIsAlreadyAtTarget)
        {
            return;
        }

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentMode = newMode;
        followTarget = newMode.modeType == CameraModeType.FollowPlayer ? newMode.target : null;
        isTransitioning = true;
        currentTransition = StartCoroutine(TransitionToMode(newMode));

        if (newMode.destroyAfterTransition)
            zoneToDestroyAfterTransition = triggerZone;

        if (playerController != null)
        {
            if (newMode.lockPlayerInput)
            {
                playerController.DisableInput();
                playerController.FreezeMovement(true);
                playerController.GetComponent<PlayerPawn>()?.playerAnimator?.SetIdle();
                inputWasLocked = true;
                resumeOnlyOnInput = true;
            }
            else
            {
                playerController.EnableInput();

                if (inputWasLocked)
                    playerController.FreezeMovement(false);

                inputWasLocked = false;
                resumeOnlyOnInput = false;
            }
        }
    }

    private IEnumerator TransitionToMode(CameraMode mode)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, mode.transitionDuration);

        Vector3 startPos = mainCamera.transform.position;
        float startZoom = mainCamera.orthographicSize;
        float targetZoom = mode.zoom;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = mode.transitionCurve.Evaluate(elapsed / duration);

            Vector3 dynamicTargetPos = mode.modeType == CameraModeType.FollowPlayer && mode.target != null
                ? mode.target.position + mode.offset
                : mode.position;

            Vector3 lerpedPos = Vector3.Lerp(startPos, dynamicTargetPos, t);
            lerpedPos.z = mainCamera.transform.position.z;

            mainCamera.transform.position = lerpedPos;
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);

            yield return null;
        }

        if (mode.modeType == CameraModeType.FollowPlayer && mode.target != null)
        {
            Vector3 finalTargetPos = mode.target.position + mode.offset;
            mainCamera.transform.position = new Vector3(finalTargetPos.x, finalTargetPos.y, mainCamera.transform.position.z);
        }
        else
        {
            mainCamera.transform.position = new Vector3(mode.position.x, mode.position.y, mainCamera.transform.position.z);
        }

        mainCamera.orthographicSize = targetZoom;
        isTransitioning = false;

        if (zoneToDestroyAfterTransition != null)
        {
            Destroy(zoneToDestroyAfterTransition);
            zoneToDestroyAfterTransition = null;
        }
    }

    private void LateUpdate()
    {
        if (followTarget != null && !isTransitioning)
        {
            Vector3 followPos = followTarget.position + currentMode.offset;
            mainCamera.transform.position = new Vector3(
                followPos.x,
                followPos.y,
                mainCamera.transform.position.z
            );
        }

        if (!isTransitioning && inputWasLocked)
        {
            if (resumeOnlyOnInput && !AnyInputPressed())
                return;

            playerController.EnableInput();
            playerController.FreezeMovement(false);
            inputWasLocked = false;
            resumeOnlyOnInput = false;
        }
    }

    private bool AnyInputPressed()
    {
        return Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
    }
}
