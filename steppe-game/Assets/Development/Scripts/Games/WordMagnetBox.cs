using UnityEngine;
using TMPro;
using DG.Tweening;

public class WordMagnetBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform model;
    [SerializeField] private RectTransform wrongEffectTransform;
    [SerializeField] private ParticleSystem wrongEffect;
    [SerializeField] private bool isWrong = false;
    private WordData wordData;
    private float speed;
    private bool isCaptured = false;
    private WordMagnet wordMagnet;
    private Sequence scaleSequence;

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

        // Запускаем постоянную анимацию увеличения-уменьшения
        StartBreathingAnimation();
    }

    private void StartBreathingAnimation()
    {
        if (model == null) return;

        // Убиваем предыдущую анимацию если она есть
        scaleSequence?.Kill();

        // Создаем зацикленную последовательность
        scaleSequence = DOTween.Sequence();
        scaleSequence.Append(model.DOScale(1.1f, 0.5f).SetEase(Ease.InOutSine));
        scaleSequence.Append(model.DOScale(1.0f, 0.5f).SetEase(Ease.InOutSine));
        scaleSequence.SetLoops(-1); // Бесконечный цикл
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

        // Останавливаем анимацию дыхания
        scaleSequence?.Kill();

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

        // Уменьшение размера всего объекта при захвате
        rectTransform.DOScale(0f, 0.3f).SetEase(Ease.InQuad);

        if (isWrong && wrongEffect)
        {
            wrongEffectTransform.parent.SetParent(transform.parent.parent);
            Destroy(wrongEffectTransform.gameObject, wrongEffect.main.duration);
            wrongEffect.Play();
        }
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

    private void OnDestroy()
    {
        // Очищаем анимацию при уничтожении объекта
        scaleSequence?.Kill();
    }
}