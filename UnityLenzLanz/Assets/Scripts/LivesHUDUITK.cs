using UnityEngine;
using UnityEngine.UIElements;

public class LivesHUDUITK : MonoBehaviour
{
    Label lbl;

    void Awake()
    {
        var ui = GetComponent<UIDocument>();
        if (ui == null)
        {
            Debug.LogError("[HUD] Kein UIDocument auf diesem GameObject. Füge eines hinzu und wähle HUD.uxml als Source Asset.");
            return;
        }

        var root = ui.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[HUD] UIDocument hat (noch) keinen VisualTree. Ist 'Source Asset' (HUD.uxml) gesetzt?");
            return;
        }

        lbl = root.Q<Label>("lblLives");
        if (lbl == null)
        {
            Debug.LogError("[HUD] Label mit Name 'lblLives' nicht gefunden. Prüfe den Name im UXML (nicht nur den Text!).");
            // Fallback-Label erzeugen, damit man trotzdem etwas sieht
            lbl = new Label("Leben: ?") { name = "lblLives" };
            lbl.style.position = Position.Absolute;
            lbl.style.left = 16; lbl.style.top = 16;
            lbl.style.fontSize = 22; lbl.style.color = Color.white;
            lbl.style.backgroundColor = new Color(0,0,0,0.35f);
            lbl.style.paddingLeft = lbl.style.paddingRight = 10;
            lbl.style.paddingTop = lbl.style.paddingBottom = 6;
            root.Add(lbl);
        }

        UpdateText(GameSession.I != null ? GameSession.I.Lives : 3);
        GameSession.OnLivesChanged += UpdateText;
    }

    void OnDestroy()
    {
        GameSession.OnLivesChanged -= UpdateText;
    }

    void UpdateText(int lives)
    {
        if (lbl != null) lbl.text = $"Leben: {lives}";
    }
}