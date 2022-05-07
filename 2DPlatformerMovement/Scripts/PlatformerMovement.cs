using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace CyberBuggy.Movement
{
    
    public class PlatformerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;

        [Header("Walking Settings")]

        [SerializeField] private float _moveSpeed = 5;
        [SerializeField] private float _maxSpeed = 20;
        [SerializeField] private float _accelPower = 2;
        [SerializeField] private float _decelPower = 5;
        [SerializeField] private float _turningPower = 5;

        [Header("Jump Settings")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private float _groundCheckRadius = 1;
        [SerializeField] private LayerMask _groundCheckLayer;
        [SerializeField] private float _jumpPower = 10;
        [SerializeField] private float _jumpingReleaseMultiplier = 0.5f;
        [SerializeField] private float _jumpBufferThreshold = 0.1f;
        [SerializeField] private float _coyoteTimeThreshold = 0.2f;

        [Header("Airborne Settings")]
        [SerializeField] private float _extraFallMultiplier = 0.7f;
        [SerializeField] private float _antiGravityApexThreshold = 0.1f;
        [SerializeField] private float _antiGravityApexMultiplier = 0.5f;
        [SerializeField] private float _airborneTurningPower = 10;
        [SerializeField] private float _maxFallSpeed = 20;

        [Header("Debug Settings")]

        [SerializeField] private bool _enableWalkingGizmos;
        [SerializeField] private bool _enableJumpingGizmos;
        public Vector2 Velocity {get => _rigidbody.velocity;}
        public bool IsGrounded {get => _isGrounded;}
        private Vector2 _currentDirection;
        private bool _isGrounded;
        private float _isGroundedBuffer;
        private bool _isHoldingJump;
        private float _isJumpingBuffer;
        private bool _hasJumpedLastFrame;

#if ENABLE_INPUT_SYSTEM
        public void SetWalkInput(CallbackContext ctx)
        {
            SetWalkInput(ctx.ReadValue<Vector2>());
        }
#endif
        public void SetWalkInput(Vector2 direction)
        {
            _currentDirection = direction;
        }
#if ENABLE_INPUT_SYSTEM
        public void SetJumpInput(CallbackContext ctx)
        {
            if(ctx.phase == InputActionPhase.Performed)
            {
                _isHoldingJump = true;  
                _isJumpingBuffer = _jumpBufferThreshold; 
            }
            else if(ctx.phase == InputActionPhase.Canceled)
                _isHoldingJump = false;
        }
#endif
        public void SetJumpInput(bool isHoldingJump)
        {
            _isHoldingJump = isHoldingJump;
        }
        private void Update()
        {
            CheckIfGrounded();
        }
        private void FixedUpdate()
        {
            ApplyWalkingForces();
            ApplyFallingForce();
        }
        private void CheckIfGrounded()
        {
            if (_isGroundedBuffer > 0 && _isJumpingBuffer > 0)
                ApplyJumpingForce();

            var groundCollider = Physics2D.OverlapCircle(_groundCheckPoint.position, _groundCheckRadius, _groundCheckLayer);
            _isGrounded = groundCollider != null;

            _isGroundedBuffer = _isGrounded ? _coyoteTimeThreshold : Mathf.Clamp(_isGroundedBuffer -= Time.deltaTime, 0, Mathf.Infinity);
            _isJumpingBuffer = Mathf.Clamp(_isJumpingBuffer -= Time.deltaTime, 0, Mathf.Infinity);

            if (!_isGrounded && _hasJumpedLastFrame)
                OnJumpTakeOff();

            void OnJumpTakeOff()
            {
                _isGroundedBuffer = 0;
                _isJumpingBuffer = 0;
                _hasJumpedLastFrame = false;
            }
        }
        private void ApplyWalkingForces()
        {
            float acceleration;
            float targetSpeed;

            var isMovingHorizontally = Mathf.Abs(_currentDirection.x) > Mathf.Epsilon;

            if(isMovingHorizontally)
            {
                targetSpeed = _moveSpeed * _currentDirection.x;

                var isMovingOpposite = Mathf.Sign(_currentDirection.x) != Mathf.Sign(_rigidbody.velocity.x);
                acceleration = isMovingOpposite ? _turningPower : _accelPower; 
                if(isMovingOpposite && !_isGrounded) acceleration = _airborneTurningPower;
            }
            else
            {
                targetSpeed = -(_rigidbody.velocity.x);
                acceleration = _decelPower;
            }
            _rigidbody.AddForce(targetSpeed * acceleration * _rigidbody.transform.right);

            var velocity = _rigidbody.velocity;
            velocity.x = Mathf.Clamp(_rigidbody.velocity.x, -_maxSpeed, _maxSpeed);
            _rigidbody.velocity = velocity;
        }

        private void ApplyJumpingForce()
        {
            ResetVerticalVelocity();
            _rigidbody.AddForce(_rigidbody.transform.up * _jumpPower, ForceMode2D.Impulse);
            _hasJumpedLastFrame = true;

            void ResetVerticalVelocity()
            {
                var velocity = _rigidbody.velocity;
                velocity.y = 0;
                _rigidbody.velocity = velocity;
            }
        }

        private void ApplyFallingForce()
        {
            if (_isGrounded)
                return;
            
            if (!_isHoldingJump && _rigidbody.velocity.y > 0)
                ApplyJumpCutForce();
            else if(Mathf.Abs(_rigidbody.velocity.y) < _antiGravityApexThreshold)
                ApplyAntiGravityApexForce();
            else if(_rigidbody.velocity.y < 0)
                ApplyExtraFallingForce();
            ApplyMaxFallSpeedClamp();

            void ApplyJumpCutForce() => _rigidbody.AddForce(Vector2.down * _rigidbody.velocity.y * (1 - _jumpingReleaseMultiplier));
            void ApplyExtraFallingForce() => _rigidbody.AddForce(Vector2.down * _rigidbody.velocity.y * (1 - _extraFallMultiplier));
            void ApplyAntiGravityApexForce() => _rigidbody.AddForce(Vector2.down * _rigidbody.velocity.y * (_antiGravityApexMultiplier - 1));
            void ApplyMaxFallSpeedClamp()
            {
                var velocity = _rigidbody.velocity;
                velocity.y = Mathf.Max(-_maxFallSpeed, velocity.y);
                _rigidbody.velocity = velocity;
            } 
        }
        private void OnDrawGizmosSelected()
        {
            if(_enableWalkingGizmos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(_rigidbody.transform.position, _currentDirection);
            }
            if(_enableJumpingGizmos)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);
            }
        }
    }
}
