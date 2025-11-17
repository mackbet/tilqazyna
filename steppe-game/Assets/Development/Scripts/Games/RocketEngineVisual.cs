using UnityEngine;
using UnityEngine.WSA;

public class RocketEngineVisual : MonoBehaviour
{
    [SerializeField] private RocketButton rocketButton;
    [SerializeField] private ParticleSystem fireVFX;

    private void OnEnable()
    {
        rocketButton.OnStateChanged += UpdateVFX;
    }

    private void OnDisable()
    {
        rocketButton.OnStateChanged -= UpdateVFX;
    }

    private void UpdateVFX(bool state)
    {
        if (state)
            fireVFX.Play();
        else
            fireVFX.Stop();
    }
}
