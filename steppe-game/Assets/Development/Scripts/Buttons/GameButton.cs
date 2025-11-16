using UnityEngine;

public class GameButton : CustomButton
{
    protected override void Clicked()
    {
        base.Clicked();
        Debug.Log($"GameButton {name}");
    }
}
