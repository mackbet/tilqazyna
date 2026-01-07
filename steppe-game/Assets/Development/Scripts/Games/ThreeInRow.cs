using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ThreeInRow : GameController
{
    [Header("Grid Settings")]
    [SerializeField] private ThreeInRowCell[] cells; // 25 ячеек (5x5), построчно сверху вниз
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 5;

    [Header("Items")]
    [SerializeField] private ThreeInRowItem itemPrefab;
    [SerializeField] private Sprite[] itemSprites;
    [SerializeField] private AudioClip[] itemAudios; // Аудио для каждого типа элемента
    [SerializeField] private int itemTypesCount = 5;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;

    [Header("Game Settings")]
    [SerializeField] private int movesLimit = 20;
    [SerializeField] private int scoreGoal = 100;

    [Header("Sounds")]
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip swapSound;
    [SerializeField] private AudioClip invalidSound;

    private ThreeInRowItem selectedItem;
    private int currentScore = 0;
    private int movesLeft;
    private bool isProcessing = false;
    private bool isEnded = false;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        movesLeft = movesLimit;
        currentScore = 0;

        InitializeCells();
        FillGrid();
        UpdateUI();
    }

    private void InitializeCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            cells[i].Initialize(col, row);
        }
    }

    private void FillGrid()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            CreateItem(i);
        }

        // Проверяем и убираем начальные совпадения
        StartCoroutine(CheckAndResolveInitialMatches());
    }

    private IEnumerator CheckAndResolveInitialMatches()
    {
        yield return new WaitForSeconds(0.5f);

        while (FindMatches().Count > 0)
        {
            // Пересоздаем элементы, которые образуют совпадения
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].Item != null)
                {
                    Destroy(cells[i].Item.gameObject);
                    CreateItem(i);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void CreateItem(int cellIndex)
    {
        if (itemPrefab == null || cellIndex < 0 || cellIndex >= cells.Length) return;

        ThreeInRowCell cell = cells[cellIndex];
        int randomType = Random.Range(0, itemTypesCount);
        Sprite sprite = randomType < itemSprites.Length ? itemSprites[randomType] : null;

        ThreeInRowItem item = Instantiate(itemPrefab);
        item.Initialize(randomType, sprite, cell.GridPosition);
        item.OnItemClicked += OnItemClicked;

        cell.SetItem(item);
        item.AnimateAppear();
    }

    private void OnItemClicked(ThreeInRowItem clickedItem)
    {
        if (movesLeft <= 0)
            return;

        if (isProcessing) return;

        if (selectedItem == null)
        {
            // Выбираем первый элемент
            selectedItem = clickedItem;
            selectedItem.SetSelected(true);
        }
        else if (selectedItem == clickedItem)
        {
            // Отменяем выбор
            selectedItem.SetSelected(false);
            selectedItem = null;
        }
        else
        {
            // Проверяем, являются ли элементы соседними
            if (AreNeighbors(selectedItem, clickedItem))
            {
                StartCoroutine(SwapItems(selectedItem, clickedItem));
            }
            else
            {
                // Выбираем новый элемент
                selectedItem.SetSelected(false);
                selectedItem = clickedItem;
                selectedItem.SetSelected(true);
            }
        }
    }

    private bool AreNeighbors(ThreeInRowItem item1, ThreeInRowItem item2)
    {
        Vector2Int pos1 = item1.GridPosition;
        Vector2Int pos2 = item2.GridPosition;

        int colDiff = Mathf.Abs(pos1.x - pos2.x);
        int rowDiff = Mathf.Abs(pos1.y - pos2.y);

        return (colDiff == 1 && rowDiff == 0) || (colDiff == 0 && rowDiff == 1);
    }

    private IEnumerator SwapItems(ThreeInRowItem item1, ThreeInRowItem item2)
    {
        isProcessing = true;
        selectedItem.SetSelected(false);
        selectedItem = null;

        ThreeInRowCell cell1 = GetCell(item1.GridPosition);
        ThreeInRowCell cell2 = GetCell(item2.GridPosition);

        if (cell1 == null || cell2 == null)
        {
            isProcessing = false;
            yield break;
        }

        // Анимация обмена
        Vector3 pos1 = cell1.GetWorldPosition();
        Vector3 pos2 = cell2.GetWorldPosition();

        item1.AnimateMoveToPosition(pos2);
        item2.AnimateMoveToPosition(pos1);

        // Воспроизводим звук
        if (swapSound != null)
        {
            AudioManager.Instance.PlaySound(swapSound);
        }

        yield return new WaitForSeconds(0.3f);

        // Обновляем позиции
        Vector2Int gridPos1 = item1.GridPosition;
        Vector2Int gridPos2 = item2.GridPosition;

        cell1.SetItem(item2);
        cell2.SetItem(item1);

        item1.SetGridPosition(gridPos2);
        item2.SetGridPosition(gridPos1);

        // Проверяем совпадения
        List<ThreeInRowItem> matches = FindMatches();

        if (matches.Count > 0)
        {
            // Есть совпадения - ход засчитывается
            movesLeft--;
            UpdateUI();

            yield return StartCoroutine(ProcessMatches());
        }
        else
        {
            // Нет совпадений - возвращаем обратно
            if (invalidSound != null)
            {
                AudioManager.Instance.PlaySound(invalidSound);
            }

            item1.AnimateMoveToPosition(pos1);
            item2.AnimateMoveToPosition(pos2);

            yield return new WaitForSeconds(0.3f);

            cell1.SetItem(item1);
            cell2.SetItem(item2);

            item1.SetGridPosition(gridPos1);
            item2.SetGridPosition(gridPos2);
        }

        isProcessing = false;

        // Проверяем условия окончания игры
        CheckGameEnd();
    }

    private List<ThreeInRowItem> FindMatches()
    {
        List<ThreeInRowItem> matches = new List<ThreeInRowItem>();

        // Проверка по горизонтали
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns - 2; col++)
            {
                ThreeInRowItem item1 = GetItemAt(col, row);
                ThreeInRowItem item2 = GetItemAt(col + 1, row);
                ThreeInRowItem item3 = GetItemAt(col + 2, row);

                if (item1 != null && item2 != null && item3 != null)
                {
                    if (item1.ItemType == item2.ItemType && item2.ItemType == item3.ItemType)
                    {
                        if (!matches.Contains(item1)) matches.Add(item1);
                        if (!matches.Contains(item2)) matches.Add(item2);
                        if (!matches.Contains(item3)) matches.Add(item3);
                    }
                }
            }
        }

        // Проверка по вертикали
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows - 2; row++)
            {
                ThreeInRowItem item1 = GetItemAt(col, row);
                ThreeInRowItem item2 = GetItemAt(col, row + 1);
                ThreeInRowItem item3 = GetItemAt(col, row + 2);

                if (item1 != null && item2 != null && item3 != null)
                {
                    if (item1.ItemType == item2.ItemType && item2.ItemType == item3.ItemType)
                    {
                        if (!matches.Contains(item1)) matches.Add(item1);
                        if (!matches.Contains(item2)) matches.Add(item2);
                        if (!matches.Contains(item3)) matches.Add(item3);
                    }
                }
            }
        }

        return matches;
    }

    private IEnumerator ProcessMatches()
    {
        List<ThreeInRowItem> matches = FindMatches();

        while (matches.Count > 0)
        {
            // Добавляем очки
            currentScore += matches.Count * 10;
            UpdateUI();

            // Собираем уникальные типы элементов в совпадениях
            HashSet<int> matchedTypes = new HashSet<int>();
            foreach (var item in matches)
            {
                matchedTypes.Add(item.ItemType);
            }

            // Воспроизводим аудио для каждого типа элемента
            foreach (int itemType in matchedTypes)
            {
                if (itemAudios != null && itemType >= 0 && itemType < itemAudios.Length)
                {
                    AudioClip itemAudio = itemAudios[itemType];
                    if (itemAudio != null)
                    {
                        AudioManager.Instance.PlaySound(itemAudio);
                        yield return new WaitForSeconds(itemAudio.length + 0.1f);
                    }
                }
            }

            // Воспроизводим общий звук совпадения
            if (matchSound != null)
            {
                AudioManager.Instance.PlaySound(matchSound);
            }

            // Удаляем совпавшие элементы
            foreach (var item in matches)
            {
                ThreeInRowCell cell = GetCell(item.GridPosition);
                if (cell != null)
                {
                    cell.ClearItem();
                    item.AnimateDestroy();
                }
            }

            yield return new WaitForSeconds(0.3f);

            // Опускаем элементы
            yield return StartCoroutine(DropItems());

            // Заполняем пустые места
            FillEmptySpaces();

            yield return new WaitForSeconds(0.3f);

            // Проверяем новые совпадения
            matches = FindMatches();
        }

        // Проверяем наличие доступных ходов
        if (!HasAvailableMoves())
        {
            yield return StartCoroutine(ReshuffleBoard());
        }
    }

    private IEnumerator DropItems()
    {
        bool itemsMoved = false;

        for (int col = 0; col < columns; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                ThreeInRowCell cell = GetCellAt(col, row);
                if (cell != null && cell.IsEmpty)
                {
                    // Ищем элемент выше
                    for (int aboveRow = row - 1; aboveRow >= 0; aboveRow--)
                    {
                        ThreeInRowCell aboveCell = GetCellAt(col, aboveRow);
                        if (aboveCell != null && !aboveCell.IsEmpty)
                        {
                            ThreeInRowItem item = aboveCell.Item;
                            aboveCell.ClearItem();
                            cell.SetItem(item);
                            item.SetGridPosition(cell.GridPosition);
                            item.AnimateMoveToPosition(cell.GetWorldPosition());
                            itemsMoved = true;
                            break;
                        }
                    }
                }
            }
        }

        if (itemsMoved)
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void FillEmptySpaces()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].IsEmpty)
            {
                CreateItem(i);
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentScore}";
        }

        if (movesText != null)
        {
            movesText.text = $"{movesLeft}";
        }
    }

    private void CheckGameEnd()
    {
        if (isEnded)
            return;

        if (currentScore >= scoreGoal)
        {
            isEnded = true;
            finishPanel.SetReward(10, 60, currentScore);
            finishPanel.SetState(true);
            finishPanel.SetStars(3);
            FinishGame();
        }
        else if (movesLeft <= 0)
        {
            isEnded = true;
            FailGame();
        }
    }

    private bool HasAvailableMoves()
    {
        // Проверяем каждую возможную пару соседних элементов
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                ThreeInRowItem item = GetItemAt(col, row);
                if (item == null) continue;

                // Проверяем обмен с правым соседом
                if (col < columns - 1)
                {
                    ThreeInRowItem rightItem = GetItemAt(col + 1, row);
                    if (rightItem != null && WouldCreateMatch(col, row, col + 1, row))
                    {
                        return true;
                    }
                }

                // Проверяем обмен с нижним соседом
                if (row < rows - 1)
                {
                    ThreeInRowItem bottomItem = GetItemAt(col, row + 1);
                    if (bottomItem != null && WouldCreateMatch(col, row, col, row + 1))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool WouldCreateMatch(int col1, int row1, int col2, int row2)
    {
        ThreeInRowItem item1 = GetItemAt(col1, row1);
        ThreeInRowItem item2 = GetItemAt(col2, row2);

        if (item1 == null || item2 == null) return false;

        // Временно меняем типы элементов
        int temp = item1.ItemType;
        item1.SetItemType(item2.ItemType);
        item2.SetItemType(temp);

        // Проверяем, создастся ли совпадение
        bool hasMatch = CheckMatchAt(col1, row1) || CheckMatchAt(col2, row2);

        // Возвращаем типы обратно
        item2.SetItemType(item1.ItemType);
        item1.SetItemType(temp);

        return hasMatch;
    }

    private bool CheckMatchAt(int col, int row)
    {
        ThreeInRowItem item = GetItemAt(col, row);
        if (item == null) return false;

        int type = item.ItemType;

        // Проверка по горизонтали
        int horizontalCount = 1;

        // Считаем влево
        for (int c = col - 1; c >= 0; c--)
        {
            ThreeInRowItem leftItem = GetItemAt(c, row);
            if (leftItem != null && leftItem.ItemType == type)
                horizontalCount++;
            else
                break;
        }

        // Считаем вправо
        for (int c = col + 1; c < columns; c++)
        {
            ThreeInRowItem rightItem = GetItemAt(c, row);
            if (rightItem != null && rightItem.ItemType == type)
                horizontalCount++;
            else
                break;
        }

        if (horizontalCount >= 3) return true;

        // Проверка по вертикали
        int verticalCount = 1;

        // Считаем вверх
        for (int r = row - 1; r >= 0; r--)
        {
            ThreeInRowItem topItem = GetItemAt(col, r);
            if (topItem != null && topItem.ItemType == type)
                verticalCount++;
            else
                break;
        }

        // Считаем вниз
        for (int r = row + 1; r < rows; r++)
        {
            ThreeInRowItem bottomItem = GetItemAt(col, r);
            if (bottomItem != null && bottomItem.ItemType == type)
                verticalCount++;
            else
                break;
        }

        return verticalCount >= 3;
    }

    private IEnumerator ReshuffleBoard()
    {
        // Собираем все элементы
        List<ThreeInRowItem> allItems = new List<ThreeInRowItem>();

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].Item != null)
            {
                allItems.Add(cells[i].Item);
                cells[i].ClearItem();
            }
        }

        // Перемешиваем элементы
        for (int i = 0; i < allItems.Count; i++)
        {
            int randomIndex = Random.Range(i, allItems.Count);
            ThreeInRowItem temp = allItems[i];
            allItems[i] = allItems[randomIndex];
            allItems[randomIndex] = temp;
        }

        // Расставляем элементы обратно
        for (int i = 0; i < cells.Length && i < allItems.Count; i++)
        {
            ThreeInRowItem item = allItems[i];
            cells[i].SetItem(item);
            item.SetGridPosition(cells[i].GridPosition);
            item.AnimateAppear();
        }

        yield return new WaitForSeconds(0.5f);

        // Проверяем и убираем совпадения после перемешивания
        List<ThreeInRowItem> matches = FindMatches();
        if (matches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatches());
        }
        // Если после перемешивания всё ещё нет ходов, попробуем ещё раз
        else if (!HasAvailableMoves())
        {
            yield return StartCoroutine(ReshuffleBoard());
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Отписываемся от всех элементов
        if (cells != null)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].Item != null)
                {
                    cells[i].Item.OnItemClicked -= OnItemClicked;
                }
            }
        }
    }

    // Вспомогательные методы
    private ThreeInRowCell GetCell(Vector2Int gridPosition)
    {
        return GetCellAt(gridPosition.x, gridPosition.y);
    }

    private ThreeInRowCell GetCellAt(int col, int row)
    {
        if (col < 0 || col >= columns || row < 0 || row >= rows) return null;
        int index = row * columns + col;
        return cells[index];
    }

    private ThreeInRowItem GetItemAt(int col, int row)
    {
        ThreeInRowCell cell = GetCellAt(col, row);
        return cell != null ? cell.Item : null;
    }

    // Публичные методы для получения информации
    public int GetScore() => currentScore;
    public int GetMovesLeft() => movesLeft;
    public float GetProgress() => scoreGoal > 0 ? (float)currentScore / scoreGoal : 0f;
}
