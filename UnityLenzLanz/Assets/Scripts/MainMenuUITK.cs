using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;   // <— neu

public class MainMenuUITK : MonoBehaviour
{
    [SerializeField] string gameSceneName = "GameScene";

    VisualElement root, howToOverlay;
    Button btnStart, btnHowTo, btnQuit, btnBack;

    void Awake()
    {
        var ui = GetComponent<UIDocument>();
        root = ui.rootVisualElement;

        btnStart     = root.Q<Button>("btnStart");
        btnHowTo     = root.Q<Button>("btnHowTo");
        btnQuit      = root.Q<Button>("btnQuit");
        btnBack      = root.Q<Button>("btnBack");
        howToOverlay = root.Q<VisualElement>("HowToOverlay");

        if (btnStart != null) btnStart.clicked += () => SceneManager.LoadScene(gameSceneName);
        if (btnHowTo != null) btnHowTo.clicked += () => howToOverlay.style.display = DisplayStyle.Flex;
        if (btnBack  != null) btnBack.clicked  += () => howToOverlay.style.display = DisplayStyle.None;
        if (btnQuit  != null) btnQuit.clicked  += OnQuit;

        if (howToOverlay != null) howToOverlay.style.display = DisplayStyle.None;

        root.RegisterCallback<KeyDownEvent>(e =>
        {
            if (howToOverlay != null && howToOverlay.resolvedStyle.display != DisplayStyle.None &&
                (e.keyCode == KeyCode.Escape))
            {
                howToOverlay.style.display = DisplayStyle.None;
                e.StopPropagation();
            }
        });
    }

    void Update()
    {
        if (howToOverlay != null && howToOverlay.resolvedStyle.display != DisplayStyle.None)
        {
            var kb = Keyboard.current;
            var gp = Gamepad.current;

            if ((kb != null && kb.escapeKey.wasPressedThisFrame) ||
                (gp != null && (gp.bButton.wasPressedThisFrame || gp.startButton.wasPressedThisFrame)))
            {
                howToOverlay.style.display = DisplayStyle.None;
            }
        }
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
