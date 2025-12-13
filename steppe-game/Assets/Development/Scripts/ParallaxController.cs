using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private ParallaxLayer[] parallaxLayers;

    private bool isInitialized = false;

    private void Start()
    {
        InitializeLayers();
    }

    public void InitializeLayers()
    {
        if (parallaxLayers == null)
        {
            isInitialized = false;
            return;
        }

        foreach (var layer in parallaxLayers)
        {
            if (layer.image == null) continue;

            if (layer.spawnChance >= 1f)
            {
                layer.isActive = true;
                layer.image.enabled = true;
                Rect uvRect = layer.image.uvRect;
                uvRect.x = layer.uvOffsetX;
                layer.image.uvRect = uvRect;
            }
            else
            {
                layer.isActive = false;
                layer.image.enabled = false;
                layer.nextSpawnTime = Time.time + Random.Range(layer.minInterval, layer.maxInterval);
            }
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdateParallax();
    }

    private void UpdateParallax()
    {
        if (parallaxLayers == null) return;

        foreach (var layer in parallaxLayers)
        {
            if (layer.image == null) continue;

            // Проверяем, нужно ли активировать слой
            if (!layer.isActive && layer.spawnChance < 1f)
            {
                if (Time.time >= layer.nextSpawnTime && Random.value <= layer.spawnChance)
                {
                    layer.isActive = true;
                    layer.image.enabled = true;
                    layer.image.uvRect = new Rect(layer.uvOffsetX, 0, layer.image.uvRect.width, layer.image.uvRect.height);
                }
            }

            // Двигаем активный слой
            if (layer.isActive)
            {
                Rect uvRect = layer.image.uvRect;
                uvRect.x += layer.speed * Time.deltaTime;
                layer.image.uvRect = uvRect;

                // Деактивируем слой после полного прохода (опционально)
                if (layer.spawnChance < 1f && uvRect.x >= 1f)
                {
                    layer.isActive = false;
                    layer.image.enabled = false;
                    layer.nextSpawnTime = Time.time + Random.Range(layer.minInterval, layer.maxInterval);
                }
            }
        }
    }

    public void SetSpeed(float speedMultiplier)
    {
        if (parallaxLayers == null) return;

        foreach (var layer in parallaxLayers)
        {
            if (layer.image != null)
            {
                layer.speed *= speedMultiplier;
            }
        }
    }

    public void StopParallax()
    {
        isInitialized = false;
    }

    public void ResumeParallax()
    {
        isInitialized = true;
    }

    public void ResetParallax()
    {
        InitializeLayers();
    }
}
