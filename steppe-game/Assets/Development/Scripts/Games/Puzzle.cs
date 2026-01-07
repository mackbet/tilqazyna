using UnityEngine;

public class Puzzle : GameController
{
    [System.Serializable]
    public class PuzzleSet
    {
        public string name;
        public Sprite[] pieceSprites; // Спрайты кусочков для одного пазла
    }

    [SerializeField] private PuzzleSet[] puzzleSets; // Несколько наборов пазлов
    [SerializeField] private PuzzlePiece[] pieces;
    [SerializeField] private AudioClip clickSound;
    private int lockedCount = 0;
    private PuzzleSet currentPuzzleSet;

    protected override void OnEnable()
    {
        base.OnEnable();
        foreach (var piece in pieces)
        {
            piece.OnPieceLocked += OnPieceLockedHandler;
        }

        InitializePuzzle();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var piece in pieces)
        {
            piece.OnPieceLocked -= OnPieceLockedHandler;
        }
    }

    public void InitializePuzzle()
    {
        lockedCount = 0;

        // Выбираем случайный набор пазла
        currentPuzzleSet = puzzleSets[Random.Range(0, puzzleSets.Length)];

        if (currentPuzzleSet.pieceSprites.Length != pieces.Length)
        {
            Debug.LogError($"Количество спрайтов ({currentPuzzleSet.pieceSprites.Length}) не совпадает с количеством кусочков ({pieces.Length})!");
            return;
        }

        // Устанавливаем спрайт для каждого кусочка с его правильной позицией
        for (int i = 0; i < pieces.Length; i++)
        {
            pieces[i].SetSprite(currentPuzzleSet.pieceSprites[i]);
        }
    }

    private void OnPieceLockedHandler(PuzzlePiece piece)
    {
        lockedCount++;
        Debug.Log($"Кусочек {piece.name} зафиксирован! ({lockedCount}/{pieces.Length})");
        AudioManager.Instance.PlaySound(clickSound);

        if (lockedCount >= pieces.Length)
        {
            finishPanel.SetReward(8, 70, 100);
            finishPanel.SetState(true);
            finishPanel.SetStars(lives);
            FinishGame();
        }
    }

    public void ResetPuzzle()
    {
        lockedCount = 0;

        for (int i = 0; i < pieces.Length; i++)
        {
            pieces[i].Reset();
        }
    }

    public void LoadSpecificPuzzle(int puzzleIndex)
    {
        if (puzzleIndex < 0 || puzzleIndex >= puzzleSets.Length)
        {
            Debug.LogError($"Неверный индекс пазла: {puzzleIndex}");
            return;
        }

        lockedCount = 0;
        currentPuzzleSet = puzzleSets[puzzleIndex];

        for (int i = 0; i < pieces.Length; i++)
        {
            Vector2 correctPosition = pieces[i].GetComponent<RectTransform>().anchoredPosition;
            pieces[i].SetSprite(currentPuzzleSet.pieceSprites[i]);
        }
    }
}