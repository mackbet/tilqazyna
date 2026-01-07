using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberOfTargets : GameController
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI numberDisplayText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CustomButton increaseButton;
    [SerializeField] private CustomButton decreaseButton;
    [SerializeField] private Image glow;
    [SerializeField] private RectTransform picture;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private float pictureTime = 0.5f;

    [Header("Groups")]
    [SerializeField] private TargetGroup[] groups;

    [Header("Settings")]
    [SerializeField] private int minHiddenPerGroup = 1;
    [SerializeField] private int maxHiddenPerGroup = 3;
    [SerializeField] private int minNumber = 0;
    [SerializeField] private int maxNumber = 10;
    [SerializeField] private float timeToAnswer = 10f;

    [Header("Sounds")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip tickSound;

    private TargetGroup selectedGroup;
    private int correctAnswer;
    private int currentNumber;
    private bool isAnswering = false;
    private float currentTime;
    private bool isTimerRunning = false;

    [Serializable]
    private class TargetGroup
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public AudioClip Audio { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public GameObject[] Objects { get; private set; }

        public int VisibleCount { get; set; }
    }

    protected override void InitializeGame()
    {
        base.InitializeGame();

        // Скрываем случайные объекты
        HideRandomObjects();

        // Выбираем случайную группу для вопроса
        SelectRandomGroup();

        // Настраиваем кнопки
        SetupButtons();

        // Инициализируем текущее число
        currentNumber = minNumber;
        UpdateNumberDisplay();

        // Показываем вопрос и запускаем таймер
        ShowQuestion();
    }

    private void Update()
    {
        if (!isTimerRunning || isAnswering) return;

        currentTime -= Time.deltaTime;

        // Обновляем отображение таймера
        UpdateTimerDisplay();

        // Проверяем звук тика (последние 3 секунды)
        if (currentTime <= 3f && currentTime > 2.9f && tickSound != null)
        {
            AudioManager.Instance.PlaySound(tickSound);
        }

        // Время вышло - автоматически проверяем ответ
        if (currentTime <= 0f)
        {
            StopTimer();
            AutoSubmit();
        }
    }

    public void StartTimer()
    {
        AudioManager.Instance.PlaySound(selectedGroup.Audio);
        currentTime = timeToAnswer;
        isTimerRunning = true;
        UpdateTimerDisplay();

        PlayAnimation();
    }

    private void StopTimer()
    {
        isTimerRunning = false;
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(currentTime);
            timerText.text = seconds.ToString();

            // Можно изменить цвет при малом времени
            if (currentTime <= 3f)
            {
                timerText.color = Color.red;
            }
            else if (currentTime <= 5f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    private void AutoSubmit()
    {
        Debug.Log("Время вышло! Автоматическая проверка ответа.");
        OnSubmitClicked();
    }

    private void HideRandomObjects()
    {
        foreach (var group in groups)
        {
            // Определяем сколько объектов скрыть в этой группе
            int totalObjects = group.Objects.Length;
            int objectsToHide = UnityEngine.Random.Range(minHiddenPerGroup, Mathf.Min(maxHiddenPerGroup + 1, totalObjects));

            // Создаем список индексов
            List<int> indices = new List<int>();
            for (int i = 0; i < totalObjects; i++)
            {
                indices.Add(i);
            }

            // Случайно выбираем объекты для скрытия
            for (int i = 0; i < objectsToHide; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, indices.Count);
                int objectIndex = indices[randomIndex];

                if (group.Objects[objectIndex] != null)
                {
                    group.Objects[objectIndex].SetActive(false);
                }

                indices.RemoveAt(randomIndex);
            }

            // Подсчитываем видимые объекты
            int visibleCount = 0;
            foreach (var obj in group.Objects)
            {
                if (obj != null && obj.activeSelf)
                {
                    visibleCount++;
                }
            }
            group.VisibleCount = visibleCount;

            Debug.Log($"Группа {group.Name}: всего {totalObjects}, видимых {visibleCount}, скрытых {objectsToHide}");
        }
    }

    private void SelectRandomGroup()
    {
        selectedGroup = groups[UnityEngine.Random.Range(0, groups.Length)];
        correctAnswer = selectedGroup.VisibleCount;

        Debug.Log($"Выбрана группа: {selectedGroup.Name}, правильный ответ: {correctAnswer}");
    }

    private void SetupButtons()
    {
        increaseButton.OnButtonClicked += OnIncreaseClicked;
        decreaseButton.OnButtonClicked += OnDecreaseClicked;
    }

    private void OnIncreaseClicked()
    {
        if (isAnswering) return;

        currentNumber++;
        if (currentNumber > maxNumber)
        {
            currentNumber = maxNumber;
        }

        UpdateNumberDisplay();

        // Воспроизводим звук клика
        if (buttonClickSound != null)
        {
            AudioManager.Instance.PlaySound(buttonClickSound);
        }
    }

    private void OnDecreaseClicked()
    {
        if (isAnswering) return;

        currentNumber--;
        if (currentNumber < minNumber)
        {
            currentNumber = minNumber;
        }

        UpdateNumberDisplay();

        // Воспроизводим звук клика
        if (buttonClickSound != null)
        {
            AudioManager.Instance.PlaySound(buttonClickSound);
        }
    }

    private void UpdateNumberDisplay()
    {
        if (numberDisplayText != null)
        {
            numberDisplayText.text = currentNumber.ToString();
        }
    }

    private void ShowQuestion()
    {
        // Показываем текст вопроса
        if (questionText != null)
        {
            questionText.text = $"{selectedGroup.Name}";
        }

        isAnswering = false;
    }

    private void PlayAnimation()
    {
        gameUI.SetActive(true);
        picture.gameObject.SetActive(true);
        glow.gameObject.SetActive(true);
        glow.color = Color.white;
        picture.localScale = Vector3.one * 1.5f;

        // Анимация цвета с белого в прозрачный
        glow.DOColor(new Color(1f, 1f, 1f, 0f), pictureTime).SetEase(Ease.Linear);

        // Анимация масштаба с 1.5 до 1
        picture.DOScale(Vector3.one, pictureTime).SetEase(Ease.OutBack);
    }

    private void OnSubmitClicked()
    {
        if (isAnswering) return;

        isAnswering = true;
        StopTimer();

        Debug.Log($"Игрок ответил: {currentNumber}, правильный ответ: {correctAnswer}");

        if (currentNumber == correctAnswer)
        {
            OnCorrectAnswer(currentNumber);
        }
        else
        {
            OnWrongAnswer(currentNumber);
        }
    }

    private void OnCorrectAnswer(int answer)
    {
        Debug.Log("Правильный ответ!");

        // Воспроизводим звук
        if (correctSound != null)
        {
            AudioManager.Instance.PlaySound(correctSound);
        }

        finishPanel.SetReward(10, 85, 100);
        finishPanel.SetState(true);
        finishPanel.SetStars(3);
        // Завершаем игру
        Invoke(nameof(FinishGame), 1f);
    }

    private void OnWrongAnswer(int answer)
    {
        Debug.Log("Неправильный ответ!");

        // Воспроизводим звук
        if (wrongSound != null)
        {
            AudioManager.Instance.PlaySound(wrongSound);
        }

        finishPanel.SetReward(2, 20, 50);
        finishPanel.SetState(false);
        finishPanel.SetStars(0);
        // Завершаем игру
        Invoke(nameof(FinishGame), 1f);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Отписываемся от кнопок
        if (increaseButton != null)
        {
            increaseButton.OnButtonClicked -= OnIncreaseClicked;
        }

        if (decreaseButton != null)
        {
            decreaseButton.OnButtonClicked -= OnDecreaseClicked;
        }
        // Показываем все объекты обратно
        ShowAllObjects();
    }

    private void ShowAllObjects()
    {
        foreach (var group in groups)
        {
            foreach (var obj in group.Objects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    public void ResetGame()
    {
        isAnswering = false;
        isTimerRunning = false;
        ShowAllObjects();
        InitializeGame();
    }

    // Методы для получения информации
    public int GetCorrectAnswer() => correctAnswer;
    public string GetSelectedGroupName() => selectedGroup?.Name;
}
