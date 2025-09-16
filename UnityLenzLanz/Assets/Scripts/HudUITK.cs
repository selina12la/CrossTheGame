using UnityEngine;
using UnityEngine.UIElements;

public class HudUITK : MonoBehaviour
{
    private UIDocument _doc;
    private Label _lblLives;
    private Label _lblScore;

    void Awake()
    {
        _doc = GetComponent<UIDocument>();
        if (_doc == null)
        {
            Debug.LogError("[HUD] Kein UIDocument gefunden.");
            return;
        }

        var root = _doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[HUD] UIDocument hat kein RootVisualElement (Source Asset gesetzt?).");
            return;
        }

        _lblLives = root.Q<Label>("lblLives");
        _lblScore = root.Q<Label>("lblScore");

        if (GameSession.I != null)
        {
            UpdateLives(GameSession.I.Lives);
            UpdateScore(GameSession.I.Score);
        }

        GameSession.OnLivesChanged += UpdateLives;
        GameSession.OnScoreChanged += UpdateScore;
    }

    void OnDestroy()
    {
        GameSession.OnLivesChanged -= UpdateLives;
        GameSession.OnScoreChanged -= UpdateScore;
    }

    private void UpdateLives(int v)
    {
        if (_lblLives != null) _lblLives.text = $"Leben: {v}";
    }

    private void UpdateScore(int v)
    {
        if (_lblScore != null) _lblScore.text = $"Punkte: {v}";
    }
}