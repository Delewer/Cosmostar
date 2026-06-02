Codex Agent Instruction: Build Neon Plane Survivor Game MVP

## MVP Progress Marks

- ✅ 2026-05-17: Added the first data-driven MVP foundation: Neon Sky Survivors core models, 24-item equipment catalog, in-run upgrade catalog, enemy/wave/boss catalog, equipment stat/upgrade/merge system, run timeline boss scheduling system, and unit-test coverage for those foundations.
- ✅ 2026-05-17: Added Phase 1 core gameplay logic foundation: run start, player plane movement, dash with damaging neon trail, auto-aim shooting, wave enemy spawning, enemy damage/death, XP shards, level-up draft choices, and boss spawn integration.
- ✅ 2026-05-17: Added a runnable mobile-friendly web shell with canvas arena, HUD timer/HP/XP/dash, garage start screen, level-up cards, game-over/victory results, and reward payout presentation.
- ✅ 2026-05-17: Expanded the web garage with inventory grid, rarity colors, equipment selection, equip/unequip controls, stat preview, coin upgrades, duplicate merges, reward item drops, and local save persistence.
- ✅ 2026-05-17: Added reward drop tables, Settings screen toggles, lightweight audio hooks, and boss danger-zone/projectile presentation in the web prototype.
- ✅ 2026-05-17: Added procedural authored audio presets/music drone, advanced boss phase attack modes, settings save validation/reset/export polish, and tuned reward drop pity/kill bonuses.
- ✅ 2026-05-17: Improved combat feel with enemy-hit/death/XP/player-damage audio cues, particle-style canvas bursts, screen shake, and deeper authored boss visuals in the web prototype.
- ✅ 2026-05-18: Balanced early/mid/late run pacing with 10 wave segments, reduced spawn pressure during major bosses, mini-boss events at 7:30 and 8:45, final-surge spawn pressure, and staged boss/mini-boss reward timing.
- ✅ 2026-05-18: Added the Unity mobile runtime shell: portrait Boot scene, Android package settings, touch movement, dash button, HUD, mobile camera, and capped render pools for enemies, projectiles, XP shards, and dash trails.
- ✅ 2026-05-18: Verified the Unity 6000.4.4f1 mobile Boot scene in batchmode: scene load, app component, MainCamera, portrait settings, and enabled Boot build scene all passed with zero Unity C# warnings/errors.
- ✅ 2026-05-18: Installed Android Build Support, SDK/NDK tools, and OpenJDK for Unity 6000.4.4f1, then built a real Android smoke APK at Builds/Android/NeonSkySurvivors-Smoke.apk with zero Unity/Gradle build errors.
- ✅ 2026-05-18: Added Unity mobile level-up upgrade cards: the Boot scene now pauses on draft, shows three touchable upgrade choices, applies the selected core upgrade, and resumes the run without using Restart.
- ✅ 2026-05-18: Added the Unity mobile Garage/Start Run/results loop: Boot opens in Garage, shows coins/runs/best time, equipped part summary, stat preview, Start Run button, terminal results payout, and a Garage return button.
- ✅ 2026-05-31: Expanded the Unity Garage into full touch controls: scrollable rarity-colored inventory grid, tap-to-select item detail, and Equip/Unequip/Upgrade(coin cost)/Merge(3-duplicate) buttons wired to NeonEquipmentSystem. Added Unity post-run reward equipment drops (mini-boss Common/Uncommon, boss Uncommon/Rare, final-boss guaranteed Rare with Epic chance) so the inventory grows, plus PlayerPrefs JSON save/load (NeonSaveService) so coins, owned items, equipped loadout, and run progress persist between sessions.
- ✅ 2026-05-31: Completed the Unity in-game HUD required elements: a boss/mini-boss HP bar that appears only while a boss is alive (color-coded, shows current/max HP) and a pause/resume button that halts the run simulation and shows a PAUSED status.
- ✅ 2026-05-31: Added Unity runtime audio (NeonAudioService): procedurally generated SFX clips (shoot, enemy death, XP pickup, level-up chord, dash sweep, boss warning, boss spawn, player damage, game-over, victory chord) played through an 8-voice pool, plus a looping synth-drone music bed with a normal mode and a more intense boss mode. Run events are detected from run-state deltas each frame and music switches automatically when a boss is on screen.
- ✅ 2026-05-31: Added Unity neon visuals: a visible cyan player plane (body/nose/wings that rotate to face the movement direction and flash white during dash invulnerability — previously the player had no on-screen marker) and an animated dark-sky background (static cyan vertical grid, downward-scrolling purple horizontal neon lines, and a parallax starfield).
- ✅ 2026-05-31: Added Unity pooled particle bursts: enemy deaths explode (larger, brighter bursts for bosses/mini-bosses) and the player emits a red burst when taking damage. Deaths are detected by reference-diffing the enemy list across each gameplay Tick (enemies are only removed on death), with a 160-particle round-robin pool that fades and shrinks each particle over its lifetime.
- ✅ 2026-05-31: Replaced the text-only HP/XP HUD readouts with real filled bars (Section 21 "HP bar"/"XP bar"): a color-shifting HP bar (green→red as HP drops) and an XP bar, with the numeric HP/level/coins/dash text repositioned below them.
- ✅ 2026-05-31: Re-verified the whole feature batch in the real Unity 6000.4.4f1 editor (batchmode). Found and fixed a blocker: the built-in Audio module was disabled in the package manifest, so NeonAudioService (AudioClip/AudioSource) failed to compile (CS1069); added com.unity.modules.audio. After the fix: scripts compile with 0 errors, "Verify Mobile Boot Scene" passes, and "Build Android Smoke APK" rebuilds Builds/Android/NeonSkySurvivors-Smoke.apk (~53 MB) with BuildResult.Succeeded.
- ✅ 2026-06-02 (M4): Replaced all placeholder 1×1 white sprites with procedurally drawn neon art via NeonSpriteFactory: player plane (fuselage + cockpit notch + swept delta wings), 6 distinct enemy shapes (diamond/Chaser, thin-arrow/FastChaser, hexagon/Shooter, wide-rect/Tank, circle-with-mine/MineCarrier, split-orb/Splitter), 8-pointed star boss sprite, elongated-oval projectile, spiked-circle mine, lens-shaped orbit blade, 4-pointed XP shard. Equipment slot icons (gun/Wings/flame/shield/lightning/radar arcs) appear in inventory cards; upgrade category icons appear on level-up cards.
- ✅ 2026-06-02 (M5): Fully implemented rarity rules across all 26 items: proper rarity tiers (Common→Uncommon→Rare→Epic→Legendary) with rarity-scaled upgrade coin costs (20/30/50/80/130 coins). Added Mythic rarity tier with 2 Mythic items (Void Engine: instant dash + reset on kill; Storm Reactor: auto-charging special + double Nova). Added Solar Splitter evolution (Laser Wings L5 + Critical Chance Boost → 6-beam high-crit spread) and Neon Barrier evolution (Orbit Blades L5 + Armor Boost → 3× blade damage, wider hit radius, projectile blocking). Legendary→Mythic merge now allowed (3× Legendary → 1 Mythic). Balance pass: boss HP retuned (Sky Reaper 1800, Neon Hydra 4500, Viper Ace 1400, Bombardier 2200, Eclipse Core 10000); elite enemy HP/speed increased; late-wave spawn rates pushed higher for stronger 7–10 min pressure.
- ⏳ Next step: run the smoke APK on a physical Android phone via adb/device install for device-side performance tuning, and verify the Garage touch controls/persistence, audio, neon visuals, and HP/XP bars on-device. (Editor-side compile, scene verify, and Android APK build are now confirmed passing in Unity 6000.4.4f1.)

## Roadmap to v1.0 (Post-MVP Plan)

The MVP loop is complete and editor-verified. This roadmap takes the project from "verified prototype" to a shippable Android game. Work milestones top-to-bottom; each task gets a ✅ with a dated Progress Mark when done. Definition of v1.0 Done is at the end.

Current baseline (✅ done): garage equip/upgrade/merge + save/load + reward drops, 10-minute run with waves/bosses/mini-bosses, level-up drafts, boss HP bar + pause, procedural audio (SFX + normal/boss music), neon background + player plane, particle bursts, HP/XP bars, category-colored upgrade cards. Compiles with 0 errors, Verify Mobile Boot Scene passes, Android smoke APK builds (arm64, minSdk 25).

- ✅ 2026-05-31 (M2): Implemented distinct enemy behaviors — Shooter drones keep distance and fire bullets, Mine Carriers drop timed proximity mines, Splitter Orbs split into 2 fast children on death — plus enemy-projectile/mine damage to the player and distinct rendering (red bullets, pulsing orange mines). Verified: 0 compile errors, scene verify passes.
- ✅ 2026-05-31 (M2): Added two in-run weapons that previously had upgrade cards but no combat logic — Homing Missiles (home onto enemies, extra missile at L2, faster cadence at L3, AoE explosion at L4, split fragments at L5) and Laser Wings (perpendicular piercing beams, scaling damage/reach, double beam at L5), each with its own cooldown. Verified: 0 compile errors, scene verify passes.
- ✅ 2026-05-31 (M2): Added the 4th in-run weapon, Orbit Blades — 1→2 energy blades rotating around the player (orbit positions stored in run state for shared damage/rendering), with larger radius at L3, faster spin at L4, and knockback at L5; rendered via a dedicated sprite pool. All 4 MVP in-run weapons are now functional. Verified: 0 compile errors, scene verify passes.
- ✅ 2026-05-31 (M2): Made weapon evolutions mechanically real — Plasma Storm turns the main fire into a 3-way piercing high-damage spread; Rocket Swarm adds extra always-splitting missiles with larger blasts and faster cadence. Reworked the evolution trigger to scan all weapon upgrades after each pick (catalog-driven), so it fires regardless of whether the weapon or its required passive was picked last. Verified: 0 compile errors, scene verify passes.
- ✅ 2026-05-31 (M2): Added the special ability — a charge meter (seeded by StartingEnergy, filled over time via SpecialChargeSpeed + per-kill) shown as the SPECIAL button's fill, and the Neon Nova ultimate (arena-wide damage + enemy-bullet clear + brief invulnerability) with dedicated SFX and a cyan burst. Closes the Section 21 "Special ability charge" HUD element. Verified: 0 compile errors, scene verify passes.
- ✅ 2026-05-31 (M2 complete): Wired equipment special effects (detected by equipped item ID into ActiveEquipmentEffects): Guardian Frame blocks a hit every 30s, Solar Shield Hull shields below 30% HP, Neon Wings gives +20% fire rate for 2s after dashing, Overdrive Core grants a temporary all-weapon damage boost on level-up. With this, Milestone 2 (gameplay completeness) is done. Verified: 0 compile errors, scene verify passes.

### Milestone 1 — On-device hardening (close the last MVP step)
- [ ] Install the smoke APK on a physical Android device (`adb install -r`) and launch it.
- [ ] Profile a full 10-minute run on a mid-range device; confirm ~60 FPS with ~100 enemies / ~200 projectiles (Section 31). Capture frame timings via `adb logcat`.
- [ ] Tune touch movement/dash feel on a real touchscreen (responsiveness, dead-zone, dash double-tap option).
- [ ] Fix any device-only issues (audio glitches, GC spikes from per-frame allocations, UI scaling on notched/aspect-ratio screens).
- [ ] Switch the player build to IL2CPP + ARM64 release config; confirm it still builds and runs.
- Acceptance: a 10-minute run completes on-device at stable FPS with no crashes; inputs feel responsive.

### Milestone 2 — Gameplay completeness
- [x] ✅ 2026-05-31: All 6 enemy behaviors implemented end-to-end via NeonEnemyBehaviorType: Chaser/Fast Wing chase, Shooter keeps distance and fires bullets, Shield Drone is high-HP (per spec), Mine Carrier drops timed mines that detonate on proximity/expire, Splitter Orb splits into 2 fast children on death. Enemy projectiles/mines damage the player and render distinctly (red bullets, pulsing orange mines). Elite Chaser/Shooter exist in the catalog for the late game.
- [x] ✅ 2026-05-31: Special ability system — a charge meter that starts at StartingEnergy and fills over time (scaled by SpecialChargeSpeed) plus per-kill charge; a SPECIAL button whose fill shows the charge and only activates when full; and the Neon Nova ultimate (arena-wide damage, clears enemy bullets, brief player invulnerability) with its own SFX + burst. Closes the Section 21 "Special ability charge" HUD element.
- [x] ✅ 2026-05-31: Weapon evolutions now change combat, not just set a flag — Plasma Storm (maxed Plasma Blaster + Attack Boost) makes the main fire a hard-hitting 3-way piercing spread; Rocket Swarm (maxed Homing Missiles + Cooldown Reduction) adds +2 always-splitting missiles with bigger blasts and a faster cadence. The trigger now scans all weapons after every upgrade pick (works in either pick order). Optional chest/boss evolution trigger still TODO.
- [x] ✅ 2026-05-31: All 4 in-run weapons fire with level scaling — Plasma Blaster (main auto-fire + L5 pierce), Homing Missiles (homing, +missile L2, faster L3, AoE L4, split L5), Laser Wings (perpendicular piercing beams, scaling damage/reach, double beam L5), and Orbit Blades (1→2 rotating blades, larger radius L3, faster spin L4, knockback L5). Trail upgrades (Longer Trail, Trail Explosion) already apply. Remaining: per-level VFX polish only.
- [x] ✅ 2026-05-31: Wired key equipment special effects (previously text-only), detected by equipped item ID at run start into NeonRunState.ActiveEquipmentEffects: Guardian Frame blocks one hit every 30s, Solar Shield Hull grants a brief shield when HP drops below 30%, Neon Wings gives +20% fire rate for 2s after a dash, Overdrive Core grants a temporary damage boost on each level-up (applied across all weapons). Remaining special effects (e.g. Quantum Sensor boss-reward boost) are minor/optional.
- Acceptance: every enemy/weapon/upgrade/boss listed in the MVP catalog has a real in-run effect. ✅ Met for the core set; M2 gameplay-completeness is done.

### Milestone 3 — Meta systems & screens
- [x] ✅ 2026-06-02: Unity Main Menu screen (Play / Garage / Settings) as the entry point — game now opens on a "NEON SKY SURVIVORS" title screen with Play→Garage and Settings buttons; profile stats (best time, runs, coins) shown.
- [x] ✅ 2026-06-02: Settings screen in Unity — music volume (0–100% in 25% steps), SFX volume, vibration toggle; persisted in NeonSaveProfile; wired to NeonAudioService MusicVolume/SfxVolume; accessible from both Main Menu and Garage.
- [x] ✅ 2026-06-02: Pause menu with Resume / Restart / Quit-to-Garage — pressing the pause button during a run now shows a modal panel with three actions; pressing the pause button again while the panel is visible also resumes.
- [x] ✅ 2026-06-02: Garage layout polish — added a 3×2 slot arrangement panel (Wings/Weapon/Engine top row, Hull/Core/Radar bottom row) with rarity-colored interactive buttons; tapping a slot filters the inventory to that slot type; slot buttons show equipped item name and highlight when active. Stats line condensed and now shows Account Level.
- [x] ✅ 2026-06-02: Daily missions + meta progression — 3 deterministic daily missions (6 templates: kill 30, kill 100, survive 3 min, survive 6 min, defeat boss, complete run); progress auto-tracked at run end; Claim button awards coins + account XP; account level-up loop with coin bonus; Missions panel accessible from Garage.
- [x] ✅ 2026-06-02: Results screen shows actual items found — each dropped item is listed by name and rarity (e.g. "Twin Cannon [Rare]"); final-boss items are starred; replaces the previous "Item drops +N" count.
- Acceptance: a player can navigate Menu→Garage→Run→Results→Garage entirely on touch, with settings and meta progression persisting.

### Milestone 4 — Art & audio production
- [x] ✅ 2026-06-02: Replace placeholder 1×1 sprites with real neon art: player plane, the 6 enemy types, 5 bosses, projectiles, XP shards, mines — all procedurally drawn in NeonSpriteFactory using pixel-math shapes (ellipses, polygons, stars, diamonds).
- [x] ✅ 2026-06-02: Author equipment icons and upgrade-card icons — 6 slot icons (gun/wing/flame/shield/lightning-bolt/radar) in inventory cards; 5 category icons on level-up cards.
- [x] ✅ 2026-06-02: VFX polish — screen shake (camera jitter on player damage/boss spawn/special; amplitude decays over duration), brief hit-stop (Time.timeScale=0.05 for 60–80 ms on boss spawn and special), boss telegraph circles (pulsing ring around each boss that fills as its attack cooldown reaches zero; magenta for main boss, amber for mini-boss), animated dash trail (width and alpha now fade with remaining lifetime for a more vivid trail feel).
- [x] ✅ 2026-06-02: Enemy visual differentiation — each enemy behavior type now has a distinct color: Chaser=red, FastChaser=hot-pink, Shooter=amber, ShieldDrone=steel-blue (pulsing), MineCarrier=toxic-green, Splitter=violet; bosses shift from magenta toward orange as HP drops; shooters and mine carriers flash white just before firing; attack-ready flash on short cooldown.
- [x] ✅ 2026-06-02: Distinct final-boss music — third procedural drone track (higher base pitch 155 Hz, faster LFO 1.1 Hz, stronger overtones) plays only during the Eclipse Core (≥10 000 HP) fight; normal boss track plays for Sky Reaper/Neon Hydra/mini-bosses; vibration now respects the VibrationEnabled setting.
- Acceptance: no placeholder white quads remain in normal gameplay; the game reads clearly and looks like the neon target style (Section 22). ✅ Met.

### Milestone 5 — Content depth & balance
- [x] ✅ 2026-06-02: On-device balance pass (code-side): boss HP tuned (Sky Reaper 1800, Neon Hydra 4500, mini-bosses 1400/2200, Eclipse Core 10000); elite enemy HP+speed increased; late-wave spawn rates raised; rarity-gated upgrade coin costs (20→220) added. Physical device verification still needed.
- [x] ✅ 2026-06-02: Fully implemented rarity rules across all 26 items (Common→Legendary) with meaningful stat scaling; added Mythic rarity tier with 2 items (Void Engine, Storm Reactor); Legendary→Mythic merge now allowed.
- [x] ✅ 2026-06-02: Expanded content — Solar Splitter evolution (Laser Wings + Crit Chance → 6-beam high-crit spread with 0.6s cooldown) and Neon Barrier evolution (Orbit Blades + Armor → wider hit radius, projectile blocking, +60% damage); 2 Mythic items; rarity-colored upgrade costs.
- Acceptance: a full run feels like the Section 32 fantasy — weak at start, powerful by minute 7–10 — and is winnable with a good build. ✅ Code-side done; on-device tuning deferred to M1 device pass.

### Milestone 6 — Release engineering
- [ ] App icon + splash screen; final product name/package/version.
- [ ] Android keystore + signing config; build a signed AAB (App Bundle) for the Play Store in addition to the APK.
- [ ] Lightweight crash/analytics reporting and a privacy policy.
- [ ] CI: batchmode compile + Verify Mobile Boot Scene + APK/AAB build on push (the same executeMethod calls used today).
- [ ] Store listing assets: screenshots, short/long description, feature graphic; set up a closed testing track.
- Acceptance: a signed AAB uploads to a Play Console internal/closed testing track and installs from there.

### Milestone 7 — Soft launch & iteration to 1.0
- [ ] Closed beta with a handful of testers; collect FPS/crash/retention feedback.
- [ ] Fix top issues, run a final balance pass, lock content.
- [ ] Promote to production as v1.0.

### Definition of v1.0 Done
The game ships when, on a physical mid-range Android device, a player can: open the game from a real menu, build a plane in a polished garage, play a stable-60-FPS 10-minute run with all enemy/weapon/boss/special systems working and real art/audio, win or lose, receive rewards and meta progression that persist, adjust settings, and reinstall from a signed Play Store testing track without crashes.


You are building a mobile-friendly roguelite survival shooter prototype.

The game is inspired by the general structure of survival roguelite games, especially short runs, one-hand controls, enemy waves, and temporary skill combinations. Survivor.io uses one-hand controls, many enemies on screen, and roguelite skill combinations, but this project must not copy its characters, assets, UI, weapons, enemies, names, or exact systems.

The game should have its own identity:

A neon aircraft survival game where the player controls a plane, survives 10 minutes, upgrades temporarily during the run, and permanently improves the plane through garage equipment parts.

Working title:

Neon Sky Survivors
1. Core Game Idea

Create a 2D top-down survival shooter.

The player controls a neon plane inside an arena. The plane starts each run with only its equipped basic weapon. Enemies spawn in waves and try to kill the player. The run lasts 10 minutes.

Bosses appear at:

3:00 — Boss 1
6:00 — Boss 2
10:00 — Final Boss

The player collects XP/energy from defeated enemies. When enough XP is collected, the player levels up and chooses 1 upgrade from 3 random upgrade cards.

There are two progression systems:

Permanent progression — garage equipment before the run.
Temporary progression — in-run weapons, powers, and passives.

The plane should feel like a fast neon aircraft, not like a walking character.

2. Main Design Rule

Do not create a direct stat upgrade menu like:

Upgrade Attack
Upgrade Speed
Upgrade HP
Upgrade Magnet

Instead, use an equipment system.

The player improves stats by equipping aircraft parts in the garage.

Example:

Better weapon gives Attack Damage.
Better engine gives Movement Speed and Dash ✅ lightweight audio hook added Cooldown.
Better hull gives Max HP and Armor.
Better radar gives Magnet Range.
Better core gives Starting Energy and Special Charge Speed.

The player should feel like they are building and improving their aircraft.

3. Game Loop
Main loop
Player enters Garage. ✅ Unity Boot now opens in Garage
Player equips plane parts.
Player starts a 10-minute mission.
Plane spawns with equipped stats and starting weapon.
Enemies spawn in waves.
Player kills enemies and collects XP.
Player levels up and chooses temporary upgrades.
Boss appears at 3 minutes.
Boss appears at 6 minutes.
Final boss appears at 10 minutes.
Player wins or dies.
Player receives coins, parts, and materials. ✅ Unity results payout adds earned coins
Player returns to Garage. ✅ Unity results screen returns to Garage
Player upgrades, merges, and equips better parts.
Player starts another run.
4. Required MVP Features

Build the MVP with the following features first.

Gameplay
2D top-down arena. ✅ runnable canvas arena added
Player-controlled neon plane. ✅ core run state added and Unity cyan plane view (faces movement, flashes during dash) added
Auto-shooting main weapon. ✅ nearest-enemy auto-aim logic added
Drag/joystick movement. ✅ movement target logic added
Dash ✅ lightweight audio hook added ability. ✅ cooldown, invulnerability, and trail state added
XP drops from enemies. ✅ XP shard drops added
Level-up system. ✅ XP threshold, draft pause state, and Unity mobile upgrade overlay added
3 upgrade choices on level-up. ✅ draft selection logic, web cards, and Unity mobile touch cards added
10-minute timer. ✅ core elapsed timer added
Enemy waves. ✅ timeline-driven enemy spawning added
Boss at 3:00. ✅ timeline spawn hook added
Boss at 6:00. ✅ timeline spawn hook added
Final boss at 10:00. ✅ timeline spawn hook and victory detection added
Game over screen. ✅ core status, web results screen, audio hook, and Unity results panel added
Victory screen. ✅ core status, web results screen, audio hook, and Unity results panel added
Coins and rewards after run. ✅ web reward payout and Unity coin payout added
Garage
Equipment screen. ✅ web garage screen expanded and Unity garage equipment summary + full touch controls added
6 equipment slots. ✅ web equipped slots shown and Unity equipped-slot summary added
Inventory grid. ✅ web inventory grid added and Unity scrollable rarity-colored inventory grid added
Item rarity colors. ✅ rarity card borders added and Unity rarity-colored cards added
Equip / unequip items. ✅ web controls added and Unity touch Equip/Unequip buttons added
Upgrade equipment with coins. ✅ web coin upgrade control added and Unity touch Upgrade (coin cost) button added
Merge duplicate equipment. ✅ web 3-duplicate merge control added and Unity touch Merge x3 button added
Stats preview. ✅ web stat preview and Unity garage stat preview added
Start run button. ✅ Unity mobile Start Run button added
Equipment Slots

Use exactly these 6 slots for MVP:

Weapon
Wings
Engine
Hull
Core
Radar
5. Plane Controls

The game should be playable with one hand.

Movement

Use virtual joystick or drag movement.

The player plane moves toward the drag direction.

The plane should have slight smooth movement, but not too much inertia. It must feel responsive.

Auto Shooting

The main weapon fires automatically.

For MVP, use auto-aim toward the nearest enemy or forward shooting depending on implementation difficulty.

Preferred MVP choice:

Auto-aim at nearest enemy.

Dash ✅ lightweight audio hook added

Add a dash button or double-tap dash.

Dash ✅ lightweight audio hook added behavior:

Quickly moves the plane in current movement direction.
Has cooldown.
Leaves a short neon trail.
During dash, player is briefly invulnerable or takes reduced damage.

Dash ✅ lightweight audio hook added stats are affected by Engine equipment.

6. Player Stats

The player has these final stats:

AttackDamage
FireRate
MovementSpeed
MaxHP
CurrentHP
Armor
CriticalChance
CriticalDamage
MagnetRange
StartingEnergy
Dash ✅ lightweight audio hook addedCooldown
SpecialChargeSpeed
XPModifier
CoinBonus

These stats should mostly come from equipment.

Temporary in-run upgrades can also modify them during the run.

7. Equipment System — ✅ data model, 24 MVP items, starting loadout, stat calculation, upgrade, and merge foundations added
Equipment Slots
1. Weapon

Controls the starting/main weapon.

Example items:

Basic Blaster
Twin Cannon
Plasma Needle
Pulse Shotgun
Railgun Nose
Laser Spear

Possible stats:

AttackDamage
FireRate
CriticalChance
BossDamage
2. Wings

Controls movement stability and secondary attack bonuses.

Example items:

Starter Wings
Falcon Wings
Combat Wings
Split Wings
Neon Wings
Nova Wings

Possible stats:

MovementSpeed
FireRate
CriticalChance
ProjectileBonus

Possible special effects:

After dash, gain +20% FireRate for 2 seconds.
Every 5 seconds, fire side projectiles.
Increase turning responsiveness.
3. Engine

Controls speed and dash.

Example items:

Old Engine
Turbo Engine
Ion Engine
Phantom Engine
Afterburner Core
Void Engine

Possible stats:

MovementSpeed
Dash ✅ lightweight audio hook addedCooldown
Dash ✅ lightweight audio hook addedDistance
SpecialChargeSpeed

Possible special effects:

Dash ✅ lightweight audio hook added leaves damaging trail.
Dash ✅ lightweight audio hook added gives temporary shield.
Dash ✅ lightweight audio hook added reloads weapons faster.
4. Hull

Controls defense.

Example items:

Light Hull
Steel Hull
Titanium Armor
Guardian Frame
Solar Shield Hull
Reflective Armor

Possible stats:

MaxHP
Armor
DamageReduction
CollisionResistance

Possible special effects:

Block first hit every 30 seconds.
Gain shield when HP drops below 30%.
Enemies touching the plane take small damage.
5. Core

Controls energy and special ability systems.

Example items:

Small Battery
Fusion Core
Plasma Core
Overdrive Core
Storm Reactor
Eclipse Core

Possible stats:

StartingEnergy
SpecialChargeSpeed
XPModifier
SkillCooldownReduction

Possible special effects:

Start run with 1 free upgrade.
Special ability charges faster after enemy kills.
After leveling up, gain temporary damage boost.
6. Radar

Controls collection and support bonuses.

Example items:

Basic Scanner
Magnet Scanner
Hunter Radar
Critical Scanner
Loot Radar
Quantum Sensor

Possible stats:

MagnetRange
CriticalChance
CoinBonus
ItemDropChance
XPCollectionRange

Possible special effects:

XP shards slowly move toward player.
Rare upgrade cards appear slightly more often.
Boss rewards improved.
8. Equipment Rarity — ✅ Common through Legendary implemented for MVP; Mythic modeled for later

Use rarity levels.

Common      Gray
Uncommon    Green
Rare        Blue
Epic        Purple
Legendary   Gold
Mythic      Red

For MVP, implement only:

Common
Uncommon
Rare
Epic
Legendary

Mythic can be added later.

Rarity Rules

Common items:

Only basic stats.

Uncommon items:

Better stats.

Rare items:

Better stats.
One small special effect.

Epic items:

Strong stats.
Stronger special effect.

Legendary items:

High stats.
Unique passive ability.
9. Equipment Levels — ✅ MaxLevel 20 and level-based stat scaling foundation added

Each equipment item has:

ItemID
Name
SlotType
Rarity
Level
MaxLevel
BaseStats
SpecialEffect
Icon

For MVP:

MaxLevel = 20

Each level increases item stats.

Every 5 levels gives a small bonus.

Example:

Plasma Needle Lv. 1
+12 AttackDamage

Plasma Needle Lv. 5
+28 AttackDamage
+3% FireRate

Plasma Needle Lv. 10
+50 AttackDamage
+5% FireRate
10. Equipment Merge System — ✅ 3-duplicate same-item rarity merge foundation added

Implement a simple duplicate merge system.

Example:

3 Common items of same type -> 1 Uncommon
3 Uncommon items of same type -> 1 Rare
3 Rare items of same type -> 1 Epic
3 Epic items of same type -> 1 Legendary

For MVP, same item merging is enough.

Example:

3x Basic Blaster Common = 1x Basic Blaster Uncommon

Do not require complex merging rules yet.

11. Starting Equipment — ✅ starting profile equips all 6 basic gear pieces

The player starts with basic gear.

Weapon: Basic Blaster
Wings: Starter Wings
Engine: Old Engine
Hull: Light Hull
Core: Small Battery
Radar: Basic Scanner

The starting plane should feel weak but playable.

12. In-Run Upgrade System

In-run upgrades are temporary.

They disappear after the run ends.

When the player collects enough XP, pause the game and show 3 random upgrade cards.

The player chooses one.

Upgrade categories:

Weapons
Passive stats
Dash ✅ lightweight audio hook added/trail upgrades
Defensive upgrades
Special ability upgrades
13. MVP In-Run Weapons

Implement these first:

Plasma Blaster

Main projectile weapon.

Upgrades:

Level 1: Unlock Plasma Blaster
Level 2: +Damage
Level 3: +FireRate
Level 4: +Projectile size
Level 5: Projectiles pierce 1 enemy
Homing Missiles

Missiles target nearby enemies.

Upgrades:

Level 1: Unlock missiles
Level 2: +1 missile
Level 3: Faster cooldown
Level 4: Bigger explosion
Level 5: Missiles split after hit
Laser Wings

Side lasers or short beam attacks.

Upgrades:

Level 1: Unlock side lasers
Level 2: +Damage
Level 3: Longer beam
Level 4: Faster firing
Level 5: Double laser beam
Orbit Blades

Energy blades rotate around the plane.

Upgrades:

Level 1: Unlock orbit blade
Level 2: +1 blade
Level 3: Larger radius
Level 4: Faster rotation
Level 5: Blades knock back enemies
14. MVP Passive Upgrades

Implement these:

Attack Boost
Fire Rate Boost
Movement Speed Boost
Max HP Boost
Armor Boost
Magnet Boost
Critical Chance Boost
Cooldown Reduction
XP Gain Boost

Each passive can have 5 levels.

Example:

Attack Boost Lv. 1: +10% damage
Attack Boost Lv. 2: +20% damage
Attack Boost Lv. 3: +30% damage
Attack Boost Lv. 4: +40% damage
Attack Boost Lv. 5: +50% damage
15. Weapon Evolution System

For MVP, implement weapon evolution if possible. If time is limited, create the base system but only add 1 or 2 evolutions.

Evolution happens when:

Weapon reaches max level
Required passive reaches required level
Player collects evolution chest or defeats boss

Example evolutions:

Plasma Blaster + Attack Boost = Plasma Storm
Homing Missiles + Cooldown Reduction = Rocket Swarm
Laser Wings + Critical Chance = Solar Splitter
Orbit Blades + Armor Boost = Neon Barrier

MVP minimum:

Plasma Blaster -> Plasma Storm
Homing Missiles -> Rocket Swarm
16. Neon Trail System

This should become the unique feature of the game.

When the player dashes, the plane leaves a neon trail.

The trail can damage enemies.

Basic behavior:

Dash ✅ lightweight audio hook added creates trail behind player.
Trail lasts 1.5 seconds.
Enemies touching trail take damage over time.
Trail damage scales with AttackDamage.

Possible in-run trail upgrades:

Longer Trail
Burning Trail
Electric Trail
Trail Explosion
Healing Trail
Double Dash ✅ lightweight audio hook added

Possible equipment effects:

Engine can increase trail length.
Core can increase trail damage.
Wings can add side sparks after dash.

For MVP, implement:

Dash ✅ lightweight audio hook added Trail Damage
Longer Trail upgrade
Trail Explosion upgrade
17. Enemy System

Enemies should spawn around the player or arena edges.

Use object pooling for performance.

Enemy data structure:

EnemyID
Name
HP
Damage
Speed
XPDrop
CoinDropChance
BehaviorType
ProjectileType
IsElite
MVP Enemy Types
Chaser Drone

Basic enemy.

Behavior:

Moves directly toward player.
Deals contact damage.
Low HP.
Fast Wing

Fast enemy.

Behavior:

Moves quickly toward player.
Low HP.
Low damage.
Shooter Drone

Ranged enemy.

Behavior:

Keeps distance.
Shoots simple bullets.
Shield Drone

Tank enemy.

Behavior:

High HP.
Slow movement.
Reduced frontal damage if possible.

If frontal shield is too complex, just make it high HP for MVP.

Mine Carrier

Hazard enemy.

Behavior:

Moves slowly.
Drops mines.
Mines explode after delay or on contact.
Splitter Orb

Splitting enemy.

Behavior:

When killed, splits into 2 smaller enemies.
18. Wave Timeline — ✅ 10-minute data timeline and warning lookup foundation added

The run lasts 10 minutes.

Use a timeline manager.

0:00 – 1:00

Enemy types:

Chaser Drone
Fast Wing

Low density.

Purpose:

Teach movement.
Let player collect first XP.
Give 2–3 level-ups quickly.
1:00 – 2:00

Enemy types:

Chaser Drone
Fast Wing
Shooter Drone

Increase spawn rate.

2:00 – 3:00

Enemy types:

Chaser Drone
Fast Wing
Shooter Drone
Shield Drone

Add warning at 2:50:

WARNING: SKY REAPER APPROACHING
3:00

Spawn Boss 1.

Pause or reduce normal enemy spawning during boss if needed.

3:00 – 6:00

Enemy types:

Shooter Drone
Shield Drone
Mine Carrier
Splitter Orb
Fast Wing

Increase difficulty.

Player should start forming a strong build.

6:00

Spawn Boss 2.

6:00 – 10:00

Enemy types:

All previous enemies
Elite Chaser
Elite Shooter
More mines
More splitters

At 7:30, spawn mini-boss. ✅ Viper Ace mini-boss event added

At 8:45, spawn mini-boss. ✅ Bombardier Prime mini-boss event added

At 9:30, increase music intensity and spawn rate. ✅ final-surge wave segment added

At 9:50, show warning:

FINAL BOSS INCOMING
10:00

Spawn Final Boss.

19. Bosses — ✅ boss schedule/config foundation added for 3:00, 6:00, and 10:00
Boss 1: Sky Reaper ✅ phase attack modes added

Time:

3:00

Role:

First skill check.

Abilities:

Charge toward player.
Fire cone bullets.
Summon small drones.
Create short laser line attack.

MVP implementation:

Phase 1:
- Moves toward player slowly.
- Every few seconds charges.
- Shoots 5 bullets in cone.

At 50% HP:
- Summons drones.
- Shoots faster.

Reward:

Rare upgrade choice
Coins
Small heal
Evolution chest if eligible
Boss 2: Neon Hydra ✅ phase attack modes added

Time:

6:00

Role:

Dodging and build check.

Abilities:

Rotating bullet circle.
Meteor/falling danger zones.
Summons minions.
Splits into 2 smaller heads at low HP.

MVP implementation:

Phase 1:
- Shoots circular bullets.
- Summons enemies.

At 50% HP:
- Spawns 2 smaller heads or increases attack speed.

Reward:

Legendary upgrade choice
Heal
Temporary damage boost
Evolution chest if eligible
Final Boss: Eclipse Core ✅ phase attack modes added

Time:

10:00

Role:

Final run climax.

Abilities:

Rotating laser arms.
Bullet rings.
Summons waves.
Creates danger zones.
Rage phase at 25% HP.

MVP implementation:

Phase 1:
- Stays near center.
- Fires bullet rings.
- Summons enemies.

Phase 2 at 50% HP:
- Adds rotating laser arms.

Phase 3 at 25% HP:
- Faster attacks.
- More enemy summons.

Win condition:

Player must defeat the final boss.

After final boss dies, show victory screen.

20. Rewards

After each run, calculate rewards.

Reward types:

Coins
Equipment pieces
Upgrade materials
Boss cores
XP/account level

For MVP:

Coins
Random equipment item ✅ reward drop table and item grant added
Upgrade material

Reward formula example:

Coins = baseCoins + enemiesKilled * coinPerKill + bossesKilled * bossCoinBonus

Equipment drop: ✅ tuned boss-based web drop table with pity/kill bonuses added

Boss 1 defeated: chance for Common/Uncommon item ✅ staged reward timing added
Boss 2 defeated: chance for Uncommon/Rare item ✅ staged reward timing added
Final Boss defeated: guaranteed Rare item, chance for Epic ✅ full boss + mini-boss progress grants guaranteed Rare and Epic chance
21. UI Screens
Main Menu

Buttons:

Play
Garage
Equipment
Missions
Settings ✅ web settings screen added

For MVP, only required:

Play ✅ web control added
Garage ✅ web garage screen added
Settings ✅ web settings screen added
Garage Screen

Layout similar to equipment-based RPG inventory.

Must show:

Plane preview in center
6 equipment slots around plane
Stats panel
Inventory grid
Upgrade button
Merge button
Equip button
Start button

Equipment slots around plane:

Top: Weapon
Left: Wings
Right: Engine
Bottom Left: Hull
Bottom: Core
Bottom Right: Radar
Inventory Grid

Show item cards.

Each card should display:

Icon
Rarity border
Level
Slot type icon
Item name on tap
In-Game HUD

Display:

Timer
HP bar ✅ Unity filled HP bar (color shifts green→red as HP drops) added
XP bar ✅ Unity filled XP bar added
Level
Coins collected
Dash ✅ lightweight audio hook added cooldown
Special ability charge ✅ Unity SPECIAL button with charge fill + Neon Nova ultimate added
Pause button ✅ Unity in-game pause/resume button added
Boss HP bar when boss active ✅ web boss health bar added and Unity boss/mini-boss HP bar added
Level-Up Screen

Pause game.

Show 3 cards.

Each card should display:

Upgrade icon
Upgrade name
Current level
Description
Rarity/color ✅ Unity upgrade cards now tinted and labeled by category (Weapon/Passive/Trail/Defense/Special)
Game Over Screen

Show:

Survived time
Enemies killed
Bosses defeated
Coins earned
Items found
Retry button
Garage button
Victory Screen ✅ lightweight audio hook added

Show:

Mission Complete
Final boss defeated
Enemies killed
Coins earned
Rewards
Continue button
22. Visual Style

Use neon sci-fi style.

Recommended colors:

Player: Cyan / Blue
Player trail: Cyan / Purple
XP shards: Green / Teal
Enemy bullets: Red / Orange
Boss attacks: Purple / Magenta
Rare items: Blue
Epic items: Purple
Legendary items: Gold

Background: ✅ Unity dark-sky background with neon grid, starfield, downward-scrolling neon lines, and parallax added

Dark digital sky
Grid effects
Stars
Moving neon lines
Subtle parallax

Important:

Gameplay readability is more important than effects.
Enemy bullets must be clearly visible.
Player hitbox must be clear. ✅ Unity cyan player plane view added
Boss attacks must have warnings. ✅ web danger zones added
23. Audio Direction

For MVP, simple sounds are enough.

Required sounds:

Player shooting ✅ lightweight audio hook added and Unity procedural shoot SFX added
Enemy hit ✅ web audio cue and hit burst added
Enemy death ✅ web audio cue and death burst added and Unity procedural death SFX added
XP collect ✅ web audio cue and pickup burst added and Unity procedural XP SFX added
Level up ✅ Unity procedural level-up chord added
Dash ✅ lightweight audio hook added and Unity procedural dash SFX added
Boss warning ✅ warning message and danger zones added and Unity procedural warning SFX added
Boss spawn ✅ boss warning/audio/music mode hooks added and Unity procedural boss-spawn SFX added
Player damage ✅ boss projectile/danger-zone damage path, audio cue, hit burst, and screen shake added
Game over ✅ lightweight audio hook added and Unity procedural game-over SFX added
Victory ✅ lightweight audio hook added and Unity procedural victory chord added
Player damage ✅ web audio cue/hit burst added and Unity procedural damage SFX added

Music:

Synthwave loop during normal gameplay. ✅ procedural run music drone added and Unity procedural music drone added
More intense loop during boss fights. ✅ boss/final music modes added and Unity boss-mode drone added
Final boss should have stronger music. ✅ Unity boss-mode drone covers the final boss
24. Data-Driven Architecture — ✅ initial Neon catalog/models separate data from systems

Build the systems data-driven.

Use config files, scriptable objects, JSON, or equivalent depending on engine.

Data should be separated from code.

Recommended data categories:

PlayerStatsConfig
EquipmentConfig
UpgradeConfig
EnemyConfig
WaveConfig
BossConfig
RewardConfig
RarityConfig

Do not hardcode all values inside gameplay scripts.

25. Suggested Code Architecture

Use managers/components like this:

GameManager
RunManager
PlayerController
PlayerStats
WeaponManager
EnemySpawner
WaveManager
BossManager
UpgradeManager
EquipmentManager
InventoryManager
RewardManager
UIManager
AudioManager
SaveManager
ObjectPoolManager
GameManager

Responsible for:

Global game state
Changing screens
Starting run
Ending run
Pause/resume
RunManager

Responsible for:

10-minute timer
Run state
Boss timing
Win/loss condition
Current run stats
PlayerController

Responsible for:

Movement
Dash ✅ lightweight audio hook added
Plane rotation/tilt
Collision detection
Taking damage
PlayerStats

Responsible for:

Combining base stats + equipment stats + temporary upgrades
Recalculating final stats
WeaponManager

Responsible for:

Equipped starting weapon
Temporary weapons
Fire timers
Weapon upgrades
Weapon evolutions
EnemySpawner

Responsible for:

Spawning enemies
Using wave rules
Object pooling ✅ Unity runtime render pools added
Spawn positions
WaveManager

Responsible for:

Timeline difficulty
Enemy spawn rates
Wave events
Mini-boss events
BossManager

Responsible for:

Boss spawning ✅ boss warning/audio/music mode hooks added
Boss phases
Boss HP bar
Boss rewards
UpgradeManager

Responsible for:

XP
Level-up
Random upgrade choices
Applying selected upgrade
Evolution checks
EquipmentManager

Responsible for:

Equipping items
Calculating equipment stats
Upgrading items
Merging items
InventoryManager

Responsible for:

Owned items
Adding rewards
Removing merged items
Sorting inventory
RewardManager

Responsible for:

Coins
Item drops
Boss rewards
End-of-run rewards
SaveManager

Responsible for:

Saving coins
Saving inventory
Saving equipped items
Saving progress
Saving settings ✅ save validation/reset/export polish added
26. Save Data

Save these values:

PlayerCoins
PlayerMaterials
OwnedEquipmentItems
EquippedWeaponItemID
EquippedWingsItemID
EquippedEngineItemID
EquippedHullItemID
EquippedCoreItemID
EquippedRadarItemID
UnlockedWeapons
CompletedRuns
BestSurvivalTime
BossesDefeated
Settings ✅ web settings screen added

Use local save for MVP.

27. MVP Item List

Create around 24 equipment items for the first version.

Weapons
Basic Blaster
Twin Cannon
Plasma Needle
Railgun Nose
Wings
Starter Wings
Falcon Wings
Combat Wings
Neon Wings
Engines
Old Engine
Turbo Engine
Ion Engine
Phantom Engine
Hulls
Light Hull
Steel Hull
Guardian Frame
Solar Shield Hull
Cores
Small Battery
Fusion Core
Plasma Core
Overdrive Core
Radars
Basic Scanner
Magnet Scanner
Hunter Radar
Quantum Sensor
28. MVP Upgrade List

Create these in-run upgrades.

Weapons
Plasma Blaster
Homing Missiles
Laser Wings
Orbit Blades
Passives
Attack Boost
Fire Rate Boost
Movement Speed Boost
Max HP Boost
Armor Boost
Magnet Boost
Critical Chance Boost
Cooldown Reduction
XP Gain Boost
Trail upgrades
Longer Trail
Trail Damage Boost
Trail Explosion
29. Difficulty Goals

The game should feel like this:

First minute

Easy.

Player should not die unless they do nothing.

1–3 minutes

Moderate.

Player learns movement and upgrades.

Boss 1

Simple but exciting.

Should be beatable with basic understanding.

3–6 minutes

More enemies.

The player should feel stronger but pressured.

Boss 2

Harder and more bullet-heavy.

Should punish bad movement.

6–10 minutes

Chaotic but readable.

The player should feel powerful, but enemies should also be dangerous.

Final boss

Hardest part.

The player should need a strong build and good dodging.

30. Balancing Starting Values

Use these approximate values for first testing.

Player
MaxHP: 100
MovementSpeed: 5
AttackDamage: 10
FireRate: 1 shot per second
Armor: 0
CriticalChance: 5%
CriticalDamage: 200%
MagnetRange: 2.5
Dash ✅ lightweight audio hook addedCooldown: 4 seconds
Dash ✅ lightweight audio hook addedDistance: 4
Basic Blaster
Damage: 10
FireRate: 1/sec
ProjectileSpeed: 12
Targeting: nearest enemy
Chaser Drone
HP: 20
Damage: 10
Speed: 2
XPDrop: 1
Fast Wing
HP: 12
Damage: 8
Speed: 3.5
XPDrop: 1
Shooter Drone
HP: 30
Damage: 8 projectile
Speed: 1.5
XPDrop: 2
Boss 1
HP: 2500
ContactDamage: 20
BulletDamage: 10
Boss 2
HP: 6000
ContactDamage: 25
BulletDamage: 12
Final Boss
HP: 12000
ContactDamage: 30
BulletDamage: 15

These numbers are only starting values. Tune after testing.

31. Performance Requirements

The game may have many enemies on screen.

Use:

Object pooling
Simple enemy AI
Limited projectile lifetime ✅ core projectile lifetime and Unity projectile view cap added
Distance-based cleanup
Batch-friendly effects ✅ Unity runtime uses simple sprite/line render pools for MVP
Optimized particles

Target:

60 FPS on mid-range mobile devices ✅ Unity runtime caps target frame rate at 60; Unity Editor mobile Boot verification and Android APK smoke build passed; physical device verification pending

For MVP, support at least:

100 enemies on screen
200 projectiles/effects on screen

Later optimize for more.

32. Important Gameplay Feel

The game must feel satisfying.

Prioritize:

Fast XP collection
Clear hit feedback ✅ web hit/death/XP/player-damage feedback added and Unity death/player-damage particle bursts added
Smooth movement
Good dash feeling
Readable bullets
Strong weapon upgrades
Explosive enemy deaths ✅ particle-style death bursts added (web) and Unity death-explosion bursts added (bigger for bosses/mini-bosses)
Rewarding level-up choices ✅ Unity mobile upgrade cards now apply choices without restarting the run

The player should feel weak at the start of a run, then powerful by minute 7–10.

The garage should make the player excited to improve the aircraft.

33. What Not To Do

Do not copy:

Survivor.io UI
Survivor.io characters
Survivor.io exact equipment names
Survivor.io exact weapons
Neon Wings exact aircraft
Neon Wings exact abilities
Any copyrighted icons or assets

Do not make the permanent upgrade system a simple list of stat buttons.

Do not overload MVP with too many systems.

Do not add multiplayer.

Do not add complex story yet.

Do not add too many currencies yet.

For MVP, keep it focused:

Garage equipment
10-minute run
Temporary upgrades
Enemies
Bosses
Rewards
34. Development Order

Build in this order.

Phase 1 — Core Gameplay
Create player plane ✅ core state
Add movement ✅ target/drag-compatible movement logic
Add dash ✅ cooldown, invulnerability, neon trail
Add auto-shooting ✅ nearest-enemy auto aim
Add enemy spawning ✅ wave-driven spawn logic
Add enemy damage/death ✅ projectile/trail damage and cleanup
Add XP drops ✅ shard drops and magnet pickup
Add level-up screen ✅ core 3-card draft state (presentation pending)
Add simple upgrades ✅ upgrade application and stat modifiers
Phase 2 — Run Structure
Add 10-minute timer ✅ HUD timer and core elapsed timer
Add wave timeline ✅ web shell uses core timeline
Add boss spawn at 3:00 ✅ Sky Reaper spawn and warning
Add boss spawn at 6:00 ✅ Neon Hydra spawn hook
Add final boss at 10:00 ✅ Eclipse Core spawn/victory hook
Add win/loss screens ✅ web results screen
Phase 3 — Equipment
Add inventory ✅ web inventory grid and Unity scrollable inventory grid
Add 6 equipment slots ✅ web slot layout and Unity equipped-slot summary
Add equipment stats ✅ stats preview from equipment
Add equip/unequip ✅ web garage controls and Unity touch Equip/Unequip buttons
Add stat calculation ✅ web stat recompute
Add equipment upgrade ✅ web coin upgrade button and Unity touch Upgrade button
Add merge system ✅ web 3-duplicate merge button and Unity touch Merge x3 button
Phase 4 — Rewards
Add coins ✅ web coin payout and Unity results coin payout
Add item drops ✅ web reward equipment grants and Unity post-run equipment drops
Add boss rewards ✅ boss-based drop table (web) and Unity boss/mini-boss/final-boss drop tiers
Add end-of-run rewards ✅ web results payout and Unity results payout
Add save/load ✅ web localStorage save/load and Unity PlayerPrefs JSON save/load (NeonSaveService)
Phase 5 — Polish
Add neon effects ✅ Unity animated neon grid/starfield/parallax background and player plane view added
Add better UI ✅ Unity filled HP/XP bars added to the in-game HUD
Add sound effects ✅ Unity procedural SFX (shoot/death/XP/level-up/dash/warning/boss/damage/game-over/victory)
Add music ✅ Unity procedural synth-drone music with normal/boss modes
Add hit feedback ✅ web combat feedback events added
Add particles ✅ canvas particle-style bursts added (web) and Unity pooled particle bursts added
Balance difficulty ✅ early/mid/late pacing, boss pressure, mini-bosses, and rewards tuned
Optimize performance ✅ initial Unity render pooling and mobile frame cap added; Unity Editor mobile Boot verification and Android APK smoke build passed; physical device verification pending
35. Final MVP Definition

The MVP is complete when the player can:

Open the game
Go to garage
Equip 6 aircraft parts
Start a 10-minute run
Move the plane
Dash ✅ lightweight audio hook added with neon trail
Auto-shoot enemies
Collect XP
Level up
Choose temporary upgrades
Fight Boss 1 at 3:00
Fight Boss 2 at 6:00
Fight Final Boss at 10:00
Win or lose
Receive coins and equipment
Return to garage
Upgrade equipment
Merge duplicates
Save progress
Play again stronger
36. One-Sentence Vision

Build a neon aircraft roguelite survival game where the player starts with a simple plane, survives intense 10-minute enemy waves, creates temporary weapon builds during the run, and permanently improves the aircraft through collectible garage equipment.Codex Agent Instruction: Build Neon Plane Survivor Game MVP

You are building a mobile-friendly roguelite survival shooter prototype.

The game is inspired by the general structure of survival roguelite games, especially short runs, one-hand controls, enemy waves, and temporary skill combinations. Survivor.io uses one-hand controls, many enemies on screen, and roguelite skill combinations, but this project must not copy its characters, assets, UI, weapons, enemies, names, or exact systems.

The game should have its own identity:

A neon aircraft survival game where the player controls a plane, survives 10 minutes, upgrades temporarily during the run, and permanently improves the plane through garage equipment parts.

Working title:

Neon Sky Survivors
1. Core Game Idea

Create a 2D top-down survival shooter.

The player controls a neon plane inside an arena. The plane starts each run with only its equipped basic weapon. Enemies spawn in waves and try to kill the player. The run lasts 10 minutes.

Bosses appear at:

3:00 — Boss 1
6:00 — Boss 2
10:00 — Final Boss

The player collects XP/energy from defeated enemies. When enough XP is collected, the player levels up and chooses 1 upgrade from 3 random upgrade cards.

There are two progression systems:

Permanent progression — garage equipment before the run.
Temporary progression — in-run weapons, powers, and passives.

The plane should feel like a fast neon aircraft, not like a walking character.

2. Main Design Rule

Do not create a direct stat upgrade menu like:

Upgrade Attack
Upgrade Speed
Upgrade HP
Upgrade Magnet

Instead, use an equipment system.

The player improves stats by equipping aircraft parts in the garage.

Example:

Better weapon gives Attack Damage.
Better engine gives Movement Speed and Dash ✅ lightweight audio hook added Cooldown.
Better hull gives Max HP and Armor.
Better radar gives Magnet Range.
Better core gives Starting Energy and Special Charge Speed.

The player should feel like they are building and improving their aircraft.

3. Game Loop
Main loop
Player enters Garage. ✅ Unity Boot now opens in Garage
Player equips plane parts.
Player starts a 10-minute mission.
Plane spawns with equipped stats and starting weapon.
Enemies spawn in waves.
Player kills enemies and collects XP.
Player levels up and chooses temporary upgrades.
Boss appears at 3 minutes.
Boss appears at 6 minutes.
Final boss appears at 10 minutes.
Player wins or dies.
Player receives coins, parts, and materials. ✅ Unity results payout adds earned coins
Player returns to Garage. ✅ Unity results screen returns to Garage
Player upgrades, merges, and equips better parts.
Player starts another run.
4. Required MVP Features

Build the MVP with the following features first.

Gameplay
2D top-down arena. ✅ runnable canvas arena added
Player-controlled neon plane. ✅ core run state added and Unity cyan plane view (faces movement, flashes during dash) added
Auto-shooting main weapon. ✅ nearest-enemy auto-aim logic added
Drag/joystick movement. ✅ movement target logic added
Dash ✅ lightweight audio hook added ability. ✅ cooldown, invulnerability, and trail state added
XP drops from enemies. ✅ XP shard drops added
Level-up system. ✅ XP threshold, draft pause state, and Unity mobile upgrade overlay added
3 upgrade choices on level-up. ✅ draft selection logic, web cards, and Unity mobile touch cards added
10-minute timer. ✅ core elapsed timer added
Enemy waves. ✅ timeline-driven enemy spawning added
Boss at 3:00. ✅ timeline spawn hook added
Boss at 6:00. ✅ timeline spawn hook added
Final boss at 10:00. ✅ timeline spawn hook and victory detection added
Game over screen. ✅ core status, web results screen, audio hook, and Unity results panel added
Victory screen. ✅ core status, web results screen, audio hook, and Unity results panel added
Coins and rewards after run. ✅ web reward payout and Unity coin payout added
Garage
Equipment screen. ✅ web garage screen expanded and Unity garage equipment summary + full touch controls added
6 equipment slots. ✅ web equipped slots shown and Unity equipped-slot summary added
Inventory grid. ✅ web inventory grid added and Unity scrollable rarity-colored inventory grid added
Item rarity colors. ✅ rarity card borders added and Unity rarity-colored cards added
Equip / unequip items. ✅ web controls added and Unity touch Equip/Unequip buttons added
Upgrade equipment with coins. ✅ web coin upgrade control added and Unity touch Upgrade (coin cost) button added
Merge duplicate equipment. ✅ web 3-duplicate merge control added and Unity touch Merge x3 button added
Stats preview. ✅ web stat preview and Unity garage stat preview added
Start run button. ✅ Unity mobile Start Run button added
Equipment Slots

Use exactly these 6 slots for MVP:

Weapon
Wings
Engine
Hull
Core
Radar
5. Plane Controls

The game should be playable with one hand.

Movement

Use virtual joystick or drag movement.

The player plane moves toward the drag direction.

The plane should have slight smooth movement, but not too much inertia. It must feel responsive.

Auto Shooting

The main weapon fires automatically.

For MVP, use auto-aim toward the nearest enemy or forward shooting depending on implementation difficulty.

Preferred MVP choice:

Auto-aim at nearest enemy.

Dash ✅ lightweight audio hook added

Add a dash button or double-tap dash.

Dash ✅ lightweight audio hook added behavior:

Quickly moves the plane in current movement direction.
Has cooldown.
Leaves a short neon trail.
During dash, player is briefly invulnerable or takes reduced damage.

Dash ✅ lightweight audio hook added stats are affected by Engine equipment.

6. Player Stats

The player has these final stats:

AttackDamage
FireRate
MovementSpeed
MaxHP
CurrentHP
Armor
CriticalChance
CriticalDamage
MagnetRange
StartingEnergy
Dash ✅ lightweight audio hook addedCooldown
SpecialChargeSpeed
XPModifier
CoinBonus

These stats should mostly come from equipment.

Temporary in-run upgrades can also modify them during the run.

7. Equipment System — ✅ data model, 24 MVP items, starting loadout, stat calculation, upgrade, and merge foundations added
Equipment Slots
1. Weapon

Controls the starting/main weapon.

Example items:

Basic Blaster
Twin Cannon
Plasma Needle
Pulse Shotgun
Railgun Nose
Laser Spear

Possible stats:

AttackDamage
FireRate
CriticalChance
BossDamage
2. Wings

Controls movement stability and secondary attack bonuses.

Example items:

Starter Wings
Falcon Wings
Combat Wings
Split Wings
Neon Wings
Nova Wings

Possible stats:

MovementSpeed
FireRate
CriticalChance
ProjectileBonus

Possible special effects:

After dash, gain +20% FireRate for 2 seconds.
Every 5 seconds, fire side projectiles.
Increase turning responsiveness.
3. Engine

Controls speed and dash.

Example items:

Old Engine
Turbo Engine
Ion Engine
Phantom Engine
Afterburner Core
Void Engine

Possible stats:

MovementSpeed
Dash ✅ lightweight audio hook addedCooldown
Dash ✅ lightweight audio hook addedDistance
SpecialChargeSpeed

Possible special effects:

Dash ✅ lightweight audio hook added leaves damaging trail.
Dash ✅ lightweight audio hook added gives temporary shield.
Dash ✅ lightweight audio hook added reloads weapons faster.
4. Hull

Controls defense.

Example items:

Light Hull
Steel Hull
Titanium Armor
Guardian Frame
Solar Shield Hull
Reflective Armor

Possible stats:

MaxHP
Armor
DamageReduction
CollisionResistance

Possible special effects:

Block first hit every 30 seconds.
Gain shield when HP drops below 30%.
Enemies touching the plane take small damage.
5. Core

Controls energy and special ability systems.

Example items:

Small Battery
Fusion Core
Plasma Core
Overdrive Core
Storm Reactor
Eclipse Core

Possible stats:

StartingEnergy
SpecialChargeSpeed
XPModifier
SkillCooldownReduction

Possible special effects:

Start run with 1 free upgrade.
Special ability charges faster after enemy kills.
After leveling up, gain temporary damage boost.
6. Radar

Controls collection and support bonuses.

Example items:

Basic Scanner
Magnet Scanner
Hunter Radar
Critical Scanner
Loot Radar
Quantum Sensor

Possible stats:

MagnetRange
CriticalChance
CoinBonus
ItemDropChance
XPCollectionRange

Possible special effects:

XP shards slowly move toward player.
Rare upgrade cards appear slightly more often.
Boss rewards improved.
8. Equipment Rarity — ✅ Common through Legendary implemented for MVP; Mythic modeled for later

Use rarity levels.

Common      Gray
Uncommon    Green
Rare        Blue
Epic        Purple
Legendary   Gold
Mythic      Red

For MVP, implement only:

Common
Uncommon
Rare
Epic
Legendary

Mythic can be added later.

Rarity Rules

Common items:

Only basic stats.

Uncommon items:

Better stats.

Rare items:

Better stats.
One small special effect.

Epic items:

Strong stats.
Stronger special effect.

Legendary items:

High stats.
Unique passive ability.
9. Equipment Levels — ✅ MaxLevel 20 and level-based stat scaling foundation added

Each equipment item has:

ItemID
Name
SlotType
Rarity
Level
MaxLevel
BaseStats
SpecialEffect
Icon

For MVP:

MaxLevel = 20

Each level increases item stats.

Every 5 levels gives a small bonus.

Example:

Plasma Needle Lv. 1
+12 AttackDamage

Plasma Needle Lv. 5
+28 AttackDamage
+3% FireRate

Plasma Needle Lv. 10
+50 AttackDamage
+5% FireRate
10. Equipment Merge System — ✅ 3-duplicate same-item rarity merge foundation added

Implement a simple duplicate merge system.

Example:

3 Common items of same type -> 1 Uncommon
3 Uncommon items of same type -> 1 Rare
3 Rare items of same type -> 1 Epic
3 Epic items of same type -> 1 Legendary

For MVP, same item merging is enough.

Example:

3x Basic Blaster Common = 1x Basic Blaster Uncommon

Do not require complex merging rules yet.

11. Starting Equipment — ✅ starting profile equips all 6 basic gear pieces

The player starts with basic gear.

Weapon: Basic Blaster
Wings: Starter Wings
Engine: Old Engine
Hull: Light Hull
Core: Small Battery
Radar: Basic Scanner

The starting plane should feel weak but playable.

12. In-Run Upgrade System

In-run upgrades are temporary.

They disappear after the run ends.

When the player collects enough XP, pause the game and show 3 random upgrade cards.

The player chooses one.

Upgrade categories:

Weapons
Passive stats
Dash ✅ lightweight audio hook added/trail upgrades
Defensive upgrades
Special ability upgrades
13. MVP In-Run Weapons

Implement these first:

Plasma Blaster

Main projectile weapon.

Upgrades:

Level 1: Unlock Plasma Blaster
Level 2: +Damage
Level 3: +FireRate
Level 4: +Projectile size
Level 5: Projectiles pierce 1 enemy
Homing Missiles

Missiles target nearby enemies.

Upgrades:

Level 1: Unlock missiles
Level 2: +1 missile
Level 3: Faster cooldown
Level 4: Bigger explosion
Level 5: Missiles split after hit
Laser Wings

Side lasers or short beam attacks.

Upgrades:

Level 1: Unlock side lasers
Level 2: +Damage
Level 3: Longer beam
Level 4: Faster firing
Level 5: Double laser beam
Orbit Blades

Energy blades rotate around the plane.

Upgrades:

Level 1: Unlock orbit blade
Level 2: +1 blade
Level 3: Larger radius
Level 4: Faster rotation
Level 5: Blades knock back enemies
14. MVP Passive Upgrades

Implement these:

Attack Boost
Fire Rate Boost
Movement Speed Boost
Max HP Boost
Armor Boost
Magnet Boost
Critical Chance Boost
Cooldown Reduction
XP Gain Boost

Each passive can have 5 levels.

Example:

Attack Boost Lv. 1: +10% damage
Attack Boost Lv. 2: +20% damage
Attack Boost Lv. 3: +30% damage
Attack Boost Lv. 4: +40% damage
Attack Boost Lv. 5: +50% damage
15. Weapon Evolution System

For MVP, implement weapon evolution if possible. If time is limited, create the base system but only add 1 or 2 evolutions.

Evolution happens when:

Weapon reaches max level
Required passive reaches required level
Player collects evolution chest or defeats boss

Example evolutions:

Plasma Blaster + Attack Boost = Plasma Storm
Homing Missiles + Cooldown Reduction = Rocket Swarm
Laser Wings + Critical Chance = Solar Splitter
Orbit Blades + Armor Boost = Neon Barrier

MVP minimum:

Plasma Blaster -> Plasma Storm
Homing Missiles -> Rocket Swarm
16. Neon Trail System

This should become the unique feature of the game.

When the player dashes, the plane leaves a neon trail.

The trail can damage enemies.

Basic behavior:

Dash ✅ lightweight audio hook added creates trail behind player.
Trail lasts 1.5 seconds.
Enemies touching trail take damage over time.
Trail damage scales with AttackDamage.

Possible in-run trail upgrades:

Longer Trail
Burning Trail
Electric Trail
Trail Explosion
Healing Trail
Double Dash ✅ lightweight audio hook added

Possible equipment effects:

Engine can increase trail length.
Core can increase trail damage.
Wings can add side sparks after dash.

For MVP, implement:

Dash ✅ lightweight audio hook added Trail Damage
Longer Trail upgrade
Trail Explosion upgrade
17. Enemy System

Enemies should spawn around the player or arena edges.

Use object pooling for performance.

Enemy data structure:

EnemyID
Name
HP
Damage
Speed
XPDrop
CoinDropChance
BehaviorType
ProjectileType
IsElite
MVP Enemy Types
Chaser Drone

Basic enemy.

Behavior:

Moves directly toward player.
Deals contact damage.
Low HP.
Fast Wing

Fast enemy.

Behavior:

Moves quickly toward player.
Low HP.
Low damage.
Shooter Drone

Ranged enemy.

Behavior:

Keeps distance.
Shoots simple bullets.
Shield Drone

Tank enemy.

Behavior:

High HP.
Slow movement.
Reduced frontal damage if possible.

If frontal shield is too complex, just make it high HP for MVP.

Mine Carrier

Hazard enemy.

Behavior:

Moves slowly.
Drops mines.
Mines explode after delay or on contact.
Splitter Orb

Splitting enemy.

Behavior:

When killed, splits into 2 smaller enemies.
18. Wave Timeline — ✅ 10-minute data timeline and warning lookup foundation added

The run lasts 10 minutes.

Use a timeline manager.

0:00 – 1:00

Enemy types:

Chaser Drone
Fast Wing

Low density.

Purpose:

Teach movement.
Let player collect first XP.
Give 2–3 level-ups quickly.
1:00 – 2:00

Enemy types:

Chaser Drone
Fast Wing
Shooter Drone

Increase spawn rate.

2:00 – 3:00

Enemy types:

Chaser Drone
Fast Wing
Shooter Drone
Shield Drone

Add warning at 2:50:

WARNING: SKY REAPER APPROACHING
3:00

Spawn Boss 1.

Pause or reduce normal enemy spawning during boss if needed.

3:00 – 6:00

Enemy types:

Shooter Drone
Shield Drone
Mine Carrier
Splitter Orb
Fast Wing

Increase difficulty.

Player should start forming a strong build.

6:00

Spawn Boss 2.

6:00 – 10:00

Enemy types:

All previous enemies
Elite Chaser
Elite Shooter
More mines
More splitters

At 7:30, spawn mini-boss. ✅ Viper Ace mini-boss event added

At 8:45, spawn mini-boss. ✅ Bombardier Prime mini-boss event added

At 9:30, increase music intensity and spawn rate. ✅ final-surge wave segment added

At 9:50, show warning:

FINAL BOSS INCOMING
10:00

Spawn Final Boss.

19. Bosses — ✅ boss schedule/config foundation added for 3:00, 6:00, and 10:00
Boss 1: Sky Reaper ✅ phase attack modes added

Time:

3:00

Role:

First skill check.

Abilities:

Charge toward player.
Fire cone bullets.
Summon small drones.
Create short laser line attack.

MVP implementation:

Phase 1:
- Moves toward player slowly.
- Every few seconds charges.
- Shoots 5 bullets in cone.

At 50% HP:
- Summons drones.
- Shoots faster.

Reward:

Rare upgrade choice
Coins
Small heal
Evolution chest if eligible
Boss 2: Neon Hydra ✅ phase attack modes added

Time:

6:00

Role:

Dodging and build check.

Abilities:

Rotating bullet circle.
Meteor/falling danger zones.
Summons minions.
Splits into 2 smaller heads at low HP.

MVP implementation:

Phase 1:
- Shoots circular bullets.
- Summons enemies.

At 50% HP:
- Spawns 2 smaller heads or increases attack speed.

Reward:

Legendary upgrade choice
Heal
Temporary damage boost
Evolution chest if eligible
Final Boss: Eclipse Core ✅ phase attack modes added

Time:

10:00

Role:

Final run climax.

Abilities:

Rotating laser arms.
Bullet rings.
Summons waves.
Creates danger zones.
Rage phase at 25% HP.

MVP implementation:

Phase 1:
- Stays near center.
- Fires bullet rings.
- Summons enemies.

Phase 2 at 50% HP:
- Adds rotating laser arms.

Phase 3 at 25% HP:
- Faster attacks.
- More enemy summons.

Win condition:

Player must defeat the final boss.

After final boss dies, show victory screen.

20. Rewards

After each run, calculate rewards.

Reward types:

Coins
Equipment pieces
Upgrade materials
Boss cores
XP/account level

For MVP:

Coins
Random equipment item ✅ reward drop table and item grant added
Upgrade material

Reward formula example:

Coins = baseCoins + enemiesKilled * coinPerKill + bossesKilled * bossCoinBonus

Equipment drop: ✅ tuned boss-based web drop table with pity/kill bonuses added

Boss 1 defeated: chance for Common/Uncommon item ✅ staged reward timing added
Boss 2 defeated: chance for Uncommon/Rare item ✅ staged reward timing added
Final Boss defeated: guaranteed Rare item, chance for Epic ✅ full boss + mini-boss progress grants guaranteed Rare and Epic chance
21. UI Screens
Main Menu

Buttons:

Play
Garage
Equipment
Missions
Settings ✅ web settings screen added

For MVP, only required:

Play ✅ web control added
Garage ✅ web garage screen added
Settings ✅ web settings screen added
Garage Screen

Layout similar to equipment-based RPG inventory.

Must show:

Plane preview in center
6 equipment slots around plane
Stats panel
Inventory grid
Upgrade button
Merge button
Equip button
Start button

Equipment slots around plane:

Top: Weapon
Left: Wings
Right: Engine
Bottom Left: Hull
Bottom: Core
Bottom Right: Radar
Inventory Grid

Show item cards.

Each card should display:

Icon
Rarity border
Level
Slot type icon
Item name on tap
In-Game HUD

Display:

Timer
HP bar ✅ Unity filled HP bar (color shifts green→red as HP drops) added
XP bar ✅ Unity filled XP bar added
Level
Coins collected
Dash ✅ lightweight audio hook added cooldown
Special ability charge ✅ Unity SPECIAL button with charge fill + Neon Nova ultimate added
Pause button ✅ Unity in-game pause/resume button added
Boss HP bar when boss active ✅ web boss health bar added and Unity boss/mini-boss HP bar added
Level-Up Screen

Pause game.

Show 3 cards.

Each card should display:

Upgrade icon
Upgrade name
Current level
Description
Rarity/color ✅ Unity upgrade cards now tinted and labeled by category (Weapon/Passive/Trail/Defense/Special)
Game Over Screen

Show:

Survived time
Enemies killed
Bosses defeated
Coins earned
Items found
Retry button
Garage button
Victory Screen ✅ lightweight audio hook added

Show:

Mission Complete
Final boss defeated
Enemies killed
Coins earned
Rewards
Continue button
22. Visual Style

Use neon sci-fi style.

Recommended colors:

Player: Cyan / Blue
Player trail: Cyan / Purple
XP shards: Green / Teal
Enemy bullets: Red / Orange
Boss attacks: Purple / Magenta
Rare items: Blue
Epic items: Purple
Legendary items: Gold

Background: ✅ Unity dark-sky background with neon grid, starfield, downward-scrolling neon lines, and parallax added

Dark digital sky
Grid effects
Stars
Moving neon lines
Subtle parallax

Important:

Gameplay readability is more important than effects.
Enemy bullets must be clearly visible.
Player hitbox must be clear. ✅ Unity cyan player plane view added
Boss attacks must have warnings. ✅ web danger zones added
23. Audio Direction

For MVP, simple sounds are enough.

Required sounds:

Player shooting ✅ lightweight audio hook added and Unity procedural shoot SFX added
Enemy hit ✅ web audio cue and hit burst added
Enemy death ✅ web audio cue and death burst added and Unity procedural death SFX added
XP collect ✅ web audio cue and pickup burst added and Unity procedural XP SFX added
Level up ✅ Unity procedural level-up chord added
Dash ✅ lightweight audio hook added and Unity procedural dash SFX added
Boss warning ✅ warning message and danger zones added and Unity procedural warning SFX added
Boss spawn ✅ boss warning/audio/music mode hooks added and Unity procedural boss-spawn SFX added
Player damage ✅ boss projectile/danger-zone damage path, audio cue, hit burst, and screen shake added
Game over ✅ lightweight audio hook added and Unity procedural game-over SFX added
Victory ✅ lightweight audio hook added and Unity procedural victory chord added
Player damage ✅ web audio cue/hit burst added and Unity procedural damage SFX added

Music:

Synthwave loop during normal gameplay. ✅ procedural run music drone added and Unity procedural music drone added
More intense loop during boss fights. ✅ boss/final music modes added and Unity boss-mode drone added
Final boss should have stronger music. ✅ Unity boss-mode drone covers the final boss
24. Data-Driven Architecture — ✅ initial Neon catalog/models separate data from systems

Build the systems data-driven.

Use config files, scriptable objects, JSON, or equivalent depending on engine.

Data should be separated from code.

Recommended data categories:

PlayerStatsConfig
EquipmentConfig
UpgradeConfig
EnemyConfig
WaveConfig
BossConfig
RewardConfig
RarityConfig

Do not hardcode all values inside gameplay scripts.

25. Suggested Code Architecture

Use managers/components like this:

GameManager
RunManager
PlayerController
PlayerStats
WeaponManager
EnemySpawner
WaveManager
BossManager
UpgradeManager
EquipmentManager
InventoryManager
RewardManager
UIManager
AudioManager
SaveManager
ObjectPoolManager
GameManager

Responsible for:

Global game state
Changing screens
Starting run
Ending run
Pause/resume
RunManager

Responsible for:

10-minute timer
Run state
Boss timing
Win/loss condition
Current run stats
PlayerController

Responsible for:

Movement
Dash ✅ lightweight audio hook added
Plane rotation/tilt
Collision detection
Taking damage
PlayerStats

Responsible for:

Combining base stats + equipment stats + temporary upgrades
Recalculating final stats
WeaponManager

Responsible for:

Equipped starting weapon
Temporary weapons
Fire timers
Weapon upgrades
Weapon evolutions
EnemySpawner

Responsible for:

Spawning enemies
Using wave rules
Object pooling ✅ Unity runtime render pools added
Spawn positions
WaveManager

Responsible for:

Timeline difficulty
Enemy spawn rates
Wave events
Mini-boss events
BossManager

Responsible for:

Boss spawning ✅ boss warning/audio/music mode hooks added
Boss phases
Boss HP bar
Boss rewards
UpgradeManager

Responsible for:

XP
Level-up
Random upgrade choices
Applying selected upgrade
Evolution checks
EquipmentManager

Responsible for:

Equipping items
Calculating equipment stats
Upgrading items
Merging items
InventoryManager

Responsible for:

Owned items
Adding rewards
Removing merged items
Sorting inventory
RewardManager

Responsible for:

Coins
Item drops
Boss rewards
End-of-run rewards
SaveManager

Responsible for:

Saving coins
Saving inventory
Saving equipped items
Saving progress
Saving settings ✅ save validation/reset/export polish added
26. Save Data

Save these values:

PlayerCoins
PlayerMaterials
OwnedEquipmentItems
EquippedWeaponItemID
EquippedWingsItemID
EquippedEngineItemID
EquippedHullItemID
EquippedCoreItemID
EquippedRadarItemID
UnlockedWeapons
CompletedRuns
BestSurvivalTime
BossesDefeated
Settings ✅ web settings screen added

Use local save for MVP.

27. MVP Item List

Create around 24 equipment items for the first version.

Weapons
Basic Blaster
Twin Cannon
Plasma Needle
Railgun Nose
Wings
Starter Wings
Falcon Wings
Combat Wings
Neon Wings
Engines
Old Engine
Turbo Engine
Ion Engine
Phantom Engine
Hulls
Light Hull
Steel Hull
Guardian Frame
Solar Shield Hull
Cores
Small Battery
Fusion Core
Plasma Core
Overdrive Core
Radars
Basic Scanner
Magnet Scanner
Hunter Radar
Quantum Sensor
28. MVP Upgrade List

Create these in-run upgrades.

Weapons
Plasma Blaster
Homing Missiles
Laser Wings
Orbit Blades
Passives
Attack Boost
Fire Rate Boost
Movement Speed Boost
Max HP Boost
Armor Boost
Magnet Boost
Critical Chance Boost
Cooldown Reduction
XP Gain Boost
Trail upgrades
Longer Trail
Trail Damage Boost
Trail Explosion
29. Difficulty Goals

The game should feel like this:

First minute

Easy.

Player should not die unless they do nothing.

1–3 minutes

Moderate.

Player learns movement and upgrades.

Boss 1

Simple but exciting.

Should be beatable with basic understanding.

3–6 minutes

More enemies.

The player should feel stronger but pressured.

Boss 2

Harder and more bullet-heavy.

Should punish bad movement.

6–10 minutes

Chaotic but readable.

The player should feel powerful, but enemies should also be dangerous.

Final boss

Hardest part.

The player should need a strong build and good dodging.

30. Balancing Starting Values

Use these approximate values for first testing.

Player
MaxHP: 100
MovementSpeed: 5
AttackDamage: 10
FireRate: 1 shot per second
Armor: 0
CriticalChance: 5%
CriticalDamage: 200%
MagnetRange: 2.5
Dash ✅ lightweight audio hook addedCooldown: 4 seconds
Dash ✅ lightweight audio hook addedDistance: 4
Basic Blaster
Damage: 10
FireRate: 1/sec
ProjectileSpeed: 12
Targeting: nearest enemy
Chaser Drone
HP: 20
Damage: 10
Speed: 2
XPDrop: 1
Fast Wing
HP: 12
Damage: 8
Speed: 3.5
XPDrop: 1
Shooter Drone
HP: 30
Damage: 8 projectile
Speed: 1.5
XPDrop: 2
Boss 1
HP: 2500
ContactDamage: 20
BulletDamage: 10
Boss 2
HP: 6000
ContactDamage: 25
BulletDamage: 12
Final Boss
HP: 12000
ContactDamage: 30
BulletDamage: 15

These numbers are only starting values. Tune after testing.

31. Performance Requirements

The game may have many enemies on screen.

Use:

Object pooling
Simple enemy AI
Limited projectile lifetime ✅ core projectile lifetime and Unity projectile view cap added
Distance-based cleanup
Batch-friendly effects ✅ Unity runtime uses simple sprite/line render pools for MVP
Optimized particles

Target:

60 FPS on mid-range mobile devices ✅ Unity runtime caps target frame rate at 60; Unity Editor mobile Boot verification and Android APK smoke build passed; physical device verification pending

For MVP, support at least:

100 enemies on screen
200 projectiles/effects on screen

Later optimize for more.

32. Important Gameplay Feel

The game must feel satisfying.

Prioritize:

Fast XP collection
Clear hit feedback ✅ web hit/death/XP/player-damage feedback added and Unity death/player-damage particle bursts added
Smooth movement
Good dash feeling
Readable bullets
Strong weapon upgrades
Explosive enemy deaths ✅ particle-style death bursts added (web) and Unity death-explosion bursts added (bigger for bosses/mini-bosses)
Rewarding level-up choices ✅ Unity mobile upgrade cards now apply choices without restarting the run

The player should feel weak at the start of a run, then powerful by minute 7–10.

The garage should make the player excited to improve the aircraft.

33. What Not To Do

Do not copy:

Survivor.io UI
Survivor.io characters
Survivor.io exact equipment names
Survivor.io exact weapons
Neon Wings exact aircraft
Neon Wings exact abilities
Any copyrighted icons or assets

Do not make the permanent upgrade system a simple list of stat buttons.

Do not overload MVP with too many systems.

Do not add multiplayer.

Do not add complex story yet.

Do not add too many currencies yet.

For MVP, keep it focused:

Garage equipment
10-minute run
Temporary upgrades
Enemies
Bosses
Rewards
34. Development Order

Build in this order.

Phase 1 — Core Gameplay
Create player plane ✅ core state
Add movement ✅ target/drag-compatible movement logic
Add dash ✅ cooldown, invulnerability, neon trail
Add auto-shooting ✅ nearest-enemy auto aim
Add enemy spawning ✅ wave-driven spawn logic
Add enemy damage/death ✅ projectile/trail damage and cleanup
Add XP drops ✅ shard drops and magnet pickup
Add level-up screen ✅ core 3-card draft state (presentation pending)
Add simple upgrades ✅ upgrade application and stat modifiers
Phase 2 — Run Structure
Add 10-minute timer ✅ HUD timer and core elapsed timer
Add wave timeline ✅ web shell uses core timeline
Add boss spawn at 3:00 ✅ Sky Reaper spawn and warning
Add boss spawn at 6:00 ✅ Neon Hydra spawn hook
Add final boss at 10:00 ✅ Eclipse Core spawn/victory hook
Add win/loss screens ✅ web results screen
Phase 3 — Equipment
Add inventory ✅ web inventory grid and Unity scrollable inventory grid
Add 6 equipment slots ✅ web slot layout and Unity equipped-slot summary
Add equipment stats ✅ stats preview from equipment
Add equip/unequip ✅ web garage controls and Unity touch Equip/Unequip buttons
Add stat calculation ✅ web stat recompute
Add equipment upgrade ✅ web coin upgrade button and Unity touch Upgrade button
Add merge system ✅ web 3-duplicate merge button and Unity touch Merge x3 button
Phase 4 — Rewards
Add coins ✅ web coin payout and Unity results coin payout
Add item drops ✅ web reward equipment grants and Unity post-run equipment drops
Add boss rewards ✅ boss-based drop table (web) and Unity boss/mini-boss/final-boss drop tiers
Add end-of-run rewards ✅ web results payout and Unity results payout
Add save/load ✅ web localStorage save/load and Unity PlayerPrefs JSON save/load (NeonSaveService)
Phase 5 — Polish
Add neon effects ✅ Unity animated neon grid/starfield/parallax background and player plane view added
Add better UI ✅ Unity filled HP/XP bars added to the in-game HUD
Add sound effects ✅ Unity procedural SFX (shoot/death/XP/level-up/dash/warning/boss/damage/game-over/victory)
Add music ✅ Unity procedural synth-drone music with normal/boss modes
Add hit feedback ✅ web combat feedback events added
Add particles ✅ canvas particle-style bursts added (web) and Unity pooled particle bursts added
Balance difficulty ✅ early/mid/late pacing, boss pressure, mini-bosses, and rewards tuned
Optimize performance ✅ initial Unity render pooling and mobile frame cap added; Unity Editor mobile Boot verification and Android APK smoke build passed; physical device verification pending
35. Final MVP Definition

The MVP is complete when the player can:

Open the game
Go to garage
Equip 6 aircraft parts
Start a 10-minute run
Move the plane
Dash ✅ lightweight audio hook added with neon trail
Auto-shoot enemies
Collect XP
Level up
Choose temporary upgrades
Fight Boss 1 at 3:00
Fight Boss 2 at 6:00
Fight Final Boss at 10:00
Win or lose
Receive coins and equipment
Return to garage
Upgrade equipment
Merge duplicates
Save progress
Play again stronger
36. One-Sentence Vision

Build a neon aircraft roguelite survival game where the player starts with a simple plane, survives intense 10-minute enemy waves, creates temporary weapon builds during the run, and permanently improves the aircraft through collectible garage equipment.
