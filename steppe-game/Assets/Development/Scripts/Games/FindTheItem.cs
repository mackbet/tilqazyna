using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FindTheItem : GameController
{
    [Header("Wagons")]
    [SerializeField] private FindTheItemWagon[] wagons; // 3 вагона

    [Header("Items")]
    [SerializeField] private FindTheItemVariant[] availableItems; // Доступные предметы

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Image questionItemImage;
    [SerializeField] private SoundButton questionSound;
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject rememberItemsPanel; // Панель "Запомни предметы в вагонах"

    [Header("Game Settings")]
    [SerializeField] private int roundsCount = 5;
    [SerializeField] private float showItemsDuration = 3f; // Время показа предметов перед перемешиванием
    [SerializeField] private float delayBeforeShuffle = 1f; // Задержка после скрытия, перед перемешиванием
    [SerializeField] private float shuffleDuration = 0.5f; // Длительность одного перемещения

    [Header("Sounds")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip shuffleSound;

    private int currentRound = 0;
    private int wonRounds = 0;
    private FindTheItemVariant targetItem;
    private FindTheItemWagon correctWagon;
    private bool isProcessing = false;
    private List<Vector3> originalWagonPositions = new List<Vector3>();

    protected override void InitializeGame()
    {
        base.InitializeGame();

        currentRound = 0;

        // Сохраняем исходные позиции вагонов
        originalWagonPositions.Clear();
        foreach (var wagon in wagons)
        {
            originalWagonPositions.Add(wagon.GetPosition());
        }

        // Подписываемся на клики по вагонам
        foreach (var wagon in wagons)
        {
            wagon.OnWagonClicked += OnWagonClicked;
        }

        StartCoroutine(StartNewRound());
    }

    private IEnumerator StartNewRound()
    {
        if (currentRound >= roundsCount)
        {
            float value = (float)wonRounds / roundsCount;
            finishPanel.SetReward(Mathf.RoundToInt(20 * value), Mathf.RoundToInt(100 * value), Mathf.RoundToInt(100));
            finishPanel.SetState(wonRounds > 0);
            finishPanel.SetStars(wonRounds > 0 ? Mathf.CeilToInt(value * 3) : 0);
            FinishGame();
            yield break;
        }

        isProcessing = true;
        currentRound++;

        // Скрываем панель вопроса
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }

        // Расставляем предметы в вагоны
        PlaceItemsInWagons();

        // ФАЗА 1: Показываем содержимое вагонов для запоминания
        Debug.Log($"Раунд {currentRound}: Показываем предметы в вагонах");

        // Показываем панель с подсказкой
        if (rememberItemsPanel != null)
        {
            rememberItemsPanel.SetActive(true);
        }

        // Анимируем появление предметов в вагонах БЕЗ воспроизведения аудио
        yield return StartCoroutine(AnimateShowAllWagons());

        foreach (var wagon in wagons)
        {
            wagon.SetInteractable(false);
        }

        // Даем время игроку запомнить расположение предметов
        yield return new WaitForSeconds(showItemsDuration);

        // ФАЗА 2: Скрываем содержимое перед перемешиванием
        Debug.Log("Скрываем содержимое вагонов");

        // Скрываем панель с подсказкой
        if (rememberItemsPanel != null)
        {
            rememberItemsPanel.SetActive(false);
        }

        // Анимируем исчезновение предметов
        yield return StartCoroutine(AnimateHideAllWagons());

        // Задержка перед началом перемешивания
        yield return new WaitForSeconds(delayBeforeShuffle);

        // ФАЗА 3: Перемешиваем вагоны
        Debug.Log("Начинаем перемешивание вагонов");
        yield return StartCoroutine(ShuffleWagons());

        // Выбираем целевой предмет
        SelectTargetItem();

        // Показываем вопрос
        ShowQuestion();

        // Включаем взаимодействие с вагонами
        foreach (var wagon in wagons)
        {
            wagon.SetInteractable(true);
        }

        isProcessing = false;
    }

    private void PlaceItemsInWagons()
    {
        if (availableItems == null || availableItems.Length < wagons.Length)
        {
            Debug.LogError("FindTheItem: Недостаточно предметов!");
            return;
        }

        // Создаем пул уникальных предметов (убираем дубликаты если есть)
        HashSet<FindTheItemVariant> uniqueItems = new HashSet<FindTheItemVariant>(availableItems);
        List<FindTheItemVariant> itemPool = new List<FindTheItemVariant>(uniqueItems);

        if (itemPool.Count < wagons.Length)
        {
            Debug.LogError($"FindTheItem: Недостаточно уникальных предметов! Нужно минимум {wagons.Length}, есть только {itemPool.Count}");
            return;
        }

        // Выбираем случайные уникальные предметы
        List<FindTheItemVariant> selectedItems = new List<FindTheItemVariant>();

        for (int i = 0; i < wagons.Length; i++)
        {
            int randomIndex = Random.Range(0, itemPool.Count);
            FindTheItemVariant selectedItem = itemPool[randomIndex];

            selectedItems.Add(selectedItem);
            itemPool.RemoveAt(randomIndex); // Удаляем из пула чтобы не выбрать повторно
        }

        // Проверка что все предметы уникальны
        HashSet<FindTheItemVariant> checkUnique = new HashSet<FindTheItemVariant>(selectedItems);
        if (checkUnique.Count != selectedItems.Count)
        {
            Debug.LogError("FindTheItem: Обнаружены дубликаты предметов в раунде!");
            return;
        }

        // Расставляем предметы по вагонам
        for (int i = 0; i < wagons.Length; i++)
        {
            wagons[i].SetItem(selectedItems[i]);
            Debug.Log($"Вагон {i}: {selectedItems[i].ItemName}");
        }
    }

    private IEnumerator ShuffleWagons()
    {
        int shuffleCount = Random.Range(3, 6); // 3-5 перемешиваний

        for (int i = 0; i < shuffleCount; i++)
        {
            // Воспроизводим звук перемешивания для каждого обмена
            if (shuffleSound != null)
            {
                AudioManager.Instance.PlaySound(shuffleSound);
            }

            // Выбираем два случайных вагона для обмена
            int index1 = Random.Range(0, wagons.Length);
            int index2;
            do
            {
                index2 = Random.Range(0, wagons.Length);
            } while (index2 == index1);

            // Меняем позиции
            Vector3 pos1 = wagons[index1].GetPosition();
            Vector3 pos2 = wagons[index2].GetPosition();

            wagons[index1].AnimateToPosition(pos2, shuffleDuration);
            wagons[index2].AnimateToPosition(pos1, shuffleDuration);

            yield return new WaitForSeconds(shuffleDuration);

            // Меняем вагоны местами в массиве
            FindTheItemWagon temp = wagons[index1];
            wagons[index1] = wagons[index2];
            wagons[index2] = temp;
        }
    }

    private IEnumerator AnimateShowAllWagons()
    {
        int shownCount = 0;

        // Запускаем анимацию для всех вагонов
        foreach (var wagon in wagons)
        {
            wagon.AnimateShowContent(playAudio: false, duration: 0.5f, onComplete: () => { shownCount++; });
        }

        // Ждем пока все анимации завершатся
        while (shownCount < wagons.Length)
        {
            yield return null;
        }
    }

    private IEnumerator AnimateHideAllWagons()
    {
        int hiddenCount = 0;

        // Запускаем анимацию для всех вагонов
        foreach (var wagon in wagons)
        {
            wagon.AnimateHideContent(0.5f, () => { hiddenCount++; });
        }

        // Ждем пока все анимации завершатся
        while (hiddenCount < wagons.Length)
        {
            yield return null;
        }
    }

    private void SelectTargetItem()
    {
        // Выбираем случайный вагон как правильный
        int randomIndex = Random.Range(0, wagons.Length);
        correctWagon = wagons[randomIndex];
        targetItem = correctWagon.CurrentItem;
    }

    private void ShowQuestion()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
        }

        if (questionItemImage != null && targetItem != null)
        {
            questionItemImage.sprite = targetItem.Sprite;
            questionSound.SetAudio(targetItem.AudioClip);
        }

        // Воспроизводим аудио вопроса
        if (targetItem != null && targetItem.AudioClip != null)
        {
            AudioManager.Instance.PlaySound(targetItem.AudioClip);
        }
    }

    private void OnWagonClicked(FindTheItemWagon clickedWagon)
    {
        if (isProcessing) return;

        isProcessing = true;

        // Отключаем взаимодействие со всеми вагонами
        foreach (var wagon in wagons)
        {
            wagon.SetInteractable(false);
        }

        // Показываем содержимое выбранного вагона
        clickedWagon.ShowContent();

        // Проверяем правильность выбора
        StartCoroutine(CheckAnswer(clickedWagon));
    }

    private IEnumerator CheckAnswer(FindTheItemWagon clickedWagon)
    {
        yield return new WaitForSeconds(0.5f);

        bool isCorrect = clickedWagon == correctWagon;

        if (isCorrect)
        {
            wonRounds++;
            if (correctSound != null)
            {
                AudioManager.Instance.PlaySound(correctSound);
            }
            Debug.Log("Правильно!");
        }
        else
        {
            if (wrongSound != null)
            {
                AudioManager.Instance.PlaySound(wrongSound);
            }
            Debug.Log("Неправильно!");

            // Показываем правильный вагон
            yield return new WaitForSeconds(0.5f);
            correctWagon.ShowContent();
        }

        yield return new WaitForSeconds(2f);

        // Скрываем все содержимое
        foreach (var wagon in wagons)
        {
            wagon.HideContent();
        }

        // Возвращаем вагоны на исходные позиции
        yield return StartCoroutine(ReturnWagonsToOriginalPositions());

        // Начинаем новый раунд
        StartCoroutine(StartNewRound());
    }

    private IEnumerator ReturnWagonsToOriginalPositions()
    {
        // Находим текущий порядок вагонов и возвращаем их на исходные позиции
        for (int i = 0; i < wagons.Length; i++)
        {
            wagons[i].AnimateToPosition(originalWagonPositions[i], shuffleDuration);
        }

        yield return new WaitForSeconds(shuffleDuration);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Отписываемся от событий
        if (wagons != null)
        {
            foreach (var wagon in wagons)
            {
                wagon.OnWagonClicked -= OnWagonClicked;
            }
        }
    }
}
