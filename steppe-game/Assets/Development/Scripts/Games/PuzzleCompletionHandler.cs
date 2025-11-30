using System;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleCompletionHandler : MonoBehaviour
{
    [Header("Puzzle Pieces")]
    [SerializeField] private PuzzlePiece[] pieces;

    [Header("Events")]
    [SerializeField] private UnityEvent onSinglePieceLocked;
    [SerializeField] private UnityEvent onAllPiecesLocked;

    [Header("Settings")]
    [SerializeField] private bool executeOnEachPiece = true;
    [SerializeField] private float delayBeforeCompletion = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip pieceLockedSound;
    [SerializeField] private AudioClip allLockedSound;
    [SerializeField] private float pieceSoundVolume = 1f;
    [SerializeField] private float completionSoundVolume = 1f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject completionEffectPrefab;
    [SerializeField] private Transform effectParent;

    private int lockedCount = 0;

    public event Action<PuzzlePiece> OnPieceLockedEvent;
    public event Action OnCompletedEvent;

    public int LockedCount => lockedCount;
    public int TotalPieces => pieces.Length;
    public bool IsCompleted => lockedCount >= pieces.Length;

    private void OnEnable()
    {
        lockedCount = 0;

        if (pieces != null)
        {
            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    piece.OnPieceLocked += OnPieceLockedHandler;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (pieces != null)
        {
            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    piece.OnPieceLocked -= OnPieceLockedHandler;
                }
            }
        }
    }

    private void OnPieceLockedHandler(PuzzlePiece piece)
    {
        lockedCount++;

        Debug.Log($"Кусочек заблокирован: {piece.name} ({lockedCount}/{pieces.Length})");

        // Вызываем событие для каждого кусочка
        OnPieceLockedEvent?.Invoke(piece);

        // Выполняем действие на каждом кусочке
        if (executeOnEachPiece)
        {
            OnEachPieceLocked(piece);
        }

        // Воспроизводим звук для одного кусочка
        if (pieceLockedSound != null)
        {
            AudioManager.Instance.PlaySound(pieceLockedSound, pieceSoundVolume);
        }

        // Вызываем UnityEvent для одного кусочка
        onSinglePieceLocked?.Invoke();

        // Проверяем, все ли кусочки собраны
        if (lockedCount >= pieces.Length)
        {
            Invoke(nameof(OnAllPiecesCompleted), delayBeforeCompletion);
        }
    }

    protected virtual void OnEachPieceLocked(PuzzlePiece piece)
    {
        // Переопределите этот метод в наследниках для кастомных действий
        // Например, анимация кусочка, эффекты частиц и т.д.
    }

    private void OnAllPiecesCompleted()
    {
        Debug.Log("Все кусочки пазла собраны!");

        // Выполняем действие при завершении
        OnAllPiecesLocked();

        // Вызываем событие
        OnCompletedEvent?.Invoke();

        // Вызываем UnityEvent
        onAllPiecesLocked?.Invoke();

        // Воспроизводим звук завершения
        if (allLockedSound != null)
        {
            AudioManager.Instance.PlaySound(allLockedSound, completionSoundVolume);
        }

        // Создаем эффект завершения
        if (completionEffectPrefab != null)
        {
            Transform parent = effectParent != null ? effectParent : transform;
            Instantiate(completionEffectPrefab, parent);
        }
    }

    protected virtual void OnAllPiecesLocked()
    {
        // Переопределите этот метод в наследниках для кастомных действий
        // Например, анимация всех кусочков, показ UI и т.д.
    }

    public void ResetHandler()
    {
        lockedCount = 0;
    }

    public void SetPieces(PuzzlePiece[] newPieces)
    {
        // Отписываемся от старых кусочков
        if (pieces != null)
        {
            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    piece.OnPieceLocked -= OnPieceLockedHandler;
                }
            }
        }

        // Устанавливаем новые кусочки
        pieces = newPieces;
        lockedCount = 0;

        // Подписываемся на новые кусочки
        if (pieces != null)
        {
            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    piece.OnPieceLocked += OnPieceLockedHandler;
                }
            }
        }
    }

    public float GetProgress()
    {
        return pieces.Length > 0 ? (float)lockedCount / pieces.Length : 0f;
    }
}
