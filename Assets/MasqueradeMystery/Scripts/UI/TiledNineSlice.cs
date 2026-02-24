#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace MasqueradeMystery
{
    /// <summary>
    /// Renders a 9-slice sprite with tiled edges and center (no stretching).
    /// Creates 9 child Image objects: 4 corners (Simple), 4 edges (Tiled), 1 center (Tiled).
    /// The source sprite must have border values set in the Sprite Editor.
    ///
    /// Uses a custom shader to remap the sprite's alpha so that semi-transparent
    /// fill pixels (e.g. 55% in the source) can be driven to any target opacity,
    /// while fully transparent pixels (alpha=0) remain untouched.
    /// </summary>
    [ExecuteAlways]
    public class TiledNineSlice : MonoBehaviour
    {
        [SerializeField] private Sprite sourceSprite;
        [SerializeField] private Color color = Color.white;
        [SerializeField, Range(0f, 1f)] private float opacity = 0.9f;

        private Sprite[] _subSprites;
        private Transform _container;
        private Material _material;

        private static readonly string[] SliceNames =
            { "BL", "B", "BR", "L", "C", "R", "TL", "T", "TR" };

        private static readonly int OpacityID = Shader.PropertyToID("_Opacity");
        private static readonly int SpriteMaxAlphaID = Shader.PropertyToID("_SpriteMaxAlpha");

        private void OnEnable()
        {
            Rebuild();
        }

        private void OnDestroy()
        {
            CleanupSprites();
            if (_material != null)
            {
                if (Application.isPlaying)
                    Destroy(_material);
                else
                    DestroyImmediate(_material);
                _material = null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EditorApplication.delayCall += () =>
            {
                if (this != null) Rebuild();
            };
        }
#endif

        public void SetSprite(Sprite sprite)
        {
            sourceSprite = sprite;
            Rebuild();
        }

        public void SetColor(Color c)
        {
            color = c;
            if (_container != null)
            {
                foreach (var img in _container.GetComponentsInChildren<Image>())
                {
                    img.color = color;
                }
            }
        }

        public void SetOpacity(float value)
        {
            opacity = Mathf.Clamp01(value);
            if (_material != null)
            {
                _material.SetFloat(OpacityID, opacity);
            }
        }

        private Material GetOrCreateMaterial()
        {
            if (_material != null) return _material;

            var shader = Shader.Find("UI/Remap Alpha");
            if (shader == null)
            {
                Debug.LogWarning("TiledNineSlice: 'UI/Remap Alpha' shader not found, falling back to default UI.");
                shader = Shader.Find("UI/Default");
            }

            _material = new Material(shader);
            _material.name = "TiledNineSlice (Instance)";
            return _material;
        }

        private float DetectMaxAlpha(Texture2D tex)
        {
            // The sprite has 3 alpha levels: 0, 141/255 (~0.553), and 255/255 (1.0).
            // The "fill" is 0.553. We want _Opacity to control what that fill becomes.
            // Borders at 1.0 will map to _Opacity/0.553, clamped to 1 by saturate().
            // So _SpriteMaxAlpha should be the fill alpha, not the absolute max.
            //
            // If the texture is readable, detect it. Otherwise default to 0.553.
            if (!tex.isReadable) return 141f / 255f;

            // Sample the center pixel to find the fill alpha
            int cx = tex.width / 2;
            int cy = tex.height / 2;
            return tex.GetPixel(cx, cy).a;
        }

        public void Rebuild()
        {
            if (sourceSprite == null || sourceSprite.texture == null) return;

            var tex = sourceSprite.texture;
            var border = sourceSprite.border; // (left, bottom, right, top) in pixels
            float bL = border.x;
            float bB = border.y;
            float bR = border.z;
            float bT = border.w;
            float texW = tex.width;
            float texH = tex.height;

            if (bL + bR > texW || bB + bT > texH) return;

            CleanupSprites();

            // Set up material with alpha remapping
            var mat = GetOrCreateMaterial();
            float maxAlpha = DetectMaxAlpha(tex);
            mat.SetFloat(SpriteMaxAlphaID, maxAlpha);
            mat.SetFloat(OpacityID, opacity);

            // Find or create container so slices stay behind panel content
            _container = transform.Find("__nine_slice__");
            if (_container == null)
            {
                var containerObj = new GameObject("__nine_slice__");
                containerObj.transform.SetParent(transform, false);
                containerObj.AddComponent<RectTransform>();
                _container = containerObj.transform;
            }

            // Extend the container outward so the decorative borders wrap
            // around the panel rather than eating into the content area.
            var crt = _container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(-bL, -bB);
            crt.offsetMax = new Vector2(bR, bT);

            _container.SetAsFirstSibling();

            // 9 regions in sprite pixel coordinates (y=0 is bottom of texture)
            var rects = new Rect[]
            {
                new Rect(0,         0,          bL,                 bB),                 // 0: BL
                new Rect(bL,        0,          texW - bL - bR,     bB),                 // 1: B
                new Rect(texW - bR, 0,          bR,                 bB),                 // 2: BR
                new Rect(0,         bB,         bL,                 texH - bB - bT),     // 3: L
                new Rect(bL,        bB,         texW - bL - bR,     texH - bB - bT),     // 4: C
                new Rect(texW - bR, bB,         bR,                 texH - bB - bT),     // 5: R
                new Rect(0,         texH - bT,  bL,                 bT),                 // 6: TL
                new Rect(bL,        texH - bT,  texW - bL - bR,     bT),                 // 7: T
                new Rect(texW - bR, texH - bT,  bR,                 bT),                 // 8: TR
            };

            // PPU = 100 matches default Canvas.referencePixelsPerUnit so
            // 48 sprite pixels = 48 UI units = 48 screen pixels at reference resolution.
            const float ppu = 100f;

            _subSprites = new Sprite[9];

            for (int i = 0; i < 9; i++)
            {
                if (rects[i].width <= 0 || rects[i].height <= 0) continue;

                _subSprites[i] = Sprite.Create(tex, rects[i], new Vector2(0.5f, 0.5f), ppu);
                _subSprites[i].name = SliceNames[i];

                Transform child = _container.Find(SliceNames[i]);
                GameObject obj;
                Image img;

                if (child == null)
                {
                    obj = new GameObject(SliceNames[i]);
                    obj.transform.SetParent(_container, false);
                    obj.AddComponent<RectTransform>();
                    img = obj.AddComponent<Image>();
                }
                else
                {
                    obj = child.gameObject;
                    img = obj.GetComponent<Image>();
                }

                img.sprite = _subSprites[i];
                img.color = color;
                img.material = mat;
                img.raycastTarget = false;

                bool isCorner = (i == 0 || i == 2 || i == 6 || i == 8);
                img.type = isCorner ? Image.Type.Simple : Image.Type.Tiled;

                PositionSlice(obj.GetComponent<RectTransform>(), i, bL, bB, bR, bT);
            }
        }

        private static void PositionSlice(RectTransform rt, int index, float l, float b, float r, float t)
        {
            switch (index)
            {
                case 0: // BL corner
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(0, 0);
                    rt.pivot = new Vector2(0, 0);
                    rt.sizeDelta = new Vector2(l, b);
                    rt.anchoredPosition = Vector2.zero;
                    break;
                case 1: // Bottom edge
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 0);
                    rt.offsetMin = new Vector2(l, 0);
                    rt.offsetMax = new Vector2(-r, b);
                    break;
                case 2: // BR corner
                    rt.anchorMin = new Vector2(1, 0);
                    rt.anchorMax = new Vector2(1, 0);
                    rt.pivot = new Vector2(1, 0);
                    rt.sizeDelta = new Vector2(r, b);
                    rt.anchoredPosition = Vector2.zero;
                    break;
                case 3: // Left edge
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(0, 1);
                    rt.offsetMin = new Vector2(0, b);
                    rt.offsetMax = new Vector2(l, -t);
                    break;
                case 4: // Center
                    rt.anchorMin = new Vector2(0, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.offsetMin = new Vector2(l, b);
                    rt.offsetMax = new Vector2(-r, -t);
                    break;
                case 5: // Right edge
                    rt.anchorMin = new Vector2(1, 0);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.offsetMin = new Vector2(-r, b);
                    rt.offsetMax = new Vector2(0, -t);
                    break;
                case 6: // TL corner
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(0, 1);
                    rt.pivot = new Vector2(0, 1);
                    rt.sizeDelta = new Vector2(l, t);
                    rt.anchoredPosition = Vector2.zero;
                    break;
                case 7: // Top edge
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.offsetMin = new Vector2(l, -t);
                    rt.offsetMax = new Vector2(-r, 0);
                    break;
                case 8: // TR corner
                    rt.anchorMin = new Vector2(1, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(1, 1);
                    rt.sizeDelta = new Vector2(r, t);
                    rt.anchoredPosition = Vector2.zero;
                    break;
            }
        }

        private void CleanupSprites()
        {
            if (_subSprites == null) return;
            for (int i = 0; i < _subSprites.Length; i++)
            {
                if (_subSprites[i] != null)
                {
                    if (Application.isPlaying)
                        Destroy(_subSprites[i]);
                    else
                        DestroyImmediate(_subSprites[i]);
                }
            }
            _subSprites = null;
        }
    }
}
