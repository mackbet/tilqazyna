using UnityEngine;

public class TimedVesselAnimator : MonoBehaviour
{
    [SerializeField] private TimedVessel _vessel;
    [SerializeField] private ImageAnimator _animator;

    private void OnEnable()
    {
        _vessel.CookingStarted += OnCookingStarted;
        _vessel.CookingFinished += OnCookingFinished;
    }

    private void OnDisable()
    {
        _vessel.CookingStarted -= OnCookingStarted;
        _vessel.CookingFinished -= OnCookingFinished;
    }

    private void OnCookingStarted()
    {
        _animator.gameObject.SetActive(true);
        _animator.StartAnimation();
    }

    private void OnCookingFinished()
    {
        _animator.StopAnimation();
        _animator.gameObject.SetActive(false);
    }
}
