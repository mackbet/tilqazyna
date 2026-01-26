using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Excavations : GameController
{
    [Header("Grid Settings")]
    [SerializeField] private ExcavationCell[] cells;
    [SerializeField] private int rows = 4;
    [SerializeField] private int columns = 4;

    [Header("Artifacts")]
    [SerializeField] private Sprite[] artifactSprites;
    [SerializeField] private int artifactCount = 4;

    [Header("Shovel UI")]
    [SerializeField] private Image shovelImage;
    [SerializeField] private Sprite[] shovelSprites; // Разные состояния лопаты (от новой до сломанной)
    [SerializeField] private TextMeshProUGUI digsLeftText;
    [SerializeField] private int maxDigs = 10;

    [Header("Sounds")]
    [SerializeField] private AudioClip digSound;
    [SerializeField] private AudioClip artifactFoundSound;
    [SerializeField] private AudioClip emptyDigSound;

    private int digsRemaining;
    private int artifactsFound;
    private int totalArtifacts;
    private bool isGameActive;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        InitializeCells();
        PlaceArtifacts();

        digsRemaining = maxDigs;
        artifactsFound = 0;
        totalArtifacts = artifactCount;
        isGameActive = true;

        UpdateShovelUI();
    }

    private void InitializeCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            int col = i % columns;
            int row = i / columns;

            cells[i].Initialize(col, row);
            cells[i].OnCellClicked += OnCellClicked;
        }
    }

    private void PlaceArtifacts()
    {
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < cells.Length; i++)
        {
            availableIndices.Add(i);
        }

        // Перемешиваем индексы
        for (int i = availableIndices.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (availableIndices[i], availableIndices[randomIndex]) = (availableIndices[randomIndex], availableIndices[i]);
        }

        // Размещаем артефакты
        int artifactsToPlace = Mathf.Min(artifactCount, cells.Length, artifactSprites.Length);
        for (int i = 0; i < artifactsToPlace; i++)
        {
            int cellIndex = availableIndices[i];
            Sprite artifactSprite = artifactSprites[i % artifactSprites.Length];
            cells[cellIndex].SetArtifact(artifactSprite);
        }
    }

    private void OnCellClicked(ExcavationCell cell)
    {
        if (!isGameActive || digsRemaining <= 0) return;

        // Подсчитываем соседние артефакты
        int neighborCount = CountNeighborArtifacts(cell.GridPosition);

        // Раскрываем ячейку (byPlayer = true)
        cell.Reveal(neighborCount, byPlayer: true);

        if (cell.HasArtifact)
        {
            // Нашли артефакт - ход не тратится
            artifactsFound++;

            if (artifactFoundSound != null)
                AudioManager.Instance.PlaySound(artifactFoundSound);

            // Проверяем победу
            if (artifactsFound >= totalArtifacts)
            {
                WinGame();
                return;
            }
        }
        else
        {
            // Тратим ход только если не нашли артефакт
            digsRemaining--;
            UpdateShovelUI();

            if (emptyDigSound != null)
                AudioManager.Instance.PlaySound(emptyDigSound);

            // Если пустая ячейка без соседних артефактов - раскрываем соседние
            if (neighborCount == 0)
            {
                RevealNeighbors(cell.GridPosition);
            }
        }

        if (digSound != null)
            AudioManager.Instance.PlaySound(digSound);

        // Проверяем поражение
        if (digsRemaining <= 0 && artifactsFound < totalArtifacts)
        {
            LoseGame();
        }
    }

    private int CountNeighborArtifacts(Vector2Int position)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = position.x + dx;
                int ny = position.y + dy;

                ExcavationCell neighbor = GetCellAt(nx, ny);
                if (neighbor != null && neighbor.HasArtifact)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void RevealNeighbors(Vector2Int position)
    {
        Queue<Vector2Int> toReveal = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        toReveal.Enqueue(position);
        visited.Add(position);

        while (toReveal.Count > 0)
        {
            Vector2Int current = toReveal.Dequeue();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    Vector2Int neighborPos = new Vector2Int(current.x + dx, current.y + dy);

                    if (visited.Contains(neighborPos)) continue;
                    visited.Add(neighborPos);

                    ExcavationCell neighbor = GetCellAt(neighborPos.x, neighborPos.y);
                    if (neighbor == null || neighbor.IsRevealed) continue;

                    int neighborCount = CountNeighborArtifacts(neighborPos);
                    neighbor.Reveal(neighborCount);

                    // Если у соседа тоже нет соседних артефактов - добавляем в очередь
                    if (neighborCount == 0 && !neighbor.HasArtifact)
                    {
                        toReveal.Enqueue(neighborPos);
                    }
                }
            }
        }
    }

    private void UpdateShovelUI()
    {
        // Обновляем текст
        if (digsLeftText != null)
        {
            digsLeftText.text = digsRemaining.ToString();
        }

        // Обновляем иконку лопаты
        if (shovelImage != null && shovelSprites != null && shovelSprites.Length > 0)
        {
            float digRatio = (float)digsRemaining / maxDigs;
            int spriteIndex = Mathf.FloorToInt((1f - digRatio) * (shovelSprites.Length - 1));
            spriteIndex = Mathf.Clamp(spriteIndex, 0, shovelSprites.Length - 1);
            shovelImage.sprite = shovelSprites[spriteIndex];
        }
    }

    private ExcavationCell GetCellAt(int col, int row)
    {
        if (col < 0 || col >= columns || row < 0 || row >= rows)
            return null;

        int index = row * columns + col;
        if (index < 0 || index >= cells.Length)
            return null;

        return cells[index];
    }

    private void WinGame()
    {
        isGameActive = false;
        SetAllCellsInteractable(false);

        // Раскрываем все оставшиеся ячейки
        RevealAllCells();

        if (finishPanel != null)
        {
            int stars = CalculateStars();
            finishPanel.SetStars(stars);
            finishPanel.SetState(true);
        }

        FinishGame();
    }

    private void LoseGame()
    {
        isGameActive = false;
        SetAllCellsInteractable(false);

        // Раскрываем все ячейки чтобы показать где были артефакты
        RevealAllCells();

        if (finishPanel != null)
        {
            finishPanel.SetState(false);
        }

        FailGame();
    }

    private void RevealAllCells()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (!cells[i].IsRevealed)
            {
                int neighborCount = CountNeighborArtifacts(cells[i].GridPosition);
                cells[i].Reveal(neighborCount);
            }
        }
    }

    private int CalculateStars()
    {
        float efficiency = (float)(digsRemaining + maxDigs) / maxDigs;

        if (efficiency >= 0.5f)
            return 3;
        if (efficiency >= 0.25f)
            return 2;
        return 1;
    }

    private void SetAllCellsInteractable(bool interactable)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].SetInteractable(interactable);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (cells != null)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                    cells[i].OnCellClicked -= OnCellClicked;
            }
        }
    }
}
