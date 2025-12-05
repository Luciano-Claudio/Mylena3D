using UnityEngine;
using Mylena;
using Mylena.Core;

namespace Mylena.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMovementController : MonoBehaviour
    {
        [Header("Platform")]
        [Tooltip("Eixo principal da plataforma. Ex: (1,0,0) = eixo X, (0,0,1) = eixo Z.")]
        [SerializeField] private Vector3 platformAxis = Vector3.right;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask groundMask; // Layer do chão

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool logGroundChanges = false;
        [SerializeField] private bool logVelocity = false;

        private Rigidbody _rb;
        private CapsuleCollider _capsule;

        // input 1D (esquerda/direita)
        private float _moveInputX;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _isSprinting;

        // número de pulos já usados (para pulo duplo no futuro)
        private int _currentJumps;

        private GlobalVariables GV => GlobalVariables.Instance;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();

            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // cria ground check se não tiver
            if (groundCheck == null)
            {
                var g = new GameObject("GroundCheck");
                g.transform.SetParent(transform);
                g.transform.localPosition =
                    new Vector3(0f, -_capsule.height * 0.5f + 0.05f, 0f);
                groundCheck = g.transform;
            }

            // garante eixo de plataforma no plano XZ
            if (platformAxis == Vector3.zero)
                platformAxis = Vector3.right;

            platformAxis.y = 0f;
            platformAxis.Normalize();

            if (GV == null)
            {
                Debug.LogError("[PlayerMovementController] GlobalVariables.Instance está null. Coloque o asset de GlobalVariables na cena ou em Resources.");
            }
        }

        private void OnEnable()
        {
            GameEvents.OnMoveInput += HandleMoveInput;
            GameEvents.OnJumpPressed += HandleJumpPressed;
            GameEvents.OnSprintStarted += HandleSprintStarted;
            GameEvents.OnSprintCanceled += HandleSprintCanceled;
        }

        private void OnDisable()
        {
            GameEvents.OnMoveInput -= HandleMoveInput;
            GameEvents.OnJumpPressed -= HandleJumpPressed;
            GameEvents.OnSprintStarted -= HandleSprintStarted;
            GameEvents.OnSprintCanceled -= HandleSprintCanceled;
        }

        private void Update()
        {
            CheckGround();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyGravity();

            GameEvents.RaisePlayerVelocityChanged(_rb.linearVelocity);

            if (logVelocity)
            {
                Debug.Log($"[PlayerMovementController] vel = {_rb.linearVelocity}");
            }
        }

        #region Handlers de Input

        private void HandleMoveInput(Vector2 input)
        {
            // Plataforma: só eixo horizontal (A/D, setas, etc.)
            _moveInputX = input.x;
        }

        private void HandleSprintStarted()
        {
            _isSprinting = true;
        }

        private void HandleSprintCanceled()
        {
            _isSprinting = false;
        }

        private void HandleJumpPressed()
        {
            if (GV == null) return;

            if (_isGrounded || _currentJumps < GV.maxJumps)
            {
                var vel = _rb.linearVelocity;
                vel.y = GV.jumpForce;
                _rb.linearVelocity = vel;
                _currentJumps++;
            }
        }

        #endregion

        #region Movimento

        private void ApplyMovement()
        {
            if (GV == null) return;

            // Velocidade alvo (positiva ou negativa) ao longo do eixo
            float maxSpeed = _isSprinting ? GV.sprintSpeed : GV.walkSpeed;
            float targetSpeed = maxSpeed * _moveInputX; // negativo = esquerda, positivo = direita

            // Eixo da plataforma
            Vector3 axis = platformAxis;

            // Velocidade atual
            Vector3 currentVel = _rb.linearVelocity;
            float currentSpeedAlongAxis = Vector3.Dot(currentVel, axis); // componente ao longo do eixo

            // Escolhe aceleração/deceleração dependendo de chão/ar
            bool hasInput = Mathf.Abs(targetSpeed) > 0.01f;

            float accel = _isGrounded ? GV.groundAcceleration : GV.airAcceleration;
            float decel = _isGrounded ? GV.groundDeceleration : GV.airDeceleration;

            float usedAccel = hasInput ? accel : decel;

            float newSpeedAlongAxis = Mathf.MoveTowards(
                currentSpeedAlongAxis,
                targetSpeed,
                usedAccel * Time.fixedDeltaTime
            );

            // Monta nova velocidade
            Vector3 newVel = axis * newSpeedAlongAxis;
            newVel.y = _rb.linearVelocity.y; // mantém Y (pulo/gravidade)

            _rb.linearVelocity = newVel;
        }

        private void ApplyGravity()
        {
            if (GV == null) return;

            if (_isGrounded && _rb.linearVelocity.y <= 0f)
            {
                // Mantém colado no chão e reseta pulos
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, -2f, _rb.linearVelocity.z);
                _currentJumps = 0;
                return;
            }

            float gravity = GV.gravity;
            if (_rb.linearVelocity.y < 0f)
            {
                gravity *= GV.fallMultiplier;
            }

            _rb.linearVelocity += Vector3.up * gravity * Time.fixedDeltaTime;
        }

        #endregion

        #region Ground Check

        private void CheckGround()
        {
            _wasGrounded = _isGrounded;

            // se não marcar nada no inspector, usa todas as layers
            int mask = groundMask.value != 0 ? groundMask.value : ~0;

            _isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                mask,
                QueryTriggerInteraction.Ignore
            );

            if (logGroundChanges && _isGrounded != _wasGrounded)
            {
                Debug.Log($"[PlayerMovementController] Grounded mudou: now={_isGrounded}");
            }

            if (_isGrounded != _wasGrounded)
            {
                GameEvents.RaisePlayerGroundedChanged(_isGrounded);
            }

            if (_isGrounded && !_wasGrounded)
            {
                GameEvents.RaisePlayerLanded();
            }

            if (!_isGrounded && _wasGrounded && _rb.linearVelocity.y < 0f)
            {
                GameEvents.RaisePlayerStartedFalling();
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;

            if (groundCheck != null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }

            // desenha eixo da plataforma
            Gizmos.color = Color.cyan;
            Vector3 origin = transform.position;
            Vector3 axis = platformAxis;
            axis.y = 0f;
            if (axis == Vector3.zero) axis = Vector3.right;
            axis.Normalize();
            Gizmos.DrawLine(origin - axis, origin + axis);
        }

        #endregion
    }
}
