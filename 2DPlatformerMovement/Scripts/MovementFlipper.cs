using UnityEngine;

namespace CyberBuggy.Movement
{
    public class MovementFlipper : MonoBehaviour
    {
        [SerializeField] private PlatformerMovement _platformerMovement;

        [SerializeField] private Transform _transformToFlip;

        private bool _isFlipped;

        private void Update()
        {
            var wasFlipped = _isFlipped;
            _isFlipped = _platformerMovement.Velocity.x < 0;

            if (wasFlipped == _isFlipped)
                return;
            
            var localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }
}
