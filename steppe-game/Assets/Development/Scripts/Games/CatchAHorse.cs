using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatchAHorse : GameController
{
    [Header("UI Elements")]
    [SerializeField] private Slider slider;
    [SerializeField] private Button catchButton; // Кнопка для ловли лошади
    [SerializeField] private RectTransform targetZone; // Визуальное представление целевой зоны
    [SerializeField] private SpriteAnimController[] horseAnimators; // Лошади с анимациями
    [SerializeField] private TextMeshProUGUI wordText; // Текст для отображения слова
    [SerializeField] private ParallaxController parallaxController; // Контроллер движения фона
    [SerializeField] private ParticleSystem dustParticles; // Частицы пыли при беге лошади

    [Header("Word Data")]
    [SerializeField] private WordData[] words; // Список слов для озвучки

    [Header("Game Settings")]
    [SerializeField] private int totalSessions = 5; // Количество сессий (лошадей)
    [SerializeField] private float sliderSpeed = 2f; // Скорость движения слайдера
    [SerializeField] private float initialRangeSize = 0.3f; // Начальный размер диапазона (0-1)
    [SerializeField] private float rangeDecreasePerSession = 0.05f; // Уменьшение диапазона каждую сессию
    [SerializeField] private float delayAfterWordAudio = 0.5f; // Задержка после озвучки слова перед следующей сессией

    [Header("Sounds")]
    [SerializeField] private AudioClip horseRunSound; // Звук бега коня
    [SerializeField] private AudioClip horseCaughtSound; // Звук коня при захвате
    [SerializeField] private AudioClip catchSuccessSound; // Звук успешной ловли
    [SerializeField] private AudioClip catchFailSound; // Звук провала

    private int currentSession = 0;
    private int currentHorseIndex = -1; // Индекс текущей лошади
    private WordData currentWord; // Текущее слово
    private float currentRangeSize;
    private float targetRangeMin; // Минимальное значение целевого диапазона (0-1)
    private float targetRangeMax; // Максимальное значение целевого диапазона (0-1)
    private bool isSliderMoving = false;
    private bool sessionActive = false;
    private int sliderDirection = 1; // 1 = вверх, -1 = вниз
    private AudioSource horseRunAudioSource; // AudioSource для звука бега коня

    private void OnValidate()
    {
        if (words != null)
            words = System.Array.FindAll(words, w => w != null);
    }

    protected override void InitializeGame()
    {
        base.InitializeGame();

        currentSession = 0;
        currentRangeSize = initialRangeSize;

        // Отключаем все лошади в начале
        if (horseAnimators != null)
        {
            foreach (var horseAnimator in horseAnimators)
            {
                if (horseAnimator != null)
                {
                    horseAnimator.gameObject.SetActive(false);
                }
            }
        }

        SetLives(3);

        // Настраиваем слайдер
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false; // Пользователь не может двигать слайдер вручную
        }

        // Подписываемся на кнопку
        if (catchButton != null)
        {
            catchButton.onClick.AddListener(OnCatchButtonClicked);
        }

        // Запускаем первую сессию
        StartNextSession();
    }

    private void StartNextSession()
    {
        if (currentSession >= totalSessions)
        {
            // Все сессии пройдены - победа

            finishPanel.SetReward(15, 80, 100);
            finishPanel.SetState(true);
            finishPanel.SetStars(3);
            FinishGame();
            return;
        }

        sessionActive = true;

        // Скрываем текст слова в начале сессии
        if (wordText != null)
        {
            wordText.gameObject.SetActive(false);
        }

        // Отключаем предыдущего коня, если есть
        if (currentHorseIndex >= 0 && currentHorseIndex < horseAnimators.Length && horseAnimators[currentHorseIndex] != null)
        {
            horseAnimators[currentHorseIndex].gameObject.SetActive(false);
        }

        // Выбираем случайную лошадь из массива
        if (horseAnimators != null && horseAnimators.Length > 0)
        {
            currentHorseIndex = Random.Range(0, horseAnimators.Length);
        }

        // Выбираем случайное слово из массива
        if (words != null && words.Length > 0)
        {
            currentWord = words[Random.Range(0, words.Length)];
        }

        // Вычисляем новый целевой диапазон
        GenerateNewTargetRange();

        // Обновляем визуализацию целевой зоны
        UpdateTargetZoneVisual();

        // Включаем и запускаем анимацию "Run" для случайно выбранной лошади
        if (currentHorseIndex >= 0 && currentHorseIndex < horseAnimators.Length && horseAnimators[currentHorseIndex] != null)
        {
            horseAnimators[currentHorseIndex].gameObject.SetActive(true);
            horseAnimators[currentHorseIndex].Play("Run", loop: true);

            // Воспроизводим зациклённый звук бега коня
            if (horseRunSound != null)
            {
                horseRunAudioSource = AudioManager.Instance.PlaySound(horseRunSound, volume: 1f, loop: true);
            }
        }

        // Запускаем движение фона
        if (parallaxController != null)
        {
            parallaxController.ResumeParallax();
        }

        // Запускаем частицы пыли
        if (dustParticles != null)
        {
            dustParticles.Play();
        }

        // Запускаем движение слайдера
        if (!isSliderMoving)
        {
            StartCoroutine(MoveSlider());
        }
    }

    private void GenerateNewTargetRange()
    {
        // Вычисляем текущий размер диапазона
        currentRangeSize = Mathf.Max(0.1f, initialRangeSize - (currentSession * rangeDecreasePerSession));

        // Генерируем случайную позицию для центра диапазона
        float rangeCenter = Random.Range(currentRangeSize / 2f, 1f - currentRangeSize / 2f);

        targetRangeMin = rangeCenter - currentRangeSize / 2f;
        targetRangeMax = rangeCenter + currentRangeSize / 2f;

        // Убеждаемся что диапазон в пределах [0, 1]
        targetRangeMin = Mathf.Clamp01(targetRangeMin);
        targetRangeMax = Mathf.Clamp01(targetRangeMax);
    }

    private void UpdateTargetZoneVisual()
    {
        if (targetZone == null || slider == null) return;

        // Получаем RectTransform слайдера
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        if (sliderRect == null) return;

        float sliderWidth = sliderRect.rect.width;

        // Вычисляем позицию и размер целевой зоны
        float zoneWidth = sliderWidth * currentRangeSize;
        float zoneCenterX = sliderWidth * (targetRangeMin + targetRangeMax) / 2f - sliderWidth / 2f;

        targetZone.sizeDelta = new Vector2(zoneWidth, targetZone.sizeDelta.y);
        targetZone.anchoredPosition = new Vector2(zoneCenterX, targetZone.anchoredPosition.y);
    }

    private IEnumerator MoveSlider()
    {
        isSliderMoving = true;

        while (isSliderMoving)
        {
            if (!sessionActive)
            {
                yield return null;
                continue;
            }

            // Двигаем слайдер
            slider.value += sliderDirection * sliderSpeed * Time.deltaTime;

            // Меняем направление при достижении границ
            if (slider.value >= slider.maxValue)
            {
                slider.value = slider.maxValue;
                sliderDirection = -1;
            }
            else if (slider.value <= slider.minValue)
            {
                slider.value = slider.minValue;
                sliderDirection = 1;
            }

            yield return null;
        }
    }

    private void OnCatchButtonClicked()
    {
        if (!sessionActive) return;

        sessionActive = false;

        // Проверяем попадание в диапазон
        bool success = slider.value >= targetRangeMin && slider.value <= targetRangeMax;

        if (success)
        {
            // Успех!
            OnCatchSuccess();
        }
        else
        {
            // Промах!
            OnCatchFail();
        }
    }

    private void OnCatchSuccess()
    {
        // Останавливаем звук бега коня
        if (horseRunAudioSource != null)
        {
            horseRunAudioSource.Stop();
            Destroy(horseRunAudioSource.gameObject);
            horseRunAudioSource = null;
        }

        // Останавливаем движение фона
        if (parallaxController != null)
        {
            parallaxController.StopParallax();
        }

        // Останавливаем частицы пыли
        if (dustParticles != null)
        {
            dustParticles.Stop();
        }

        // Воспроизводим звук коня при захвате
        if (horseCaughtSound != null)
        {
            AudioManager.Instance.PlaySound(horseCaughtSound);
        }

        // Останавливаем анимацию "Run" и запускаем "Caught" для текущей лошади
        if (currentHorseIndex >= 0 && currentHorseIndex < horseAnimators.Length && horseAnimators[currentHorseIndex] != null)
        {
            horseAnimators[currentHorseIndex].Play("Caught", loop: false, onComplete: () =>
            {
                // После завершения анимации показываем слово с анимацией
                ShowWordWithAnimation();

                // Озвучиваем слово
                if (currentWord != null && currentWord.AudioClip != null)
                    AudioManager.Instance.PlaySound(currentWord.AudioClip);
                else
                    AudioManager.Instance.PlaySound(catchSuccessSound);

                // После завершения анимации переходим к следующей сессии
                currentSession++;
                StartCoroutine(DelayedNextSession());
            });
        }
        else
        {
            currentSession++;
            StartCoroutine(DelayedNextSession());
        }
    }

    private void ShowWordWithAnimation()
    {
        if (wordText == null || currentWord == null) return;

        // Устанавливаем текст
        wordText.text = currentWord.Word;

        // Включаем объект
        wordText.gameObject.SetActive(true);

        // Запускаем анимацию появления (вырастает)
        StartCoroutine(AnimateWordAppearance());
    }

    private IEnumerator AnimateWordAppearance()
    {
        if (wordText == null) yield break;

        RectTransform wordRect = wordText.GetComponent<RectTransform>();
        if (wordRect == null) yield break;

        // Начинаем с нулевого масштаба
        wordRect.localScale = Vector3.zero;

        float duration = 0.3f; // Длительность анимации
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Используем ease out для плавного появления
            float scale = Mathf.Lerp(0f, 1f, 1f - Mathf.Pow(1f - t, 3f));
            wordRect.localScale = Vector3.one * scale;

            yield return null;
        }

        // Убеждаемся что масштаб точно 1
        wordRect.localScale = Vector3.one;
    }

    private void OnCatchFail()
    {
        // Воспроизводим звук провала
        if (catchFailSound != null)
        {
            AudioManager.Instance.PlaySound(catchFailSound);
        }

        // Отнимаем жизнь
        SetLives(lives - 1);

        // Проверяем проигрыш
        if (lives <= 0)
        {
            isSliderMoving = false;
            finishPanel.SetReward(3, 20, 15);
            finishPanel.SetState(false);
            finishPanel.SetStars(0);
            FinishGame();
            return;
        }

        // НЕ перемещаем диапазон - продолжаем с той же зоной
        sessionActive = true;
    }

    private IEnumerator DelayedNextSession()
    {
        // Вычисляем задержку: длительность аудио слова + дополнительная задержка
        float delay = delayAfterWordAudio;

        if (currentWord != null && currentWord.AudioClip != null)
        {
            delay += currentWord.AudioClip.length;
        }

        yield return new WaitForSeconds(delay);
        StartNextSession();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        isSliderMoving = false;
        sessionActive = false;

        // Останавливаем звук бега коня при выходе из игры
        if (horseRunAudioSource != null)
        {
            horseRunAudioSource.Stop();
            Destroy(horseRunAudioSource.gameObject);
            horseRunAudioSource = null;
        }

        // Останавливаем частицы пыли при выходе из игры
        if (dustParticles != null)
        {
            dustParticles.Stop();
        }

        if (catchButton != null)
        {
            catchButton.onClick.RemoveListener(OnCatchButtonClicked);
        }
    }
}
