using TMPro;
using UnityEngine;

public class GameView : CanvasView
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private UIPanelFinish finishPanel;
    [SerializeField] private UIPanelRestart restartPanel;
    [SerializeField] private GameController gameController;

    private void OnEnable()
    {
        gameController.OnGameFinished += Finish;
        gameController.OnGameFailed += Restart;
    }

    private void OnDisable()
    {
        gameController.OnGameFinished -= Finish;
        gameController.OnGameFailed -= Restart;
    }


    public void SetTitle(string text)
    {
        title.text = text;
    }

    public void Close()
    {
        CanvasViewManager.Instance.Unload(this);
    }

    private void Restart()
    {
        restartPanel.Show();
    }

    private void Finish()
    {
        finishPanel.Show();
    }
}
