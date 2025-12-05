using UnityEngine;

namespace Mylena.Core
{
    /// <summary>
    /// Configurações globais do jogo (velocidades, gravidade, etc).
    /// Editável via Inspector.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GlobalVariables",
        menuName = "Mylena/Global Variables",
        order = 0)]
    public class GlobalVariables : ScriptableObject
    {
        // Instância única em runtime
        public static GlobalVariables Instance { get; private set; }

        // Chamado quando o asset é carregado
        private void OnEnable()
        {
            Instance = this;
        }

        // [OPCIONAL] Se quiser garantir que ele é carregado automaticamente
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoLoad()
        {
            if (Instance != null) return;

            // procura um asset chamado "GlobalVariables" na pasta Resources
            GlobalVariables asset = Resources.Load<GlobalVariables>("GlobalVariables");
            if (asset == null)
            {
                Debug.LogError("[GlobalVariables] Nenhum asset encontrado em Resources/GlobalVariables. " +
                               "Crie um asset via Create > Mylena > Global Variables e coloque em Resources.");
            }
            else
            {
                Instance = asset;
            }
        }

        // =====================
        //  MOVIMENTO
        // =====================
        [Header("Movimento")]
        public float walkSpeed = 6f;
        public float sprintSpeed = 8f;

        // Chão
        public float groundAcceleration = 60f;
        public float groundDeceleration = 70f;

        // Ar
        public float airAcceleration = 20f;
        public float airDeceleration = 10f;


        [Header("Pulo")]
        public float jumpForce = 8f;
        public int maxJumps = 1;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.1f;

        [Header("Gravidade")]
        public float gravity = -35f;
        public float fallMultiplier = 2f;

        // =====================
        //  CÂMERA
        // =====================
        [Header("Câmera")]
        public float cameraFollowSpeed = 10f;

        // =====================
        //  DEBUG
        // =====================
        [Header("Debug")]
        public bool showDebugInfo = true;
    }
}
