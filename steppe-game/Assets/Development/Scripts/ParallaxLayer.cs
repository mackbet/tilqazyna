using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ParallaxLayer
{
    public RawImage image;
    public float speed;
    [Range(0f, 1f)] public float spawnChance = 1f; // 1 = всегда, 0 = никогда
    public float minInterval = 5f; // Минимальный интервал между появлениями
    public float maxInterval = 10f; // Максимальный интервал между появлениями
    [Range(-1f, 1f)] public float uvOffsetX = 0f; // Начальный сдвиг UV по X (например, -0.5 для смещения влево)
    [HideInInspector] public float nextSpawnTime;
    [HideInInspector] public bool isActive;
}
