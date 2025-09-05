using UnityEngine;
using UnityEngine.Playables;

public enum CameraModeType
{
    FollowPlayer,
    FixedZone,
    Cinematic

}


[System.Serializable] // this allows this class to be visibled and editable in the inspector
public class CameraMode
{
    public CameraModeType modeType;

    public Transform target; // the object to follow like player, an object and so on 
    public Vector3 position; // the fixed position we will be at if were not following a target
    public float zoom = 5f;// this is just the zoom level default 
    public float transitionDuration = 1f; // how long  it takes to transfer into this mode
    public bool destroyAfterTransition = false;
    public bool lockPlayerInput = false;  // EDIT  this will prpbably be moved over to player controller or pawn as it makes more sense.  
    public bool overrideMode = false; // should this forcfully overide any camera mode
    public AnimationCurve transitionCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public Transform lookAtTarget; // a possible second thing for the camera to focus on
    public Vector3 offset; // off set from the target 
    

}

    


