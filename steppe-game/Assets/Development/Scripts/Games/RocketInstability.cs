using UnityEngine;

public class RocketInstability : MonoBehaviour
{
    [Header("Instability Settings")]
    [SerializeField] private float instabilityStrength = 2f;
    [SerializeField] private float instabilityFrequency = 0.1f;
    [SerializeField] private bool usePerlinNoise = true;

    private float instabilityTimer = 0f;
    private float currentInstabilityTorque = 0f;
    private float perlinNoiseOffset;
    private bool isActive = false;

    public float CurrentTorque => enabled && isActive ? currentInstabilityTorque : 0f;

    private void FixedUpdate()
    {
        if (!isActive || !enabled) return;

        UpdateInstability(Time.fixedDeltaTime);
    }

    private void UpdateInstability(float deltaTime)
    {
        instabilityTimer += deltaTime;

        if (instabilityTimer >= instabilityFrequency)
        {
            instabilityTimer = 0f;

            if (usePerlinNoise)
            {
                perlinNoiseOffset += deltaTime;
                float perlinValue = Mathf.PerlinNoise(perlinNoiseOffset * 10f, 0f);
                currentInstabilityTorque = (perlinValue * 2f - 1f) * instabilityStrength;
            }
            else
            {
                currentInstabilityTorque = Random.Range(-instabilityStrength, instabilityStrength);
            }
        }
    }

    public void Initialize()
    {
        instabilityTimer = 0f;
        currentInstabilityTorque = 0f;
        perlinNoiseOffset = Random.Range(0f, 100f);
        isActive = true;
    }

    public void Reset()
    {
        instabilityTimer = 0f;
        currentInstabilityTorque = 0f;
        isActive = false;
    }

    public void SetEnabled(bool value)
    {
        enabled = value;
        if (!enabled)
        {
            currentInstabilityTorque = 0f;
        }
    }

    public void SetActive(bool value)
    {
        isActive = value;
        if (!isActive)
        {
            currentInstabilityTorque = 0f;
        }
    }
}