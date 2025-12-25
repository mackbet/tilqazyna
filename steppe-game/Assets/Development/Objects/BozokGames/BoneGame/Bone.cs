using System;
using System.Collections;
using Development.Objects.BozokGames.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Development.Managers.Bozok.BoneGame
{
    public class Bone : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float _dragPower = 10f;
        [SerializeField] protected float _deceleration = 2f;
        [SerializeField] private float _secToReturn = 4f;
        [SerializeField] private bool _returnOnOffScreen = true;
        
        private Canvas _parentCanvas;

        protected RectTransform _rectTransform;
        protected Vector2 _initPosition;
        protected Vector2 _releasePosition;
        protected Vector2 _velocity;
        protected bool _isFlying;
        private bool _isNeedToDropCoroutine;

        protected bool _canDrag = true;
        protected Coroutine _autoReturnCoroutine;

        public static event Action OnReturn;
        public static event Action OnMovingStarted;
        public static event Action<Vector2> OnMoving;
        public static event Action OnMovingEnded;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _initPosition = _rectTransform.anchoredPosition;
            _parentCanvas = GetComponentInParent<Canvas>();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!_canDrag)
            {
                return;
            }
            OnMovingStarted?.Invoke();
            _velocity = Vector2.zero;
            _isFlying = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!_canDrag)
            {
                return;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            _rectTransform.anchoredPosition = localPoint;
            OnMoving?.Invoke(localPoint);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!_canDrag)
            {
                return;
            }
            _canDrag = false;
            _releasePosition = _rectTransform.anchoredPosition;
            _velocity = (_initPosition - _releasePosition) * _dragPower;
            _isFlying = true;
            _autoReturnCoroutine = StartCoroutine(AutoReturn());
            OnMovingEnded?.Invoke();
        }

        protected virtual void Update()
        {
            if (_isFlying)
            {
                _rectTransform.anchoredPosition += _velocity * Time.deltaTime;

                _velocity = Vector2.Lerp(_velocity, Vector2.zero, _deceleration * Time.deltaTime);
                if (_returnOnOffScreen && IsOffScreen())
                {
                    // Останавливаем корутину автотоварата, если она запущена
                    if (_autoReturnCoroutine != null)
                    {
                        StopCoroutine(_autoReturnCoroutine);
                        _autoReturnCoroutine = null;
                    }
                    ResetObject();
                }
            }
        }

        private IEnumerator AutoReturn()
        {
            yield return new WaitForSeconds(_secToReturn);
            ResetObject();
        }

        public virtual void ResetObject()
        {
            if (_rectTransform == null) return;
            _isFlying = false;
            _rectTransform.anchoredPosition = _initPosition;
            _rectTransform.rotation = Quaternion.identity;
            _canDrag = true;
            OnReturn?.Invoke();
        }

        private void OnCollisionEnter2D(Collision2D target)
        {
            if (_isNeedToDropCoroutine)
            {
                StopCoroutine(_autoReturnCoroutine);
            }

            RectTransform targetRect = target.gameObject.GetComponent<RectTransform>();

            Vector2 collisionDirection = (targetRect.position - _rectTransform.position).normalized;

            BoneNonInteractable nonInteractable = target.gameObject.GetComponent<BoneNonInteractable>();
            if (nonInteractable != null)
            {
                float launchSpeed = _velocity.magnitude;
                nonInteractable.Launch(collisionDirection, launchSpeed);
            }
        }

        private void SetDropCoroutine(bool newValue) => _isNeedToDropCoroutine = newValue;

        private void OnEnable()
        {
            BozokBoneQuizManager.ChangeGameModeToDefault += SetDropCoroutine;
            TouchTranslator.OnBeginDragAction += OnBeginDrag;
            TouchTranslator.OnDragAction += OnDrag;
            TouchTranslator.OnEndDragAction += OnEndDrag;
        }

        protected virtual void OnDisable()
        {
            BozokBoneQuizManager.ChangeGameModeToDefault -= SetDropCoroutine;
            TouchTranslator.OnBeginDragAction -= OnBeginDrag;
            TouchTranslator.OnDragAction -= OnDrag;
            TouchTranslator.OnEndDragAction -= OnEndDrag;
            ResetObject();
        }
        
        private bool IsOffScreen()
        {
            Vector3 worldPos = _rectTransform.position;
            Camera cam = null;
            if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = _parentCanvas.worldCamera;
                // Если Canvas в ScreenSpace-Camera или WorldSpace, то используем worldCamera.
            }
            // Для ScreenSpace-Overlay camera аргумент null подходит.
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

            // Проверяем, вышла ли точка за пределы экрана
            if (screenPos.x < 0f || screenPos.x > Screen.width ||
                screenPos.y < 0f || screenPos.y > Screen.height)
            {
                return true;
            }
            return false;
        }
    }
}