using UnityEngine;
using UnityEngine.UI;

public class RocketLaunchVisual : MonoBehaviour
{
    [Header("Target rocket")]
    [SerializeField] private RocketLaunch rocketLaunch;
    [SerializeField] private Image outline;
    [SerializeField] private Gradient gradient;
    [Header("Slider")]
    [SerializeField] private Slider progressSlider;
    [Header("Sky")]
    [SerializeField] private RectTransform skyTransform;

    private void OnEnable()
    {
        rocketLaunch.OnAngleDifferenceChanged += UpdateTargetRocketVisual;
        rocketLaunch.OnTimeCoefChanged += UpdateSliderVisual;
    }

    private void OnDisable()
    {
        rocketLaunch.OnAngleDifferenceChanged -= UpdateTargetRocketVisual;
        rocketLaunch.OnTimeCoefChanged -= UpdateSliderVisual;
    }

    private void UpdateTargetRocketVisual(float coef)
    {
        outline.color = gradient.Evaluate(coef);
    }

    private void UpdateSliderVisual(float coef)
    {
        progressSlider.value = coef;
        skyTransform.pivot = new Vector2(skyTransform.pivot.x, coef);
    }
}
