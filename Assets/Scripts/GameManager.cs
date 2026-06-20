using UnityEngine;
using UnityEngine.UI;

// Tracks the score and draws a simple on-screen HUD.
// It builds its own Canvas + Text at runtime so the scene setup
// stays simple. There should be exactly one in the scene.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool HasKey => hasKey;

    private int score = 0;
    private int totalCoins = 0;
    private bool keySpawned = false;
    private bool hasKey = false;
    private Text label;
    private Text inventoryLabel;
    private Image inventoryPanel;
    private Image keySlot;
    private Text keySlotLabel;

    void Awake()
    {
        Instance = this;
        BuildHud();
        totalCoins = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
        UpdateLabel();
        UpdateInventory();
    }

    public void AddPoint()
    {
        score++;

        if (!keySpawned && totalCoins > 0 && score >= totalCoins)
            SpawnKey();

        UpdateLabel();
    }

    void SpawnKey()
    {
        keySpawned = true;

        Vector3 spawnPosition = new Vector3(0f, 0.85f, 0f);
        var keyRoot = new GameObject("House Key");
        keyRoot.transform.position = spawnPosition;

        Color gold = new Color(1f, 0.78f, 0.12f);
        Color darkGold = new Color(0.72f, 0.45f, 0.06f);

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Key Ring";
        ring.transform.SetParent(keyRoot.transform, false);
        ring.transform.localPosition = new Vector3(-0.45f, 0f, 0f);
        ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.transform.localScale = new Vector3(0.28f, 0.045f, 0.28f);
        ring.GetComponent<Renderer>().material.color = gold;
        Destroy(ring.GetComponent<Collider>());

        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = "Key Shaft";
        shaft.transform.SetParent(keyRoot.transform, false);
        shaft.transform.localPosition = new Vector3(0.15f, 0f, 0f);
        shaft.transform.localScale = new Vector3(0.85f, 0.12f, 0.12f);
        shaft.GetComponent<Renderer>().material.color = gold;
        Destroy(shaft.GetComponent<Collider>());

        var toothA = GameObject.CreatePrimitive(PrimitiveType.Cube);
        toothA.name = "Key Tooth";
        toothA.transform.SetParent(keyRoot.transform, false);
        toothA.transform.localPosition = new Vector3(0.55f, -0.15f, 0f);
        toothA.transform.localScale = new Vector3(0.16f, 0.28f, 0.12f);
        toothA.GetComponent<Renderer>().material.color = darkGold;
        Destroy(toothA.GetComponent<Collider>());

        var toothB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        toothB.name = "Key Tooth";
        toothB.transform.SetParent(keyRoot.transform, false);
        toothB.transform.localPosition = new Vector3(0.33f, -0.12f, 0f);
        toothB.transform.localScale = new Vector3(0.14f, 0.22f, 0.12f);
        toothB.GetComponent<Renderer>().material.color = darkGold;
        Destroy(toothB.GetComponent<Collider>());

        var trigger = keyRoot.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.75f;
        keyRoot.AddComponent<KeyPickup>();
    }

    public void CollectKey()
    {
        hasKey = true;
        if (label != null)
            label.text = "Key collected! Next: open a house door.";
        UpdateInventory();
    }

    public void UseKey()
    {
        hasKey = false;
        UpdateInventory();
    }

    void UpdateInventory()
    {
        if (inventoryLabel == null) return;

        inventoryLabel.text = hasKey ? "Inventory" : "Inventory: Empty";
        if (keySlot != null)
            keySlot.color = hasKey ? new Color(1f, 0.78f, 0.12f, 0.95f) : new Color(0f, 0f, 0f, 0.38f);
        if (keySlotLabel != null)
            keySlotLabel.text = hasKey ? "Key" : "";
    }

    public void ShowMessage(string message)
    {
        if (label != null)
            label.text = message;
    }

    void UpdateLabel()
    {
        if (label == null) return;

        if (keySpawned)
            label.text = "All coins collected. Find the key near the house!";
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
        rt.sizeDelta = new Vector2(760f, 120f);

        var inventoryPanelGO = new GameObject("Inventory Bar");
        inventoryPanelGO.transform.SetParent(canvasGO.transform, false);
        inventoryPanel = inventoryPanelGO.AddComponent<Image>();
        inventoryPanel.color = new Color(0f, 0f, 0f, 0.62f);
        var panelRt = inventoryPanel.rectTransform;
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.anchoredPosition = new Vector2(0f, 16f);
        panelRt.sizeDelta = new Vector2(540f, 92f);

        var inventoryGO = new GameObject("Inventory Text");
        inventoryGO.transform.SetParent(inventoryPanelGO.transform, false);
        inventoryLabel = inventoryGO.AddComponent<Text>();
        inventoryLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inventoryLabel.fontSize = 24;
        inventoryLabel.alignment = TextAnchor.MiddleLeft;
        inventoryLabel.color = Color.white;
        var invRt = inventoryLabel.rectTransform;
        invRt.anchorMin = new Vector2(0f, 1f);
        invRt.anchorMax = new Vector2(1f, 1f);
        invRt.pivot = new Vector2(0.5f, 1f);
        invRt.anchoredPosition = new Vector2(0f, -6f);
        invRt.sizeDelta = new Vector2(-24f, 28f);

        var keySlotGO = new GameObject("Key Slot");
        keySlotGO.transform.SetParent(inventoryPanelGO.transform, false);
        keySlot = keySlotGO.AddComponent<Image>();
        keySlot.color = new Color(0f, 0f, 0f, 0.38f);
        var slotRt = keySlot.rectTransform;
        slotRt.anchorMin = new Vector2(0f, 0f);
        slotRt.anchorMax = new Vector2(0f, 0f);
        slotRt.pivot = new Vector2(0f, 0f);
        slotRt.anchoredPosition = new Vector2(18f, 14f);
        slotRt.sizeDelta = new Vector2(62f, 46f);

        var keyLabelGO = new GameObject("Key Slot Label");
        keyLabelGO.transform.SetParent(keySlotGO.transform, false);
        keySlotLabel = keyLabelGO.AddComponent<Text>();
        keySlotLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        keySlotLabel.fontSize = 20;
        keySlotLabel.alignment = TextAnchor.MiddleCenter;
        keySlotLabel.color = Color.black;
        var keyLabelRt = keySlotLabel.rectTransform;
        keyLabelRt.anchorMin = Vector2.zero;
        keyLabelRt.anchorMax = Vector2.one;
        keyLabelRt.offsetMin = Vector2.zero;
        keyLabelRt.offsetMax = Vector2.zero;

        // --- Crosshair ---
        var crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvasGO.transform, false);

        // Horizontal bar
        var hBar = new GameObject("Crosshair H");
        hBar.transform.SetParent(crosshairGO.transform, false);
        var hImg = hBar.AddComponent<Image>();
        hImg.color = new Color(1f, 1f, 1f, 0.85f);
        var hRt = hImg.rectTransform;
        hRt.anchorMin = new Vector2(0.5f, 0.5f);
        hRt.anchorMax = new Vector2(0.5f, 0.5f);
        hRt.pivot = new Vector2(0.5f, 0.5f);
        hRt.anchoredPosition = Vector2.zero;
        hRt.sizeDelta = new Vector2(20f, 2f);

        // Vertical bar
        var vBar = new GameObject("Crosshair V");
        vBar.transform.SetParent(crosshairGO.transform, false);
        var vImg = vBar.AddComponent<Image>();
        vImg.color = new Color(1f, 1f, 1f, 0.85f);
        var vRt = vImg.rectTransform;
        vRt.anchorMin = new Vector2(0.5f, 0.5f);
        vRt.anchorMax = new Vector2(0.5f, 0.5f);
        vRt.pivot = new Vector2(0.5f, 0.5f);
        vRt.anchoredPosition = Vector2.zero;
        vRt.sizeDelta = new Vector2(2f, 20f);
    }
}
public class KeyPickup : MonoBehaviour
{
    public float hoverSpeed = 2f;
    public float hoverHeight = 0.18f;
    public float spinSpeed = 90f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * hoverSpeed) * hoverHeight);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.CollectKey();

        Destroy(gameObject);
    }
}