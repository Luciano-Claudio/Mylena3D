# Sistema de Movimento

[← Voltar ao Índice](../index.md)

---

## 🎮 Visão Geral

O **Sistema de Movimento** implementa a física e controle do player, seguindo os pilares de design do MYLENA: **movimento expressivo, responsivo e preciso** inspirado em jogos como **Ori** e **Celeste**.

---

## 🏗️ Arquitetura

### Componente Principal: PlayerMovementController

```
PlayerMovementController
├── Rigidbody (física)
├── CapsuleCollider (colisão)
├── GroundCheck Transform (detecção de chão)
└── GlobalVariables (configurações)
```

**Responsabilidades**:
- ✅ Aplicar movimento horizontal (andar/correr)
- ✅ Aplicar gravidade customizada
- ✅ Detectar ground state
- ✅ Implementar pulo (simples e duplo)
- ✅ Emitir eventos de estado (velocity, grounded, etc)

---

## 🎯 Mecânicas Implementadas

### 1. Movimento Horizontal (Plataforma 2.5D)

#### Conceito de "Eixo de Plataforma"
O jogo usa um **eixo principal** configurável para movimento:

```csharp
[SerializeField] private Vector3 platformAxis = Vector3.right; // (1,0,0)
```

**Por quê?**  
- Em plataformas 2.5D, o player se move ao longo de um eixo (ex: X ou Z)
- Permite rotação de câmera mantendo controle intuitivo
- Y é reservado para pulo/gravidade

#### Implementação

```csharp
private void ApplyMovement()
{
    // 1. Calcular velocidade alvo
    float maxSpeed = _isSprinting ? GV.sprintSpeed : GV.walkSpeed;
    float targetSpeed = maxSpeed * _moveInputX; // -1 a +1
    
    // 2. Velocidade atual ao longo do eixo
    Vector3 currentVel = _rb.linearVelocity;
    float currentSpeedAlongAxis = Vector3.Dot(currentVel, platformAxis);
    
    // 3. Escolher aceleração baseado em estado
    float accel = _isGrounded ? GV.groundAcceleration : GV.airAcceleration;
    float decel = _isGrounded ? GV.groundDeceleration : GV.airDeceleration;
    float usedAccel = (Mathf.Abs(targetSpeed) > 0.01f) ? accel : decel;
    
    // 4. Interpolar suavemente
    float newSpeed = Mathf.MoveTowards(
        currentSpeedAlongAxis, 
        targetSpeed, 
        usedAccel * Time.fixedDeltaTime
    );
    
    // 5. Aplicar ao rigidbody
    Vector3 newVel = platformAxis * newSpeed;
    newVel.y = _rb.linearVelocity.y; // Manter Y (gravidade)
    _rb.linearVelocity = newVel;
}
```

**Parâmetros Configuráveis** (via GlobalVariables):

| Parâmetro | Valor Padrão | Descrição |
|-----------|--------------|-----------|
| `walkSpeed` | 6 m/s | Velocidade de caminhada |
| `sprintSpeed` | 8 m/s | Velocidade de corrida |
| `groundAcceleration` | 60 m/s² | Aceleração no chão |
| `groundDeceleration` | 70 m/s² | Desaceleração no chão |
| `airAcceleration` | 20 m/s² | Aceleração no ar |
| `airDeceleration` | 10 m/s² | Desaceleração no ar |

---

## 🐛 Bug Crítico Resolvido - Dezembro 2024

### ⚠️ Problema Identificado

**Sintoma**: Movimento e gravidade do player estavam incorretos e inconsistentes.

**Causa Raiz**: Os métodos `ApplyMovement()` e `ApplyGravity()` modificavam `_rb.linearVelocity` diretamente em sequência, causando sobrescrita de valores.

#### Código Problemático (Versão Anterior)

```csharp
// ❌ BUG - FixedUpdate
private void FixedUpdate()
{
    ApplyMovement();  // Modifica velocity.x e velocity.z
    ApplyGravity();   // Pega velocity MODIFICADO e aplica Y
    
    BroadcastVelocityChanged();
}

private void ApplyMovement()
{
    // ... cálculos ...
    
    // PROBLEMA: Modifica diretamente _rb.linearVelocity
    var vel = _rb.linearVelocity;
    vel.x = newSpeedX;
    vel.z = newSpeedZ;
    _rb.linearVelocity = vel;
}

private void ApplyGravity()
{
    // PROBLEMA: Pega velocidade JÁ MODIFICADA por ApplyMovement
    var vel = _rb.linearVelocity;
    vel.y += gravity * Time.fixedDeltaTime;
    _rb.linearVelocity = vel;
}
```

#### O Que Estava Acontecendo?

```
Frame N:
1. ApplyMovement() define velocity = (5, -2, 0)   ← De frame anterior
2. ApplyGravity() lê velocity = (5, -2, 0)        ← Pega X/Z do MESMO frame
3. ApplyGravity() define velocity = (5, -4, 0)    ← Aplica gravidade

Resultado: Movimento horizontal e vertical MISTURADOS! 🔴
```

---

### ✅ Solução Implementada

**Estratégia**: Usar variável intermediária `_targetVelocity` para acumular todas as mudanças antes de aplicar de uma vez.

#### Código Corrigido (Versão Atual)

```csharp
// ✅ CORREÇÃO
private Vector3 _targetVelocity; // Nova variável de classe

private void FixedUpdate()
{
    // ✨ PASSO 1: Capturar estado atual
    _targetVelocity = _rb.linearVelocity;
    
    // ✨ PASSO 2: Aplicar movimento (modifica _targetVelocity)
    ApplyMovement();
    
    // ✨ PASSO 3: Aplicar gravidade (modifica _targetVelocity)
    ApplyGravity();
    
    // ✨ PASSO 4: Aplicar TODAS as mudanças de uma vez
    _rb.linearVelocity = _targetVelocity;
    
    BroadcastVelocityChanged();
}

private void ApplyMovement()
{
    if (GV == null || _rb == null) return;

    float maxSpeed = _isSprinting ? GV.sprintSpeed : GV.walkSpeed;
    float targetSpeed = maxSpeed * _moveInputX;

    // ✨ CRÍTICO: Usar _targetVelocity ao invés de _rb.linearVelocity
    float currentSpeedAlongAxis = Vector3.Dot(_targetVelocity, platformAxis);
    
    // ... cálculos de aceleração ...
    
    float newSpeedAlongAxis = Mathf.MoveTowards(
        currentSpeedAlongAxis,
        targetSpeed,
        usedAccel * Time.fixedDeltaTime
    );

    // ✨ CORREÇÃO: Modificar _targetVelocity (X e Z apenas)
    Vector3 horizontalVel = platformAxis * newSpeedAlongAxis;
    _targetVelocity.x = horizontalVel.x;
    _targetVelocity.z = horizontalVel.z;
    // Y não é tocado aqui (fica para ApplyGravity)
}

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

    // ✨ CORREÇÃO: Modificar _targetVelocity.y
    _targetVelocity.y += gravity * Time.fixedDeltaTime;
}
```

#### Fluxo Correto Agora

```
Frame N:
1. _targetVelocity = _rb.linearVelocity (captura: 3, -2, 0)
2. ApplyMovement() modifica _targetVelocity.x/z → (5, -2, 0)
3. ApplyGravity() modifica _targetVelocity.y → (5, -4, 0)
4. _rb.linearVelocity = _targetVelocity → Aplica (5, -4, 0)

Resultado: Movimento horizontal e vertical INDEPENDENTES! ✅
```

---

### 📊 Impacto da Correção

| Aspecto | Antes (Bug) | Depois (Corrigido) |
|---------|-------------|-------------------|
| **Física** | Inconsistente | Previsível ✅ |
| **Pulo** | Altura variável | Altura constante ✅ |
| **Movimento** | Errático | Suave ✅ |
| **Gravidade** | Aceleração estranha | Aceleração correta ✅ |

---

### 🎓 Lição Aprendida

**Princípio**: Quando múltiplas operações modificam o mesmo estado, use uma **variável intermediária** para acumular mudanças.

**Padrão Correto**:
```csharp
// 1. Capturar estado
var temp = currentState;

// 2. Modificar temp em múltiplos passos
ModifyX(temp);
ModifyY(temp);
ModifyZ(temp);

// 3. Aplicar de uma vez
currentState = temp;
```

**Anti-Padrão (evitar)**:
```csharp
// ❌ Modificar estado diretamente em cada passo
ModifyX(currentState); // currentState muda
ModifyY(currentState); // Usa valor JÁ modificado por ModifyX!
ModifyZ(currentState); // Usa valor JÁ modificado por ModifyY!
```

---

### 🔧 Como Testar a Correção

1. **Teste de Pulo**:
   ```
   - Pressione Space no chão
   - Player deve subir EXATAMENTE para a mesma altura toda vez
   - Não deve haver variação baseada em velocidade horizontal
   ```

2. **Teste de Movimento no Ar**:
   ```
   - Pressione A/D enquanto no ar
   - Movimento horizontal deve ser suave e consistente
   - Não deve afetar velocidade vertical (queda)
   ```

3. **Teste de Gravidade**:
   ```
   - Pular e soltar Space
   - Queda deve acelerar consistentemente
   - fallMultiplier deve ser visível (queda mais rápida que subida)
   ```

---


### 2. Gravidade Customizada

#### Por que não usar Unity Physics?
Unity's gravity padrão (-9.81 m/s²) é **realista**, mas não **satisfatória** para jogos de plataforma.

**Problemas**:
- Pulos parecem "flutuantes"
- Queda é lenta demais
- Difícil fazer level design vertical

**Solução: Gravidade Custom**
```csharp
private void ApplyGravity()
{
    // 1. Se no chão, manter colado
    if (_isGrounded && _rb.linearVelocity.y <= 0f)
    {
        _rb.linearVelocity = new Vector3(
            _rb.linearVelocity.x, 
            -2f, // Pequena força para baixo
            _rb.linearVelocity.z
        );
        _currentJumps = 0;
        return;
    }
    
    // 2. Aplicar gravidade (negativa = para baixo)
    float gravity = GV.gravity; // -35 m/s²
    
    // 3. Multiplicador de queda (acelera queda para responsividade)
    if (_rb.linearVelocity.y < 0f)
    {
        gravity *= GV.fallMultiplier; // 2x
    }
    
    // 4. Aplicar aceleração
    _rb.linearVelocity += Vector3.up * gravity * Time.fixedDeltaTime;
}
```

**Parâmetros**:

| Parâmetro | Valor | Descrição |
|-----------|-------|-----------|
| `gravity` | -35 m/s² | Gravidade base (mais forte que real) |
| `fallMultiplier` | 2.0 | Multiplica gravidade na queda |

**Resultado**: Pulos "snappy", quedas rápidas, sensação boa!

---

### 3. Sistema de Pulo

#### Pulo Simples
```csharp
private void HandleJumpPressed()
{
    if (_isGrounded || _currentJumps < GV.maxJumps)
    {
        // Aplicar força de pulo (substitui velocidade Y)
        var vel = _rb.linearVelocity;
        vel.y = GV.jumpForce; // 8 m/s
        _rb.linearVelocity = vel;
        
        _currentJumps++;
    }
}
```

**Por que substituir Y ao invés de adicionar?**  
- Garante altura consistente
- Evita "super pulos" por momentum

#### Pulo Duplo (Sistema de Contagem)
```csharp
// Ao tocar o chão, resetar contador
if (_isGrounded && _rb.linearVelocity.y <= 0f)
{
    _currentJumps = 0;
}

// Permitir pulo se tiver "charges"
if (_currentJumps < GV.maxJumps) // maxJumps = 1 (simples) ou 2 (duplo)
{
    // Pular
}
```

**Configuração** (via GlobalVariables):
```csharp
public int maxJumps = 1; // 1 = pulo simples, 2 = duplo, etc
public float jumpForce = 8f;
```

---

### 4. Ground Detection

#### Técnica: Sphere Cast
```csharp
private void CheckGround()
{
    _wasGrounded = _isGrounded;
    
    // OverlapSphere na posição do GroundCheck
    _isGrounded = Physics.CheckSphere(
        groundCheck.position,    // Posição abaixo do player
        groundCheckRadius,       // 0.3f (pequeno raio)
        groundMask,              // Layer "Ground"
        QueryTriggerInteraction.Ignore // Ignora triggers
    );
    
    // Detectar mudanças de estado
    if (_isGrounded != _wasGrounded)
    {
        GameEvents.RaisePlayerGroundedChanged(_isGrounded);
    }
    
    // Aterrissagem
    if (_isGrounded && !_wasGrounded)
    {
        GameEvents.RaisePlayerLanded();
    }
    
    // Início de queda (saiu do chão sem pular)
    if (!_isGrounded && _wasGrounded && _rb.linearVelocity.y < 0f)
    {
        GameEvents.RaisePlayerStartedFalling();
    }
}
```

**Setup do GroundCheck**:
```csharp
// No Awake(), criar automaticamente se não existir
if (groundCheck == null)
{
    var g = new GameObject("GroundCheck");
    g.transform.SetParent(transform);
    g.transform.localPosition = new Vector3(
        0f, 
        -_capsule.height * 0.5f + 0.05f, // Logo abaixo do capsule
        0f
    );
    groundCheck = g.transform;
}
```

**Visualização com Gizmos**:
```csharp
private void OnDrawGizmosSelected()
{
    if (groundCheck != null)
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
```

---

## 🎨 Diferencial: Air vs Ground Control

### Filosofia de Design
Jogos de plataforma modernos (Celeste, Ori) oferecem **controle diferente no ar vs chão**:

- **No Chão**: Resposta rápida, fácil mudar direção
- **No Ar**: Mais "momentum", controle reduzido

### Implementação

| Ação | No Chão | No Ar |
|------|---------|-------|
| **Acelerar** | 60 m/s² | 20 m/s² |
| **Frear** | 70 m/s² | 10 m/s² |

**Resultado**:
- Player responde **instantaneamente** no chão (satisfatório)
- Player tem **inércia** no ar (desafio, skill ceiling)

---

## 🔄 Fluxo de Execução

### Frame-by-Frame

```
Update() (60 FPS)
    └── CheckGround()
        ├── Physics.CheckSphere()
        ├── Detectar mudanças de estado
        └── Disparar eventos (OnGroundedChanged, OnLanded, etc)

FixedUpdate() (50 FPS)
    ├── ApplyMovement()
    │   ├── Calcular velocidade alvo
    │   ├── Interpolar atual → alvo
    │   └── Aplicar ao Rigidbody
    ├── ApplyGravity()
    │   ├── Se no chão: força -2 Y
    │   └── Se no ar: aplicar gravity * fallMultiplier
    └── GameEvents.RaisePlayerVelocityChanged(_rb.linearVelocity)
```

**Por que CheckGround no Update?**  
- Colisões são mais precisas em Update (antes de FixedUpdate)
- Evita "frames perdidos" de ground detection

---

## ⚙️ Configuração no Inspector

### PlayerMovementController

```
┌─────────────────────────────────────────┐
│ Player Movement Controller (Script)     │
├─────────────────────────────────────────┤
│ Platform                                │
│   Platform Axis: (1, 0, 0)             │ ← Eixo de movimento
├─────────────────────────────────────────┤
│ Ground Check                            │
│   Ground Check: [GroundCheck Transform] │
│   Ground Check Radius: 0.3              │
│   Ground Mask: Ground                   │ ← Layer
├─────────────────────────────────────────┤
│ Debug                                   │
│   ☑ Draw Gizmos                         │
│   ☐ Log Ground Changes                  │
│   ☐ Log Velocity                        │
└─────────────────────────────────────────┘
```

---

## 🧪 Debugging

### 1. Gizmos Visuais
```csharp
// Mostra GroundCheck
Gizmos.color = _isGrounded ? Color.green : Color.red;
Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

// Mostra eixo da plataforma
Gizmos.color = Color.cyan;
Gizmos.DrawLine(transform.position - platformAxis, transform.position + platformAxis);
```

### 2. Logs Opcionais
```csharp
[SerializeField] private bool logGroundChanges = false;
[SerializeField] private bool logVelocity = false;

if (logGroundChanges && _isGrounded != _wasGrounded)
{
    Debug.Log($"[Movement] Grounded: {_isGrounded}");
}

if (logVelocity)
{
    Debug.Log($"[Movement] Vel: {_rb.linearVelocity}");
}
```

### 3. Debug UI Overlay
```csharp
// Em DebugOverlay.cs
string info = $"Grounded: {_isGrounded}\n";
info += $"Velocity: {_rb.linearVelocity}\n";
info += $"Speed: {_rb.linearVelocity.magnitude:F1} m/s\n";
info += $"Jumps: {_currentJumps}/{GV.maxJumps}";
debugText.text = info;
```

---

## 🎓 Mecânicas Futuras (Roadmap)

### Sprint 2+

#### 1. Coyote Time
Permite pular por breve momento após sair da plataforma:

```csharp
private float _coyoteTimeCounter;

void Update()
{
    if (_isGrounded)
        _coyoteTimeCounter = GV.coyoteTime; // 0.1s
    else
        _coyoteTimeCounter -= Time.deltaTime;
}

bool CanJump()
{
    return _coyoteTimeCounter > 0f || _currentJumps < GV.maxJumps;
}
```

#### 2. Jump Buffering
Registra input de pulo mesmo antes de tocar chão:

```csharp
private float _jumpBufferCounter;

void HandleJumpPressed()
{
    _jumpBufferCounter = GV.jumpBufferTime; // 0.1s
}

void FixedUpdate()
{
    if (_jumpBufferCounter > 0f)
    {
        _jumpBufferCounter -= Time.fixedDeltaTime;
        
        if (_isGrounded)
        {
            Jump();
            _jumpBufferCounter = 0f;
        }
    }
}
```

#### 3. Variable Jump Height
Altura baseada em tempo de botão pressionado:

```csharp
private bool _isJumpHeld;

void HandleJumpPressed()
{
    _isJumpHeld = true;
    Jump();
}

void HandleJumpReleased()
{
    _isJumpHeld = false;
}

void ApplyGravity()
{
    float gravity = GV.gravity;
    
    // Se soltou botão, cair mais rápido
    if (!_isJumpHeld && _rb.linearVelocity.y > 0f)
    {
        gravity *= GV.jumpCutMultiplier; // 3x
    }
    
    _rb.linearVelocity += Vector3.up * gravity * Time.fixedDeltaTime;
}
```

#### 4. Wall Climb & Slide
Detectar parede, agarrar, escalar:

```csharp
private bool IsNearWall()
{
    return Physics.Raycast(
        transform.position, 
        platformAxis, 
        out RaycastHit hit, 
        wallCheckDistance, 
        wallMask
    );
}

private void HandleWallClimb()
{
    if (IsNearWall() && Input.GetKey(KeyCode.W))
    {
        _rb.linearVelocity = new Vector3(
            _rb.linearVelocity.x, 
            GV.wallClimbSpeed, 
            _rb.linearVelocity.z
        );
    }
}
```

---

## ⚡ Performance

### Otimizações Atuais
1. **FixedUpdate** para física (50 FPS constante)
2. **Rigidbody Interpolation** para smooth rendering
3. **Collision Detection Mode: Continuous** (evita tunneling)

### Profiling
```csharp
// Unity Profiler mostra:
PlayerMovementController.FixedUpdate: ~0.05ms
    ├── ApplyMovement: ~0.02ms
    ├── ApplyGravity: ~0.01ms
    └── Events: ~0.02ms
```

**Resultado**: < 1% do frame budget (60 FPS @ 16.67ms/frame)

---

## 📚 Referências

- [Celeste Movement Breakdown](https://maddythorson.medium.com/celeste-and-towerfall-physics-d24bd2ae0fc5)
- [Math for Game Programmers: Building a Better Jump](https://www.youtube.com/watch?v=hG9SzQxaCm8)
- [Unity Rigidbody Best Practices](https://docs.unity3d.com/Manual/RigidbodiesOverview.html)

---

[← Voltar ao Índice](../index.md) | [Anterior: Sistema de Eventos](event-system.md) | [Próximo: Sistema de Animação →](animation-system.md)