using UnityEngine;

public class PressurePlateHandler : MonoBehaviour
{
    public Transform PressurePlateBottomPosition;
    public Transform PressurePlateStartPosition;
    public float pressurePlateSpeed = 1f;
    public GameObject Door;

    private int pressCount = 0;
    private float releaseCooldown = 0f;
    private DoorScript doorScript;

    private void Start()
    {
        doorScript = Door.GetComponent<DoorScript>();
    }

    private void Update()
    {
        if (pressCount <= 0 && releaseCooldown > 0f)
            releaseCooldown -= Time.deltaTime;

        bool isPressed = pressCount > 0 || releaseCooldown > 0f;
        Transform target = isPressed ? PressurePlateBottomPosition : PressurePlateStartPosition;

        transform.position = Vector3.MoveTowards(transform.position, target.position, pressurePlateSpeed * Time.deltaTime);

        if (isPressed)
            doorScript.MoveDoor();
        else
            doorScript.ResetDoor();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Box"))
        {
            pressCount++;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Box"))
        {
            pressCount = Mathf.Max(pressCount - 1, 0);
            if (pressCount == 0)
                releaseCooldown = 0.2f;
        }
    }
}
