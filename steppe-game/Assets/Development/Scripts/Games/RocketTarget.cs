using UnityEngine;

public class RocketTarget : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform targetTransform;

    [Header("Rotation Settings")]
    [SerializeField] private Vector2 targetRotationRange = new Vector2(-45f, 45f); // Диапазон углов поворота
    [SerializeField] private Vector2 rotationDurationRange = new Vector2(1f, 3f); // Диапазон длительности поворота
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Flight Pattern")]
    [SerializeField] private Vector2 straightFlightDurationRange = new Vector2(1f, 3f); // Диапазон прямого полета
    [SerializeField] private bool autoPattern = true;

    private float currentAngle;
    private float previousAngle;
    private float targetRotation;
    private float rotationDuration;
    private float straightFlightDuration;
    private float rotationProgress = 0f;
    
    private float phaseTimer = 0f;
    private FlightPhase currentPhase = FlightPhase.StraightFlight;
    
    public enum FlightPhase
    {
        StraightFlight,
        Rotating
    }

    private void Start()
    {
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();

        currentAngle = 0f;
        previousAngle = 0f;
        targetTransform.rotation = Quaternion.identity;

        // Инициализируем первые случайные значения
        straightFlightDuration = Random.Range(straightFlightDurationRange.x, straightFlightDurationRange.y);
    }

    private void Update()
    {
        if (autoPattern)
        {
            UpdateFlightPattern();
        }

        UpdateRotation();
    }

    private void UpdateFlightPattern()
    {
        phaseTimer += Time.deltaTime;

        switch (currentPhase)
        {
            case FlightPhase.StraightFlight:
                if (phaseTimer >= straightFlightDuration)
                {
                    StartRotation();
                    phaseTimer = 0f;
                }
                break;

            case FlightPhase.Rotating:
                if (phaseTimer >= rotationDuration)
                {
                    FinishRotation();
                    phaseTimer = 0f;
                }
                break;
        }
    }

    private void StartRotation()
    {
        currentPhase = FlightPhase.Rotating;
        previousAngle = currentAngle;
        rotationProgress = 0f;
        
        // Генерируем случайные значения для поворота
        targetRotation = Random.Range(targetRotationRange.x, targetRotationRange.y);
        rotationDuration = Random.Range(rotationDurationRange.x, rotationDurationRange.y);
    }

    private void FinishRotation()
    {
        currentPhase = FlightPhase.StraightFlight;
        currentAngle = targetRotation;
        rotationProgress = 1f;
        
        // Генерируем случайную длительность прямого полета
        straightFlightDuration = Random.Range(straightFlightDurationRange.x, straightFlightDurationRange.y);
    }

    private void UpdateRotation()
    {
        if (currentPhase == FlightPhase.Rotating)
        {
            rotationProgress = phaseTimer / rotationDuration;
            rotationProgress = Mathf.Clamp01(rotationProgress);
            
            float curvedProgress = rotationCurve.Evaluate(rotationProgress);
            currentAngle = Mathf.LerpAngle(previousAngle, targetRotation, curvedProgress);
        }

        targetTransform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }

    public float GetCurrentAngle() => currentAngle;
    public bool IsRotating() => currentPhase == FlightPhase.Rotating;
    public FlightPhase GetCurrentPhase() => currentPhase;

    public void SetTargetRotationRange(Vector2 range)
    {
        targetRotationRange = range;
    }

    public void SetRotationDurationRange(Vector2 range)
    {
        rotationDurationRange = range;
    }

    public void SetStraightFlightDurationRange(Vector2 range)
    {
        straightFlightDurationRange = range;
    }

    public void SetAutoPattern(bool enabled) => autoPattern = enabled;

    public void ResetPattern()
    {
        phaseTimer = 0f;
        currentPhase = FlightPhase.StraightFlight;
        currentAngle = 0f;
        previousAngle = 0f;
        rotationProgress = 0f;
        targetTransform.rotation = Quaternion.identity;
        
        // Генерируем новую длительность прямого полета
        straightFlightDuration = Random.Range(straightFlightDurationRange.x, straightFlightDurationRange.y);
    }
}