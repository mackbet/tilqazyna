using BigDreamLab.LocalizationSystem;
using UnityEngine;
using UnityEngine.Localization;

public class GameButton : CustomButton
{
    [SerializeField] private GameView gameView;
    [SerializeField] private LocalizedString gameTitle;
    protected override void Clicked()
    {
        base.Clicked();
        GameView view = CanvasViewManager.Instance.Load(gameView);
        view.SetTitle(gameTitle.GetLocalizedString());
    }
}
