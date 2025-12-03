using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StreamOfWords : GameController
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private SoundButton soundButton;
    [SerializeField] private StreamOfWordsTarget[] targets;

    [Header("Game Settings")]
    [SerializeField] private int wordsToSelect = 10;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    private List<StreamOfWordsTarget> selectedTargets = new List<StreamOfWordsTarget>();
    private int currentWordIndex = 0;
    private StreamOfWordsTarget currentCorrectTarget;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        // Выбираем случайные слова
        SelectRandomWords();

        // Подписываемся на клики
        foreach (var target in targets)
        {
            target.OnTargetClicked += OnTargetClicked;
        }

        // Показываем первое слово
        currentWordIndex = 0;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (targets != null)
        {
            foreach (var target in targets)
            {
                target.OnTargetClicked -= OnTargetClicked;
            }
        }
    }

    private void SelectRandomWords()
    {
        selectedTargets.Clear();

        // Создаем список доступных targets
        List<StreamOfWordsTarget> availableTargets = new List<StreamOfWordsTarget>(targets);

        // Выбираем случайные targets
        int count = Mathf.Min(wordsToSelect, availableTargets.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableTargets.Count);
            selectedTargets.Add(availableTargets[randomIndex]);
            availableTargets.RemoveAt(randomIndex);
        }
    }

    public void ShowCurrentWord()
    {
        if (currentWordIndex >= selectedTargets.Count)
        {
            // Все слова пройдены
            CompleteGame();
            return;
        }

        currentCorrectTarget = selectedTargets[currentWordIndex];

        // Обновляем текстовое поле
        if (textField != null)
        {
            textField.text = currentCorrectTarget.Word;
            soundButton.SetAudio(currentCorrectTarget.Phrase.LocalizedAudio);
            soundButton.PlayAudio();
        }
    }

    private void OnTargetClicked(StreamOfWordsTarget clickedTarget)
    {
        // Проверяем, правильное ли слово нажато
        if (clickedTarget == currentCorrectTarget)
        {
            // Правильно!
            OnCorrectWord(clickedTarget);
        }
        else
        {
            // Неправильно
            OnWrongWord(clickedTarget);
        }
    }

    private void OnCorrectWord(StreamOfWordsTarget target)
    {
        // Воспроизводим звук
        if (correctSound != null)
        {
            AudioManager.Instance.PlaySound(correctSound);
        }

        // Применяем временный материал для визуального эффекта
        target.ApplyTemporaryMaterial();

        // Переходим к следующему слову
        currentWordIndex++;
        ShowCurrentWord();
    }

    private void OnWrongWord(StreamOfWordsTarget target)
    {
        // Воспроизводим звук ошибки
        if (wrongSound != null)
        {
            AudioManager.Instance.PlaySound(wrongSound);
        }

        // Можно добавить визуальную обратную связь (например, встряхивание текста)
    }

    private void CompleteGame()
    {
        currentCorrectTarget = null;
        // Завершаем игру
        FinishGame();
    }

    public void ResetGame()
    {
        currentWordIndex = 0;
        selectedTargets.Clear();

        if (textField != null)
        {
            textField.text = "";
        }

        InitializeGame();
    }

    // Методы для получения информации о прогрессе
    public int GetCurrentWordIndex() => currentWordIndex;
    public int GetTotalWords() => selectedTargets.Count;
    public float GetProgress() => selectedTargets.Count > 0 ? (float)currentWordIndex / selectedTargets.Count : 0f;
}
