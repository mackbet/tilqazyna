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

        NextScene();
    }

    public void NextScene()
    {
        if (currentScene)
        {
            if (currentSceneController != null)
            {
                currentSceneController.OnSceneCompleted -= OnSceneCompleted;
            }

            currentScene.SetActive(false);
        }

        index++;

        if (index < scenes.Length)
        {
            currentScene = scenes[index];
            currentSceneController = currentScene.GetComponent<ArrangeNamesScene>();

            if (currentSceneController != null)
            {
                currentSceneController.OnSceneCompleted += OnSceneCompleted;
            }

            currentScene.SetActive(true);
        }
        else
        {
            FinishGame();
        }
    }

    private void OnSceneCompleted()
    {
        NextScene();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (currentSceneController != null)
        {
            currentSceneController.OnSceneCompleted -= OnSceneCompleted;
        }
    }
}
