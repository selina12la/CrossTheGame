using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverUITK : MonoBehaviour
{
    [SerializeField] string mainMenuScene = "MainMenu";

    void Start()
    {
        var ui = GetComponent<UIDocument>();
        var root = ui ? ui.rootVisualElement : null;
        if (root == null) { Debug.LogError("[GameOver] UIDocument/Root fehlt."); return; }

        var lblScore = root.Q<Label>("lblScore");
        var btnMenu  = root.Q<Button>("btnMenu");

        int score = (GameSession.I != null) ? GameSession.I.Score : 0;
        if (lblScore != null) lblScore.text = $"Punkte: {score}";

        if (btnMenu != null)
            btnMenu.clicked += () => SceneManager.LoadScene(mainMenuScene);

        root.RegisterCallback<KeyDownEvent>(e =>
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Space)
                SceneManager.LoadScene(mainMenuScene);
            if (e.keyCode == KeyCode.Escape)
                SceneManager.LoadScene(mainMenuScene);
        });
    }
}