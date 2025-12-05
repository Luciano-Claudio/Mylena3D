using UnityEngine;

namespace Mylena.Player
{
    /// <summary>
    /// Orquestrador do Player.
    /// Responsável por ligar/desligar sub-controllers e estado global do player.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Sub Controllers")]
        [SerializeField] private PlayerInputController _inputController;
        [SerializeField] private PlayerMovementController _movementController;
        [SerializeField] private PlayerAnimationController _animationController;

        [Header("Estado")]
        [SerializeField] private bool _canControl = true;

        public bool CanControl => _canControl;

        private void Awake()
        {
            // Auto-find se não vier preenchido
            if (_inputController == null)
                _inputController = GetComponent<PlayerInputController>();

            if (_movementController == null)
                _movementController = GetComponent<PlayerMovementController>();

            if (_animationController == null)
                _animationController = GetComponent<PlayerAnimationController>();
        }

        private void Start()
        {
            ApplyControlState();
        }

        public void SetCanControl(bool canControl)
        {
            _canControl = canControl;
            ApplyControlState();
        }

        private void ApplyControlState()
        {
            if (_inputController != null)
                _inputController.enabled = _canControl;

            if (_movementController != null)
                _movementController.enabled = _canControl;

            // animação geralmente pode continuar mesmo sem controle,
            // mas se quiser travar tudo, habilite/desabilite aqui também.
        }
    }
}
