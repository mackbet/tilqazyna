using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberOfTargets : GameController
{
    [Header("UI")]
    [SerializeField] private Image groupIconImage;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI numberDisplayText;
    [SerializeField] private CustomButton increaseButton;
    [SerializeField] private CustomButton decreaseButton;
    [SerializeField] private CustomButton submitButton;

    [Header("Groups")]
    [SerializeField] private TargetGroup[] groups;

    [Header("Settings")]
    [SerializeField] private int minHiddenPerGroup = 1;
    [SerializeField] private int maxHiddenPerGroup = 3;
    [SerializeField] private int minNumber = 0;
    [SerializeField] private int maxNumber = 10;

    [Header("Sounds")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip buttonClickSound;

    private TargetGroup selectedGroup;
    private int correctAnswer;
    private int currentNumber;
    private bool isAnswering = false;

    [Serializable]
    private class TargetGroup
    {
        [field: SerializeField] public string Name { get; private set; }
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

        // Показываем вопрос
        ShowQuestion();
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
        // Подписываемся на кнопки
        if (increaseButton != null)
        {
            increaseButton.OnButtonClicked += OnIncreaseClicked;
        }

        if (decreaseButton != null)
        {
            decreaseButton.OnButtonClicked += OnDecreaseClicked;
        }

        if (submitButton != null)
        {
            submitButton.OnButtonClicked += OnSubmitClicked;
        }
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
        // Показываем иконку группы
        if (groupIconImage != null && selectedGroup.Sprite != null)
        {
            groupIconImage.sprite = selectedGroup.Sprite;
        }

        // Показываем текст вопроса
        if (questionText != null)
        {
            questionText.text = $"Сколько {selectedGroup.Name}?";
        }

        isAnswering = false;
    }

    private void OnSubmitClicked()
    {
        if (isAnswering) return;

        isAnswering = true;

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

        // Обновляем текст вопроса
        if (questionText != null)
        {
            questionText.text = $"Правильно! {answer} {selectedGroup.Name}";
        }

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

        // Показываем правильный ответ
        if (questionText != null)
        {
            questionText.text = $"Неправильно! Правильный ответ: {correctAnswer}";
        }

        // Завершаем игру
        Invoke(nameof(FailGame), 1f);
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

        if (submitButton != null)
        {
            submitButton.OnButtonClicked -= OnSubmitClicked;
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
        ShowAllObjects();
        InitializeGame();
    }

    // Методы для получения информации
    public int GetCorrectAnswer() => correctAnswer;
    public string GetSelectedGroupName() => selectedGroup?.Name;
}
