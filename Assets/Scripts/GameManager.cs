using UnityEngine;
using UnityEngine.UI;

// Tracks the score and draws a simple on-screen HUD.
// It builds its own Canvas + Text at runtime so the scene setup
// stays simple. There should be exactly one in the scene.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int score = 0;
    private int totalCoins = 0;
    private Text label;

    void Awake()
    {
        Instance = this;
        BuildHud();
        totalCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
        UpdateLabel();
    }

    public void AddPoint()
    {
        score++;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (label == null) return;

        if (totalCoins > 0 && score >= totalCoins)
            label.text = "All " + totalCoins + " coins collected -- you win!";
        else
            label.text = "Coins: " + score + " / " + totalCoins;
    }

    void BuildHud()
    {
        var canvasGO = new GameObject("HUD Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var textGO = new GameObject("Score Text");
        textGO.transform.SetParent(canvasGO.transform, false);

        label = textGO.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 30;
        label.color = Color.white;

        var rt = label.rectTransform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -20f);
        rt.sizeDelta = new Vector2(700f, 120f);
    }
}
