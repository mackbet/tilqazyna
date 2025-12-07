using UnityEngine;

public class FallingWordPiece : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 velocity;
    private float gravity = -980f;
    private float rotationSpeed;
    private Canvas canvas;

    public void Initialize(Vector2 initialVelocity, float initialRotationSpeed)
    {
        rectTransform = GetComponent<RectTransform>();
        velocity = initialVelocity;
        rotationSpeed = initialRotationSpeed;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (rectTransform == null) return;

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;

        // Двигаем кусок
        rectTransform.anchoredPosition += velocity * Time.deltaTime;

        // Вращаем
        rectTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Проверяем выход за границы экрана
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            float bottomBound = -canvasRect.sizeDelta.y / 2f - 200f;

            if (rectTransform.anchoredPosition.y < bottomBound)
            {
                Destroy(gameObject);
            }
        }
    }
}
