using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Quiz : GameController
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questionField;
    [SerializeField] private SoundButton soundButton;
    [SerializeField] private QuizButton[] buttons;

    [Header("Questions")]
    [SerializeField] private QuizQuestion[] questions;
    [SerializeField] private int questionsCount = 5;

    [Header("Sounds")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    [Header("Settings")]
    [SerializeField] private float delayAfterAnswer = 1f;

    private List<QuizQuestion> selectedQuestions = new List<QuizQuestion>();
    private int currentQuestionIndex = 0;
    private QuizQuestion currentQuestion;
    private int correctAnswersCount = 0;
    private bool isAnswering = false;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        // Выбираем случайные вопросы
        SelectRandomQuestions();

        // Подписываемся на клики кнопок
        foreach (var button in buttons)
        {
            button.OnAnswerClicked += OnAnswerClicked;
        }

        // Показываем первый вопрос
        currentQuestionIndex = 0;
        correctAnswersCount = 0;
        ShowCurrentQuestion();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (buttons != null)
        {
            foreach (var button in buttons)
            {
                button.OnAnswerClicked -= OnAnswerClicked;
            }
        }
    }

    private void SelectRandomQuestions()
    {
        selectedQuestions.Clear();

        // Создаем список доступных вопросов
        List<QuizQuestion> availableQuestions = new List<QuizQuestion>(questions);

        // Выбираем случайные вопросы
        int count = Mathf.Min(questionsCount, availableQuestions.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableQuestions.Count);
            selectedQuestions.Add(availableQuestions[randomIndex]);
            availableQuestions.RemoveAt(randomIndex);
        }

        Debug.Log($"Выбрано {selectedQuestions.Count} вопросов для викторины");
    }

    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex >= selectedQuestions.Count)
        {
            // Все вопросы пройдены
            CompleteQuiz();
            return;
        }

        currentQuestion = selectedQuestions[currentQuestionIndex];
        isAnswering = false;

        // Обновляем текст вопроса
        if (questionField != null && currentQuestion.Question.LocalizedString != null)
        {
            questionField.text = currentQuestion.Question.LocalizedString.GetLocalizedString();
        }

        // Устанавливаем аудио вопроса
        if (soundButton != null && currentQuestion.Question.LocalizedAudio != null)
        {
            soundButton.SetAudio(currentQuestion.Question.LocalizedAudio);
        }

        // Заполняем кнопки ответами
        SetupAnswerButtons();

        Debug.Log($"Показываем вопрос {currentQuestionIndex + 1}/{selectedQuestions.Count}");
    }

    private void SetupAnswerButtons()
    {
        // Проверяем, что количество кнопок соответствует количеству ответов
        int answersCount = currentQuestion.Answers.Length;

        if (answersCount > buttons.Length)
        {
            Debug.LogWarning($"Недостаточно кнопок! Вопрос имеет {answersCount} ответов, но доступно только {buttons.Length} кнопок.");
            answersCount = buttons.Length;
        }

        // Инициализируем кнопки ответами
        for (int i = 0; i < answersCount; i++)
        {
            buttons[i].Initialize(currentQuestion.Answers[i], i);
            buttons[i].gameObject.SetActive(true);
            buttons[i].SetInteractable(true);
        }

        // Деактивируем лишние кнопки
        for (int i = answersCount; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
    }

    private void OnAnswerClicked(QuizButton clickedButton, int answerIndex)
    {
        if (isAnswering) return; // Предотвращаем множественные клики

        isAnswering = true;

        // Проверяем, правильный ли ответ
        bool isCorrect = answerIndex == currentQuestion.IndexOfCorrect;

        if (isCorrect)
        {
            OnCorrectAnswer(clickedButton);
        }
        else
        {
            OnWrongAnswer(clickedButton);
        }

        // Переходим к следующему вопросу после задержки
        Invoke(nameof(NextQuestion), delayAfterAnswer);
    }

    private void OnCorrectAnswer(QuizButton button)
    {
        Debug.Log("Правильный ответ!");

        correctAnswersCount++;

        // Показываем зеленый цвет
        button.ShowCorrect();

        // Воспроизводим звук правильного ответа
        if (correctSound != null)
        {
            AudioManager.Instance.PlaySound(correctSound);
        }

        // Делаем все кнопки неинтерактивными
        foreach (var btn in buttons)
        {
            btn.SetInteractable(false);
        }
    }

    private void OnWrongAnswer(QuizButton clickedButton)
    {
        Debug.Log("Неправильный ответ!");

        // Показываем красный цвет на неправильном ответе
        clickedButton.ShowWrong();

        // Показываем зеленый цвет на правильном ответе
        if (currentQuestion.IndexOfCorrect >= 0 && currentQuestion.IndexOfCorrect < buttons.Length)
        {
            buttons[currentQuestion.IndexOfCorrect].ShowCorrect();
        }

        // Воспроизводим звук ошибки
        if (wrongSound != null)
        {
            AudioManager.Instance.PlaySound(wrongSound);
        }

        // Делаем все кнопки неинтерактивными
        foreach (var btn in buttons)
        {
            btn.SetInteractable(false);
        }
    }

    private void NextQuestion()
    {
        // Сбрасываем визуалы кнопок
        foreach (var button in buttons)
        {
            button.ResetVisuals();
        }

        // Переходим к следующему вопросу
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void CompleteQuiz()
    {
        Debug.Log($"Викторина завершена! Правильных ответов: {correctAnswersCount}/{selectedQuestions.Count}");

        // Очищаем текстовое поле
        if (questionField != null)
        {
            questionField.text = $"Результат: {correctAnswersCount}/{selectedQuestions.Count}";
        }

        // Завершаем игру
        FinishGame();
    }

    public void ResetQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswersCount = 0;
        selectedQuestions.Clear();
        isAnswering = false;

        // Сбрасываем визуалы кнопок
        foreach (var button in buttons)
        {
            button.ResetVisuals();
        }

        if (questionField != null)
        {
            questionField.text = "";
        }

        InitializeGame();
    }

    // Методы для получения информации о прогрессе
    public int GetCurrentQuestionIndex() => currentQuestionIndex;
    public int GetTotalQuestions() => selectedQuestions.Count;
    public int GetCorrectAnswersCount() => correctAnswersCount;
    public float GetProgress() => selectedQuestions.Count > 0 ? (float)currentQuestionIndex / selectedQuestions.Count : 0f;
    public float GetScore() => selectedQuestions.Count > 0 ? (float)correctAnswersCount / selectedQuestions.Count : 0f;
}
