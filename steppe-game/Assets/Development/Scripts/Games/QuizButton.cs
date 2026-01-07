using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SimpleDialogue;

public class QuizButton : CustomButton
{
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private Image background;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;

    private QuizField currentAnswer;
    private int answerIndex;

    public event Action<QuizButton, int> OnAnswerClicked;

    public QuizField CurrentAnswer => currentAnswer;
    public int AnswerIndex => answerIndex;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!answerText)
            answerText = GetComponentInChildren<TextMeshProUGUI>();

        if (!background)
            background = GetComponent<Image>();
    }
#endif

    public void Initialize(QuizField answer, int index)
    {
        currentAnswer = answer;
        answerIndex = index;

        if (answerText != null && answer != null)
        {
            answerText.text = answer.text;
        }

        ResetVisuals();
    }

    protected override void Clicked()
    {
        base.Clicked();
        OnAnswerClicked?.Invoke(this, answerIndex);
    }

    public void ShowCorrect()
    {
        if (background != null)
        {
            background.color = correctColor;
        }
    }

    public void ShowWrong()
    {
        if (background != null)
        {
            background.color = wrongColor;
        }
    }

    public void ResetVisuals()
    {
        if (background != null)
        {
            background.color = normalColor;
        }
    }

    public void SetInteractable(bool interactable)
    {
        // Можно добавить визуальную индикацию доступности
        if (answerText != null)
        {
            answerText.alpha = interactable ? 1f : 0.5f;
        }
    }
}
