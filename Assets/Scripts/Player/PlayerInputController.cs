using UnityEngine;
using UnityEngine.InputSystem;
using Mylena;
using Mylena.Input;

namespace Mylena.Player
{
    /// <summary>
    /// ÚNICA classe que conversa com o Unity New Input System.
    /// Lê ações do PlayerInputActions e dispara eventos no GameEvents.
    /// 
    /// VERSÃO CORRIGIDA - Dezembro 2024
    /// Melhorias implementadas:
    /// - ✅ Removidas lambdas (zero GC allocation)
    /// - ✅ Removida linha redundante no OnDisable
    /// - ✅ Métodos específicos para cada callback
    /// - ✅ Melhor organização com regions
    /// - ✅ Documentação completa
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        #region Private Fields

        private PlayerInputActions _actions;
        private PlayerInputActions.PlayerActions _player;

        // Cache opcional para debug
        private Vector2 _currentMove;
        private Vector2 _currentLook;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeInputActions();
        }

        private void OnEnable()
        {
            EnableInputActions();
            SubscribeToInputEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromInputEvents();
            DisableInputActions();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializa o PlayerInputActions e extrai o action map "Player".
        /// </summary>
        private void InitializeInputActions()
        {
            _actions = new PlayerInputActions();
            _player = _actions.Player;
        }

        /// <summary>
        /// Habilita o action map "Player".
        /// </summary>
        private void EnableInputActions()
        {
            _player.Enable();
        }

        /// <summary>
        /// Desabilita o action map "Player".
        /// </summary>
        private void DisableInputActions()
        {
            _player.Disable(); // ✅ CORREÇÃO: Removida linha redundante _player.Enable()
        }

        #endregion

        #region Event Subscription

        /// <summary>
        /// Inscreve-se em todos os input actions.
        /// ✅ CORREÇÃO: Usa métodos específicos ao invés de lambdas (zero GC)
        /// </summary>
        private void SubscribeToInputEvents()
        {
            // MOVE (continuous input)
            _player.Move.performed += OnMovePerformed;
            _player.Move.canceled += OnMoveCanceled;

            // LOOK (continuous input)
            _player.Look.performed += OnLookPerformed;
            _player.Look.canceled += OnLookCanceled;

            // JUMP (button)
            _player.Jump.started += OnJumpStarted;
            _player.Jump.canceled += OnJumpCanceled;

            // SPRINT (hold button)
            _player.Sprint.started += OnSprintStarted;
            _player.Sprint.canceled += OnSprintCanceled;

            // CROUCH (hold button)
            _player.Crouch.started += OnCrouchStarted;
            _player.Crouch.canceled += OnCrouchCanceled;

            // ATTACK (button)
            _player.Attack.performed += OnAttackPerformed;

            // INTERACT (button)
            _player.Interact.performed += OnInteractPerformed;
        }

        /// <summary>
        /// Desinscreve-se de todos os input actions para evitar memory leaks.
        /// </summary>
        private void UnsubscribeFromInputEvents()
        {
            // MOVE
            _player.Move.performed -= OnMovePerformed;
            _player.Move.canceled -= OnMoveCanceled;

            // LOOK
            _player.Look.performed -= OnLookPerformed;
            _player.Look.canceled -= OnLookCanceled;

            // JUMP
            _player.Jump.started -= OnJumpStarted;
            _player.Jump.canceled -= OnJumpCanceled;

            // SPRINT
            _player.Sprint.started -= OnSprintStarted;
            _player.Sprint.canceled -= OnSprintCanceled;

            // CROUCH
            _player.Crouch.started -= OnCrouchStarted;
            _player.Crouch.canceled -= OnCrouchCanceled;

            // ATTACK
            _player.Attack.performed -= OnAttackPerformed;

            // INTERACT
            _player.Interact.performed -= OnInteractPerformed;
        }

        #endregion

        #region Input Action Callbacks - MOVE

        /// <summary>
        /// Callback quando input de movimento é executado (WASD, setas, analógico).
        /// </summary>
        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            _currentMove = ctx.ReadValue<Vector2>();
            GameEvents.RaiseMoveInput(_currentMove);
        }

        /// <summary>
        /// Callback quando input de movimento é cancelado (solta teclas).
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            _currentMove = Vector2.zero;
            GameEvents.RaiseMoveInput(_currentMove);
        }

        #endregion

        #region Input Action Callbacks - LOOK

        /// <summary>
        /// Callback quando input de câmera/mira é executado.
        /// Preparado para futuro sistema de câmera livre.
        /// </summary>
        private void OnLookPerformed(InputAction.CallbackContext ctx)
        {
            _currentLook = ctx.ReadValue<Vector2>();
            GameEvents.RaiseLookInput(_currentLook);
        }

        /// <summary>
        /// Callback quando input de câmera/mira é cancelado.
        /// </summary>
        private void OnLookCanceled(InputAction.CallbackContext ctx)
        {
            _currentLook = Vector2.zero;
            GameEvents.RaiseLookInput(_currentLook);
        }

        #endregion

        #region Input Action Callbacks - JUMP

        /// <summary>
        /// Callback quando botão de pulo é pressionado (Space).
        /// </summary>
        private void OnJumpStarted(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseJumpPressed();
        }

        /// <summary>
        /// Callback quando botão de pulo é solto.
        /// Útil para variable jump height.
        /// </summary>
        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseJumpReleased();
        }

        #endregion

        #region Input Action Callbacks - SPRINT

        /// <summary>
        /// Callback quando botão de sprint é pressionado (Shift).
        /// </summary>
        private void OnSprintStarted(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseSprintStarted();
        }

        /// <summary>
        /// Callback quando botão de sprint é solto.
        /// </summary>
        private void OnSprintCanceled(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseSprintCanceled();
        }

        #endregion

        #region Input Action Callbacks - CROUCH

        /// <summary>
        /// Callback quando botão de agachar é pressionado (Ctrl).
        /// </summary>
        private void OnCrouchStarted(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseCrouchPressed();
        }

        /// <summary>
        /// Callback quando botão de agachar é solto.
        /// </summary>
        private void OnCrouchCanceled(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseCrouchReleased();
        }

        #endregion

        #region Input Action Callbacks - ATTACK

        /// <summary>
        /// Callback quando botão de ataque é pressionado.
        /// </summary>
        private void OnAttackPerformed(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseAttackPressed();
        }

        #endregion

        #region Input Action Callbacks - INTERACT

        /// <summary>
        /// Callback quando botão de interação é pressionado (E).
        /// </summary>
        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            GameEvents.RaiseInteractPressed();
        }

        #endregion

        #region Debug Methods

#if UNITY_EDITOR
        /// <summary>
        /// [Editor Only] Retorna o último input de movimento para debug.
        /// </summary>
        public Vector2 GetCurrentMove() => _currentMove;

        /// <summary>
        /// [Editor Only] Retorna o último input de câmera para debug.
        /// </summary>
        public Vector2 GetCurrentLook() => _currentLook;
#endif

        #endregion
    }
}