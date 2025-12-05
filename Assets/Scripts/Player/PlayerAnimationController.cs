using UnityEngine;
using Mylena;

namespace Mylena.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float speedDampTime = 0.1f;

        private Animator _animator;
        private float _currentSpeed;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerVelocityChanged += HandleVelocityChanged;
            GameEvents.OnPlayerGroundedChanged += HandleGroundedChanged;
            GameEvents.OnPlayerStartedFalling += HandleStartedFalling;
            GameEvents.OnPlayerLanded += HandleLanded;

            GameEvents.OnJumpPressed += HandleJumpPressed;
            GameEvents.OnAttackPressed += HandleAttackPressed;
            GameEvents.OnCrouchPressed += HandleCrouchPressed;
            GameEvents.OnCrouchReleased += HandleCrouchReleased;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerVelocityChanged -= HandleVelocityChanged;
            GameEvents.OnPlayerGroundedChanged -= HandleGroundedChanged;
            GameEvents.OnPlayerStartedFalling -= HandleStartedFalling;
            GameEvents.OnPlayerLanded -= HandleLanded;

            GameEvents.OnJumpPressed -= HandleJumpPressed;
            GameEvents.OnAttackPressed -= HandleAttackPressed;
            GameEvents.OnCrouchPressed -= HandleCrouchPressed;
            GameEvents.OnCrouchReleased -= HandleCrouchReleased;
        }

        private void HandleVelocityChanged(Vector3 vel)
        {
            float planarSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
            _currentSpeed = Mathf.Lerp(_currentSpeed, planarSpeed, 1f - Mathf.Exp(-speedDampTime * Time.deltaTime));
            _animator.SetFloat("Speed", _currentSpeed);
        }

        private void HandleGroundedChanged(bool grounded)
        {
            _animator.SetBool("IsGrounded", grounded);
            if (grounded)
            {
                _animator.SetBool("IsFalling", false);
            }
        }

        private void HandleStartedFalling()
        {
            _animator.SetBool("IsFalling", true);
        }

        private void HandleLanded()
        {
            _animator.SetTrigger("Jump"); // ou um trigger de "Land", se você tiver
        }

        private void HandleJumpPressed()
        {
            _animator.SetTrigger("Jump");
        }

        private void HandleAttackPressed()
        {
            _animator.SetTrigger("Attack");
        }

        private void HandleCrouchPressed()
        {
            _animator.SetBool("Crouch", true);
        }

        private void HandleCrouchReleased()
        {
            _animator.SetBool("Crouch", false);
        }
    }
}
