using System;
using Development.Managers.Bozok.BoneGame;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Development.Objects.BozokGames.BallGame
{
    public sealed class Ball : Bone
    {
        public static event Action<int> OnBallThrown;

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!_canDrag)
            {
                return;
            }
            base.OnEndDrag(eventData);
            OnBallThrown?.Invoke(_initPosition.x - _releasePosition.x > 0 ? -1 : 1);
        }

        private void OnTriggerEnter2D(Collider2D other) => StopCoroutine(_autoReturnCoroutine);

        protected override void OnDisable() => ResetObject();
    }
}