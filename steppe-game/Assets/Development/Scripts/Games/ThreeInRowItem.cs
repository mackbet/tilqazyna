using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ThreeInRowItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image image;
    [SerializeField] private int itemType;
    [SerializeField] private Material material;

    private Vector2Int gridPosition;
    private bool isSelected = false;
    private RectTransform rectTransform;

    public int ItemType => itemType;
    public Vector2Int GridPosition => gridPosition;
    public bool IsSelected => isSelected;

    public event Action<ThreeInRowItem> OnItemClicked;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!image)
            image = GetComponent<Image>();
    }
#endif

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(int type, Sprite sprite, Vector2Int position)
    {
        itemType = type;
        gridPosition = position;

        if (image != null && sprite != null)
        {
            image.sprite = sprite;
        }
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    public void SetItemType(int type)
    {
        itemType = type;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (image != null)
        {
            image.material = selected ? material : null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnItemClicked?.Invoke(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Можно использовать для drag & drop
    }

    public void AnimateMove(Vector2 targetPosition, float duration = 0.3f)
    {
        rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.OutQuad);
    }

    public void AnimateMoveToPosition(Vector3 targetWorldPosition, float duration = 0.3f)
    {
        rectTransform.DOMove(targetWorldPosition, duration).SetEase(Ease.OutQuad);
    }

    public void AnimateDestroy(Action onComplete = null)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOScale(1.3f, 0.15f));
        sequence.Append(rectTransform.DOScale(0f, 0.15f));
        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            Destroy(gameObject);
        });
    }

    public void AnimateAppear(float delay = 0f)
    {
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(1f, 0.3f).SetDelay(delay).SetEase(Ease.OutBack);
    }
}
