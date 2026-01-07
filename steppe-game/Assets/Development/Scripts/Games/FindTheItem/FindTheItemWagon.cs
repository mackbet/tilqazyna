using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FindTheItemWagon : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMPro.TextMeshProUGUI itemNameText;
    [SerializeField] private SoundButton soundButton;
    [SerializeField] private GameObject contentPanel; // Панель с содержимым
    [SerializeField] private Button wagonButton;

    private FindTheItemVariant currentItem;
    private RectTransform rectTransform;

    public FindTheItemVariant CurrentItem => currentItem;
    public event Action<FindTheItemWagon> OnWagonClicked;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (wagonButton != null)
        {
            wagonButton.onClick.AddListener(HandleClick);
        }
    }

    private void OnDestroy()
    {
        if (wagonButton != null)
        {
            wagonButton.onClick.RemoveListener(HandleClick);
        }
    }

    public void SetItem(FindTheItemVariant item)
    {
        currentItem = item;

        if (item != null)
        {
            if (itemImage != null)
            {
                itemImage.sprite = item.Sprite;
                itemImage.enabled = false; // Скрываем по умолчанию
            }

            if (itemNameText != null)
            {
                itemNameText.text = item.ItemName;
                itemNameText.alpha = 0f; // Скрываем по умолчанию
            }

            if (soundButton != null)
            {
                soundButton.SetAudio(item.AudioClip);
            }
        }
    }

    public void ShowContent(bool playAudio = true)
    {
        if (contentPanel != null)
        {
            contentPanel.SetActive(true);
        }

        if (itemImage != null)
        {
            itemImage.enabled = true;
        }

        if (itemNameText != null)
        {
            itemNameText.alpha = 1f;
        }

        // Воспроизводим аудио названия предмета
        if (playAudio && soundButton != null)
        {
            soundButton.PlayAudio();
        }
    }

    public void AnimateShowContent(bool playAudio = true, float duration = 0.3f, System.Action onComplete = null)
    {
        if (contentPanel != null)
        {
            contentPanel.SetActive(true);
        }

        Sequence showSequence = DOTween.Sequence();
        bool hasAnimation = false;

        if (itemImage != null)
        {
            itemImage.enabled = true;

            // Устанавливаем начальные значения для анимации
            itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 0f);
            itemImage.transform.localScale = Vector3.zero;

            // Анимация появления: fade in + scale up
            showSequence.Append(itemImage.DOFade(1f, duration));
            showSequence.Join(itemImage.transform.DOScale(1f, duration).SetEase(Ease.OutBack));
            hasAnimation = true;
        }

        if (itemNameText != null)
        {
            // Устанавливаем начальное значение прозрачности
            itemNameText.alpha = 0f;

            // Анимация появления текста
            if (hasAnimation)
            {
                showSequence.Join(itemNameText.DOFade(1f, duration));
            }
            else
            {
                showSequence.Append(itemNameText.DOFade(1f, duration));
                hasAnimation = true;
            }
        }

        if (hasAnimation)
        {
            showSequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        else
        {
            onComplete?.Invoke();
        }

        // Воспроизводим аудио названия предмета
        if (playAudio && soundButton != null)
        {
            soundButton.PlayAudio();
        }
    }

    public void HideContent()
    {
        if (contentPanel != null)
        {
            contentPanel.SetActive(false);
        }

        if (itemImage != null)
        {
            itemImage.enabled = false;
        }

        if (itemNameText != null)
        {
            itemNameText.alpha = 0f;
        }
    }

    public void AnimateHideContent(float duration = 0.3f, System.Action onComplete = null)
    {
        Sequence hideSequence = DOTween.Sequence();
        bool hasAnimation = false;

        if (itemImage != null)
        {
            // Анимация исчезновения: fade out + scale down
            hideSequence.Append(itemImage.DOFade(0f, duration));
            hideSequence.Join(itemImage.transform.DOScale(0.5f, duration).SetEase(Ease.InBack));
            hasAnimation = true;
        }

        if (itemNameText != null)
        {
            // Анимация исчезновения текста
            if (hasAnimation)
            {
                hideSequence.Join(itemNameText.DOFade(0f, duration));
            }
            else
            {
                hideSequence.Append(itemNameText.DOFade(0f, duration));
                hasAnimation = true;
            }
        }

        if (hasAnimation)
        {
            hideSequence.OnComplete(() =>
            {
                // Возвращаем исходные значения для следующего показа (только для картинки)
                if (itemImage != null)
                {
                    itemImage.color = new Color(itemImage.color.r, itemImage.color.g, itemImage.color.b, 1f);
                    itemImage.transform.localScale = Vector3.one;
                }

                HideContent(); // Скрываем содержимое (включая установку alpha = 0 для текста)
                onComplete?.Invoke();
            });
        }
        else
        {
            HideContent();
            onComplete?.Invoke();
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (wagonButton != null)
        {
            wagonButton.interactable = interactable;
        }
    }

    public void AnimateToPosition(Vector3 targetPosition, float duration = 0.5f)
    {
        rectTransform.DOMove(targetPosition, duration).SetEase(Ease.InOutQuad);
    }

    public Vector3 GetPosition()
    {
        return rectTransform.position;
    }

    private void HandleClick()
    {
        OnWagonClicked?.Invoke(this);
    }
}
