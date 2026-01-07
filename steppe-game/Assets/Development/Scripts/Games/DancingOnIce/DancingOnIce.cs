using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DancingOnIce : GameController
{
    [Header("Game Data")]
    [SerializeField] private WordData[] words;
    [SerializeField] private int sessionsCount = 3; // Количество сессий (раундов)
    [SerializeField] private int minLettersInWord = 3; // Минимальное количество букв в слове
    [SerializeField] private int maxLettersInWord = 10; // Максимальное количество букв в слове

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI targetWordText;
    [SerializeField] private RectTransform lettersContainer;
    [SerializeField] private DancingOnIceLetter letterPrefab;
    [SerializeField] private RectTransform connectionLinesContainer;
    [SerializeField] private Image connectionLinePrefab;

    [Header("Character")]
    [SerializeField] private RectTransform character;
    [SerializeField] private float characterMoveSpeed = 500f;
    [SerializeField] private RectTransform characterParticles;

    [Header("Letter Spawn Settings")]
    [SerializeField][Range(0f, 1f)] private float minSpawnWidth = 0.1f; // Минимальная ширина спавна (0 = левый край, 1 = правый край)
    [SerializeField][Range(0f, 1f)] private float maxSpawnWidth = 0.9f; // Максимальная ширина спавна (0 = левый край, 1 = правый край)
    [SerializeField][Range(0f, 1f)] private float minSpawnHeight = 0.2f; // Минимальная высота спавна (0 = низ, 1 = верх)
    [SerializeField][Range(0f, 1f)] private float maxSpawnHeight = 0.8f; // Максимальная высота спавна (0 = низ, 1 = верх)
    [SerializeField] private float minDistanceBetweenLetters = 100f;
    [SerializeField] private float minDistanceFromCharacter = 150f; // Минимальное расстояние от персонажа (центр экрана)

    [Header("Scale Settings")]
    [SerializeField] private float minLetterScale = 0.6f; // Минимальный размер буквы (дальше - меньше)
    [SerializeField] private float maxLetterScale = 1.2f; // Максимальный размер буквы (ближе - больше)
    [SerializeField] private float endDelay = 1f; // Задержка в конце перед показом панели финиша

    [Header("Visual Settings")]
    [SerializeField] private float connectionLineWidth = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip completeSound;

    private int currentSessionIndex = 0;
    private WordData currentWord;
    private List<WordData> availableWords = new List<WordData>();
    private List<DancingOnIceLetter> letters = new List<DancingOnIceLetter>();
    private List<DancingOnIceLetter> selectedLetters = new List<DancingOnIceLetter>();
    private List<Image> connectionLines = new List<Image>();
    private bool isInputEnabled = false;
    private bool isCharacterMoving = false;
    private Vector3 originalCharacterScale;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        SetLives(3);
        FilterAvailableWords();
        currentSessionIndex = 0;

        // Сохраняем оригинальный размер персонажа для flip (абсолютные значения)
        if (character != null)
        {
            originalCharacterScale = new Vector3(
                Mathf.Abs(character.localScale.x),
                Mathf.Abs(character.localScale.y),
                Mathf.Abs(character.localScale.z)
            );
        }

        StartNextWord();
    }

    private void FilterAvailableWords()
    {
        availableWords.Clear();

        if (words == null || words.Length == 0)
        {
            return;
        }

        // Фильтруем слова по количеству букв (от min до max) и исключаем составные слова (с пробелами)
        foreach (var word in words)
        {
            if (word != null && !string.IsNullOrEmpty(word.Word))
            {
                string wordText = word.Word;
                int wordLength = wordText.Length;

                // Проверяем что слово одно (не содержит пробелов)
                if (!wordText.Contains(" ") && wordLength >= minLettersInWord && wordLength <= maxLettersInWord)
                {
                    availableWords.Add(word);
                }
            }
        }

        if (availableWords.Count == 0)
        {
            Debug.LogError($"DancingOnIce: Нет подходящих слов (одно слово, {minLettersInWord}-{maxLettersInWord} букв)!");
        }
    }

    private void StartNextWord()
    {
        if (availableWords.Count == 0)
        {
            Debug.LogError("DancingOnIce: Нет доступных слов!");
            LoseGame();
            return;
        }

        // Выбираем случайное слово из доступных
        currentWord = availableWords[Random.Range(0, availableWords.Count)];

        ResetRound();
        SetupUI();
        SpawnLetters();

        if (character != null)
        {
            character.gameObject.SetActive(true);

            // Возвращаем исходный размер персонажа (без flip)
            character.localScale = originalCharacterScale;
        }

        isInputEnabled = true;
    }

    private void ResetRound()
    {
        selectedLetters.Clear();
        isInputEnabled = false;
        isCharacterMoving = false;

        // Очищаем старые буквы
        foreach (var letter in letters)
        {
            if (letter != null)
                Destroy(letter.gameObject);
        }
        letters.Clear();

        // Очищаем линии соединения
        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();
    }

    private void SetupUI()
    {
        if (targetWordText != null && currentWord != null)
        {
            targetWordText.text = currentWord.Word.ToUpper();
        }
    }

    private void SpawnLetters()
    {
        if (currentWord == null || string.IsNullOrEmpty(currentWord.Word))
            return;

        string word = currentWord.Word.ToUpper();
        List<Vector2> usedPositions = new List<Vector2>();

        // Получаем размеры канваса для вычисления масштаба
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Создаем буквы
        for (int i = 0; i < word.Length; i++)
        {
            char letterChar = word[i];
            Vector2 position = GetRandomPosition(usedPositions);
            usedPositions.Add(position);

            DancingOnIceLetter letter = Instantiate(letterPrefab, lettersContainer);
            letter.Initialize(letterChar, i, this);
            RectTransform letterRect = letter.GetComponent<RectTransform>();
            letterRect.anchoredPosition = position;

            // Вычисляем масштаб буквы в зависимости от Y позиции (относительно лимитов спавна)
            float scale = CalculateScaleByYPositionWithLimits(position.y, canvasSize.y, minLetterScale, maxLetterScale);
            letterRect.localScale = Vector3.one * scale;

            letters.Add(letter);
        }
    }

    private Vector2 GetRandomPosition(List<Vector2> usedPositions)
    {
        // Получаем размеры канваса (экрана)
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        Vector2 position;
        int attempts = 0;
        int maxAttempts = 100;

        do
        {
            // Вычисляем случайные позиции на основе нормализованных значений (0-1)
            float minX = -canvasSize.x / 2f + minSpawnWidth * canvasSize.x;
            float maxX = -canvasSize.x / 2f + maxSpawnWidth * canvasSize.x;
            float minY = -canvasSize.y / 2f + minSpawnHeight * canvasSize.y;
            float maxY = -canvasSize.y / 2f + maxSpawnHeight * canvasSize.y;

            position = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );
            attempts++;

            if (attempts >= maxAttempts)
                break;

        } while (!IsPositionValid(position, usedPositions));

        return position;
    }

    private bool IsPositionValid(Vector2 position, List<Vector2> usedPositions)
    {
        // Проверяем расстояние от персонажа (с учетом pivot)
        if (character != null)
        {
            Vector2 characterPosition = character.anchoredPosition;
            if (Vector2.Distance(position, characterPosition) < minDistanceFromCharacter)
                return false;
        }

        // Проверяем расстояние от других букв
        foreach (var usedPos in usedPositions)
        {
            if (Vector2.Distance(position, usedPos) < minDistanceBetweenLetters)
                return false;
        }
        return true;
    }

    private float CalculateScaleByYPositionWithLimits(float yPosition, float canvasHeight, float minScale, float maxScale)
    {
        // Вычисляем границы спавна в координатах
        float minY = -canvasHeight / 2f + minSpawnHeight * canvasHeight;
        float maxY = -canvasHeight / 2f + maxSpawnHeight * canvasHeight;

        // Нормализуем Y позицию относительно границ спавна: minY (низ) = 0, maxY (верх) = 1
        float normalizedY = (yPosition - minY) / (maxY - minY);
        normalizedY = Mathf.Clamp01(normalizedY);

        // Чем выше (normalizedY ближе к 1), тем меньше размер (дальше)
        // Чем ниже (normalizedY ближе к 0), тем больше размер (ближе)
        float scale = Mathf.Lerp(maxScale, minScale, normalizedY);

        return scale;
    }


    public void OnLetterClicked(DancingOnIceLetter letter)
    {
        if (!isInputEnabled || isCharacterMoving || letter == null)
            return;

        // Проверяем, не выбрана ли уже эта буква
        if (selectedLetters.Contains(letter))
            return;

        // Определяем какая буква должна быть следующей
        int expectedPosition = selectedLetters.Count;
        char expectedChar = currentWord.Word.ToUpper()[expectedPosition];

        // Проверяем, правильная ли это буква (сравниваем символы)
        if (letter.Letter == expectedChar)
        {
            // Правильная буква
            selectedLetters.Add(letter);
            letter.SetSelected(true);

            if (correctSound != null)
                AudioManager.Instance.PlaySound(correctSound);

            UpdateConnectionLine();

            // Проверяем, завершено ли слово
            if (selectedLetters.Count == letters.Count)
            {
                OnWordCompleted();
            }
        }
        else
        {
            // Неправильная буква - сброс выбора и потеря жизни
            OnWordFailed();
        }
    }

    private void UpdateConnectionLine()
    {
        if (connectionLinePrefab == null || connectionLinesContainer == null)
            return;

        // Очищаем старые линии
        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();

        // Создаем линии между последовательными выбранными буквами
        for (int i = 0; i < selectedLetters.Count - 1; i++)
        {
            DancingOnIceLetter fromLetter = selectedLetters[i];
            DancingOnIceLetter toLetter = selectedLetters[i + 1];

            if (fromLetter == null || toLetter == null)
                continue;

            RectTransform fromRect = fromLetter.GetComponent<RectTransform>();
            RectTransform toRect = toLetter.GetComponent<RectTransform>();

            // Создаем линию
            Image line = Instantiate(connectionLinePrefab, connectionLinesContainer);
            RectTransform lineRect = line.GetComponent<RectTransform>();

            // Вычисляем позицию и размер линии
            Vector2 fromPos = fromRect.anchoredPosition;
            Vector2 toPos = toRect.anchoredPosition;

            // Центр линии - посередине между двумя буквами
            Vector2 center = (fromPos + toPos) / 2f;
            lineRect.anchoredPosition = center;

            // Вычисляем расстояние и угол
            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Устанавливаем размер линии (длина = расстояние, высота = ширина линии)
            lineRect.sizeDelta = new Vector2(distance, connectionLineWidth);

            // Поворачиваем линию
            lineRect.rotation = Quaternion.Euler(0, 0, angle);

            connectionLines.Add(line);
        }
    }

    private void OnWordCompleted()
    {
        isInputEnabled = false;

        if (completeSound != null)
            AudioManager.Instance.PlaySound(completeSound);

        // Воспроизводим аудио слова, если есть
        if (currentWord.AudioClip != null)
        {
            AudioManager.Instance.PlaySound(currentWord.AudioClip);
        }

        // Запускаем движение персонажа
        StartCoroutine(MoveCharacterThroughLetters());
    }

    private IEnumerator MoveCharacterThroughLetters()
    {
        if (character == null)
        {
            yield return new WaitForSeconds(0.5f);

            // Увеличиваем счетчик сессий после успешного завершения
            currentSessionIndex++;

            if (currentSessionIndex < sessionsCount)
            {
                StartNextWord();
            }
            else
            {
                WinGame();
            }
            yield break;
        }

        isCharacterMoving = true;

        // Активируем систему частиц
        characterParticles.position = character.position;

        // Двигаемся к каждой букве по порядку
        foreach (var letter in selectedLetters)
        {
            if (letter == null) continue;

            // Используем мировую позицию для точного движения (не зависит от pivot)
            Vector3 targetWorldPos = letter.transform.position;
            Vector3 startPos = character.position;

            // Вычисляем направление для поворота (только по горизонтали)
            Vector2 direction = (targetWorldPos - startPos).normalized;

            // Поворот персонажа только по горизонтали (flip лево/право)
            // Если движется влево (direction.x < 0), то отражаем по X
            float flipX = direction.x < 0 ? -1f : 1f;

            // Двигаемся к букве
            while (Vector3.Distance(character.position, targetWorldPos) > 5f)
            {
                // Двигаемся в мировых координатах (не зависит от pivot)
                character.position = Vector3.MoveTowards(
                    character.position,
                    targetWorldPos,
                    characterMoveSpeed * Time.deltaTime
                );

                // Применяем только flip без изменения размера
                character.localScale = new Vector3(
                    originalCharacterScale.x * flipX,
                    originalCharacterScale.y,
                    originalCharacterScale.z
                );

                characterParticles.position = character.position;

                yield return null;
            }

            // Небольшая пауза на букве
            yield return new WaitForSeconds(0.2f);
        }

        isCharacterMoving = false;

        // Поворачиваем персонажа в сторону центра экрана
        if (character != null)
        {
            // Если персонаж левее центра - смотрит вправо (flipX = 1)
            // Если персонаж правее центра - смотрит влево (flipX = -1)
            float flipX = character.position.x < 0 ? 1f : -1f;
            character.localScale = new Vector3(
                originalCharacterScale.x * flipX,
                originalCharacterScale.y,
                originalCharacterScale.z
            );
        }

        // Увеличиваем счетчик сессий после успешного завершения
        currentSessionIndex++;

        // Переходим к следующему слову или завершаем игру
        yield return new WaitForSeconds(0.5f);

        if (currentSessionIndex < sessionsCount)
        {
            StartNextWord();
        }
        else
        {
            WinGame();
        }
    }

    private void OnWordFailed()
    {
        ResetSelection();
        SetLives(Lives - 1);

        if (wrongSound != null)
            AudioManager.Instance.PlaySound(wrongSound);

        if (Lives <= 0)
        {
            LoseGame();
        }
        else
        {
            // Перезапускаем текущее слово
            ResetRound();
            SetupUI();
            SpawnLetters();
            isInputEnabled = true;
        }
    }

    private void ResetSelection()
    {
        foreach (var letter in selectedLetters)
        {
            if (letter != null)
                letter.SetSelected(false);
        }

        selectedLetters.Clear();

        // Очищаем линии соединения
        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();
    }

    private void WinGame()
    {
        StartCoroutine(WinGameWithDelay());
    }

    private void LoseGame()
    {
        StartCoroutine(LoseGameWithDelay());
    }

    private IEnumerator WinGameWithDelay()
    {
        yield return new WaitForSeconds(endDelay);

        if (finishPanel != null)
        {
            finishPanel.SetReward(7 + Lives * 5, 80, 80);
            finishPanel.SetState(true);
            finishPanel.SetStars(Lives);
        }
        FinishGame();
    }

    private IEnumerator LoseGameWithDelay()
    {
        yield return new WaitForSeconds(endDelay);

        if (finishPanel != null)
        {
            finishPanel.SetReward(2, 20, 30);
            finishPanel.SetState(false);
            finishPanel.SetStars(0);
        }
        FinishGame();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Очищаем буквы
        foreach (var letter in letters)
        {
            if (letter != null)
                Destroy(letter.gameObject);
        }
        letters.Clear();
        selectedLetters.Clear();

        // Очищаем линии соединения
        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();
    }
}
