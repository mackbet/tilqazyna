using UnityEngine;
using UnityEngine.UI;

public class Puzzle : GameController
{
    [System.Serializable]
    public class PuzzleSet
    {
        public string name;
        public Sprite picture;
        public Sprite[] pieceSprites; // Спрайты кусочков для одного пазла
    }

    [SerializeField] private PuzzleSet[] puzzleSets; // Несколько наборов пазлов
    [SerializeField] private PuzzlePiece[] pieces;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private GameObject puzzlePanel;
    [SerializeField] private RectTransform puzzleButtonContainer;
    [SerializeField] private CustomButton puzzleButtonPrefab;
    private int lockedCount = 0;
    private PuzzleSet currentPuzzleSet;

    private void Start()
    {

    }

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

        foreach (PuzzleSet puzzleSet in puzzleSets)
        {
            CustomButton button = Instantiate(puzzleButtonPrefab, puzzleButtonContainer);
            button.Image.sprite = puzzleSet.picture;
            button.OnButtonClicked += () => SetPuzzle(puzzleSet);
        }
    }

    private void SetPuzzle(PuzzleSet puzzleSet)
    {
        currentPuzzleSet = puzzleSet;

        // Устанавливаем спрайт для каждого кусочка с его правильной позицией
        for (int i = 0; i < pieces.Length; i++)
            pieces[i].SetSprite(currentPuzzleSet.pieceSprites[i]);

        selectionPanel.SetActive(false);
        puzzlePanel.SetActive(true);
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