using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession I { get; private set; }

    [Header("Config")]
    [SerializeField] int startLives = 3;
    [SerializeField] string mainMenuScene = "MainMenu";

    public int Lives { get; private set; }

    public static System.Action<int> OnLivesChanged; 

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        Lives = startLives;
    }

    public static void LoseLife()
    {
        if (I == null) return;
        I.Lives = Mathf.Max(0, I.Lives - 1);
        OnLivesChanged?.Invoke(I.Lives);

        if (I.Lives > 0)
        {
            var s = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(s);
        }
        else
        {
            
            I.Lives = I.startLives;
            OnLivesChanged?.Invoke(I.Lives);
            SceneManager.LoadScene(I.mainMenuScene);
        }
    }

    public static void ResetLives() 
    {
        if (I == null) return;
        I.Lives = I.startLives;
        OnLivesChanged?.Invoke(I.Lives);
    }
}