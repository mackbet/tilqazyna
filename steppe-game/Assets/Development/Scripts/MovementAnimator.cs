using UnityEngine;
using DG.Tweening;

public class MovementAnimator : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rocketRigidbody;
    [SerializeField] private float speed;
    [SerializeField] private float time;

    public void Animate()
    {
        Vector2 direction = rocketRigidbody.transform.up;
        DOTween.To(() => 0f, x =>
        {
            // Каждый кадр двигаем ракету вперед
            rocketRigidbody.MovePosition(rocketRigidbody.position + direction * speed * Time.deltaTime);
        }, 1f, time)
        .SetEase(Ease.Linear);
    }
}