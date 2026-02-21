using TMPro;
using UnityEngine;

public class UIPanelFinish : UIPanel
{
    [SerializeField] private TextMeshProUGUI titleField;
    [SerializeField] private TextMeshProUGUI stateField;
    [SerializeField] private TextMeshProUGUI coinField;
    [SerializeField] private TextMeshProUGUI pointField;
    [SerializeField] private TextMeshProUGUI expField;
    [SerializeField] private GameObject oneStar;
    [SerializeField] private GameObject twoStars;
    [SerializeField] private GameObject threeStars;
    [SerializeField] private ActivateableObject[] passedObjects;
    [SerializeField] private ActivateableObject[] failedObjects;
    private string coinText = "тиын";
    private string pointText = "ұпай";
    private int coins = 4;
    private int points = 60;
    private int exp = 80;

    public override void Show()
    {
        base.Show();

        // Сначала начисляем награды
        StateManager.Instance.CurrencyAmount += coins;
        StateManager.Instance.PointsAmount += points;
        StateManager.Instance.ExperienceAmount += exp;

        // Затем выводим актуальные значения
        coinField.text = coins + " " + coinText;
        pointField.text = points + " " + pointText;
        expField.text = StateManager.Instance.ExperienceAmount.ToString();

        // Сохраняем данные и отправляем в лидерборд
        SaveAndSubmit();
        SoundManager.Instance.PlayExpGainSound();
    }

    private async void SaveAndSubmit()
    {
        // Сохраняем данные в Cloud Save (лидерборд обновляется автоматически)
        var realtimeManager = GetRealtimeManager();
        if (realtimeManager != null)
        {
            await realtimeManager.SaveUserData(
                StateManager.Instance.PlayerName,
                StateManager.Instance.CharacterSex,
                StateManager.Instance.ExperienceAmount,
                StateManager.Instance.PointsAmount
            );

            await realtimeManager.SaveCoins(StateManager.Instance.CurrencyAmount);
        }
    }

    private RealtimeManager GetRealtimeManager()
    {
        return RealtimeManager.Instance;
    }

    public void SetTitle(string title)
    {
        titleField.text = title;
    }

    public void SetReward(int newCoins, int newPoints, int newExp)
    {
        coins = newCoins;
        points = newPoints;
        exp = newExp / 2;
    }

    public void SetState(bool isPassed)
    {
        stateField.text = isPassed ? "өттің" : "сәтсіз";

        foreach (ActivateableObject ao in isPassed ? passedObjects : failedObjects)
            ao.Call();
    }

    public void SetStars(int count)
    {
        if (count == 1)
            oneStar.gameObject.SetActive(true);
        else if (count == 2)
            twoStars.gameObject.SetActive(true);
        else if (count > 2)
            threeStars.gameObject.SetActive(true);
        else
        {
            oneStar.gameObject.SetActive(false);
            twoStars.gameObject.SetActive(false);
            threeStars.gameObject.SetActive(false);
        }
    }
}
