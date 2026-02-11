using System;
using UnityEngine;
using UnityEngine.UI;

public class TimedVessel : Vessel
{
    [SerializeField] private float _cookingTime = 5f;
    [SerializeField] private Slider _timerSlider;

    private RecipeData _pendingRecipe;
    private float _elapsed;
    private bool _isCooking;

    public bool IsCooking => _isCooking;

    public event Action CookingStarted;
    public event Action CookingFinished;

    protected override void OnRecipeReady(RecipeData recipe)
    {
        _pendingRecipe = recipe;
        _elapsed = 0f;
        _isCooking = true;

        if (_timerSlider != null)
        {
            _timerSlider.gameObject.SetActive(true);
            _timerSlider.value = 0f;
        }

        CookingStarted?.Invoke();
    }

    private void Update()
    {
        if (!_isCooking) return;

        _elapsed += Time.deltaTime;

        if (_timerSlider != null)
            _timerSlider.value = _elapsed / _cookingTime;

        if (_elapsed >= _cookingTime)
        {
            _isCooking = false;

            if (_timerSlider != null)
                _timerSlider.gameObject.SetActive(false);

            CompleteRecipe(_pendingRecipe);
            _pendingRecipe = null;

            CookingFinished?.Invoke();
        }
    }
}
