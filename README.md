# Cosmostar

Cosmostar is a Unity-ready portrait roguelite vertical slice built around one-thumb survival combat, authored missions, fair F2P hooks, and a neon sci-fi presentation.

The repository also contains an additive **Neon Sky Survivors** MVP prototype. That work now lives beside the original Cosmostar vertical-slice assets instead of replacing Unity project settings, scenes, packages, runtime scripts, or tests.

## What is implemented

### Cosmostar vertical slice

- A Unity project scaffold under `UnityProject/`
- A testable pure-C# gameplay core shared from the Unity source tree
- Content definitions for ships, weapons, abilities, upgrades, modules, waves, boss phases, missions, rewards, saves, and analytics
- Runtime services for save/profile persistence, analytics logging, rewarded-ad placeholders, and content loading
- Catalog validation for ships, weapons, abilities, upgrades, modules, enemies, waves, missions, and unlock-track data
- A code-driven runtime app controller that handles:
  - `MetaHub`
  - `MissionSelect`
  - `Run`
  - `Results`
- An editor seeder that creates:
  - `Assets/Resources/Cosmostar/VerticalSliceCatalog.asset`
  - `Assets/Scenes/Boot.unity`
- .NET tests for the non-Unity core logic

### Neon Sky Survivors MVP prototype

- `Instructions.md` — the full game design and agent instruction file. Completed foundation items are marked with ✅ near the relevant sections.
- `UnityProject/Assets/Scripts/Core/Models/NeonSkyModels.cs` — data models for equipment, stats, upgrades, enemies, waves, bosses, rewards, and saves.
- `UnityProject/Assets/Scripts/Core/Design/NeonSkySurvivorsBlueprints.cs` — the first data-driven MVP catalog with starting equipment, 24 equipment items, upgrades, enemies, waves, and bosses.
- `UnityProject/Assets/Scripts/Core/Systems/NeonEquipmentSystem.cs` — garage equipment foundations for starting loadout, stat calculation, equip/unequip, upgrades, and duplicate merges.
- `UnityProject/Assets/Scripts/Core/Systems/NeonRunTimelineSystem.cs` — 10-minute wave, warning, boss timing, and final victory helpers.
- `UnityProject/Assets/Scripts/Core/Systems/NeonRunGameplaySystem.cs` — Phase 1 core gameplay logic for run start, movement, dash trails, auto-fire, enemy spawning, XP, and level-up drafts.
- `src/NeonSkySurvivors.Core/NeonSkySurvivors.Core.csproj` — .NET core project that links the engine-neutral gameplay files.
- `tests/NeonSkySurvivors.Core.Tests/` — xUnit coverage for the Neon foundation and Phase 1 gameplay logic.
- `web/index.html` — runnable mobile-friendly browser shell for the garage, canvas arena, HUD, dash controls, level-up cards, and results screens.
- `web/gameCore.js` — browser gameplay core mirroring the MVP systems for the runnable web prototype.
- `package.json` — lightweight scripts for the browser-core tests and local static web server.

## Open in Unity

1. Open `UnityProject/` in Unity `2022.3 LTS` or newer.
2. Let Unity import packages.
3. The editor seeder will create the starter content asset and `Boot.unity` automatically if they are missing.
4. Open `Assets/Scenes/Boot.unity` and press Play.

## Run the web prototype

```bash
npm run start:web
```

Then open `http://localhost:4173`.

## Verify core logic and content locally

```powershell
dotnet test .\tests\Cosmostar.Core.Tests\Cosmostar.Core.Tests.csproj
dotnet test .\tests\NeonSkySurvivors.Core.Tests\NeonSkySurvivors.Core.Tests.csproj
```

The Cosmostar test suite includes gameplay-core rules and validation that the default vertical-slice catalog is structurally valid. The Neon test suite covers the additive MVP systems.

## Verify browser core

```bash
npm run test:web
```

## Notes

- The Cosmostar Unity scaffold remains intact so Unity can still import packages, open the boot scene, and run the original vertical-slice loop.
- Runtime visuals are code-driven placeholder neon UI/gameplay art designed to be replaced by authored assets without changing the gameplay architecture.
- Follow `Instructions.md` step by step for Neon Sky Survivors work. When a requirement is completed, add a ✅ progress mark near the relevant instruction before committing.
