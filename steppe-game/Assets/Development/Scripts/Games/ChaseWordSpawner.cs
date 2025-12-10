using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WordSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform wordsContainer;
    [SerializeField] private GameObject wordContainerPrefab; // Префаб с WordContainer компонентом
    [SerializeField] private RectTransform resultPanel;
    [SerializeField] private TextMeshProUGUI resultField;

    [Header("Word Data")]
    [SerializeField] private List<WordData> availableWords = new List<WordData>();

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private bool useRandomInterval = false;

    [Header("Movement Settings")]
    [SerializeField] private float wordSpeed = 300f;
    [Range(0f, 1f)][SerializeField] private float minYNormalized = 0.2f; // 0 = низ экрана, 1 = верх экрана
    [Range(0f, 1f)][SerializeField] private float maxYNormalized = 0.8f;

    private List<WordContainer> activeWords = new List<WordContainer>();
    private List<WordData> clickedWords = new List<WordData>();
    private Coroutine spawnCoroutine;
    private bool isActive = false;

    public void StartSpawning()
    {
        isActive = true;
        clickedWords.Clear();
        ClearAllWords();

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
        while (isActive)
        {
            float interval = useRandomInterval
                ? Random.Range(minSpawnInterval, maxSpawnInterval)
                : spawnInterval;

            yield return new WaitForSeconds(interval);
            SpawnWord();
        }
    }

    private void SpawnWord()
    {
        if (wordsContainer == null || wordContainerPrefab == null || availableWords.Count == 0)
            return;

        WordData wordData = availableWords[Random.Range(0, availableWords.Count)];
        if (wordData == null) return;

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

        // Конвертируем нормализованные значения в реальные координаты
        float minY = screenBottom + (screenHeight * minYNormalized);
        float maxY = screenBottom + (screenHeight * maxYNormalized);
        float randomY = Random.Range(minY, maxY);

        Vector2 startPos = new Vector2(screenRight + 100f, randomY);

        container.Initialize(wordData, startPos, wordSpeed, OnWordOutOfBounds);
        container.OnWordClicked += OnWordClicked;
        activeWords.Add(container);
    }

    private void OnWordClicked(WordContainer container)
    {
        if (container != null)
        {
            WordData wordData = container.GetWordData();
            if (wordData != null)
            {
                clickedWords.Add(wordData);
            }

            //activeWords.Remove(container);
            //Destroy(container.gameObject);
        }
    }

    private void OnWordOutOfBounds(WordContainer container)
    {
        if (container != null)
        {
            activeWords.Remove(container);
            Destroy(container.gameObject);
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
        if (clickedWords.Count > 0)
        {
            string words = string.Join(", ", clickedWords.Select(w => w.Word));
            resultField.text = "Сен осындай сөздерді білдің: \n \n" + words;
        }
        else
        {
            resultField.text = "Сен ешқандай сөз үйренбедің(";
        }

        resultPanel.gameObject.SetActive(true);
    }
}