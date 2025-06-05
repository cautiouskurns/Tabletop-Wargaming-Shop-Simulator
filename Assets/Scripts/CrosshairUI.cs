using UnityEngine;
using UnityEngine.UI;

namespace TabletopShop
{
    /// <summary>
    /// UI component that manages the crosshair and interaction feedback
    /// </summary>
    public class CrosshairUI : MonoBehaviour
    {
        [Header("Crosshair Settings")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color interactableColor = Color.yellow;
        [SerializeField] private float transitionSpeed = 5f;
        
        [Header("Interaction Text")]
        [SerializeField] private Text interactionText;
        [SerializeField] private CanvasGroup interactionTextGroup;
        
        private Color targetColor;
        private bool showingInteractionText = false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Get components if not assigned
            if (crosshairImage == null)
                crosshairImage = GetComponent<Image>();
            
            if (interactionTextGroup == null && interactionText != null)
                interactionTextGroup = interactionText.GetComponent<CanvasGroup>();
            
            // Set initial state
            targetColor = normalColor;
            if (crosshairImage != null)
                crosshairImage.color = normalColor;
                
            // Hide interaction text initially
            if (interactionTextGroup != null)
                interactionTextGroup.alpha = 0f;
        }
        
        private void Update()
        {
            // Smoothly transition crosshair color
            if (crosshairImage != null && crosshairImage.color != targetColor)
            {
                crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * transitionSpeed);
            }
            
            // Handle interaction text alpha
            if (interactionTextGroup != null)
            {
                float targetAlpha = showingInteractionText ? 1f : 0f;
                interactionTextGroup.alpha = Mathf.Lerp(interactionTextGroup.alpha, targetAlpha, Time.deltaTime * transitionSpeed);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show that an interactable object is being looked at
        /// </summary>
        /// <param name="interactionText">Text to display for the interaction</param>
        public void ShowInteractable(string interactionText = "")
        {
            targetColor = interactableColor;
            
            if (this.interactionText != null && !string.IsNullOrEmpty(interactionText))
            {
                this.interactionText.text = interactionText;
                showingInteractionText = true;
            }
        }
        
        /// <summary>
        /// Hide interactable indication
        /// </summary>
        public void HideInteractable()
        {
            targetColor = normalColor;
            showingInteractionText = false;
        }
        
        /// <summary>
        /// Set crosshair visibility
        /// </summary>
        /// <param name="visible">Whether the crosshair should be visible</param>
        public void SetVisible(bool visible)
        {
            if (crosshairImage != null)
                crosshairImage.enabled = visible;
                
            if (interactionTextGroup != null)
                interactionTextGroup.gameObject.SetActive(visible);
        }
        
        #endregion
        
        #region Setup Methods
        
        /// <summary>
        /// Create a basic crosshair UI setup programmatically
        /// </summary>
        /// <param name="canvas">The canvas to create the UI on</param>
        /// <returns>The created CrosshairUI component</returns>
        public static CrosshairUI CreateCrosshairUI(Canvas canvas)
        {
            if (canvas == null)
            {
                Debug.LogError("Cannot create CrosshairUI without a canvas!");
                return null;
            }
            
            // Create crosshair container
            GameObject crosshairContainer = new GameObject("CrosshairUI");
            crosshairContainer.transform.SetParent(canvas.transform, false);
            
            // Set up RectTransform to center on screen
            RectTransform containerRect = crosshairContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(20, 20);
            
            // Add CrosshairUI component
            CrosshairUI crosshairUI = crosshairContainer.AddComponent<CrosshairUI>();
            
            // Create crosshair image
            GameObject crosshairImageObj = new GameObject("CrosshairImage");
            crosshairImageObj.transform.SetParent(crosshairContainer.transform, false);
            
            RectTransform imageRect = crosshairImageObj.AddComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
            
            Image image = crosshairImageObj.AddComponent<Image>();
            image.color = Color.white;
            image.sprite = CreateCrosshairSprite();
            
            // Create interaction text
            GameObject textObj = new GameObject("InteractionText");
            textObj.transform.SetParent(crosshairContainer.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0f);
            textRect.anchorMax = new Vector2(0.5f, 0f);
            textRect.anchoredPosition = new Vector2(0, -30);
            textRect.sizeDelta = new Vector2(200, 30);
            
            Text text = textObj.AddComponent<Text>();
            text.text = "";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            CanvasGroup textGroup = textObj.AddComponent<CanvasGroup>();
            textGroup.alpha = 0f;
            
            // Assign references
            crosshairUI.crosshairImage = image;
            crosshairUI.interactionText = text;
            crosshairUI.interactionTextGroup = textGroup;
            
            return crosshairUI;
        }
        
        /// <summary>
        /// Create a simple crosshair sprite
        /// </summary>
        /// <returns>A crosshair sprite</returns>
        private static Sprite CreateCrosshairSprite()
        {
            // Create a simple 20x20 crosshair texture
            Texture2D texture = new Texture2D(20, 20);
            Color[] pixels = new Color[400];
            
            // Fill with transparent
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            // Draw crosshair lines
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    // Horizontal line
                    if (y == 10 && (x >= 5 && x <= 15))
                    {
                        pixels[y * 20 + x] = Color.white;
                    }
                    // Vertical line
                    if (x == 10 && (y >= 5 && y <= 15))
                    {
                        pixels[y * 20 + x] = Color.white;
                    }
                    // Center dot
                    if (x == 10 && y == 10)
                    {
                        pixels[y * 20 + x] = Color.white;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 20, 20), new Vector2(0.5f, 0.5f));
        }
        
        #endregion
    }
}
