# MYLENA - Documentação Técnica

> Documentação técnica completa do projeto Mylena - Um jogo de plataforma 2.5D Metroidvania

![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow)
![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-blue)
![C%23](https://img.shields.io/badge/C%23-10.0-purple)
![URP](https://img.shields.io/badge/URP-14.0-green)

---

## 📚 Navegação Rápida

### 🎯 Visão Geral
- [Sobre o Projeto](pages/overview.md)
- [Arquitetura Geral](pages/architecture.md)
- [Estrutura de Pastas](pages/folder-structure.md)
- [Fluxo de Dados](pages/data-flow.md)

### 🎮 Sistemas Core
- [Sistema de Input](pages/input-system.md)
- [Sistema de Eventos](pages/event-system.md)
- [Sistema de Movimento](pages/movement-system.md)
- [Sistema de Câmera](pages/camera-system.md)
- [Sistema de Animação](pages/animation-system.md)

### 🔧 Componentes
- [PlayerController](pages/components/player-controller.md)
- [PlayerInputController](pages/components/player-input-controller.md)
- [PlayerMovementController](pages/components/player-movement-controller.md)
- [PlayerAnimationController](pages/components/player-animation-controller.md)
- [GlobalVariables](pages/components/global-variables.md)

### 📖 Guias
- [Guia de Instalação](pages/guides/installation.md)
- [Guia de Contribuição](pages/guides/contributing.md)
- [Padrões de Código](pages/guides/coding-standards.md)
- [Debugging e Testes](pages/guides/debugging.md)

### 🚀 Sprint Atual
- [Sprint 1 - Overview](pages/sprints/sprint1-overview.md)
- [Sprint 1 - Phase 1](pages/sprints/sprint1-phase1.md)
- [Roadmap](pages/sprints/roadmap.md)

### 📝 Referências
- [Glossário](pages/glossary.md)
- [FAQ](pages/faq.md)
- [Changelog](pages/changelog.md)

---

## 🎯 Sobre o Projeto

**MYLENA** é um jogo de plataforma 2.5D que combina mecânicas precisas de movimento inspiradas em **Ori** e **Celeste** com rotação de câmera em pontos específicos inspirada em **FEZ**, estrutura metroidvania leve e narrativa emocional sobre trauma e escolhas morais.

### Pilares de Design

1. **Movimento Expressivo** - Controle responsivo, preciso e satisfatório
2. **Perspectiva como Puzzle** - Rotação de câmera revela caminhos ocultos
3. **Escolhas que Importam** - Sistema de moralidade ramificado
4. **Conexão Emocional** - Relação com o pet guia a jornada

---

## 🏗️ Arquitetura High-Level

```
┌─────────────────────────────────────────────────────────────┐
│                        INPUT LAYER                          │
│  ┌────────────────────────────────────────────────────┐    │
│  │ Unity New Input System → PlayerInputActions.cs     │    │
│  └────────────────────────────────────────────────────┘    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│                      CONTROLLER LAYER                       │
│  ┌────────────────────────────────────────────────────┐    │
│  │           PlayerInputController.cs                  │    │
│  │  • Lê Input Actions                                │    │
│  │  • Dispara GameEvents                              │    │
│  └────────────────────────────────────────────────────┘    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│                       EVENT LAYER                           │
│  ┌────────────────────────────────────────────────────┐    │
│  │               GameEvents.cs (Static)                │    │
│  │  • Hub centralizado de eventos                     │    │
│  │  • Desacopla sistemas                              │    │
│  └────────────────────────────────────────────────────┘    │
└────────┬──────────────────────────────────┬─────────────────┘
         ↓                                   ↓
┌──────────────────────────┐    ┌──────────────────────────┐
│   LOGIC LAYER            │    │   PRESENTATION LAYER     │
│  PlayerMovementController│    │  PlayerAnimationController│
│  • Física                │    │  • Animações             │
│  • Rigidbody             │    │  • Feedback Visual       │
│  • Colisões              │    │                          │
└──────────────────────────┘    └──────────────────────────┘
         ↓                                   ↓
┌─────────────────────────────────────────────────────────────┐
│                      ORCHESTRATION LAYER                    │
│  ┌────────────────────────────────────────────────────┐    │
│  │             PlayerController.cs                     │    │
│  │  • Coordena sub-controllers                        │    │
│  │  • Gerencia estado global (CanControl)            │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

---

## ⚙️ Stack Tecnológica

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| **Unity** | 2022.3 LTS | Game Engine |
| **URP** | 14.0+ | Rendering Pipeline |
| **C#** | 10.0 | Linguagem Principal |
| **New Input System** | 1.7+ | Input Management |
| **Cinemachine** | 2.9+ | Camera System |
| **ProBuilder** | 5.0+ | Level Prototyping |
| **Git** | 2.40+ | Version Control |

---

## 🚀 Quick Start

```bash
# 1. Clone o repositório
git clone https://github.com/seu-usuario/mylena.git

# 2. Abra no Unity Hub
# Unity 2022.3 LTS ou superior

# 3. Instale dependências
# Package Manager → Input System, Cinemachine

# 4. Abra a cena principal
Assets/_Mylena/Scenes/MainScene.unity

# 5. Play!
```

---

## 📊 Status do Projeto

### Sprint 1 - Phase 1 ✅ (Concluída)
- [x] Setup Unity + URP
- [x] New Input System configurado
- [x] Event System implementado
- [x] Core Classes Architecture
- [x] Player Movement Base
- [x] Cinemachine Camera
- [x] Test Map

### Sprint 1 - Phase 2 🚧 (Em Progresso)
- [ ] Mecânicas avançadas (Dash, Wall Climb)
- [ ] Sistema de Rotação de Câmera
- [ ] Polish de Movimento (Coyote Time, Jump Buffer)
- [ ] Efeitos Visuais

---

## 👥 Equipe

- **Desenvolvedor Solo**: Luciano Claudio
- **Engine**: Unity
- **Metodologia**: Sprints de 2 semanas

---

## 📄 Licença

Este projeto é de propriedade privada e ainda não possui licença pública definida.

---

## 🔗 Links Úteis

- [GitHub Repository](#)
- [Trello Board](#)
- [Art References](#)
- [Game Design Document](https://github.com/seu-usuario/mylena/blob/main/GDD.md)

---

**Última atualização:** Dezembro 2024  
**Versão da Documentação:** 1.0.0
