# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Unity 6 FPS game** project (TFG = Trabajo de Fin de Grado / Final Degree Project). It uses **Hexagonal Architecture (Ports & Adapters)** to separate game logic from Unity dependencies.

## Architecture

```
Assets/Scripts/
├── Domain/           # Pure C# — no UnityEngine, fully unit-testable
│   ├── WeaponDomain.cs
│   ├── PlayerVidaDomain.cs
│   └── PlayerExperienciaDomain.cs
├── Adapters/         # Unity bridge layer (MonoBehaviours)
│   ├── WeaponAdapter.cs
│   ├── BulletAdapter.cs
│   └── [UI adapters]
├── GameManager.cs    # Wires domain + adapters; future Firebase/n8n entry point
├── PlayerMovement.cs # FPS controller (movement, dash, grappling hook)
└── BarraVidaUI.cs / PanelNivelUI.cs  # UI adapters
```

**Key principle**: Domain layer contains zero `using UnityEngine` and communicates via typed `Action` events. Adapters subscribe to these events and call domain methods — never the reverse.

## Development Commands

- **Open in Unity Editor**: Open `TFG_Santi.sln` in Visual Studio or launch Unity Hub and open this directory as a project.
- **Build**: Use Unity's Build window (Ctrl+Shift+B).
- **Play Mode**: Press Play in the Unity Editor.

## Key Systems

### PlayerMovement (`PlayerMovement.cs`)
- Unity 6 new Input System (code-generated actions, no `.inputactions` asset).
- WASD movement via `CharacterController`, FPS camera with mouse look, gravity, double jump.
- **Dash**: Left Shift — velocity impulse with duration and cooldown (IEnumerator coroutine).
- **Grappling Hook**: Right mouse button — Raycast from camera center, flies toward hit point. Visual cable via `LineRenderer` with configurable `grappleOrigin` transform (child of camera/hand).
- Priority: grapple > dash > normal movement.

### Weapon System
- `WeaponDomain` (pure C#): manages ammo, fire rate cooldown, reload timer. Emits typed events for all state changes.
- `WeaponAdapter` (MonoBehaviour): reads input, calls domain methods, handles raycast-from-crosshair aiming (bullet direction = where center screen points, not firePoint forward).
- `BulletAdapter` (MonoBehaviour, `RequireComponent(typeof(Rigidbody))`): manages projectile lifecycle (auto-destroy by lifetime or on collision), spawns impact effects.

### RPG Systems (Domain layer)
- `PlayerVidaDomain`: HP management with damage/healing, `SetVidaMaxima()` for level-up scaling.
- `PlayerExperienciaDomain`: XP with automatic level-up loop, emits `OnNivelSubido(int nivel, float nuevaVidaMaxima)` for cross-domain sync.
- `GameManager` wires these two domains and updates `PlayerVidaDomain` when level increases.

### UI Adapters
- `BarraVidaUI`: subscribes to `PlayerVidaDomain.OnVidaCambiada`, updates `Image.fillAmount`.
- `PanelNivelUI`: subscribes to `PlayerExperienciaDomain.OnExperienciaCambiada`, generates its own white `Sprite` at runtime to avoid 9-slice artifact on `Image.Type.Filled`.

## Input System Notes
- Uses `UnityEngine.InputSystem` (new API). Direct reads via `Keyboard.current`, `Mouse.current`.
- No Input Action Asset file required — all actions created in code (`PlayerMovement.Awake()`).

## Testing
- Domain classes (`PlayerVidaDomain`, `PlayerExperienciaDomain`, `WeaponDomain`) are plain C# classes — instantiate and call methods directly. No Unity test framework needed for unit tests.
- No test directory exists yet.

## MCP Server (mi-servidor)

**Location**: `C:/Users/Santi/Documents/MCP_PROJECT/mi-servidor-mcp/index.ts`

Connected via global Claude Code config (`%APPDATA%/Claude/claude_desktop_config.json`). Protocol: stdio.

### Available Tools

| Tool | Description |
|------|-------------|
| `saludo_inicial` | Test connection |
| `guardar_progreso_unity` | Save `{id_jugador, nivel, puntos}` to Firestore collection `jugadores` |
| `leer_progreso_unity` | Read player data by `id_jugador` from Firestore |
| `notificar_evento_n8n` | POST to `localhost:5678/webhook-test/subida-nivel` with `{evento, jugador, detalles, fecha}` |
| `iniciar_n8n` | Spawn `npx n8n` in background (detached), sets `NODES_EXCLUDE: "[]"` |
| `hacer_commit_n8n` | POST to `localhost:5678/webhook-test/github-ops` to trigger git add/commit/push via n8n |
| `leer_errores_unity` | Read last 150 lines of `C:/Users/Santi/AppData/Local/Unity/Editor/Editor.log` |
| `leer_codigo_unity` | Read a `.cs` file by absolute path |
| `escribir_codigo_unity` | Write/overwrite a `.cs` file by absolute path |
| `ejecutar_comando_terminal` | Execute arbitrary Windows commands |

### Dependencies
- Firebase: requires `serviceAccountKey.json` at `C:/Users/Santi/Documents/MCP_PROJECT/mi-servidor-mcp/`
- Firebase project: `TFGDB`
- n8n: expects `localhost:5678`

### Security Note
`ejecutar_comando_terminal` allows arbitrary command execution — do not share the server config widely.

## Future Integration
- GameManager contains `[FUTURO]` comments marking where Firebase/cloud sync will be added via n8n MCP server (persist level, XP, HP to cloud).
- `notificar_evento_n8n` and `hacer_commit_n8n` are the primary integration points for n8n automation.
