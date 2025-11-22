using System;
using UnityEngine;

public class ArrangeNames : GameController
{
    [SerializeField] private GameObject[] scenes;
    private int index = -1;
    private GameObject currentScene = null;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        NextScene();
    }

    public void NextScene()
    {
        if (currentScene)
            currentScene.SetActive(false);

        index++;

        currentScene = scenes[index];
        currentScene.SetActive(true);
    }
}
