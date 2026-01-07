using Development.Managers.Bozok.HorseGame;
using UnityEngine;

namespace Development.Objects.SevenTreasuresGame.Frisky
{
    public sealed class HorseAnimationController : MonoBehaviour
    {
        // private ImageLottiePlayer _lottie;
        // private bool isPaused;
        //
        // private void Awake()
        // {
        //     _lottie = GetComponent<ImageLottiePlayer>();
        //     HorseGameManager.OnGameSpeedChanged += ChangeAnimationSpeed;
        // }
        //
        // private void OnDestroy() => HorseGameManager.OnGameSpeedChanged -= ChangeAnimationSpeed;
        //
        // private void ChangeAnimationSpeed(float speed)
        // {
        //     if (speed <= 0)
        //     {
        //         _lottie.Pause();
        //         isPaused = true;
        //     }
        //     else
        //     {
        //         _lottie.Unpause();
        //         isPaused = false;
        //     }
        // }
        //
        // private void OnEnable()
        // {
        //     if (isPaused)
        //     {
        //         _lottie.Pause();
        //     }
        // }
    }
}