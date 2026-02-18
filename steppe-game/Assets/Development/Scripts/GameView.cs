using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class GameView : CanvasView
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI finishTitle;
    [SerializeField] private UIPanelFinish finishPanel;
    [SerializeField] private UIPanelRestart restartPanel;
    [SerializeField] private GameController gameController;

    private LocalizedString _localizedTitle;

    protected virtual void OnEnable()
    {
        gameController.OnGameFinished += Finish;
        gameController.OnGameFailed += Restart;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        gameController.OnGameFinished -= Finish;
        gameController.OnGameFailed -= Restart;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (_localizedTitle != null)
            SetTitle(_localizedTitle);
    }

    public void SetTitle(LocalizedString localizedTitle)
    {
        if (!title)
            return;
        _localizedTitle = localizedTitle;
        var text = localizedTitle.GetLocalizedString();
        title.text = text;
        finishPanel.SetTitle(text);
    }

    public void Close()
    {
        CanvasViewManager.Instance.Unload(this);
    }

    private void Restart()
    {
        finishPanel.Show();
    }

    private void Finish()
    {
        finishPanel.Show();
    }
}
