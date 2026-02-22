using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform wordsContainer;
    [SerializeField] private GameObject wordContainerPrefab;
    [SerializeField] private RectTransform resultPanel;
    [SerializeField] private TextMeshProUGUI resultField;

    [Header("Sentence")]
    [SerializeField] private List<string> sentences = new List<string>();

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private bool useRandomInterval = false;

    [Header("Movement Settings")]
    [SerializeField] private float wordSpeed = 300f;
    [Range(0f, 1f)][SerializeField] private float minYNormalized = 0.2f;
    [Range(0f, 1f)][SerializeField] private float maxYNormalized = 0.8f;

    private List<WordContainer> activeWords = new List<WordContainer>();
    private Coroutine spawnCoroutine;
    private bool isActive = false;

    // Sentence mode
    private string[] sentenceWords;
    private int currentWordIndex = 0;
    private HashSet<int> collectedIndices = new HashSet<int>();
    private Queue<int> pendingWords = new Queue<int>();

    public event Action OnAllWordsCollected;

    public void StartSpawning()
    {
        isActive = true;
        collectedIndices.Clear();
        ClearAllWords();

        // Выбираем случайное предложение и разбиваем на слова
        if (sentences.Count > 0)
        {
            string sentence = sentences[UnityEngine.Random.Range(0, sentences.Count)];
            sentenceWords = sentence.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            sentenceWords = new string[0];
        }

        currentWordIndex = 0;
        pendingWords.Clear();

        // Добавляем все индексы слов в очередь по порядку
        for (int i = 0; i < sentenceWords.Length; i++)
        {
            pendingWords.Enqueue(i);
        }

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnWordsCoroutine());
    }

    public void StopSpawning()
    {
        isActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        ShowResult();
        ClearAllWords();
    }

    private IEnumerator SpawnWordsCoroutine()
    {
        while (isActive && pendingWords.Count > 0)
        {
            float interval = useRandomInterval
                ? UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval)
                : spawnInterval;

            yield return new WaitForSeconds(interval);

            if (!isActive) break;

            SpawnNextWord();
        }
    }

    private void SpawnNextWord()
    {
        if (wordsContainer == null || wordContainerPrefab == null)
            return;

        if (pendingWords.Count == 0)
            return;

        int wordIndex = pendingWords.Dequeue();
        string word = sentenceWords[wordIndex];

        GameObject wordObj = Instantiate(wordContainerPrefab, wordsContainer);
        WordContainer container = wordObj.GetComponent<WordContainer>();

        if (container == null)
        {
            Destroy(wordObj);
            return;
        }

        float screenRight = Screen.width / 2f;
        float screenHeight = Screen.height;
        float screenBottom = -screenHeight / 2f;

        float minY = screenBottom + (screenHeight * minYNormalized);
        float maxY = screenBottom + (screenHeight * maxYNormalized);
        float randomY = UnityEngine.Random.Range(minY, maxY);

        Vector2 startPos = new Vector2(screenRight + 100f, randomY);

        container.Initialize(word, startPos, wordSpeed, (c) => OnWordOutOfBounds(c, wordIndex));
        container.OnWordClicked += (c) => OnWordClicked(c, wordIndex);
        activeWords.Add(container);
    }

    private void OnWordClicked(WordContainer container, int wordIndex)
    {
        if (container != null && !collectedIndices.Contains(wordIndex))
        {
            collectedIndices.Add(wordIndex);

            // Проверяем, все ли слова собраны
            if (collectedIndices.Count >= sentenceWords.Length)
            {
                OnAllWordsCollected?.Invoke();
            }
        }
    }

    private void OnWordOutOfBounds(WordContainer container, int wordIndex)
    {
        if (container != null)
        {
            activeWords.Remove(container);
            Destroy(container.gameObject);

            // Если слово не было нажато — добавляем обратно в очередь
            if (!collectedIndices.Contains(wordIndex))
            {
                pendingWords.Enqueue(wordIndex);

                // Если корутина уже завершилась, перезапускаем
                if (spawnCoroutine == null && isActive)
                {
                    spawnCoroutine = StartCoroutine(SpawnWordsCoroutine());
                }
            }
        }
    }

    private void ClearAllWords()
    {
        foreach (var word in activeWords)
        {
            if (word != null && word.gameObject != null)
                Destroy(word.gameObject);
        }
        activeWords.Clear();
    }

    private void OnDisable()
    {
        StopSpawning();
    }

    private void ShowResult()
    {
        if (sentenceWords == null || sentenceWords.Length == 0)
        {
            if (resultField != null)
                resultField.text = "";
            if (resultPanel != null)
                resultPanel.gameObject.SetActive(true);
            return;
        }

        int total = sentenceWords.Length;
        int collected = collectedIndices.Count;

        if (resultField != null)
        {
            if (collected >= total)
            {
                resultField.text = string.Join(" ", sentenceWords);
            }
            else
            {
                resultField.text = $"Жиналған сөздер: {collected}/{total}";
            }
        }

        if (resultPanel != null)
            resultPanel.gameObject.SetActive(true);
    }
}
