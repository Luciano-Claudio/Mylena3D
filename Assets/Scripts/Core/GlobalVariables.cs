using UnityEngine;

namespace Mylena.Core
{
    /// <summary>
    /// Configurações globais do jogo (velocidades, gravidade, etc).
    /// Editável via Inspector, compartilhado via Singleton.
    /// 
    /// VERSÃO MELHORADA - Dezembro 2024
    /// Melhorias implementadas:
    /// - ✅ Singleton mais robusto com validação
    /// - ✅ Melhor documentação dos parâmetros
    /// - ✅ Organização aprimorada
    /// </summary>
    [CreateAssetMenu(
        fileName = "GlobalVariables",
        menuName = "Mylena/Global Variables",
        order = 0)]
    public class GlobalVariables : ScriptableObject
    {
        #region Singleton

        private static GlobalVariables _instance;

        /// <summary>
        /// Instância única do GlobalVariables.
        /// Carrega automaticamente de Resources se necessário.
        /// </summary>
        public static GlobalVariables Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GlobalVariables>("GlobalVariables");

                    if (_instance == null)
                    {
                        Debug.LogError(
                            "[GlobalVariables] Asset não encontrado em 'Resources/GlobalVariables.asset'! " +
                            "Crie um via: Assets > Create > Mylena > Global Variables"
                        );
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Chamado quando o asset é carregado/habilitado.
        /// </summary>
        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogWarning(
                    $"[GlobalVariables] Múltiplos assets detectados! " +
                    $"Usando '{_instance.name}', ignorando '{name}'."
                );
            }
        }

        /// <summary>
        /// AutoLoad via RuntimeInitializeOnLoadMethod para garantir carregamento.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoLoad()
        {
            // Força acesso ao Instance para carregar o asset
            _ = Instance;
        }

        #endregion

        #region Movimento Horizontal

        [Header("Movimento Horizontal")]
        [Tooltip("Velocidade de caminhada (m/s)")]
        [Range(1f, 15f)]
        public float walkSpeed = 6f;

        [Tooltip("Velocidade de corrida/sprint (m/s)")]
        [Range(1f, 20f)]
        public float sprintSpeed = 8f;

        #endregion

        #region Aceleração - Chão

        [Header("Aceleração - No Chão")]
        [Tooltip("Aceleração ao começar a andar (m/s²). Valores altos = resposta instantânea")]
        [Range(10f, 200f)]
        public float groundAcceleration = 60f;

        [Tooltip("Desaceleração ao parar de andar (m/s²). Valores altos = parada rápida")]
        [Range(10f, 200f)]
        public float groundDeceleration = 70f;

        #endregion

        #region Aceleração - Ar

        [Header("Aceleração - No Ar")]
        [Tooltip("Aceleração no ar (m/s²). Menor que no chão para mais desafio")]
        [Range(5f, 100f)]
        public float airAcceleration = 20f;

        [Tooltip("Desaceleração no ar (m/s²). Menor que no chão para manter momentum")]
        [Range(5f, 100f)]
        public float airDeceleration = 10f;

        #endregion

        #region Sistema de Pulo

        [Header("Sistema de Pulo")]
        [Tooltip("Força inicial do pulo (m/s). Valores altos = pulo mais alto")]
        [Range(5f, 20f)]
        public float jumpForce = 8f;

        [Tooltip("Número máximo de pulos (1 = simples, 2 = duplo, etc)")]
        [Range(1, 3)]
        public int maxJumps = 1;

        [Tooltip("Coyote Time: tempo após sair da plataforma que ainda pode pular (segundos)")]
        [Range(0f, 0.3f)]
        public float coyoteTime = 0.1f;

        [Tooltip("Jump Buffer: tempo que input de pulo fica registrado antes de tocar chão (segundos)")]
        [Range(0f, 0.3f)]
        public float jumpBufferTime = 0.1f;

        #endregion

        #region Gravidade

        [Header("Gravidade")]
        [Tooltip("Gravidade customizada (m/s²). Valores negativos puxam para baixo. Unity padrão = -9.81")]
        [Range(-50f, -10f)]
        public float gravity = -35f;

        [Tooltip("Multiplicador de queda. Valores > 1 fazem cair mais rápido (mais responsivo)")]
        [Range(1f, 5f)]
        public float fallMultiplier = 2f;

        #endregion

        #region Câmera

        [Header("Câmera")]
        [Tooltip("Velocidade de follow da câmera (maior = mais rápido)")]
        [Range(1f, 30f)]
        public float cameraFollowSpeed = 10f;

        #endregion

        #region Debug

        [Header("Debug")]
        [Tooltip("Mostrar informações de debug na tela")]
        public bool showDebugInfo = true;

        #endregion

        #region Validação

        /// <summary>
        /// Validação dos valores no Inspector (Unity Editor only).
        /// </summary>
        private void OnValidate()
        {
            // Garantir que sprint é mais rápido que walk
            if (sprintSpeed <= walkSpeed)
            {
                Debug.LogWarning(
                    "[GlobalVariables] Sprint Speed deve ser maior que Walk Speed! Ajustando...",
                    this
                );
                sprintSpeed = walkSpeed * 1.5f;
            }

            // Garantir que gravidade é negativa
            if (gravity > 0f)
            {
                Debug.LogWarning(
                    "[GlobalVariables] Gravidade deve ser negativa! Ajustando...",
                    this
                );
                gravity = -Mathf.Abs(gravity);
            }

            // Garantir que aceleração no chão > ar
            if (groundAcceleration < airAcceleration)
            {
                Debug.LogWarning(
                    "[GlobalVariables] Aceleração no chão deve ser >= ar para controle melhor! Ajustando...",
                    this
                );
                groundAcceleration = airAcceleration * 1.5f;
            }
        }

        #endregion

        #region Presets

        /// <summary>
        /// Aplica preset de movimento "Responsivo" (estilo Celeste).
        /// </summary>
        [ContextMenu("Preset: Responsivo (Celeste)")]
        private void ApplyResponsivePreset()
        {
            walkSpeed = 5f;
            sprintSpeed = 7f;
            groundAcceleration = 80f;
            groundDeceleration = 90f;
            airAcceleration = 30f;
            airDeceleration = 15f;
            jumpForce = 9f;
            gravity = -40f;
            fallMultiplier = 2.5f;

            Debug.Log("[GlobalVariables] Preset 'Responsivo' aplicado!");
        }

        /// <summary>
        /// Aplica preset de movimento "Fluído" (estilo Ori).
        /// </summary>
        [ContextMenu("Preset: Fluído (Ori)")]
        private void ApplyFluidPreset()
        {
            walkSpeed = 7f;
            sprintSpeed = 10f;
            groundAcceleration = 50f;
            groundDeceleration = 60f;
            airAcceleration = 25f;
            airDeceleration = 12f;
            jumpForce = 8f;
            gravity = -30f;
            fallMultiplier = 2f;

            Debug.Log("[GlobalVariables] Preset 'Fluído' aplicado!");
        }

        /// <summary>
        /// Aplica preset de movimento "Pesado" (estilo Dark Souls).
        /// </summary>
        [ContextMenu("Preset: Pesado")]
        private void ApplyHeavyPreset()
        {
            walkSpeed = 4f;
            sprintSpeed = 6f;
            groundAcceleration = 30f;
            groundDeceleration = 40f;
            airAcceleration = 10f;
            airDeceleration = 5f;
            jumpForce = 7f;
            gravity = -25f;
            fallMultiplier = 1.5f;

            Debug.Log("[GlobalVariables] Preset 'Pesado' aplicado!");
        }

        #endregion
    }
}