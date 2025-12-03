using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Texture2D spriteSheet; // Texture2D с несколькими спрайтами
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float frameDuration = 0.07f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool pingPong = false; // Анимация вперед-назад

    private Coroutine _animationCoroutine;
    private WaitForSeconds _waitForSeconds; // Кэшируем для избежания GC аллокаций

    private void Awake()
    {
        // Кэшируем WaitForSeconds один раз
        _waitForSeconds = new WaitForSeconds(frameDuration);
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            StartAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimation(); // Исправлено: было StartAnimation()
    }

    public void StartAnimation()
    {
        // Проверка на валидность
        if (sprites == null || sprites.Length == 0 || targetImage == null) return;

        // Останавливаем предыдущую анимацию если она была
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        targetImage.enabled = true;
        _animationCoroutine = StartCoroutine(AnimateSprites());
    }

    public void StopAnimation()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }

        if (targetImage != null)
        {
            targetImage.enabled = false;
        }
    }

    private IEnumerator AnimateSprites()
    {
        int index = 0;
        int direction = 1; // 1 = вперед, -1 = назад
        int spritesLength = sprites.Length; // Кэшируем длину

        while (true)
        {
            // Дополнительная проверка на случай если targetImage был уничтожен
            if (targetImage == null) yield break;

            targetImage.sprite = sprites[index];

            if (pingPong)
            {
                // Ping-pong режим: вперед-назад
                index += direction;

                // Меняем направление на границах
                if (index >= spritesLength - 1)
                {
                    index = spritesLength - 1;
                    direction = -1;
                }
                else if (index <= 0)
                {
                    index = 0;
                    direction = 1;
                }
            }
            else
            {
                // Обычный режим: по кругу
                index = (index + 1) % spritesLength;
            }

            yield return _waitForSeconds; // Используем кэшированный WaitForSeconds
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Автоматически находим Image если не указан
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        // Автоматически извлекаем спрайты из Texture2D
        if (sprites.Length == 0)
        {
            LoadSpritesFromTexture();
        }
    }

    private void LoadSpritesFromTexture()
    {
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(spriteSheet);

        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogWarning("ImageAnimator: Не удалось найти путь к текстуре.");
            return;
        }

        // Загружаем все спрайты из текстуры
        Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);

        System.Collections.Generic.List<Sprite> loadedSprites = new System.Collections.Generic.List<Sprite>();

        foreach (var asset in assets)
        {
            if (asset is Sprite sprite)
            {
                loadedSprites.Add(sprite);
            }
        }

        if (loadedSprites.Count > 0)
        {
            sprites = loadedSprites.ToArray();
            Debug.Log($"ImageAnimator: Загружено {sprites.Length} спрайтов из текстуры '{spriteSheet.name}'");
        }
        else
        {
            Debug.LogWarning($"ImageAnimator: В текстуре '{spriteSheet.name}' не найдено спрайтов. Убедитесь что Texture Type = Sprite и Sprite Mode = Multiple.");
        }

        targetImage.sprite = loadedSprites[0];
    }
#endif
}
