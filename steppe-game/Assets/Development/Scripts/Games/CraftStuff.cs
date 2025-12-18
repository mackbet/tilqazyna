using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CraftStuff : GameController
{
    [SerializeField] private Image personImage;
    [SerializeField] private LocalizableText personName;
    [SerializeField] private CraftToolsProfession[] professions;
    [SerializeField] private CraftStuffButton[] stuffButtons;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private float reactionDuration = 1f;
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private float shrinkDuration = 0.3f;

    private CraftToolsProfession selectedProfession;
    private int correctSelections = 0;
    private int totalCorrectItems = 0;
    private Tween reactionTween;
    private List<CraftStuffButton> correctButtons = new List<CraftStuffButton>();

    protected override void InitializeGame()
    {
        base.InitializeGame();

        selectedProfession = professions[Random.Range(0, professions.Length)];
        correctButtons.Clear();
        correctSelections = 0;
        personImage.sprite = selectedProfession.WaitingSprite;
        personName.SetString(selectedProfession.Name);

        FillButtons();

        foreach (var button in stuffButtons)
            button.OnStuffClicked += OnStuffSelected;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        reactionTween?.Kill();

        foreach (var button in stuffButtons)
            button.OnStuffClicked -= OnStuffSelected;
    }

    private void FillButtons()
    {
        var allItems = new List<(CraftStuffVariant stuff, CraftToolsProfession profession)>();
        var usedStuff = new HashSet<CraftStuffVariant>();

        foreach (var stuff in selectedProfession.Stuff)
        {
            if (usedStuff.Add(stuff))
                allItems.Add((stuff, selectedProfession));
        }

        totalCorrectItems = allItems.Count;

        var otherItems = professions
            .Where(p => p != selectedProfession)
            .SelectMany(p => p.Stuff.Where(s => usedStuff.Add(s)).Select(s => (s, p)))
            .OrderBy(_ => Random.value)
            .Take(stuffButtons.Length - totalCorrectItems);

        allItems.AddRange(otherItems);
        allItems = allItems.OrderBy(_ => Random.value).ToList();

        for (int i = 0; i < stuffButtons.Length; i++)
        {
            if (i < allItems.Count)
            {
                stuffButtons[i].Initialize(allItems[i].stuff, allItems[i].profession);
                stuffButtons[i].gameObject.SetActive(true);
            }
            else
            {
                stuffButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnStuffSelected(CraftStuffButton button, CraftStuffVariant stuff, CraftToolsProfession profession)
    {
        bool isCorrect = profession == selectedProfession;
        button.SetSelected(isCorrect);

        if (isCorrect)
        {
            correctButtons.Add(button);
            correctSelections++;
            AudioManager.Instance.PlaySound(correctSound);

            if (correctSelections >= totalCorrectItems)
                OnAllItemsCollected();
            else
                ShowReaction(selectedProfession.FunSprite);
        }
        else
        {
            AudioManager.Instance.PlaySound(wrongSound);
            ShowReaction(selectedProfession.SadSprite);
        }
    }

    private void ShowReaction(Sprite reactionSprite)
    {
        reactionTween?.Kill();
        personImage.sprite = reactionSprite;
        reactionTween = DOVirtual.DelayedCall(reactionDuration, () => personImage.sprite = selectedProfession.WaitingSprite);
    }

    private void OnAllItemsCollected()
    {
        reactionTween?.Kill();

        var wrongButtons = stuffButtons.Where(b => b.gameObject.activeInHierarchy && !correctButtons.Contains(b)).ToList();

        foreach (var button in wrongButtons)
        {
            button.transform.DOScale(0f, shrinkDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => button.Hide());
        }

        float delay = wrongButtons.Count > 0 ? shrinkDuration : 0f;
        Vector3 targetPos = personImage.transform.position;

        DOVirtual.DelayedCall(delay, () =>
        {
            var sequence = DOTween.Sequence();

            foreach (var button in correctButtons)
            {
                button.HideSoundButton();
                sequence.Join(button.transform.DOMove(targetPos, flyDuration).SetEase(Ease.InBack));
                sequence.Join(button.transform.DOScale(0f, flyDuration).SetEase(Ease.InBack));
            }

            sequence.OnComplete(() =>
            {
                foreach (var button in correctButtons)
                    button.gameObject.SetActive(false);

                personImage.sprite = selectedProfession.FinalSprite;
                finishPanel.SetReward(10, 80, 100);
                FinishGame();
            });
        });
    }

    public void ResetGame()
    {
        reactionTween?.Kill();
        DOTween.Kill(this);
        correctSelections = 0;
        correctButtons.Clear();

        foreach (var button in stuffButtons)
            button.Reset();

        InitializeGame();
    }
}