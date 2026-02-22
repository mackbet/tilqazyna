using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Chase : GameController, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private WordSpawner wordSpawner;
    [Header("Characters")]
    [SerializeField] private RectTransform rabbit;
    [SerializeField] private RectTransform wolf;

    [Header("Parallax Layers")]
    [SerializeField] private ParallaxLayer[] parallaxLayers;

    [Header("Obstacles")]
    [SerializeField] private RectTransform obstaclesContainer;
    [SerializeField] private GameObject[] highObstaclePrefabs; // Высокие препятствия - нужно кувыркаться (на уровне зайца)
    [SerializeField] private GameObject[] lowObstaclePrefabs; // Низкие препятствия - нужно прыгать (внизу экрана)
    [SerializeField] private float obstacleSpawnInterval = 2f;
    [SerializeField] private float obstacleSpeed = 500f;

    [Header("Rabbit Settings")]
    [SerializeField] private float rabbitY = 0f;
    [SerializeField] private float jumpHeight = 200f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private Ease jumpEase = Ease.OutQuad;

    [Header("Wolf Settings")]
    [SerializeField] private float wolfCatchDistance = 50f;
    [SerializeField] private float wolfMoveDuration = 0.5f; // Длительность движения волка
    [SerializeField] private Ease wolfMoveEase = Ease.InOutQuad;

    [Header("Game Settings")]
    [SerializeField] private int startLives = 3;

    [Header("Sounds")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private bool isJumping = false;
    private bool isRolling = false;
    private bool isGameActive = false;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private Coroutine obstacleSpawnerCoroutine;
    private float rabbitBaseY;
    private Vector2 touchStartPos;
    private float currentWolfDistance;
    private float initialWolfDistance; // Начальное расстояние, рассчитанное из позиций
    private int currentWolfStep = 0; // Текущий шаг волка (0 - далеко, wolfMaxSteps - поймал)
    private Tween wolfMoveTween;
    private Tween jumpTween;
    private Tween rollTween;

    protected override void InitializeGame()
    {
        base.InitializeGame();

        KillAllTweens();

        SetLives(startLives);
        isGameActive = true;
        currentWolfStep = 0;

        if (rabbit != null)
        {
            rabbitBaseY = rabbitY;
            rabbit.anchoredPosition = new Vector2(rabbit.anchoredPosition.x, rabbitBaseY);
            rabbit.localRotation = Quaternion.identity;
        }

        if (rabbit != null && wolf != null)
        {
            initialWolfDistance = rabbit.position.x - wolf.position.x;
            currentWolfDistance = initialWolfDistance;
        }

        InitializeParallaxLayers();
        ClearObstacles();
        obstacleSpawnerCoroutine = StartCoroutine(SpawnObstacles());

        wordSpawner.OnAllWordsCollected += WinGame;
        wordSpawner.StartSpawning();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        isGameActive = false;

        KillAllTweens();

        if (wordSpawner != null)
            wordSpawner.OnAllWordsCollected -= WinGame;

        if (obstacleSpawnerCoroutine != null)
        {
            StopCoroutine(obstacleSpawnerCoroutine);
        }

        ClearObstacles();
    }

    private void KillAllTweens()
    {
        wolfMoveTween?.Kill();
        jumpTween?.Kill();
        rollTween?.Kill();
    }

    private void Update()
    {
        if (!isGameActive) return;

        UpdateParallax();
        UpdateObstacles();
    }

    private void InitializeParallaxLayers()
    {
        if (parallaxLayers == null) return;

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
                layer.nextSpawnTime = Time.time + UnityEngine.Random.Range(layer.minInterval, layer.maxInterval);
            }
        }
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
                if (Time.time >= layer.nextSpawnTime && UnityEngine.Random.value <= layer.spawnChance)
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
                    layer.nextSpawnTime = Time.time + UnityEngine.Random.Range(layer.minInterval, layer.maxInterval);
                }
            }
        }
    }

    private void MoveWolfCloser()
    {
        if (wolf == null || rabbit == null) return;
        currentWolfStep++;
        currentWolfStep = Mathf.Min(currentWolfStep, startLives);

        float stepProgress = (float)currentWolfStep / startLives;
        float targetDistance = Mathf.Lerp(initialWolfDistance, wolfCatchDistance, stepProgress);

        wolfMoveTween?.Kill();

        Vector3 targetPos = rabbit.position;
        targetPos.x -= targetDistance;

        wolfMoveTween = wolf.DOMove(targetPos, wolfMoveDuration)
            .SetEase(wolfMoveEase)
            .OnUpdate(() =>
            {
                if (rabbit != null && wolf != null)
                    currentWolfDistance = rabbit.position.x - wolf.position.x;
            })
            .OnComplete(() =>
            {
                currentWolfDistance = targetDistance;
                if (currentWolfStep >= startLives)
                    LoseGame();
            });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isGameActive) return;
        touchStartPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isGameActive) return;

        Vector2 touchEndPos = eventData.position;
        Vector2 swipeDelta = touchEndPos - touchStartPos;

        float minSwipeDistance = 50f;

        if (swipeDelta.magnitude > minSwipeDistance)
        {
            // Проверяем, что свайп больше вертикальный чем горизонтальный
            if (Mathf.Abs(swipeDelta.y) > Mathf.Abs(swipeDelta.x))
            {
                if (swipeDelta.y > 0)
                {
                    // Свайп вверх - прыжок
                    Jump();
                }
                else
                {
                    // Свайп вниз - кувырок
                    Roll();
                }
            }
        }
    }

    public void Jump()
    {
        if (!isGameActive || isJumping || isRolling) return;

        isJumping = true;

        if (jumpSound != null)
        {
            AudioManager.Instance.PlaySound(jumpSound, 0.5f);
        }

        // Убиваем предыдущий твин прыжка
        jumpTween?.Kill();

        // Создаем последовательность: прыжок вверх -> прыжок вниз
        Sequence jumpSequence = DOTween.Sequence();

        jumpSequence.Append(
            rabbit.DOAnchorPosY(rabbitBaseY + jumpHeight, jumpDuration / 2f)
                .SetEase(jumpEase)
        );

        jumpSequence.Append(
            rabbit.DOAnchorPosY(rabbitBaseY, jumpDuration / 2f)
                .SetEase(jumpEase)
        );

        jumpSequence.OnComplete(() =>
        {
            isJumping = false;
        });

        jumpTween = jumpSequence;
    }

    public void Roll()
    {
        if (!isGameActive || isJumping || isRolling) return;

        isRolling = true;

        if (jumpSound != null)
        {
            AudioManager.Instance.PlaySound(jumpSound);
        }

        // Убиваем предыдущий твин кувырка
        rollTween?.Kill();

        // Создаем вращение на 360 градусов
        rollTween = rabbit.DOLocalRotate(new Vector3(0f, 0f, -360f), rollDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                rabbit.localRotation = Quaternion.identity;
                isRolling = false;
            });
    }

    private IEnumerator SpawnObstacles()
    {
        WaitForSeconds wait = new WaitForSeconds(obstacleSpawnInterval);

        while (isGameActive)
        {
            yield return wait;
            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        if (obstaclesContainer == null)
            return;

        // Получаем ширину экрана в единицах Canvas
        float screenRight = Screen.width / 2f;
        // Выбираем случайный тип препятствия
        bool isHighObstacle = UnityEngine.Random.value > 0.5f;
        GameObject[] selectedPrefabs = isHighObstacle ? highObstaclePrefabs : lowObstaclePrefabs;

        if (selectedPrefabs == null || selectedPrefabs.Length == 0)
            return;

        GameObject prefab = selectedPrefabs[UnityEngine.Random.Range(0, selectedPrefabs.Length)];
        GameObject obstacle = Instantiate(prefab, obstaclesContainer);

        // Добавляем тег для определения типа препятствия
        ObstacleType obstacleType = obstacle.AddComponent<ObstacleType>();
        obstacleType.isHigh = isHighObstacle;

        RectTransform obstacleRect = obstacle.GetComponent<RectTransform>();
        if (obstacleRect != null)
        {
            // Препятствия появляются снизу экрана
            float spawnX = screenRight + obstacleRect.rect.width;
            obstacleRect.anchoredPosition = new Vector2(spawnX, 0);
        }

        activeObstacles.Add(obstacle);
    }

    private void UpdateObstacles()
    {
        // Получаем ширину экрана в единицах Canvas
        float screenLeft = -Screen.width / 2f;

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obstacle = activeObstacles[i];
            if (obstacle == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            RectTransform obstacleRect = obstacle.GetComponent<RectTransform>();
            Vector2 pos = obstacleRect.anchoredPosition;

            // Препятствия движутся влево
            pos.x -= obstacleSpeed * Time.deltaTime;

            // Препятствия поднимаются снизу к уровню зайца
            if (pos.y < rabbitY)
            {
                pos.y += obstacleSpeed * 0.5f * Time.deltaTime;
                pos.y = Mathf.Min(pos.y, rabbitY);
            }

            obstacleRect.anchoredPosition = pos;

            // Получаем ширину препятствия
            float obstacleWidth = obstacleRect.rect.width;

            // Удаляем препятствия, которые полностью ушли за левый край экрана
            if (pos.x + obstacleWidth / 2f < screenLeft)
            {
                Destroy(obstacle);
                activeObstacles.RemoveAt(i);
                continue;
            }

            // Проверяем столкновение только если препятствие достигло уровня зайца
            if (rabbit != null && pos.y >= rabbitY)
            {
                ObstacleType obstacleType = obstacle.GetComponent<ObstacleType>();
                bool canAvoid = false;

                if (obstacleType != null)
                {
                    // Высокое препятствие - можно избежать кувырком
                    if (obstacleType.isHigh && isRolling)
                        canAvoid = true;
                    // Низкое препятствие - можно избежать прыжком
                    else if (!obstacleType.isHigh && isJumping)
                        canAvoid = true;
                }

                // Если не избегаем препятствие, проверяем столкновение
                if (!canAvoid && CheckCollision(rabbit, obstacleRect))
                {
                    OnObstacleHit(obstacle, i);
                }
            }
        }
    }

    private bool CheckCollision(RectTransform a, RectTransform b)
    {
        Vector2 aPos = a.anchoredPosition;
        Vector2 bPos = b.anchoredPosition;
        Vector2 aSize = a.sizeDelta * 0.4f;
        Vector2 bSize = b.sizeDelta * 0.4f;

        return Mathf.Abs(aPos.x - bPos.x) < (aSize.x + bSize.x) / 2f &&
               Mathf.Abs(aPos.y - bPos.y) < (aSize.y + bSize.y) / 2f;
    }

    private void OnObstacleHit(GameObject obstacle, int index)
    {
        if (hitSound != null)
        {
            AudioManager.Instance.PlaySound(hitSound);
        }

        Destroy(obstacle);
        activeObstacles.RemoveAt(index);

        SetLives(Lives - 1);

        // Волк делает шаг к зайцу через DOTween
        MoveWolfCloser();

        if (Lives <= 0)
        {
            // LoseGame будет вызван после завершения анимации волка
            return;
        }
    }

    private void ClearObstacles()
    {
        foreach (var obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                Destroy(obstacle);
            }
        }
        activeObstacles.Clear();
    }

    private void WinGame()
    {
        isGameActive = false;

        if (winSound != null)
        {
            AudioManager.Instance.PlaySound(winSound);
        }

        FinishGame();
        rabbit.gameObject.SetActive(false);
        wolf.gameObject.SetActive(false);
        wordSpawner.StopSpawning();
    }

    private void LoseGame()
    {
        isGameActive = false;

        if (loseSound != null)
        {
            AudioManager.Instance.PlaySound(loseSound);
        }

        FailGame();
        rabbit.gameObject.SetActive(false);
        wolf.gameObject.SetActive(false);
        wordSpawner.StopSpawning();
    }
}

// Компонент для определения типа препятствия
public class ObstacleType : MonoBehaviour
{
    public bool isHigh; // true = высокое (нужен кувырок), false = низкое (нужен прыжок)
}