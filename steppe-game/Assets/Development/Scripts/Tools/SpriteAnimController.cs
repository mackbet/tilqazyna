using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpriteAnimClip
{
    [SerializeField] private string animationName; // Название анимации
    [SerializeField] private Sprite[] sprites; // Массив спрайтов для анимации
    [SerializeField] private float frameDuration = 0.07f; // Длительность одного кадра

    public string AnimationName => animationName;
    public Sprite[] Sprites => sprites;
    public float FrameDuration => frameDuration;
}

public class SpriteAnimController : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private SpriteAnimClip[] animations; // Массив анимаций

    public event Action OnAnimationComplete; // Событие завершения анимации

    private Coroutine _animationCoroutine;
    private string _currentAnimationName;
    private bool _isPlaying = false;

    private void OnDisable()
    {
        Stop();
    }

    /// <summary>
    /// Воспроизводит анимацию по имени
    /// </summary>
    /// <param name="animationName">Название анимации</param>
    /// <param name="loop">Зациклить анимацию</param>
    /// <param name="onComplete">Callback при завершении анимации (вызывается только если loop = false)</param>
    public void Play(string animationName, bool loop = true, Action onComplete = null)
    {
        // Находим анимацию по имени
        SpriteAnimClip animation = FindAnimation(animationName);

        if (animation == null)
        {
            Debug.LogWarning($"SpriteAnimController: Анимация '{animationName}' не найдена!");
            return;
        }

        if (animation.Sprites == null || animation.Sprites.Length == 0)
        {
            Debug.LogWarning($"SpriteAnimController: Анимация '{animationName}' не содержит спрайтов!");
            return;
        }

        if (targetImage == null)
        {
            Debug.LogWarning("SpriteAnimController: Target Image не назначен!");
            return;
        }

        // Останавливаем предыдущую анимацию если была
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        _currentAnimationName = animationName;
        _isPlaying = true;
        targetImage.enabled = true;

        // Запускаем новую анимацию
        _animationCoroutine = StartCoroutine(AnimateSprites(animation, loop, onComplete));
    }

    /// <summary>
    /// Останавливает текущую анимацию
    /// </summary>
    public void Stop()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }

        _isPlaying = false;
        _currentAnimationName = null;

        if (targetImage != null)
        {
            targetImage.enabled = false;
        }
    }

    /// <summary>
    /// Проверяет, воспроизводится ли анимация в данный момент
    /// </summary>
    public bool IsPlaying => _isPlaying;

    /// <summary>
    /// Получает имя текущей воспроизводимой анимации
    /// </summary>
    public string CurrentAnimationName => _currentAnimationName;

    private SpriteAnimClip FindAnimation(string animationName)
    {
        if (animations == null) return null;

        foreach (var animation in animations)
        {
            if (animation.AnimationName == animationName)
            {
                return animation;
            }
        }

        return null;
    }

    private IEnumerator AnimateSprites(SpriteAnimClip animation, bool loop, Action onComplete)
    {
        Sprite[] sprites = animation.Sprites;
        float frameDuration = animation.FrameDuration;
        int frameCount = 0;

        WaitForSeconds waitForSeconds = new WaitForSeconds(frameDuration);

        do
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                // Проверка на случай если targetImage был уничтожен
                if (targetImage == null)
                {
                    _isPlaying = false;
                    yield break;
                }

                targetImage.sprite = sprites[i];
                frameCount++;

                yield return waitForSeconds;
            }

            // Если не зацикливаем, вызываем события завершения
            if (!loop)
            {
                _isPlaying = false;
                OnAnimationComplete?.Invoke();
                onComplete?.Invoke();
            }
        } while (loop);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Автоматически находим Image если не указан
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }
#endif
}
