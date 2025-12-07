using UnityEngine;
using Mylena;
using Mylena.Core;

namespace Mylena.Player
{
    /// <summary>
    /// VERSÃO FINAL CORRIGIDA - PlayerMovementController
    /// 
    /// BUG CRÍTICO RESOLVIDO:
    /// ApplyMovement e ApplyGravity agora usam variável intermediária
    /// para evitar sobrescrever velocidade com valor do frame anterior.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMovementController : MonoBehaviour
    {
        #region Constants

        private const float GROUND_STICK_FORCE = -2f;
        private const float MIN_INPUT_THRESHOLD = 0.01f;
        private const float INPUT_BUFFER_TIME = 0.15f;

        #endregion

        #region Serialized Fields

        [Header("Platform")]
        [Tooltip("Eixo principal da plataforma. Ex: (1,0,0) = eixo X, (0,0,1) = eixo Z.")]
        [SerializeField] private Vector3 platformAxis = Vector3.right;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask groundMask;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool enableDebugLogs = false;

        #endregion

        #region Private Fields

        private Rigidbody _rb;
        private CapsuleCollider _capsule;
        private float _moveInputX;
        private bool _isGrounded;
        private bool _wasGrounded;
        private bool _isSprinting;
        private int _currentJumps;
        private float _jumpBufferTimer;
        private GlobalVariables _globalVariables;

        // ✨ CRÍTICO: Variável intermediária para acumular mudanças de velocidade
        private Vector3 _targetVelocity;

        #endregion

        #region Properties

        private GlobalVariables GV
        {
            get
            {
                if (_globalVariables == null)
                {
                    _globalVariables = GlobalVariables.Instance;

                    if (_globalVariables == null)
                    {
                        Debug.LogError("[PlayerMovementController] GlobalVariables.Instance está null!", this);
                    }
                }
                return _globalVariables;
            }
        }

        public bool IsGrounded => _isGrounded;
        public Vector3 Velocity => _rb != null ? _rb.linearVelocity : Vector3.zero;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            SetupGroundCheck();
            ValidatePlatformAxis();
            ValidateGroundMask();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            CheckGround();
            UpdateJumpBuffer();
        }

        private void FixedUpdate()
        {
            // ✨ CORREÇÃO CRÍTICA: Usar variável intermediária
            // Inicia com velocidade atual
            _targetVelocity = _rb.linearVelocity;

            // Aplica movimento (modifica _targetVelocity)
            ApplyMovement();

            // Aplica gravidade (modifica _targetVelocity)
            ApplyGravity();

            // Aplica de uma vez
            _rb.linearVelocity = _targetVelocity;

            BroadcastVelocityChanged();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponent<CapsuleCollider>();

            if (_rb == null)
            {
                Debug.LogError("[PlayerMovementController] Rigidbody não encontrado!", this);
                return;
            }

            if (_capsule == null)
            {
                Debug.LogError("[PlayerMovementController] CapsuleCollider não encontrado!", this);
                return;
            }

            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void SetupGroundCheck()
        {
            if (groundCheck == null && _capsule != null)
            {
                var g = new GameObject("GroundCheck");
                g.transform.SetParent(transform);
                g.transform.localPosition = new Vector3(0f, -_capsule.height * 0.5f + 0.05f, 0f);
                groundCheck = g.transform;

                Debug.Log("[PlayerMovementController] GroundCheck criado automaticamente.", this);
            }
        }

        private void ValidatePlatformAxis()
        {
            if (platformAxis == Vector3.zero)
            {
                Debug.LogWarning("[PlayerMovementController] Platform Axis está zerado! Usando Vector3.right.", this);
                platformAxis = Vector3.right;
            }

            platformAxis.y = 0f;
            platformAxis.Normalize();
        }

        private void ValidateGroundMask()
        {
            if (groundMask.value == 0)
            {
                Debug.LogWarning("[PlayerMovementController] Ground Mask não configurado!", this);
            }
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            GameEvents.OnMoveInput += HandleMoveInput;
            GameEvents.OnJumpPressed += HandleJumpPressed;
            GameEvents.OnSprintStarted += HandleSprintStarted;
            GameEvents.OnSprintCanceled += HandleSprintCanceled;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnMoveInput -= HandleMoveInput;
            GameEvents.OnJumpPressed -= HandleJumpPressed;
            GameEvents.OnSprintStarted -= HandleSprintStarted;
            GameEvents.OnSprintCanceled -= HandleSprintCanceled;
        }

        #endregion

        #region Input Handlers

        private void HandleMoveInput(Vector2 input)
        {
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
            _jumpBufferTimer = INPUT_BUFFER_TIME;
            TryExecuteJump();
        }

        #endregion

        #region Movement Logic

        /// <summary>
        /// Aplica movimento horizontal modificando _targetVelocity.
        /// </summary>
        private void ApplyMovement()
        {
            if (GV == null || _rb == null) return;

            // Calcular velocidade alvo
            float maxSpeed = _isSprinting ? GV.sprintSpeed : GV.walkSpeed;
            float targetSpeed = maxSpeed * _moveInputX;

            // ✨ CORREÇÃO: Usar _targetVelocity ao invés de _rb.linearVelocity
            float currentSpeedAlongAxis = Vector3.Dot(_targetVelocity, platformAxis);

            // Escolher aceleração
            bool hasInput = Mathf.Abs(targetSpeed) > MIN_INPUT_THRESHOLD;
            float accel = _isGrounded ? GV.groundAcceleration : GV.airAcceleration;
            float decel = _isGrounded ? GV.groundDeceleration : GV.airDeceleration;
            float usedAccel = hasInput ? accel : decel;

            // Interpolar velocidade
            float newSpeedAlongAxis = Mathf.MoveTowards(
                currentSpeedAlongAxis,
                targetSpeed,
                usedAccel * Time.fixedDeltaTime
            );

            // ✨ CORREÇÃO: Modificar _targetVelocity
            Vector3 horizontalVel = platformAxis * newSpeedAlongAxis;
            _targetVelocity.x = horizontalVel.x;
            _targetVelocity.z = horizontalVel.z;
            // Y não é tocado aqui (fica para ApplyGravity)
        }

        /// <summary>
        /// Aplica gravidade modificando _targetVelocity.
        /// </summary>
        private void ApplyGravity()
        {
            if (GV == null || _rb == null) return;

            // ✨ CORREÇÃO: Usar _targetVelocity ao invés de _rb.linearVelocity
            if (_isGrounded && _targetVelocity.y <= 0f)
            {
                _currentJumps = 0;

                if (_jumpBufferTimer > 0f)
                {
                    TryExecuteJump();
                }
                else
                {
                    // Manter colado no chão
                    _targetVelocity.y = GROUND_STICK_FORCE;
                }
                return;
            }

            // Aplicar gravidade
            float gravity = GV.gravity;
            if (_targetVelocity.y < 0f)
            {
                gravity *= GV.fallMultiplier;
            }

            _targetVelocity.y += gravity * Time.fixedDeltaTime;
        }

        #endregion

        #region Jump System

        private void UpdateJumpBuffer()
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpBufferTimer -= Time.deltaTime;
            }
        }

        private void TryExecuteJump()
        {
            if (GV == null || _rb == null) return;

            if (_isGrounded || _currentJumps < GV.maxJumps)
            {
                // ✨ CORREÇÃO: Modificar _targetVelocity se estamos em FixedUpdate
                // Ou _rb.linearVelocity se não estamos
                if (Time.inFixedTimeStep)
                {
                    _targetVelocity.y = GV.jumpForce;
                }
                else
                {
                    var vel = _rb.linearVelocity;
                    vel.y = GV.jumpForce;
                    _rb.linearVelocity = vel;
                }

                _currentJumps++;
                _jumpBufferTimer = 0f;

                if (enableDebugLogs)
                {
                    Debug.Log($"<color=green>[Jump]</color> Executed! Jumps: {_currentJumps}/{GV.maxJumps}");
                }
            }
        }

        #endregion

        #region Ground Check

        private void CheckGround()
        {
            if (groundCheck == null || _rb == null) return;

            _wasGrounded = _isGrounded;
            int mask = groundMask.value != 0 ? groundMask.value : LayerMask.GetMask("Default");

            _isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                mask,
                QueryTriggerInteraction.Ignore
            );

            HandleGroundStateChanges();
        }

        private void HandleGroundStateChanges()
        {
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

        #region Event Broadcasting

        private void BroadcastVelocityChanged()
        {
            if (_rb == null) return;
            GameEvents.RaisePlayerVelocityChanged(_rb.linearVelocity);
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

            Gizmos.color = Color.cyan;
            Vector3 origin = transform.position;
            Vector3 axis = platformAxis;
            axis.y = 0f;

            if (axis == Vector3.zero)
                axis = Vector3.right;

            axis.Normalize();
            Gizmos.DrawLine(origin - axis * 2f, origin + axis * 2f);

            Vector3 arrowTip = origin + axis * 2f;
            Vector3 arrowLeft = arrowTip - axis * 0.3f + Vector3.forward * 0.2f;
            Vector3 arrowRight = arrowTip - axis * 0.3f - Vector3.forward * 0.2f;
            Gizmos.DrawLine(arrowTip, arrowLeft);
            Gizmos.DrawLine(arrowTip, arrowRight);
        }

        #endregion
    }
}