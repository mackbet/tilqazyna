using System;
using UnityEngine;
using DG.Tweening;


public class RocketLaunch : GameController
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rocketRigidbody;
    [SerializeField] private RocketTarget targetRocket;
    [SerializeField] private RocketButton leftEngineButton;
    [SerializeField] private RocketButton rightEngineButton;
    [SerializeField] private RocketInstability instability;
    [SerializeField] private ParticleSystem explosionParticles; // Система частиц взрыва при превышении допустимого угла

    [Header("Rotation Settings")]
    [SerializeField] private float rotationTorque = 10f;
    [SerializeField] private float maxAngularVelocity = 100f;
    [SerializeField] private float maxRotationDifference = 5f;

    [Header("Flight Settings")]
    [SerializeField] private float flightDuration = 10f;
    [SerializeField] private bool failOnDeviation = true;
    [SerializeField] float finalFlightTime = 3f; // Время полета в секундах
    [SerializeField] float finalFlightSpeed = 5f; // Скорость полета

    private bool isLeftEngineActive = false;
    private bool isRightEngineActive = false;
    private float CurrentFlightTime
    {
        get { return currentFlightTime; }
        set
        {
            currentFlightTime = value;
            OnTimeCoefChanged?.Invoke(currentFlightTime / flightDuration);
        }
    }
    private float currentFlightTime;
    private bool isFlightActive = false;
    private bool explosionTriggered = false; // Флаг для отслеживания взрыва

    public event Action<float> OnAngleDifferenceChanged;
    public event Action<float> OnTimeCoefChanged;

    protected override void OnEnable()
    {
        base.OnEnable();
        leftEngineButton.OnStateChanged += LeftEngine;
        rightEngineButton.OnStateChanged += RightEngine;
        ResetRocket();
        StartFlight();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        leftEngineButton.OnStateChanged -= LeftEngine;
        rightEngineButton.OnStateChanged -= RightEngine;
    }

    private void FixedUpdate()
    {
        if (!isFlightActive) return;

        ApplyRotationForces();
    }

    private void Update()
    {
        if (!isFlightActive) return;

        UpdateFlight();
    }

    private void ApplyRotationForces()
    {
        float torque = 0f;

        if (isLeftEngineActive)
        {
            torque -= rotationTorque;
        }

        if (isRightEngineActive)
        {
            torque += rotationTorque;
        }

        // Добавляем случайное возмущение
        if (instability != null)
        {
            torque += instability.CurrentTorque;
        }

        // Применяем крутящий момент
        rocketRigidbody.AddTorque(torque);

        // Ограничиваем угловую скорость
        if (Mathf.Abs(rocketRigidbody.angularVelocity) > maxAngularVelocity)
        {
            rocketRigidbody.angularVelocity = Mathf.Sign(rocketRigidbody.angularVelocity) * maxAngularVelocity;
        }
    }

    private void UpdateFlight()
    {
        float currentAngle = rocketRigidbody.rotation;
        float targetAngle = targetRocket.GetCurrentAngle();
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

        OnAngleDifferenceChanged?.Invoke(Mathf.Abs(angleDifference) / maxRotationDifference);

        // Проверяем превышение допустимого угла и запускаем взрыв
        if (!explosionTriggered && Mathf.Abs(angleDifference) > maxRotationDifference)
        {
            FailFlight();
            TriggerExplosion();
        }

        // Проверяем отклонение
        // if (Mathf.Abs(angleDifference) > maxRotationDifference)
        // {
        //     FailFlight();
        //     return;
        // }

        // Угол правильный - продолжаем полет
        CurrentFlightTime += Time.deltaTime;

        // Проверяем успешное завершение
        if (CurrentFlightTime >= flightDuration)
        {
            CompleteFlight();
        }
    }

    private void StartFlight()
    {
        isFlightActive = true;
        CurrentFlightTime = 0f;

        if (instability != null)
        {
            instability.Initialize();
        }

        Debug.Log("Полет начался!");
    }

    private void CompleteFlight()
    {
        isFlightActive = false;
        instability.SetActive(false);

        rocketRigidbody.angularVelocity = 0;
        float angleInRadians = rocketRigidbody.rotation * Mathf.Deg2Rad;
        Vector2 direction = rocketRigidbody.transform.up;
        DOTween.To(() => 0f, x =>
        {
            // Каждый кадр двигаем ракету вперед
            rocketRigidbody.MovePosition(rocketRigidbody.position + direction * finalFlightSpeed * Time.deltaTime);
        }, 1f, finalFlightTime)
        .SetEase(Ease.Linear);

        Debug.Log($"Полет успешно завершен! Время: {CurrentFlightTime:F2}с");
        targetRocket.gameObject.SetActive(false);
        finishPanel.SetReward(10, 80, 100);
        FinishGame();
    }

    private void FailFlight()
    {
        isFlightActive = false;

        if (instability != null)
        {
            instability.SetActive(false);
        }

        Debug.Log($"Полет провален! Отклонение от курса. Время: {CurrentFlightTime:F2}с");

        if (failOnDeviation)
        {
            rocketRigidbody.angularVelocity = 0;
            float angleInRadians = rocketRigidbody.rotation * Mathf.Deg2Rad;
            Vector2 direction = Vector2.down;
            DOTween.To(() => 0f, x =>
            {
                // Каждый кадр двигаем ракету вперед
                rocketRigidbody.MovePosition(rocketRigidbody.position + direction * finalFlightSpeed * Time.deltaTime);
            }, 1f, finalFlightTime)
            .SetEase(Ease.Linear);
            finishPanel.SetReward(2, 25, 10);
            FinishGame();
        }
    }

    private void TriggerExplosion()
    {
        explosionTriggered = true;

        if (explosionParticles != null)
        {
            // Позиционируем частицы в месте ракеты
            explosionParticles.transform.position = rocketRigidbody.transform.position;
            explosionParticles.Play();
        }

        Debug.Log("Взрыв! Ракета превысила допустимый угол отклонения!");
    }

    private void LeftEngine(bool state)
    {
        isLeftEngineActive = state;
    }

    private void RightEngine(bool state)
    {
        isRightEngineActive = state;
    }

    private void ResetRocket()
    {
        isLeftEngineActive = false;
        isRightEngineActive = false;
        CurrentFlightTime = 0f;
        isFlightActive = false;
        explosionTriggered = false;

        if (instability != null)
        {
            instability.Reset();
        }

        rocketRigidbody.rotation = 0f;
        rocketRigidbody.angularVelocity = 0f;
    }

    public float GetFlightProgress()
    {
        return flightDuration > 0 ? CurrentFlightTime / flightDuration : 0f;
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0, flightDuration - CurrentFlightTime);
    }

    public float GetAngleDifference()
    {
        if (targetRocket == null) return 0f;
        return Mathf.DeltaAngle(rocketRigidbody.rotation, targetRocket.GetCurrentAngle());
    }

    public bool IsFlightActive()
    {
        return isFlightActive;
    }
}