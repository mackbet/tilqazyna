using DG.Tweening;
using UnityEngine;

public class PuzzleAnimationHandler : PuzzleCompletionHandler
{
    [Header("Piece Animation")]
    [SerializeField] private float pieceBounceScale = 1.2f;
    [SerializeField] private float pieceBounceTime = 0.3f;
    [SerializeField] private Ease pieceBounceEase = Ease.OutBack;

    [Header("Completion Animation")]
    [SerializeField] private float completionRotation = 360f;
    [SerializeField] private float completionTime = 1f;
    [SerializeField] private float completionScale = 1.1f;

    protected override void OnEachPieceLocked(PuzzlePiece piece)
    {
        base.OnEachPieceLocked(piece);

        // Анимация при блокировке каждого кусочка
        Transform pieceTransform = piece.transform;

        // Эффект "подпрыгивания"
        pieceTransform.DOScale(pieceBounceScale, pieceBounceTime)
            .SetEase(pieceBounceEase)
            .OnComplete(() => pieceTransform.DOScale(1f, pieceBounceTime));
    }

    protected override void OnAllPiecesLocked()
    {
        base.OnAllPiecesLocked();

        // Анимация всех кусочков при завершении
        foreach (Transform pieceTransform in transform)
        {
            // Вращение
            pieceTransform.DORotate(new Vector3(0, 0, completionRotation), completionTime, RotateMode.FastBeyond360);

            // Увеличение масштаба
            pieceTransform.DOScale(completionScale, completionTime)
                .OnComplete(() => pieceTransform.DOScale(1f, completionTime / 2));
        }
    }
}
