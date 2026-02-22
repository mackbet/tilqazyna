using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DancingOnIce : GameController
{
    [Header("Game Data")]
    [SerializeField] private List<string> sentences = new List<string>();
    [SerializeField] private int sessionsCount = 3; // Количество сессий (раундов)

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI targetWordText;
    [SerializeField] private RectTransform lettersContainer;
    [SerializeField] private DancingOnIceLetter letterPrefab;
    [SerializeField] private RectTransform connectionLinesContainer;
    [SerializeField] private Image connectionLinePrefab;

    [Header("Character")]
    [SerializeField] private RectTransform character;
    [SerializeField] private SpriteAnimController characterAnimController;
    [SerializeField] private float characterMoveSpeed = 500f;
    [SerializeField] private RectTransform characterParticles;

    [Header("Word Spawn Settings")]
    [SerializeField][Range(0f, 1f)] private float minSpawnWidth = 0.1f;
    [SerializeField][Range(0f, 1f)] private float maxSpawnWidth = 0.9f;
    [SerializeField][Range(0f, 1f)] private float minSpawnHeight = 0.2f;
    [SerializeField][Range(0f, 1f)] private float maxSpawnHeight = 0.8f;
    [SerializeField] private float minDistanceBetweenWords = 150f;
    [SerializeField] private float minDistanceFromCharacter = 150f;

    [Header("Scale Settings")]
    [SerializeField] private float minWordScale = 0.6f;
    [SerializeField] private float maxWordScale = 1.2f;
    [SerializeField] private float endDelay = 1f;

    [Header("Visual Settings")]
    [SerializeField] private float connectionLineWidth = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip completeSound;

    private int currentSessionIndex = 0;
    private string currentSentence;
    private string[] currentSentenceWords; // Слова текущего предложения
    private List<string> availableSentences = new List<string>();
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
        FilterAvailableSentences();
        currentSessionIndex = 0;

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

    private void FilterAvailableSentences()
    {
        availableSentences.Clear();

        if (sentences == null || sentences.Count == 0)
        {
            Debug.LogError("DancingOnIce: Список предложений пуст!");
            return;
        }

        foreach (var sentence in sentences)
        {
            if (!string.IsNullOrEmpty(sentence))
            {
                string[] splitWords = sentence.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (splitWords.Length >= 2)
                {
                    availableSentences.Add(sentence);
                }
            }
        }

        if (availableSentences.Count == 0)
        {
            Debug.LogError("DancingOnIce: Нет подходящих предложений (минимум 2 слова)!");
        }
    }

    private void StartNextWord()
    {
        if (availableSentences.Count == 0)
        {
            Debug.LogError("DancingOnIce: Нет доступных предложений!");
            LoseGame();
            return;
        }

        // Выбираем случайное предложение из доступных
        currentSentence = availableSentences[Random.Range(0, availableSentences.Count)];
        currentSentenceWords = currentSentence.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        ResetRound();
        SetupUI();
        SpawnWords();

        if (character != null)
        {
            character.gameObject.SetActive(true);
            character.localScale = originalCharacterScale;
        }

        isInputEnabled = true;

        if (characterAnimController != null)
        {
            characterAnimController.Play("idle", loop: true);
        }
    }

    private void ResetRound()
    {
        selectedLetters.Clear();
        isInputEnabled = false;
        isCharacterMoving = false;

        foreach (var letter in letters)
        {
            if (letter != null)
                Destroy(letter.gameObject);
        }
        letters.Clear();

        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();
    }

    private void SetupUI()
    {
        if (targetWordText != null && currentSentence != null)
        {
            targetWordText.text = currentSentence.ToUpper();
        }
    }

    private void SpawnWords()
    {
        if (currentSentenceWords == null || currentSentenceWords.Length == 0)
            return;

        List<Vector2> usedPositions = new List<Vector2>();

        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        // Перемешиваем индексы для случайного размещения слов
        List<int> shuffledIndices = new List<int>();
        for (int i = 0; i < currentSentenceWords.Length; i++)
            shuffledIndices.Add(i);

        // Fisher-Yates shuffle
        for (int i = shuffledIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = shuffledIndices[i];
            shuffledIndices[i] = shuffledIndices[j];
            shuffledIndices[j] = temp;
        }

        // Создаём кнопки слов в перемешанном порядке
        DancingOnIceLetter[] wordButtons = new DancingOnIceLetter[currentSentenceWords.Length];

        for (int si = 0; si < shuffledIndices.Count; si++)
        {
            int originalIndex = shuffledIndices[si];
            string wordText = currentSentenceWords[originalIndex];

            Vector2 position = GetRandomPosition(usedPositions);
            usedPositions.Add(position);

            DancingOnIceLetter wordButton = Instantiate(letterPrefab, lettersContainer);
            wordButton.Initialize(wordText, originalIndex, this);
            RectTransform wordRect = wordButton.GetComponent<RectTransform>();
            wordRect.anchoredPosition = position;

            float scale = CalculateScaleByYPositionWithLimits(position.y, canvasSize.y, minWordScale, maxWordScale);
            wordRect.localScale = Vector3.one * scale;

            wordButtons[originalIndex] = wordButton;
        }

        // Сохраняем в порядке оригинальных индексов
        letters.AddRange(wordButtons);
    }

    private Vector2 GetRandomPosition(List<Vector2> usedPositions)
    {
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.sizeDelta;

        Vector2 position;
        int attempts = 0;
        int maxAttempts = 100;

        do
        {
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
        if (character != null)
        {
            Vector2 characterPosition = character.anchoredPosition;
            if (Vector2.Distance(position, characterPosition) < minDistanceFromCharacter)
                return false;
        }

        foreach (var usedPos in usedPositions)
        {
            if (Vector2.Distance(position, usedPos) < minDistanceBetweenWords)
                return false;
        }
        return true;
    }

    private float CalculateScaleByYPositionWithLimits(float yPosition, float canvasHeight, float minScale, float maxScale)
    {
        float minY = -canvasHeight / 2f + minSpawnHeight * canvasHeight;
        float maxY = -canvasHeight / 2f + maxSpawnHeight * canvasHeight;

        float normalizedY = (yPosition - minY) / (maxY - minY);
        normalizedY = Mathf.Clamp01(normalizedY);

        float scale = Mathf.Lerp(maxScale, minScale, normalizedY);

        return scale;
    }


    public void OnLetterClicked(DancingOnIceLetter letter)
    {
        if (!isInputEnabled || isCharacterMoving || letter == null)
            return;

        if (selectedLetters.Contains(letter))
            return;

        // Определяем какое слово должно быть следующим
        int expectedPosition = selectedLetters.Count;
        string expectedWord = currentSentenceWords[expectedPosition];

        // Сравниваем слова (без учёта регистра)
        if (string.Equals(letter.Word, expectedWord, System.StringComparison.OrdinalIgnoreCase))
        {
            selectedLetters.Add(letter);
            letter.SetSelected(true);

            if (correctSound != null)
                AudioManager.Instance.PlaySound(correctSound);

            UpdateConnectionLine();

            if (selectedLetters.Count == currentSentenceWords.Length)
            {
                OnWordCompleted();
            }
        }
        else
        {
            OnWordFailed();
        }
    }

    private void UpdateConnectionLine()
    {
        if (connectionLinePrefab == null || connectionLinesContainer == null)
            return;

        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();

        for (int i = 0; i < selectedLetters.Count - 1; i++)
        {
            DancingOnIceLetter fromLetter = selectedLetters[i];
            DancingOnIceLetter toLetter = selectedLetters[i + 1];

            if (fromLetter == null || toLetter == null)
                continue;

            RectTransform fromRect = fromLetter.GetComponent<RectTransform>();
            RectTransform toRect = toLetter.GetComponent<RectTransform>();

            Image line = Instantiate(connectionLinePrefab, connectionLinesContainer);
            RectTransform lineRect = line.GetComponent<RectTransform>();

            Vector2 fromPos = fromRect.anchoredPosition;
            Vector2 toPos = toRect.anchoredPosition;

            Vector2 center = (fromPos + toPos) / 2f;
            lineRect.anchoredPosition = center;

            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            lineRect.sizeDelta = new Vector2(distance, connectionLineWidth);
            lineRect.rotation = Quaternion.Euler(0, 0, angle);

            connectionLines.Add(line);
        }
    }

    private void OnWordCompleted()
    {
        isInputEnabled = false;

        if (completeSound != null)
            AudioManager.Instance.PlaySound(completeSound);

        StartCoroutine(MoveCharacterThroughLetters());
    }

    private IEnumerator MoveCharacterThroughLetters()
    {
        if (character == null)
        {
            yield return new WaitForSeconds(0.5f);

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

        if (characterAnimController != null)
        {
            characterAnimController.Play("walking", loop: true);
        }

        characterParticles.position = character.position;

        foreach (var letter in selectedLetters)
        {
            if (letter == null) continue;

            Vector3 targetWorldPos = letter.transform.position;
            Vector3 startPos = character.position;

            Vector2 direction = (targetWorldPos - startPos).normalized;

            float flipX = direction.x < 0 ? -1f : 1f;

            while (Vector3.Distance(character.position, targetWorldPos) > 5f)
            {
                character.position = Vector3.MoveTowards(
                    character.position,
                    targetWorldPos,
                    characterMoveSpeed * Time.deltaTime
                );

                character.localScale = new Vector3(
                    originalCharacterScale.x * flipX,
                    originalCharacterScale.y,
                    originalCharacterScale.z
                );

                characterParticles.position = character.position;

                yield return null;
            }

            yield return new WaitForSeconds(0.2f);
        }

        isCharacterMoving = false;

        if (character != null)
        {
            float flipX = character.position.x < 0 ? 1f : -1f;
            character.localScale = new Vector3(
                originalCharacterScale.x * flipX,
                originalCharacterScale.y,
                originalCharacterScale.z
            );
        }

        if (characterAnimController != null)
        {
            characterAnimController.Play("jumping", false);
        }

        currentSessionIndex++;

        yield return new WaitForSeconds(endDelay);

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
            ResetRound();
            SetupUI();
            SpawnWords();
            isInputEnabled = true;

            if (characterAnimController != null)
            {
                characterAnimController.Play("idle", loop: true);
            }
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

        foreach (var letter in letters)
        {
            if (letter != null)
                Destroy(letter.gameObject);
        }
        letters.Clear();
        selectedLetters.Clear();

        foreach (var line in connectionLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        connectionLines.Clear();
    }
}
