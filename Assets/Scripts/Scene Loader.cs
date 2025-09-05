using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
   public static SceneLoader Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this )
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadCabinScene()
    {

        SceneManager.LoadScene("Cabin Scene");

    }

    public void LoadAbyssScene()
    {
        SceneManager.LoadScene("Abyss Scene");
    }

}
