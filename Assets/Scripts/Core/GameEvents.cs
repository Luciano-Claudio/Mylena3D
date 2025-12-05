using System;
using UnityEngine;

namespace Mylena
{
    /// <summary>
    /// Hub central de eventos do jogo.
    /// TODOS os controllers falam APENAS com essa classe.
    /// </summary>
    public static class GameEvents
    {
        // ========= INPUT DO PLAYER =========

        public static event Action<Vector2> OnMoveInput;
        public static event Action<Vector2> OnLookInput;

        public static event Action OnJumpPressed;
        public static event Action OnJumpReleased;

        public static event Action OnSprintStarted;
        public static event Action OnSprintCanceled;

        public static event Action OnCrouchPressed;
        public static event Action OnCrouchReleased;

        public static event Action OnAttackPressed;
        public static event Action OnInteractPressed;

        // ========= ESTADO DO MOVIMENTO =========

        public static event Action<Vector3> OnPlayerVelocityChanged;
        public static event Action<bool> OnPlayerGroundedChanged;
        public static event Action OnPlayerLanded;
        public static event Action OnPlayerStartedFalling;

        // ========= MÉTODOS PÚBLICOS =========

        // --- Input ---
        public static void RaiseMoveInput(Vector2 value) => OnMoveInput?.Invoke(value);
        public static void RaiseLookInput(Vector2 value) => OnLookInput?.Invoke(value);

        public static void RaiseJumpPressed() => OnJumpPressed?.Invoke();
        public static void RaiseJumpReleased() => OnJumpReleased?.Invoke();

        public static void RaiseSprintStarted() => OnSprintStarted?.Invoke();
        public static void RaiseSprintCanceled() => OnSprintCanceled?.Invoke();

        public static void RaiseCrouchPressed() => OnCrouchPressed?.Invoke();
        public static void RaiseCrouchReleased() => OnCrouchReleased?.Invoke();

        public static void RaiseAttackPressed() => OnAttackPressed?.Invoke();
        public static void RaiseInteractPressed() => OnInteractPressed?.Invoke();

        // --- Movimento ---
        public static void RaisePlayerVelocityChanged(Vector3 velocity)
            => OnPlayerVelocityChanged?.Invoke(velocity);

        public static void RaisePlayerGroundedChanged(bool isGrounded)
            => OnPlayerGroundedChanged?.Invoke(isGrounded);

        public static void RaisePlayerLanded()
            => OnPlayerLanded?.Invoke();

        public static void RaisePlayerStartedFalling()
            => OnPlayerStartedFalling?.Invoke();
    }
}
