#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace MasqueradeMystery.Editor
{
    public class SceneSetupHelper : EditorWindow
    {
        [MenuItem("Masquerade Mystery/Setup Scene")]
        public static void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("Setup Masquerade Scene",
                "This will create the game hierarchy in the current scene. Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            CreateManagers();
            CreateCamera();
            CreateBackground();
            CreateUI();
            CreateCharacterPrefab();

            Debug.Log("Scene setup complete! Remember to:\n" +
                "1. Assign the Character prefab to the CharacterSpawner\n" +
                "2. Connect UI references in the GameManager\n" +
                "3. Save the scene");
        }

        [MenuItem("Masquerade Mystery/Create Character Prefab")]
        public static void CreateCharacterPrefab()
        {
            // Check if prefab already exists
            string prefabPath = "Assets/MasqueradeMystery/Prefabs/Character.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("Character prefab already exists at " + prefabPath);
                return;
            }

            // Create character GameObject
            GameObject character = new GameObject("Character");

            // Add components
            var charComponent = character.AddComponent<Character>();
            var visuals = character.AddComponent<CharacterVisuals>();
            var hoverable = character.AddComponent<CharacterHoverable>();
            var animator = character.AddComponent<CharacterAnimator>();
            var collider = character.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1.5f);
            collider.offset = new Vector2(0, 0.25f);

            // Create body sprite
            GameObject body = new GameObject("Body");
            body.transform.SetParent(character.transform);
            body.transform.localPosition = Vector3.zero;
            var bodySR = body.AddComponent<SpriteRenderer>();
            bodySR.sprite = CreatePlaceholderSprite(Color.white, "Body");
            bodySR.sortingOrder = 0;

            // Create mask sprite
            GameObject mask = new GameObject("Mask");
            mask.transform.SetParent(character.transform);
            mask.transform.localPosition = new Vector3(0, 0.6f, 0);
            mask.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            var maskSR = mask.AddComponent<SpriteRenderer>();
            maskSR.sprite = CreatePlaceholderSprite(Color.white, "Mask");
            maskSR.sortingOrder = 1;

            // Create accessory sprite
            GameObject accessory = new GameObject("Accessory");
            accessory.transform.SetParent(character.transform);
            accessory.transform.localPosition = new Vector3(0, 0.2f, 0);
            accessory.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            var accessorySR = accessory.AddComponent<SpriteRenderer>();
            accessorySR.sprite = CreatePlaceholderSprite(Color.white, "Accessory");
            accessorySR.sortingOrder = 2;
            accessory.SetActive(false);

            // Create partner indicator
            GameObject partnerIndicator = new GameObject("PartnerIndicator");
            partnerIndicator.transform.SetParent(character.transform);
            partnerIndicator.transform.localPosition = new Vector3(0, -0.5f, 0);
            partnerIndicator.transform.localScale = new Vector3(0.5f, 0.1f, 1f);
            var partnerSR = partnerIndicator.AddComponent<SpriteRenderer>();
            partnerSR.sprite = CreatePlaceholderSprite(Color.magenta, "Partner");
            partnerSR.sortingOrder = -1;
            partnerIndicator.SetActive(false);

            // Connect serialized fields via SerializedObject
            SerializedObject so = new SerializedObject(visuals);
            so.FindProperty("bodyRenderer").objectReferenceValue = bodySR;
            so.FindProperty("maskRenderer").objectReferenceValue = maskSR;
            so.FindProperty("accessoryRenderer").objectReferenceValue = accessorySR;
            so.FindProperty("partnerLineRenderer").objectReferenceValue = partnerSR;
            so.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject charSO = new SerializedObject(charComponent);
            charSO.FindProperty("visuals").objectReferenceValue = visuals;
            charSO.FindProperty("animator").objectReferenceValue = animator;
            charSO.ApplyModifiedPropertiesWithoutUndo();

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/MasqueradeMystery/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/MasqueradeMystery", "Prefabs");
            }

            // Create prefab
            PrefabUtility.SaveAsPrefabAsset(character, prefabPath);
            DestroyImmediate(character);

            Debug.Log("Character prefab created at " + prefabPath);
        }

        private static void CreateManagers()
        {
            // Create Managers parent
            GameObject managers = new GameObject("--- MANAGERS ---");

            // GameManager
            GameObject gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(managers.transform);
            gmObj.AddComponent<GameManager>();

            // CharacterSpawner
            GameObject spawnerObj = new GameObject("CharacterSpawner");
            spawnerObj.transform.SetParent(managers.transform);
            spawnerObj.AddComponent<CharacterSpawner>();

            // Connect spawner to game manager
            var gm = gmObj.GetComponent<GameManager>();
            var spawner = spawnerObj.GetComponent<CharacterSpawner>();
            SerializedObject gmSO = new SerializedObject(gm);
            gmSO.FindProperty("spawner").objectReferenceValue = spawner;
            gmSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateCamera()
        {
            // Find or create camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            // Configure for 2D
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5;
            mainCam.transform.position = new Vector3(0, 0, -10);

            // Add controller
            if (mainCam.GetComponent<CameraController>() == null)
            {
                mainCam.gameObject.AddComponent<CameraController>();
            }
        }

        private static void CreateBackground()
        {
            GameObject world = new GameObject("--- WORLD ---");

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(world.transform);
            bg.transform.position = Vector3.zero;
            bg.transform.localScale = new Vector3(25, 15, 1);

            var sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite(new Color(0.15f, 0.1f, 0.2f), "Background");
            sr.sortingOrder = -100;
        }

        private static void CreateUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Configure CanvasScaler for different screen sizes
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Hint Panel (top-left)
            CreateHintPanel(canvasObj.transform);

            // Hover Info Panel
            CreateHoverInfoPanel(canvasObj.transform);

            // Game Status Panel (top-right)
            CreateGameStatusPanel(canvasObj.transform);

            // Game Over Panel (center)
            CreateGameOverPanel(canvasObj.transform);
        }

        private static void CreateHintPanel(Transform parent)
        {
            GameObject panel = CreatePanel("HintPanel", parent, new Vector2(250, 200));
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);

            // Add background
            var img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.7f);

            // Add HintPanelUI component
            var hintUI = panel.AddComponent<HintPanelUI>();

            // Title
            GameObject titleObj = CreateTextObject("Title", panel.transform, "Find the target:");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(-20, 30);

            // Hint container
            GameObject container = new GameObject("HintContainer");
            container.transform.SetParent(panel.transform);
            var containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.offsetMin = new Vector2(10, 10);
            containerRect.offsetMax = new Vector2(-10, -45);
            var vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.spacing = 5;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            // Hint text prefab (hidden)
            GameObject hintPrefab = CreateTextObject("HintTextPrefab", panel.transform, "• Hint text");
            hintPrefab.SetActive(false);
            var hintPrefabRect = hintPrefab.GetComponent<RectTransform>();
            hintPrefabRect.sizeDelta = new Vector2(230, 25);

            // Connect references
            SerializedObject so = new SerializedObject(hintUI);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("hintContainer").objectReferenceValue = container.transform;
            so.FindProperty("hintTextPrefab").objectReferenceValue = hintPrefab.GetComponent<TMP_Text>();
            so.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateHoverInfoPanel(Transform parent)
        {
            GameObject panel = CreatePanel("HoverInfoPanel", parent, new Vector2(200, 120));

            // Add background
            var img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Add HoverInfoUI component
            var hoverUI = panel.AddComponent<HoverInfoUI>();

            // Create info texts
            string[] labels = { "Mask:", "Clothing:", "Accessory:", "Status:" };
            string[] names = { "MaskText", "ClothingText", "AccessoryText", "DanceText" };
            TMP_Text[] texts = new TMP_Text[4];

            for (int i = 0; i < 4; i++)
            {
                GameObject textObj = CreateTextObject(names[i], panel.transform, labels[i] + " ---");
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 1);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.pivot = new Vector2(0, 1);
                textRect.anchoredPosition = new Vector2(10, -10 - (i * 25));
                textRect.sizeDelta = new Vector2(-20, 25);
                texts[i] = textObj.GetComponent<TMP_Text>();
                texts[i].fontSize = 14;
                texts[i].alignment = TextAlignmentOptions.Left;
            }

            panel.SetActive(false);

            // Connect references
            SerializedObject so = new SerializedObject(hoverUI);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("maskText").objectReferenceValue = texts[0];
            so.FindProperty("clothingText").objectReferenceValue = texts[1];
            so.FindProperty("accessoryText").objectReferenceValue = texts[2];
            so.FindProperty("danceText").objectReferenceValue = texts[3];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateGameStatusPanel(Transform parent)
        {
            GameObject panel = CreatePanel("GameStatusPanel", parent, new Vector2(200, 80));
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);

            var img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.7f);

            var statusUI = panel.AddComponent<GameStatusUI>();

            // Guesses text
            GameObject guessesObj = CreateTextObject("GuessesText", panel.transform, "Guesses remaining: 3");
            var guessesRect = guessesObj.GetComponent<RectTransform>();
            guessesRect.anchorMin = new Vector2(0, 0.5f);
            guessesRect.anchorMax = new Vector2(1, 1);
            guessesRect.offsetMin = new Vector2(10, 5);
            guessesRect.offsetMax = new Vector2(-10, -5);

            // Status text
            GameObject statusObj = CreateTextObject("StatusText", panel.transform, "");
            var statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 0.5f);
            statusRect.offsetMin = new Vector2(10, 5);
            statusRect.offsetMax = new Vector2(-10, -5);
            statusObj.GetComponent<TMP_Text>().color = Color.yellow;
            statusObj.SetActive(false);

            SerializedObject so = new SerializedObject(statusUI);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("guessesText").objectReferenceValue = guessesObj.GetComponent<TMP_Text>();
            so.FindProperty("statusText").objectReferenceValue = statusObj.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateGameOverPanel(Transform parent)
        {
            GameObject panel = CreatePanel("GameOverPanel", parent, new Vector2(400, 250));
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var img = panel.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            var gameOverUI = panel.AddComponent<GameOverUI>();

            // Title
            GameObject titleObj = CreateTextObject("Title", panel.transform, "Game Over");
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(-40, 50);
            var titleText = titleObj.GetComponent<TMP_Text>();
            titleText.fontSize = 36;
            titleText.alignment = TextAlignmentOptions.Center;

            // Message
            GameObject msgObj = CreateTextObject("Message", panel.transform, "Message text here");
            var msgRect = msgObj.GetComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0, 0.4f);
            msgRect.anchorMax = new Vector2(1, 0.7f);
            msgRect.offsetMin = new Vector2(20, 0);
            msgRect.offsetMax = new Vector2(-20, 0);
            var msgText = msgObj.GetComponent<TMP_Text>();
            msgText.alignment = TextAlignmentOptions.Center;

            // Buttons
            GameObject restartBtn = CreateButton("RestartButton", panel.transform, "Play Again");
            var restartRect = restartBtn.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0.1f, 0.1f);
            restartRect.anchorMax = new Vector2(0.45f, 0.3f);
            restartRect.offsetMin = Vector2.zero;
            restartRect.offsetMax = Vector2.zero;

            GameObject quitBtn = CreateButton("QuitButton", panel.transform, "Quit");
            var quitRect = quitBtn.GetComponent<RectTransform>();
            quitRect.anchorMin = new Vector2(0.55f, 0.1f);
            quitRect.anchorMax = new Vector2(0.9f, 0.3f);
            quitRect.offsetMin = Vector2.zero;
            quitRect.offsetMax = Vector2.zero;

            panel.SetActive(false);

            SerializedObject so = new SerializedObject(gameOverUI);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("titleText").objectReferenceValue = titleText;
            so.FindProperty("messageText").objectReferenceValue = msgText;
            so.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
            so.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            // Connect to GameManager
            var gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                gmSO.FindProperty("gameStatusUI").objectReferenceValue = FindObjectOfType<GameStatusUI>();
                gmSO.FindProperty("gameOverUI").objectReferenceValue = gameOverUI;
                gmSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);
            var rect = panel.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            return panel;
        }

        private static GameObject CreateTextObject(string name, Transform parent, string text)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            var rect = obj.AddComponent<RectTransform>();
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.color = Color.white;
            return obj;
        }

        private static GameObject CreateButton(string name, Transform parent, string text)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent);
            var rect = btnObj.AddComponent<RectTransform>();
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.4f);
            var btn = btnObj.AddComponent<Button>();

            GameObject textObj = CreateTextObject("Text", btnObj.transform, text);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textObj.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            return btnObj;
        }

        private static Sprite CreatePlaceholderSprite(Color color, string name)
        {
            // Ensure placeholders folder exists
            if (!AssetDatabase.IsValidFolder("Assets/MasqueradeMystery/Art"))
            {
                AssetDatabase.CreateFolder("Assets/MasqueradeMystery", "Art");
            }
            if (!AssetDatabase.IsValidFolder("Assets/MasqueradeMystery/Art/Placeholders"))
            {
                AssetDatabase.CreateFolder("Assets/MasqueradeMystery/Art", "Placeholders");
            }

            string texturePath = $"Assets/MasqueradeMystery/Art/Placeholders/{name}_Placeholder.png";

            // Check if sprite already exists
            Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            if (existingSprite != null)
            {
                return existingSprite;
            }

            // Create a simple white square texture
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            tex.SetPixels(pixels);
            tex.Apply();

            // Save texture as PNG
            byte[] pngData = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(texturePath, pngData);
            AssetDatabase.ImportAsset(texturePath);

            // Configure texture import settings for sprites
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            // Load and return the sprite
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            return sprite;
        }

        [MenuItem("Masquerade Mystery/Fix Existing Scene")]
        public static void FixExistingScene()
        {
            // Fix Canvas Scaler
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                    EditorUtility.SetDirty(scaler);
                    Debug.Log("Fixed Canvas Scaler settings");
                }
            }

            // Recreate Character prefab with proper sprites
            string prefabPath = "Assets/MasqueradeMystery/Prefabs/Character.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
                Debug.Log("Deleted old Character prefab");
            }
            CreateCharacterPrefab();

            // Reassign prefab to spawner
            CharacterSpawner spawner = FindObjectOfType<CharacterSpawner>();
            if (spawner != null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    SerializedObject so = new SerializedObject(spawner);
                    so.FindProperty("characterPrefab").objectReferenceValue = prefab.GetComponent<Character>();
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(spawner);
                    Debug.Log("Assigned Character prefab to CharacterSpawner");
                }
            }

            Debug.Log("Scene fix complete! Save the scene and press Play.");
        }

        [MenuItem("Masquerade Mystery/Create or Fix UI")]
        public static void CreateOrFixUI()
        {
            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // Delete existing canvas and recreate
                DestroyImmediate(canvas.gameObject);
                Debug.Log("Removed existing Canvas to recreate UI");
            }

            // Create fresh UI
            CreateUI();

            // Reconnect GameManager references
            var gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                gmSO.FindProperty("gameStatusUI").objectReferenceValue = FindObjectOfType<GameStatusUI>();
                gmSO.FindProperty("gameOverUI").objectReferenceValue = FindObjectOfType<GameOverUI>();
                gmSO.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gm);
                Debug.Log("Connected UI references to GameManager");
            }

            Debug.Log("UI created! Save the scene and press Play.");
        }
    }
}
#endif
