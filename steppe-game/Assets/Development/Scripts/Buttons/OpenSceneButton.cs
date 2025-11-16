using UnityEngine;

public class OpenSceneButton : CustomButton
{
    [SerializeField] private GameScene scene;

    protected override void Clicked()
    {
        GameManager.Instance.OpenNewScene(scene);
    }
}
