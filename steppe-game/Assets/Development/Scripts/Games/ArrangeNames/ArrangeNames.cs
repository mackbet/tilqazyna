using System;
using UnityEngine;

public class ArrangeNames : GameController
{
    [SerializeField] private GameObject[] scenes;
    private int index = -1;
    private GameObject currentScene = null;
    private ArrangeNamesScene currentSceneController = null;

    protected override void InitializeGame()
    {
        base.InitializeGame();
    }

    public void NextScene()
    {
        index++;

        if (index < scenes.Length)
        {
            if (currentScene)
                currentScene.SetActive(false);

            currentScene = scenes[index];
            currentSceneController = currentScene.GetComponent<ArrangeNamesScene>();

            currentScene.SetActive(true);
        }
        else
        {
            finishPanel.SetReward(8, 70, 90);
            finishPanel.SetState(true);
            finishPanel.SetStars(3);
            FinishGame();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
