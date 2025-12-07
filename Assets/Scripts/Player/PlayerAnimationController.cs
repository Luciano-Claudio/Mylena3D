using UnityEngine;
using Mylena;

namespace Mylena.Player
{
    /// <summary>
    /// Controlador de animações do player.
    /// Escuta eventos de estado e atualiza o Animator.
    /// 
    /// VERSÃO CORRIGIDA - Dezembro 2024
    /// Melhorias implementadas:
    /// - ✅ Trigger "Land" correto para aterrissagem
    /// - ✅ Damping de velocidade melhorado
    /// - ✅ Documentação aprimorada
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Config")]
        [SerializeField] private float speedDampTime = 0.1f;

        #endregion

        #region Private Fields

        private Animator _animator;
        private float _currentSpeed;
        private float _speedVelocity; // Para SmoothDamp

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogError("[PlayerAnimationController] Animator não encontrado!", this);
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Subscription

        /// <summary>
        /// Inscreve-se em eventos de estado do player.
        /// </summary>
        private void SubscribeToEvents()
        {
            // Estado de movimento
            GameEvents.OnPlayerVelocityChanged += HandleVelocityChanged;
            GameEvents.OnPlayerGroundedChanged += HandleGroundedChanged;
            GameEvents.OnPlayerStartedFalling += HandleStartedFalling;
            GameEvents.OnPlayerLanded += HandleLanded;

            // Input (para triggers imediatos)
            GameEvents.OnJumpPressed += HandleJumpPressed;
            GameEvents.OnAttackPressed += HandleAttackPressed;
            GameEvents.OnCrouchPressed += HandleCrouchPressed;
            GameEvents.OnCrouchReleased += HandleCrouchReleased;
        }

        /// <summary>
        /// Desinscreve-se de eventos para evitar memory leaks.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // Estado de movimento
            GameEvents.OnPlayerVelocityChanged -= HandleVelocityChanged;
            GameEvents.OnPlayerGroundedChanged -= HandleGroundedChanged;
            GameEvents.OnPlayerStartedFalling -= HandleStartedFalling;
            GameEvents.OnPlayerLanded -= HandleLanded;

            // Input
            GameEvents.OnJumpPressed -= HandleJumpPressed;
            GameEvents.OnAttackPressed -= HandleAttackPressed;
            GameEvents.OnCrouchPressed -= HandleCrouchPressed;
            GameEvents.OnCrouchReleased -= HandleCrouchReleased;
        }

        #endregion

        #region Event Handlers - Estado

        /// <summary>
        /// Atualiza parâmetro de velocidade do Animator com damping suave.
        /// </summary>
        private void HandleVelocityChanged(Vector3 vel)
        {
            if (_animator == null) return;

            // Calcular velocidade planar (sem Y)
            float planarSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;

            // ✅ MELHORADO: Usar SmoothDamp para transição mais natural
            _currentSpeed = Mathf.SmoothDamp(
                _currentSpeed,
                planarSpeed,
                ref _speedVelocity,
                speedDampTime
            );

            _animator.SetFloat("Speed", _currentSpeed);
        }

        /// <summary>
        /// Atualiza parâmetro de grounded do Animator.
        /// </summary>
        private void HandleGroundedChanged(bool grounded)
        {
            if (_animator == null) return;

            _animator.SetBool("IsGrounded", grounded);

            if (grounded)
            {
                _animator.SetBool("IsFalling", false);
            }
        }

        /// <summary>
        /// Marca que o player começou a cair.
        /// </summary>
        private void HandleStartedFalling()
        {
            if (_animator == null) return;

            _animator.SetBool("IsFalling", true);
        }

        /// <summary>
        /// Dispara trigger de aterrissagem quando player toca o chão.
        /// ✅ CORREÇÃO: Usa trigger "Land" ao invés de "Jump"
        /// </summary>
        private void HandleLanded()
        {
            if (_animator == null) return;

            _animator.SetTrigger("Land"); // ✅ CORRIGIDO
        }

        #endregion

        #region Event Handlers - Input

        /// <summary>
        /// Dispara trigger de pulo quando botão é pressionado.
        /// </summary>
        private void HandleJumpPressed()
        {
            if (_animator == null) return;

            _animator.SetTrigger("Jump");
        }

        /// <summary>
        /// Dispara trigger de ataque.
        /// </summary>
        private void HandleAttackPressed()
        {
            if (_animator == null) return;

            _animator.SetTrigger("Attack");
        }

        /// <summary>
        /// Ativa estado de agachado.
        /// </summary>
        private void HandleCrouchPressed()
        {
            if (_animator == null) return;

            _animator.SetBool("Crouch", true);
        }

        /// <summary>
        /// Desativa estado de agachado.
        /// </summary>
        private void HandleCrouchReleased()
        {
            if (_animator == null) return;

            _animator.SetBool("Crouch", false);
        }

        #endregion

        #region Animator Parameters Reference

        /*
         * PARÂMETROS ESPERADOS NO ANIMATOR:
         * 
         * Float:
         * - "Speed" : Magnitude da velocidade horizontal (0 = parado, 1+ = andando/correndo)
         * 
         * Bool:
         * - "IsGrounded" : True se player está no chão
         * - "IsFalling" : True se player está caindo (não por pulo)
         * - "Crouch" : True se player está agachado
         * 
         * Trigger:
         * - "Jump" : Disparado quando player pula
         * - "Land" : Disparado quando player aterrissa
         * - "Attack" : Disparado quando player ataca
         * 
         * SETUP RECOMENDADO:
         * 1. Criar trigger "Land" no Animator Controller
         * 2. Adicionar transição: Fall → Land (condition: Land trigger)
         * 3. Adicionar transição: Land → Idle (exit time)
         */

        #endregion
    }
}