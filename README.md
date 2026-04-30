# Cosmostar

Cosmostar is a Unity-ready portrait roguelite vertical slice built around one-thumb survival combat, authored missions, fair F2P hooks, and a neon sci-fi presentation.

## What is implemented

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

## Open in Unity

1. Open `UnityProject/` in Unity `2022.3 LTS` or newer.
2. Let Unity import packages.
3. The editor seeder will create the starter content asset and `Boot.unity` automatically if they are missing.
4. Open `Assets/Scenes/Boot.unity` and press Play.

## Verify core logic and content locally

```powershell
dotnet test .\tests\Cosmostar.Core.Tests\Cosmostar.Core.Tests.csproj
```

The test suite includes gameplay-core rules and validation that the default vertical-slice catalog is structurally valid.

## Notes

- The workspace started empty, so this implementation focuses on a production-lean scaffold with a playable vertical slice loop rather than a content-complete shipped game.
- Runtime visuals are code-driven placeholder neon UI/gameplay art designed to be replaced by authored assets without changing the gameplay architecture.
