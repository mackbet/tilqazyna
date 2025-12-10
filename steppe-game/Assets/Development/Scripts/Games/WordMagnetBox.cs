using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class WordMagnetBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private RectTransform rectTransform;

    private WordData wordData;
    private float speed;
    private bool isCaptured = false;
    private WordMagnet wordMagnet;

    public WordData WordData => wordData;
    public bool IsCaptured => isCaptured;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(WordData data, float moveSpeed, WordMagnet magnet)
    {
        wordData = data;
        speed = moveSpeed;
        wordMagnet = magnet;
        isCaptured = false;

        if (wordText != null && wordData != null)
        {
            wordText.text = wordData.Word;
        }
    }

    private void Update()
    {
        if (!isCaptured)
        {
            // Двигаем коробку вдоль локальной оси X (вправо в локальных координатах)
            // Transform.right дает направление локальной оси X с учетом поворота
            Vector2 localRight = rectTransform.right;
            rectTransform.anchoredPosition += localRight * speed * Time.deltaTime;
        }
    }

    public void Capture(RectTransform magnetTransform)
    {
        if (isCaptured) return;

        isCaptured = true;

        // Анимация притяжения к магниту
        rectTransform.DOAnchorPos(magnetTransform.anchoredPosition, 0.3f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                // Уведомляем WordMagnet о захвате
                if (wordMagnet != null)
                {
                    wordMagnet.OnBoxCaptured(this);
                }
            });

        // Уменьшение размера при захвате
        rectTransform.DOScale(0f, 0.3f).SetEase(Ease.InQuad);
    }

    public bool IsOutOfBounds(float screenWidth, float screenHeight)
    {
        Vector2 pos = rectTransform.anchoredPosition;
        float margin = 200f; // Запас для удаления объектов за границей

        return pos.x < -screenWidth / 2f - margin ||
               pos.x > screenWidth / 2f + margin ||
               pos.y < -screenHeight / 2f - margin ||
               pos.y > screenHeight / 2f + margin;
    }
}
