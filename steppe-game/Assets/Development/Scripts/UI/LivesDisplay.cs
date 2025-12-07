using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesDisplay : MonoBehaviour
{
    [Header("Lives Settings")]
    [SerializeField] private GameController gameController; // Ссылка на GameController
    [SerializeField] private GameObject lifeIconPrefab; // Префаб иконки жизни
    [SerializeField] private Transform iconsContainer; // Контейнер для иконок
    [SerializeField] private int maxLives = 3; // Максимальное количество жизней

    private List<GameObject> lifeIcons = new List<GameObject>();
    private List<Color> originalColors = new List<Color>();

    private void Awake()
    {
        if (iconsContainer == null)
            iconsContainer = transform;
    }

    private void OnEnable()
    {
        if (gameController != null)
        {
            gameController.OnLivesChanged += UpdateLives;
        }
    }

    private void OnDisable()
    {
        if (gameController != null)
        {
            gameController.OnLivesChanged -= UpdateLives;
        }
    }

    /// <summary>
    /// Инициализация жизней
    /// </summary>
    public void Initialize(int lives)
    {
        ClearIcons();
        maxLives = lives;
        CreateIcons(lives);
    }

    /// <summary>
    /// Обновление отображения жизней
    /// </summary>
    public void UpdateLives(int currentLives)
    {
        // Если иконок еще нет, создаем
        if (lifeIcons.Count == 0)
        {
            CreateIcons(currentLives);
            return;
        }

        // Обновляем цвет иконок
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
            {
                Image image = lifeIcons[i].GetComponent<Image>();
                if (image != null)
                {
                    // Если жизнь активна - оригинальный цвет, иначе черный
                    image.color = i < currentLives ? originalColors[i] : Color.black;
                }
            }
        }
    }

    /// <summary>
    /// Создание иконок жизней
    /// </summary>
    private void CreateIcons(int count)
    {
        if (lifeIconPrefab == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(lifeIconPrefab, iconsContainer);
            lifeIcons.Add(icon);

            // Сохраняем оригинальный цвет иконки
            Image image = icon.GetComponent<Image>();
            if (image != null)
            {
                originalColors.Add(image.color);
            }
            else
            {
                originalColors.Add(Color.white);
            }
        }
    }

    /// <summary>
    /// Очистка всех иконок
    /// </summary>
    private void ClearIcons()
    {
        foreach (var icon in lifeIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        lifeIcons.Clear();
        originalColors.Clear();
    }

    private void OnDestroy()
    {
        ClearIcons();
    }
}
