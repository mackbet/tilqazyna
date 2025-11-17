using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PuzzlePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;
    [SerializeField] private float snapDistance = 50f;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector2 initialPosition = Vector2.zero;
    private bool isLocked = false;
    private Vector2 dragOffset;

    public event Action<PuzzlePiece> OnPieceLocked;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!rectTransform)
            rectTransform = GetComponent<RectTransform>();

        if (!image)
            image = GetComponent<Image>();

        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();
    }
#endif

    private void Start()
    {
        image.alphaHitTestMinimumThreshold = 0.1f;
    }

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;

        isLocked = false;
        canvasGroup.blocksRaycasts = true;
        SetRandomPosition();
    }

    private Sprite CreateMaskedSprite(Texture2D sourceTexture, Texture2D maskTexture)
    {
        int width = Mathf.Min(sourceTexture.width, maskTexture.width);
        int height = Mathf.Min(sourceTexture.height, maskTexture.height);

        Texture2D maskedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color sourceColor = sourceTexture.GetPixel(x, y);
                Color maskColor = maskTexture.GetPixel(x, y);

                // Применяем альфа-канал маски к исходному цвету
                sourceColor.a *= maskColor.a;
                maskedTexture.SetPixel(x, y, sourceColor);
            }
        }

        maskedTexture.Apply();

        // Создаем спрайт из замаскированной текстуры
        Sprite sprite = Sprite.Create(
            maskedTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return sprite;
    }

    private void SetRandomPosition()
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        float padding = 500f;

        float minX = -canvasRect.rect.width / 2 + padding;
        float maxX = canvasRect.rect.width / 2 - padding;
        float minY = -canvasRect.rect.height / 2 + padding;
        float maxY = canvasRect.rect.height / 2 - padding;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        rectTransform.anchoredPosition = new Vector2(randomX, randomY);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;

        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );

        dragOffset = localPointerPosition - rectTransform.anchoredPosition;

        canvasGroup.alpha = 0.6f;
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rectTransform.anchoredPosition = localPoint - dragOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.alpha = 1f;

        float distance = Vector2.Distance(rectTransform.anchoredPosition, initialPosition);

        if (distance <= snapDistance)
        {
            rectTransform.anchoredPosition = initialPosition;
            isLocked = true;
            canvasGroup.blocksRaycasts = false;

            OnPieceLocked?.Invoke(this);
        }
    }

    public bool IsLocked() => isLocked;

    public void Reset()
    {
        isLocked = false;
        canvasGroup.blocksRaycasts = true;
        SetRandomPosition();
    }
}