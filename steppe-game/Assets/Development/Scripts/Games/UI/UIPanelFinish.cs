using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIPanelFinish : UIPanel
{
    [SerializeField] private TextMeshProUGUI titleField;
    [SerializeField] private TextMeshProUGUI coinField;
    [SerializeField] private TextMeshProUGUI pointField;
    [SerializeField] private TextMeshProUGUI expField;
    private string coinText = "тиын";
    private string pointText = "ұпай";
    private int coins = 4;
    private int points = 60;
    private int exp = 80;

    public override void Show()
    {
        base.Show();

        coinField.text = coins + " " + coinText;
        pointField.text = points + " " + pointText;
        expField.text = StateManager.Instance.ExperienceAmount.ToString();

        StateManager.Instance.CurrencyAmount += coins;
        StateManager.Instance.PointsAmount += points;
        StateManager.Instance.ExperienceAmount += exp;
    }

    public void SetTitle(string title)
    {
        titleField.text = title;
    }

    public void SetReward(int newCoins, int newPoints, int newExp)
    {
        coins = newCoins;
        points = newPoints;
        exp = newExp;
    }
}
