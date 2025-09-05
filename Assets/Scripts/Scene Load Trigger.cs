using System.Collections;
using UnityEngine;

public class SceneLoadTrigger : MonoBehaviour
{
    public float SceneLoadDelay = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(SceneLoadDelay);
        SceneLoader.Instance.LoadAbyssScene();
    }
}
