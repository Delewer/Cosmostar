# Neon Sky Survivors

Neon Sky Survivors is a mobile-friendly roguelite survival shooter prototype. The player controls a fast neon aircraft, survives a 10-minute mission, collects XP from enemy waves, chooses temporary run upgrades, and improves permanent power through garage equipment parts.

## What is implemented

- Data-driven MVP catalog with starting equipment, 24 equipment items, in-run upgrades, enemies, waves, and bosses.
- Garage equipment foundations for starting loadout, stat calculation, equip/unequip, upgrades, and duplicate merges.
- 10-minute run timeline with wave warnings, boss spawns at 3:00, 6:00, 10:00, mini-bosses at 7:30 and 8:45, and final victory helpers.
- Core gameplay logic for run start, plane movement, dash trails, auto-fire, enemy spawning, enemy damage/death, XP shards, and level-up draft choices.
- Unity mobile Boot scene with portrait orientation, Garage start screen, touch movement, dash button, HUD, touch upgrade cards, results payout, Garage return flow, capped render pools, and Android package settings.
- Unity Android smoke APK build through `Tools/Neon Sky Survivors/Build Android Smoke APK`.

## Key Files

- `Instructions.md` - product/design instruction file and progress log.
- `UnityProject/Assets/Scripts/Core/Models/NeonSkyModels.cs` - equipment, stats, upgrades, enemies, waves, bosses, rewards, and save models.
- `UnityProject/Assets/Scripts/Core/Models/NeonGameplayModels.cs` - run state, vectors, projectiles, enemies, XP shards, and player state.
- `UnityProject/Assets/Scripts/Core/Design/NeonSkySurvivorsBlueprints.cs` - MVP catalog data.
- `UnityProject/Assets/Scripts/Core/Systems/NeonEquipmentSystem.cs` - garage equipment logic.
- `UnityProject/Assets/Scripts/Core/Systems/NeonRunTimelineSystem.cs` - wave and boss timing helpers.
- `UnityProject/Assets/Scripts/Core/Systems/NeonRunGameplaySystem.cs` - engine-neutral run gameplay logic.
- `UnityProject/Assets/Scripts/Runtime/App/NeonSkySurvivorsApp.cs` - Unity mobile runtime shell.
- `UnityProject/Assets/Scenes/Boot.unity` - Unity Boot scene for mobile play mode.
- `tests/NeonSkySurvivors.Core.Tests/` - .NET headless test coverage for the Unity game-logic core.

## Open In Unity

1. Open `UnityProject/` in Unity `6000.4.4f1` or compatible Unity 6.
2. Open `Assets/Scenes/Boot.unity`.
3. Press Play. Use the Garage Start Run button, then drag/touch the arena to move and use the Dash button.

The editor seeder at `Tools/Neon Sky Survivors/Seed Mobile Boot Scene` can recreate the Boot scene and mobile player settings if needed.

## Android Target

- Portrait orientation.
- Android package id: `com.neonskysurvivors.game`.
- Runtime rendering uses capped object pools for enemies, projectiles, XP shards, and dash trails.
- Smoke APK output: `Builds/Android/NeonSkySurvivors-Smoke.apk`.

## Verify Locally

```powershell
dotnet test .\tests\NeonSkySurvivors.Core.Tests\NeonSkySurvivors.Core.Tests.csproj
& 'D:\unity6.4v\6000.4.4f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\cosmostar\UnityProject' -executeMethod NeonSkySurvivors.Editor.NeonSkySurvivorsProjectSeeder.VerifyMobileBootScene
& 'D:\unity6.4v\6000.4.4f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\cosmostar\UnityProject' -buildTarget Android -executeMethod NeonSkySurvivors.Editor.NeonSkySurvivorsProjectSeeder.BuildAndroidSmokeTest
```

## Notes

- The previous vertical-slice implementation has been removed.
- Runtime visuals are code-driven placeholder neon UI/gameplay art designed to be replaced by authored assets without changing the gameplay architecture.
- Follow `Instructions.md` step by step for Neon Sky Survivors work. When a requirement is completed, add a progress mark near the relevant instruction before committing.
