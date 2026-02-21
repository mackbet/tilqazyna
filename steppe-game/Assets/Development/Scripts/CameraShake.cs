using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    public static CameraShake Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CameraShake>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("CameraShake");
                    instance = obj.AddComponent<CameraShake>();
                }
            }
            return instance;
        }
    }

    // Храним оригинальные значения для каждого RectTransform
    private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();
    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    /// <summary>
    /// Тряска RectTransform с увеличением для скрытия краёв
    /// </summary>
    /// <param name="target">RectTransform который нужно трясти</param>
    /// <param name="intensity">Интенсивность (сила тряски)</param>
    /// <param name="duration">Длительность в секундах</param>
    /// <param name="scaleIncrease">Насколько увеличить scale (например 1.1 = +10%)</param>
    public void Shake(RectTransform target, float intensity = 10f, float duration = 0.3f, float scaleIncrease = 1.1f)
    {
        if (target == null) return;

        // Сохраняем оригинальные значения при первом обращении
        if (!originalPositions.ContainsKey(target))
        {
            originalPositions[target] = target.anchoredPosition;
            originalScales[target] = target.localScale;
        }

        Vector2 originalPosition = originalPositions[target];
        Vector3 originalScale = originalScales[target];

        // Убиваем предыдущие анимации на этом объекте
        DOTween.Kill(target);

        // Сначала мгновенно возвращаем к оригиналу (на случай если была прервана предыдущая тряска)
        target.anchoredPosition = originalPosition;
        target.localScale = originalScale;

        // Создаем последовательность анимаций
        Sequence sequence = DOTween.Sequence();

        // 1. Быстро увеличиваем scale
        sequence.Append(target.DOScale(originalScale * scaleIncrease, 0.05f));

        // 2. Трясем позицию
        sequence.Append(target.DOShakeAnchorPos(duration, intensity, 10, 90, false, true));

        // 3. Возвращаем scale и позицию к оригиналу
        sequence.Append(target.DOScale(originalScale, 0.1f));
        sequence.Join(target.DOAnchorPos(originalPosition, 0.1f));

        sequence.Play();
    }

    /// <summary>
    /// Очистить сохраненные оригинальные значения для объекта
    /// </summary>
    public void ClearOriginalValues(RectTransform target)
    {
        if (target == null) return;

        originalPositions.Remove(target);
        originalScales.Remove(target);
    }

    private void OnDestroy()
    {
        // Убиваем все анимации при уничтожении
        DOTween.KillAll();
        originalPositions.Clear();
        originalScales.Clear();
    }
}
