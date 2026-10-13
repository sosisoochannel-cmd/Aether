using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;

/// <summary>Builds the first playable graybox slice; production art and verified RTL fonts are separate milestones.</summary>
[DisallowMultipleComponent]
public sealed class AetherGameBootstrap : MonoBehaviour
{
    private static readonly Color Ink = new Color(0.025f, 0.027f, 0.035f);
    private static readonly Color Gold = new Color(0.86f, 0.67f, 0.29f);
    private static readonly Color Muted = new Color(0.68f, 0.66f, 0.61f);
    private Canvas canvas;
    private Font font;
    private GameObject worldRoot;
    private static Sprite cachedWhiteSprite;
    private bool warningVisible;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        EnsureCamera();
        BuildCanvas();
        ShowStudioIntro();
    }

    private void EnsureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var cameraObject = new GameObject("Aether Camera");
            cameraObject.tag = "MainCamera";
            cam = cameraObject.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = Ink;
        cam.transform.position = new Vector3(0f, 1.5f, -10f);
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("Aether UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        if (FindObjectOfType<EventSystem>() == null)
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }


    private void ShowStudioIntro()
    {
        ClearUI();
        Panel("StudioIntroBackdrop", Ink, Vector2.zero, Vector2.one);
        Sprite logoSprite = Resources.Load<Sprite>("Brand/VarellonLogo");
        if (logoSprite != null)
        {
            var logoObject = new GameObject("Varellon Logo", typeof(RectTransform), typeof(Image));
            logoObject.transform.SetParent(canvas.transform, false);
            var logoRect = logoObject.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.sizeDelta = new Vector2(340f, 600f);
            var logoImage = logoObject.GetComponent<Image>();
            logoImage.sprite = logoSprite;
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;

            var glintObject = new GameObject("Single Logo Glint", typeof(RectTransform), typeof(Image));
            glintObject.transform.SetParent(canvas.transform, false);
            var glintRect = glintObject.GetComponent<RectTransform>();
            glintRect.anchorMin = new Vector2(0.5f, 0.5f);
            glintRect.anchorMax = new Vector2(0.5f, 0.5f);
            glintRect.sizeDelta = new Vector2(4f, 300f);
            glintRect.localRotation = Quaternion.Euler(0f, 0f, 18f);
            var glintImage = glintObject.GetComponent<Image>();
            glintImage.color = new Color(Gold.r, Gold.g, Gold.b, 0f);
            glintImage.raycastTarget = false;
            StartCoroutine(PlayLogoGlint(glintRect, glintImage));
        }
        else
        {
            Label("VARELLON", 52, Gold, new Vector2(0.5f, 0.5f), new Vector2(0.7f, 0.16f), TextAnchor.MiddleCenter);
        }
        StartCoroutine(ShowWarningAfterIntro());
    }

    private IEnumerator PlayLogoGlint(RectTransform glint, Image image)
    {
        yield return new WaitForSecondsRealtime(0.35f);
        const float duration = 1.15f;
        float elapsed = 0f;
        while (elapsed < duration && glint != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            glint.anchoredPosition = new Vector2(Mathf.Lerp(-260f, 260f, t), 0f);
            float intensity = Mathf.Sin(t * Mathf.PI);
            image.color = new Color(Gold.r, Gold.g, Gold.b, intensity * 0.58f);
            yield return null;
        }
        if (image != null) image.color = new Color(Gold.r, Gold.g, Gold.b, 0f);
    }

    private IEnumerator ShowWarningAfterIntro()
    {
        yield return new WaitForSecondsRealtime(2.1f);
        ShowWarning();
    }

    private IEnumerator DismissWarningAfterDelay()
    {
        yield return new WaitForSecondsRealtime(7f);
        if (warningVisible) ShowMenu();
    }

    private void ShowWarning()
    {
        warningVisible = true;
        ClearUI();
        Panel("WarningPanel", Ink, Vector2.zero, Vector2.one);
        Label("هشدار", 54, Gold, new Vector2(0.5f, 0.83f), new Vector2(0.8f, 0.14f), TextAnchor.MiddleCenter);
        Label("این بازی در حال توسعه است. پیشرفت بازی ممکن است تغییر کند و برخی بخش‌ها هنوز نهایی نشده‌اند.\nبرای تجربه بهتر، دستگاه خود را به‌روز نگه دارید و در محیط امن بازی کنید.\nبا ادامه دادن، وارد نسخه آزمایشی Aether می‌شوید.", 27, Color.white,
            new Vector2(0.16f, 0.30f), new Vector2(0.68f, 0.43f), TextAnchor.MiddleCenter);
        MakeButton("ادامه", new Vector2(0.5f, 0.13f), new Vector2(0.22f, 0.09f), ShowMenu, true);
        StopCoroutine(nameof(DismissWarningAfterDelay));
        StartCoroutine(DismissWarningAfterDelay());
    }

    private void ShowMenu()
    {
        warningVisible = false;
        ClearUI();
        Panel("MenuBackdrop", Ink, Vector2.zero, Vector2.one);
        Label("V A R E L L O N", 23, Muted, new Vector2(0.5f, 0.82f), new Vector2(0.7f, 0.07f), TextAnchor.MiddleCenter);
        Label("AETHER", 92, Gold, new Vector2(0.5f, 0.62f), new Vector2(0.8f, 0.18f), TextAnchor.MiddleCenter);
        Label("THE FIRST PLAYABLE SLICE", 20, Muted, new Vector2(0.5f, 0.51f), new Vector2(0.7f, 0.06f), TextAnchor.MiddleCenter);
        MakeButton("شروع بازی", new Vector2(0.5f, 0.35f), new Vector2(0.27f, 0.09f), StartGame, true);
        MakeButton("بازگشت به هشدار", new Vector2(0.5f, 0.22f), new Vector2(0.27f, 0.075f), ShowWarning, false);
    }

    private void StartGame()
    {
        ClearUI();
        if (worldRoot != null) Destroy(worldRoot);
        worldRoot = new GameObject("Aether Prototype World");
        CreateBlock("Ground", new Vector2(0f, -3.1f), new Vector2(32f, 1f), new Color(0.10f, 0.105f, 0.12f), true);
        CreateBlock("First Step", new Vector2(4.5f, -1.6f), new Vector2(3.5f, 0.35f), Gold, true);
        CreateBlock("Second Step", new Vector2(9f, 0.0f), new Vector2(3f, 0.35f), new Color(0.34f, 0.28f, 0.18f), true);
        CreateBlock("High Ledge", new Vector2(13f, 1.3f), new Vector2(3f, 0.35f), Gold, true);
        var player = CreateBlock("Player", new Vector2(-5f, -1.8f), new Vector2(0.72f, 1.25f), new Color(0.88f, 0.86f, 0.80f), false);
        player.AddComponent<BoxCollider2D>();
        player.AddComponent<Rigidbody2D>().gravityScale = 3.2f;
        player.AddComponent<PrototypePlayerMotor>();
        Camera cam = Camera.main;
        var follow = cam.gameObject.GetComponent<PrototypeCameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<PrototypeCameraFollow>();
        follow.Target = player.transform;
        BuildGameplayUI();
    }

    private void BuildGameplayUI()
    {
        Panel("GameplayHUD", new Color(0f, 0f, 0f, 0.15f), Vector2.zero, Vector2.one);
        Label("AETHER  /  GREENWAY TEST", 19, Gold, new Vector2(0.5f, 0.94f), new Vector2(0.7f, 0.06f), TextAnchor.MiddleCenter);
        Label("MOVE • JUMP • EXPLORE", 18, Muted, new Vector2(0.5f, 0.88f), new Vector2(0.7f, 0.05f), TextAnchor.MiddleCenter);
        MakeTouchButton("◀", new Vector2(0.09f, 0.16f), new Vector2(0.105f, 0.16f), TouchHoldButton.Action.Left);
        MakeTouchButton("▶", new Vector2(0.215f, 0.16f), new Vector2(0.105f, 0.16f), TouchHoldButton.Action.Right);
        MakeTouchButton("JUMP", new Vector2(0.89f, 0.17f), new Vector2(0.14f, 0.19f), TouchHoldButton.Action.Jump);
        MakeButton("MENU", new Vector2(0.94f, 0.92f), new Vector2(0.08f, 0.07f), ReturnToMenu, false);
    }

    private void ReturnToMenu()
    {
        TouchHoldButton.ResetState();
        if (worldRoot != null) Destroy(worldRoot);
        if (Camera.main != null)
        {
            var follow = Camera.main.GetComponent<PrototypeCameraFollow>();
            if (follow != null) Destroy(follow);
            Camera.main.transform.position = new Vector3(0f, 1.5f, -10f);
        }
        ShowMenu();
    }

    private GameObject CreateBlock(string objectName, Vector2 position, Vector2 size, Color color, bool solid)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(worldRoot.transform);
        go.transform.position = position;
        go.transform.localScale = size;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = WhiteSprite();
        renderer.color = color;
        if (solid) go.AddComponent<BoxCollider2D>();
        return go;
    }

    private static Sprite WhiteSprite()
    {
        if (cachedWhiteSprite != null) return cachedWhiteSprite;
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.name = "Aether Runtime Pixel";
        cachedWhiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedWhiteSprite;
    }

    private void Panel(string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
    }

    private Text Label(string value, int size, Color color, Vector2 center, Vector2 dimensions, TextAnchor alignment)
    {
        var go = new GameObject("Label: " + value, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = center - dimensions * 0.5f;
        rect.anchorMax = center + dimensions * 0.5f;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        var text = go.GetComponent<Text>();
        text.text = value; text.font = font; text.fontSize = size; text.color = color; text.raycastTarget = false;
        text.alignment = alignment; text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private void MakeButton(string title, Vector2 center, Vector2 dimensions, UnityEngine.Events.UnityAction action, bool primary)
    {
        var go = new GameObject("Button: " + title, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = center - dimensions * 0.5f; rect.anchorMax = center + dimensions * 0.5f;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.color = primary ? new Color(Gold.r, Gold.g, Gold.b, 0.15f) : new Color(1f, 1f, 1f, 0.035f);
        var button = go.GetComponent<Button>();
        var colors = button.colors; colors.normalColor = image.color; colors.highlightedColor = new Color(Gold.r, Gold.g, Gold.b, 0.35f);
        colors.pressedColor = new Color(Gold.r, Gold.g, Gold.b, 0.55f); button.colors = colors;
        button.onClick.AddListener(action);
        var label = Label(title, 25, primary ? Gold : Color.white, center, dimensions * 0.92f, TextAnchor.MiddleCenter);
        label.transform.SetAsLastSibling();
    }

    private void MakeTouchButton(string title, Vector2 center, Vector2 dimensions, TouchHoldButton.Action action)
    {
        var go = new GameObject("Touch: " + title, typeof(RectTransform), typeof(Image), typeof(TouchHoldButton));
        go.transform.SetParent(canvas.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = center - dimensions * 0.5f; rect.anchorMax = center + dimensions * 0.5f;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, 0.16f);
        go.GetComponent<TouchHoldButton>().Configure(action);
        var label = Label(title, title == "JUMP" ? 24 : 42, Gold, center, dimensions * 0.9f, TextAnchor.MiddleCenter);
        label.transform.SetAsLastSibling();
    }

    private void ClearUI()
    {
        if (canvas == null) return;
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            Destroy(canvas.transform.GetChild(i).gameObject);
    }
}
