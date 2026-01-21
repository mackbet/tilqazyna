using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ThreeInRow : GameController
{
    [Header("Grid Settings")]
    [SerializeField] private ThreeInRowCell[] cells;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 5;

    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private GameObject gamePanel;

    [Header("Items")]
    [SerializeField] private ThreeInRowItem itemPrefab;

    [Header("Levels")]
    [SerializeField] private ThreeInRowLevelData currentLevel;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private RectTransform itemTextContainer;
    [SerializeField] private TextMeshProUGUI itemTextDisplay;
    [SerializeField] private float textAnimationDuration = 0.3f;

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

        InitializeCells();
        FillGrid();
        UpdateUI();

        gamePanel.SetActive(true);
        selectionPanel.SetActive(false);
    }

    public void SetLevel(ThreeInRowLevelData level)
    {
        currentLevel = level;
        movesLeft = currentLevel.movesLimit;
    }

    private void InitializeCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Initialize(i % columns, i / columns);
        }
    }

    private void FillGrid()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            CreateItem(i);
        }

        StartCoroutine(CheckAndResolveInitialMatches());
    }

    private IEnumerator CheckAndResolveInitialMatches()
    {
        yield return new WaitForSeconds(0.5f);

        while (FindMatches().Count > 0)
        {
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
        ThreeInRowCell cell = cells[cellIndex];
        int randomType = Random.Range(0, currentLevel.availableItems.Length);
        ThreeInRowItemData data = currentLevel.availableItems[randomType];

        ThreeInRowItem item = Instantiate(itemPrefab);
        item.Initialize(randomType, data.icon, cell.GridPosition);
        item.OnItemClicked += OnItemClicked;

        cell.SetItem(item);
        item.AnimateAppear();
    }

    private void OnItemClicked(ThreeInRowItem clickedItem)
    {
        if (isProcessing || movesLeft <= 0) return;

        if (selectedItem == null)
        {
            selectedItem = clickedItem;
            selectedItem.SetSelected(true);
        }
        else if (selectedItem == clickedItem)
        {
            selectedItem.SetSelected(false);
            selectedItem = null;
        }
        else
        {
            if (AreNeighbors(selectedItem, clickedItem))
            {
                StartCoroutine(SwapItems(selectedItem, clickedItem));
            }
            else
            {
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

        Vector3 pos1 = cell1.GetWorldPosition();
        Vector3 pos2 = cell2.GetWorldPosition();

        item1.AnimateMoveToPosition(pos2);
        item2.AnimateMoveToPosition(pos1);

        if (swapSound != null)
            AudioManager.Instance.PlaySound(swapSound);

        yield return new WaitForSeconds(0.3f);

        Vector2Int gridPos1 = item1.GridPosition;
        Vector2Int gridPos2 = item2.GridPosition;

        cell1.SetItem(item2);
        cell2.SetItem(item1);

        item1.SetGridPosition(gridPos2);
        item2.SetGridPosition(gridPos1);

        List<ThreeInRowItem> matches = FindMatches();

        if (matches.Count > 0)
        {
            movesLeft--;
            UpdateUI();
            yield return StartCoroutine(ProcessMatches());
        }
        else
        {
            if (invalidSound != null)
                AudioManager.Instance.PlaySound(invalidSound);

            item1.AnimateMoveToPosition(pos1);
            item2.AnimateMoveToPosition(pos2);

            yield return new WaitForSeconds(0.3f);

            cell1.SetItem(item1);
            cell2.SetItem(item2);

            item1.SetGridPosition(gridPos1);
            item2.SetGridPosition(gridPos2);
        }

        isProcessing = false;
        CheckGameEnd();
    }

    private List<ThreeInRowItem> FindMatches()
    {
        List<ThreeInRowItem> matches = new List<ThreeInRowItem>();

        // Горизонталь
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns - 2; col++)
            {
                ThreeInRowItem item1 = GetItemAt(col, row);
                ThreeInRowItem item2 = GetItemAt(col + 1, row);
                ThreeInRowItem item3 = GetItemAt(col + 2, row);

                if (item1 != null && item2 != null && item3 != null &&
                    item1.ItemType == item2.ItemType && item2.ItemType == item3.ItemType)
                {
                    if (!matches.Contains(item1)) matches.Add(item1);
                    if (!matches.Contains(item2)) matches.Add(item2);
                    if (!matches.Contains(item3)) matches.Add(item3);
                }
            }
        }

        // Вертикаль
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows - 2; row++)
            {
                ThreeInRowItem item1 = GetItemAt(col, row);
                ThreeInRowItem item2 = GetItemAt(col, row + 1);
                ThreeInRowItem item3 = GetItemAt(col, row + 2);

                if (item1 != null && item2 != null && item3 != null &&
                    item1.ItemType == item2.ItemType && item2.ItemType == item3.ItemType)
                {
                    if (!matches.Contains(item1)) matches.Add(item1);
                    if (!matches.Contains(item2)) matches.Add(item2);
                    if (!matches.Contains(item3)) matches.Add(item3);
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
            currentScore += matches.Count * 10;
            UpdateUI();

            HashSet<int> matchedTypes = new HashSet<int>();
            foreach (var item in matches)
                matchedTypes.Add(item.ItemType);

            // Озвучка предметов с отображением текста
            foreach (int itemType in matchedTypes)
            {
                if (itemType >= 0 && itemType < currentLevel.availableItems.Length)
                {
                    ThreeInRowItemData itemData = currentLevel.availableItems[itemType];

                    // Показываем текст с анимацией увеличения
                    if (!string.IsNullOrEmpty(itemData.itemText))
                    {
                        yield return StartCoroutine(ShowItemText(itemData.itemText));
                    }

                    // Воспроизводим озвучку
                    if (itemData.audioClip != null)
                    {
                        AudioManager.Instance.PlaySound(itemData.audioClip);
                        yield return new WaitForSeconds(itemData.audioClip.length);
                    }

                    // Скрываем текст с анимацией уменьшения
                    if (!string.IsNullOrEmpty(itemData.itemText))
                    {
                        yield return StartCoroutine(HideItemText());
                    }
                }
            }

            if (matchSound != null)
                AudioManager.Instance.PlaySound(matchSound);

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
            yield return StartCoroutine(DropItems());

            FillEmptySpaces();
            yield return new WaitForSeconds(0.3f);

            matches = FindMatches();
        }

        if (!HasAvailableMoves())
            yield return StartCoroutine(ReshuffleBoard());
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
            yield return new WaitForSeconds(0.3f);
    }

    private void FillEmptySpaces()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].IsEmpty)
                CreateItem(i);
        }
    }

    private void UpdateUI()
    {
        scoreText.text = $"{currentScore}/{currentLevel.scoreGoal}";

        movesText.text = $"{movesLeft}";
    }

    private void CheckGameEnd()
    {
        if (isEnded) return;

        if (currentScore >= currentLevel.scoreGoal)
        {
            isEnded = true;
            finishPanel.SetReward(currentLevel.coinsReward, currentLevel.experienceReward, currentScore);
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
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                ThreeInRowItem item = GetItemAt(col, row);
                if (item == null) continue;

                if (col < columns - 1)
                {
                    ThreeInRowItem rightItem = GetItemAt(col + 1, row);
                    if (rightItem != null && WouldCreateMatch(col, row, col + 1, row))
                        return true;
                }

                if (row < rows - 1)
                {
                    ThreeInRowItem bottomItem = GetItemAt(col, row + 1);
                    if (bottomItem != null && WouldCreateMatch(col, row, col, row + 1))
                        return true;
                }
            }
        }

        return false;
    }

    private bool WouldCreateMatch(int col1, int row1, int col2, int row2)
    {
        ThreeInRowItem item1 = GetItemAt(col1, row1);
        ThreeInRowItem item2 = GetItemAt(col2, row2);

        int temp = item1.ItemType;
        item1.SetItemType(item2.ItemType);
        item2.SetItemType(temp);

        bool hasMatch = CheckMatchAt(col1, row1) || CheckMatchAt(col2, row2);

        item2.SetItemType(item1.ItemType);
        item1.SetItemType(temp);

        return hasMatch;
    }

    private bool CheckMatchAt(int col, int row)
    {
        ThreeInRowItem item = GetItemAt(col, row);
        if (item == null) return false;

        int type = item.ItemType;

        // Горизонталь
        int horizontalCount = 1;
        for (int c = col - 1; c >= 0; c--)
        {
            ThreeInRowItem leftItem = GetItemAt(c, row);
            if (leftItem != null && leftItem.ItemType == type)
                horizontalCount++;
            else
                break;
        }

        for (int c = col + 1; c < columns; c++)
        {
            ThreeInRowItem rightItem = GetItemAt(c, row);
            if (rightItem != null && rightItem.ItemType == type)
                horizontalCount++;
            else
                break;
        }

        if (horizontalCount >= 3) return true;

        // Вертикаль
        int verticalCount = 1;
        for (int r = row - 1; r >= 0; r--)
        {
            ThreeInRowItem topItem = GetItemAt(col, r);
            if (topItem != null && topItem.ItemType == type)
                verticalCount++;
            else
                break;
        }

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
        List<ThreeInRowItem> allItems = new List<ThreeInRowItem>();

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].Item != null)
            {
                allItems.Add(cells[i].Item);
                cells[i].ClearItem();
            }
        }

        for (int i = 0; i < allItems.Count; i++)
        {
            int randomIndex = Random.Range(i, allItems.Count);
            ThreeInRowItem temp = allItems[i];
            allItems[i] = allItems[randomIndex];
            allItems[randomIndex] = temp;
        }

        for (int i = 0; i < cells.Length && i < allItems.Count; i++)
        {
            ThreeInRowItem item = allItems[i];
            cells[i].SetItem(item);
            item.SetGridPosition(cells[i].GridPosition);
            item.AnimateAppear();
        }

        yield return new WaitForSeconds(0.5f);

        List<ThreeInRowItem> matches = FindMatches();
        if (matches.Count > 0)
        {
            yield return StartCoroutine(ProcessMatches());
        }
        else if (!HasAvailableMoves())
        {
            yield return StartCoroutine(ReshuffleBoard());
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (cells != null)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].Item != null)
                    cells[i].Item.OnItemClicked -= OnItemClicked;
            }
        }
    }

    private ThreeInRowCell GetCell(Vector2Int gridPosition)
    {
        return GetCellAt(gridPosition.x, gridPosition.y);
    }

    private ThreeInRowCell GetCellAt(int col, int row)
    {
        if (col < 0 || col >= columns || row < 0 || row >= rows) return null;
        return cells[row * columns + col];
    }

    private ThreeInRowItem GetItemAt(int col, int row)
    {
        ThreeInRowCell cell = GetCellAt(col, row);
        return cell != null ? cell.Item : null;
    }

    private IEnumerator ShowItemText(string text)
    {
        if (itemTextContainer == null || itemTextDisplay == null)
            yield break;

        itemTextDisplay.text = text;
        itemTextContainer.localScale = Vector3.zero;
        itemTextContainer.gameObject.SetActive(true);

        // Анимация увеличения
        float elapsed = 0f;
        while (elapsed < textAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textAnimationDuration;
            float scale = Mathf.SmoothStep(0f, 1f, t);
            itemTextContainer.localScale = Vector3.one * scale;
            yield return null;
        }
        itemTextContainer.localScale = Vector3.one;
    }

    private IEnumerator HideItemText()
    {
        if (itemTextContainer == null)
            yield break;

        // Анимация уменьшения
        float elapsed = 0f;
        while (elapsed < textAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textAnimationDuration;
            float scale = Mathf.SmoothStep(1f, 0f, t);
            itemTextContainer.localScale = Vector3.one * scale;
            yield return null;
        }
        itemTextContainer.localScale = Vector3.zero;
        itemTextContainer.gameObject.SetActive(false);
    }
}