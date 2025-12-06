# Sistema de Eventos

[← Voltar ao Índice](../index.md)

---

## 📡 Visão Geral

O **Sistema de Eventos** é o coração da comunicação no MYLENA. Implementado através da classe estática `GameEvents`, ele atua como um **Message Bus** centralizado que desacopla completamente os diferentes sistemas do jogo.

---

## 🎯 Propósito

### Problema que Resolve
Sem um sistema de eventos, teríamos acoplamento direto:

```csharp
// ❌ Acoplamento Ruim
public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private PlayerMovementController movementController;
    [SerializeField] private PlayerAnimationController animationController;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            movementController.Jump(); // Acoplamento direto!
            animationController.PlayJumpAnimation(); // Mais acoplamento!
        }
    }
}
```

**Problemas**:
- Difícil de testar isoladamente
- Mudanças em uma classe afetam outras
- Difícil adicionar novos sistemas
- Referências serializadas no Inspector (prone to errors)

### Solução com Eventos
```csharp
// ✅ Desacoplado com Eventos
public class PlayerInputController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameEvents.RaiseJumpPressed(); // Sem dependências!
        }
    }
}

public class PlayerMovementController : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.OnJumpPressed += HandleJump; // Subscribe
    }
    
    void HandleJump() { /* lógica */ }
}
```

---

## 🏗️ Estrutura do GameEvents

### Arquivo Completo
```csharp
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
        public static void RaisePlayerVelocityChanged(Vector3 velocity) 
            => OnPlayerVelocityChanged?.Invoke(velocity);
        public static void RaisePlayerGroundedChanged(bool isGrounded) 
            => OnPlayerGroundedChanged?.Invoke(isGrounded);
        public static void RaisePlayerLanded() => OnPlayerLanded?.Invoke();
        public static void RaisePlayerStartedFalling() => OnPlayerStartedFalling?.Invoke();
    }
}
```

---

## 📊 Categorias de Eventos

### 1. **Eventos de Input** (Input → Logic)
Disparados pelo `PlayerInputController` quando o jogador interage.

| Evento | Parâmetros | Quando Dispara |
|--------|------------|----------------|
| `OnMoveInput` | `Vector2` | WASD/Setas pressionados |
| `OnLookInput` | `Vector2` | Mouse move (futuro) |
| `OnJumpPressed` | - | Space pressionado |
| `OnJumpReleased` | - | Space solto |
| `OnSprintStarted` | - | Shift pressionado |
| `OnSprintCanceled` | - | Shift solto |
| `OnCrouchPressed` | - | Ctrl pressionado |
| `OnCrouchReleased` | - | Ctrl solto |
| `OnAttackPressed` | - | Mouse 0 pressionado |
| `OnInteractPressed` | - | E pressionado |

**Exemplo de uso**:
```csharp
// PlayerInputController dispara
private void OnMovePerformed(InputAction.CallbackContext ctx)
{
    Vector2 input = ctx.ReadValue<Vector2>();
    GameEvents.RaiseMoveInput(input); // Dispara evento
}

// PlayerMovementController escuta
private void OnEnable()
{
    GameEvents.OnMoveInput += HandleMoveInput;
}

private void HandleMoveInput(Vector2 input)
{
    _moveInputX = input.x; // Usa o valor
}
```

---

### 2. **Eventos de Estado** (Logic → Presentation)
Disparados por controllers de lógica quando o estado do player muda.

| Evento | Parâmetros | Quando Dispara |
|--------|------------|----------------|
| `OnPlayerVelocityChanged` | `Vector3` | Rigidbody.velocity muda |
| `OnPlayerGroundedChanged` | `bool` | Ground check detecta mudança |
| `OnPlayerLanded` | - | Player toca o chão após queda |
| `OnPlayerStartedFalling` | - | Player deixa chão (não por pulo) |

**Exemplo de uso**:
```csharp
// PlayerMovementController dispara
private void CheckGround()
{
    bool wasGrounded = _isGrounded;
    _isGrounded = Physics.CheckSphere(/*...*/);
    
    if (_isGrounded != wasGrounded)
    {
        GameEvents.RaisePlayerGroundedChanged(_isGrounded);
    }
    
    if (_isGrounded && !wasGrounded)
    {
        GameEvents.RaisePlayerLanded();
    }
}

// PlayerAnimationController escuta
private void OnEnable()
{
    GameEvents.OnPlayerLanded += HandleLanded;
}

private void HandleLanded()
{
    _animator.SetTrigger("Land");
}
```

---

## 🔄 Fluxos de Comunicação

### Fluxo 1: Input → Logic → State → Presentation

```
Jogador pressiona W
       ↓
Unity Input System detecta
       ↓
PlayerInputController.OnMovePerformed()
       ↓
GameEvents.RaiseMoveInput(Vector2.up)
       ↓
PlayerMovementController.HandleMoveInput()
       ↓
Aplica força no Rigidbody
       ↓
Velocity muda
       ↓
GameEvents.RaisePlayerVelocityChanged(newVelocity)
       ↓
PlayerAnimationController.HandleVelocityChanged()
       ↓
animator.SetFloat("Speed", velocity.magnitude)
```

---

### Fluxo 2: Logic → State → Multiple Listeners

```
PlayerMovementController detecta chão
       ↓
_isGrounded = true
       ↓
GameEvents.RaisePlayerGroundedChanged(true)
       ↓
    ┌───────────┴───────────┐
    ↓                       ↓
PlayerAnimationController  ParticleController (futuro)
    ↓                       ↓
SetBool("IsGrounded")     Spawn dust particles
```

**Múltiplos listeners** podem reagir ao mesmo evento!

---

## 🎓 Best Practices

### ✅ DO - Sempre Desinscrever
```csharp
private void OnEnable()
{
    GameEvents.OnJumpPressed += HandleJump;
}

private void OnDisable()
{
    GameEvents.OnJumpPressed -= HandleJump; // CRÍTICO!
}
```

**Motivo**: Evita memory leaks e chamadas em objetos destruídos.

---

### ✅ DO - Usar Null-Conditional Operator
```csharp
public static void RaiseJumpPressed()
{
    OnJumpPressed?.Invoke(); // Seguro se ninguém estiver escutando
}
```

---

### ✅ DO - Nomear Eventos de Forma Clara
```csharp
// ✅ Bom - Ação clara
public static event Action OnJumpPressed;

// ❌ Ruim - Ambíguo
public static event Action OnJump;
```

---

### ❌ DON'T - Passar Referências Pesadas
```csharp
// ❌ Ruim - Passa objeto inteiro
public static event Action<PlayerController> OnPlayerChanged;

// ✅ Melhor - Passa apenas dados necessários
public static event Action<Vector3, int> OnPlayerDataChanged;
```

---

### ❌ DON'T - Criar Ciclos de Eventos
```csharp
// ❌ CUIDADO - Pode criar loop infinito!
private void HandleVelocityChanged(Vector3 vel)
{
    // ...
    GameEvents.RaisePlayerVelocityChanged(vel); // Loop!
}
```

---

## 🧪 Debugging Eventos

### Técnica 1: Breakpoint no Evento
Coloque breakpoint em `GameEvents.Raise*()`:

```csharp
public static void RaiseJumpPressed()
{
    Debug.Log("[GameEvents] JumpPressed raised"); // ← Breakpoint aqui
    OnJumpPressed?.Invoke();
}
```

Veja call stack para descobrir quem disparou.

---

### Técnica 2: Log de Listeners
```csharp
private void OnEnable()
{
    GameEvents.OnJumpPressed += HandleJump;
    Debug.Log($"[{gameObject.name}] Subscribed to OnJumpPressed");
}

private void OnDisable()
{
    GameEvents.OnJumpPressed -= HandleJump;
    Debug.Log($"[{gameObject.name}] Unsubscribed from OnJumpPressed");
}
```

---

### Técnica 3: Event Monitor (Debug Tool)
```csharp
public class EventMonitor : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.OnJumpPressed += () => Debug.Log("⚡ Jump!");
        GameEvents.OnMoveInput += (v) => Debug.Log($"⚡ Move: {v}");
        // ... todos os eventos
    }
}
```

Anexe a um GameObject vazio na cena para monitorar tudo!

---

## 🚀 Expandindo o Sistema

### Adicionando Novos Eventos

**Exemplo: Adicionar evento de Dash**

1. **Adicionar delegate e evento**:
```csharp
// Em GameEvents.cs
public static event Action OnDashPressed;
public static event Action OnDashCompleted;
```

2. **Adicionar método Raise**:
```csharp
public static void RaiseDashPressed() => OnDashPressed?.Invoke();
public static void RaiseDashCompleted() => OnDashCompleted?.Invoke();
```

3. **Conectar Input**:
```csharp
// Em PlayerInputController.cs
_player.Dash.performed += ctx => GameEvents.RaiseDashPressed();
```

4. **Implementar Logic**:
```csharp
// Em PlayerMovementController.cs (ou novo DashController)
private void OnEnable()
{
    GameEvents.OnDashPressed += HandleDash;
}

private void HandleDash()
{
    // Lógica do dash
    StartCoroutine(DashCoroutine());
}

private IEnumerator DashCoroutine()
{
    // Aplica dash
    yield return new WaitForSeconds(dashDuration);
    GameEvents.RaiseDashCompleted(); // Notifica fim
}
```

5. **Adicionar Feedback Visual**:
```csharp
// Em PlayerAnimationController.cs
private void OnEnable()
{
    GameEvents.OnDashPressed += HandleDashPressed;
}

private void HandleDashPressed()
{
    _animator.SetTrigger("Dash");
}
```

---

## ⚡ Performance

### Overhead de Eventos
```csharp
// Teste de performance
Stopwatch sw = Stopwatch.StartNew();
for (int i = 0; i < 1_000_000; i++)
{
    GameEvents.RaiseJumpPressed();
}
sw.Stop();
Debug.Log($"1M invocations: {sw.ElapsedMilliseconds}ms");
```

**Resultado típico**: ~2-5ms para 1 milhão de invocações.  
**Conclusão**: Overhead negligível para jogos.

---

### Memory Allocation
```csharp
// ✅ Eventos estáticos não alocam no heap
GameEvents.RaiseJumpPressed(); // Zero GC allocation

// ⚠️ Cuidado com closures
GameEvents.OnJumpPressed += () => DoSomething(); // Aloca closure!

// ✅ Melhor usar método direto
GameEvents.OnJumpPressed += DoSomething;
```

---

## 🔮 Futuras Melhorias

### 1. Event Data Classes
Para eventos complexos:

```csharp
public class PlayerStateEventData
{
    public Vector3 Position;
    public Vector3 Velocity;
    public bool IsGrounded;
    public int Health;
}

public static event Action<PlayerStateEventData> OnPlayerStateChanged;
```

### 2. Event Queuing
Para eventos que devem ser processados em ordem:

```csharp
private static Queue<Action> _eventQueue = new Queue<Action>();

public static void QueueEvent(Action action)
{
    _eventQueue.Enqueue(action);
}

// Em GameManager.Update()
void ProcessEventQueue()
{
    while (_eventQueue.Count > 0)
    {
        _eventQueue.Dequeue()?.Invoke();
    }
}
```

### 3. Event Priorities
Para controlar ordem de execução:

```csharp
public enum EventPriority { High, Normal, Low }

public static void Subscribe(Action callback, EventPriority priority)
{
    // Implementar sistema de prioridades
}
```

---

## 📚 Referências

- [C# Events and Delegates](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)
- [Observer Pattern](https://refactoring.guru/design-patterns/observer)
- [Unity Event Best Practices](https://unity.com/how-to/unity-event-system-best-practices)

---

[← Voltar ao Índice](../index.md) | [Anterior: Arquitetura](architecture.md) | [Próximo: Sistema de Movimento →](movement-system.md)
