using UnityEngine;
using UnityEngine.InputSystem;
using Mylena;
using Mylena.Input; // classe auto-gerada PlayerInputActions está nesse namespace

namespace Mylena.Player
{
    /// <summary>
    /// ÚNICA classe que conversa com o New Input System.
    /// Lê ações e dispara os eventos em GameEvents.
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerInputActions _actions;
        private PlayerInputActions.PlayerActions _player; // wrapper do action map "Player"

        // cache opcional (pra debug)
        private Vector2 _currentMove;
        private Vector2 _currentLook;

        private void Awake()
        {
            _actions = new PlayerInputActions();
            _player = _actions.Player; // isso é o map "Player" do asset
        }

        private void OnEnable()
        {
            _player.Enable();

            // MOVE
            _player.Move.performed += OnMovePerformed;
            _player.Move.canceled += OnMoveCanceled;

            // LOOK
            _player.Look.performed += OnLookPerformed;
            _player.Look.canceled += OnLookCanceled;

            // JUMP
            _player.Jump.started += ctx => GameEvents.RaiseJumpPressed();
            _player.Jump.canceled += ctx => GameEvents.RaiseJumpReleased();

            // SPRINT
            _player.Sprint.started += ctx => GameEvents.RaiseSprintStarted();
            _player.Sprint.canceled += ctx => GameEvents.RaiseSprintCanceled();

            // CROUCH
            _player.Crouch.started += ctx => GameEvents.RaiseCrouchPressed();
            _player.Crouch.canceled += ctx => GameEvents.RaiseCrouchReleased();

            // ATTACK
            _player.Attack.performed += ctx => GameEvents.RaiseAttackPressed();

            // INTERACT
            _player.Interact.performed += ctx => GameEvents.RaiseInteractPressed();
        }

        private void OnDisable()
        {
            // remover callbacks
            _player.Move.performed -= OnMovePerformed;
            _player.Move.canceled -= OnMoveCanceled;

            _player.Look.performed -= OnLookPerformed;
            _player.Look.canceled -= OnLookCanceled;

            _player.Enable(); // garante que está habilitado antes de desinscrever, se quiser
            _player.Disable();
        }

        #region Callbacks

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            _currentMove = ctx.ReadValue<Vector2>();
            GameEvents.RaiseMoveInput(_currentMove);
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            _currentMove = Vector2.zero;
            GameEvents.RaiseMoveInput(_currentMove);
        }

        private void OnLookPerformed(InputAction.CallbackContext ctx)
        {
            _currentLook = ctx.ReadValue<Vector2>();
            GameEvents.RaiseLookInput(_currentLook);
        }

        private void OnLookCanceled(InputAction.CallbackContext ctx)
        {
            _currentLook = Vector2.zero;
            GameEvents.RaiseLookInput(_currentLook);
        }

        #endregion
    }
}
