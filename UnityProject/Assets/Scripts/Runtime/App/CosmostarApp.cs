using System;
using System.Collections.Generic;
using Cosmostar.Core.Models;
using Cosmostar.Core.Systems;
using Cosmostar.Runtime.Gameplay;
using Cosmostar.Runtime.Services;
using Cosmostar.Runtime.Systems;
using UnityEngine;

namespace Cosmostar.Runtime.App
{
    public sealed class CosmostarApp : MonoBehaviour
    {
        private enum ScreenState
        {
            Boot,
            MetaHub,
            MissionSelect,
            Run,
            Results
        }

        private readonly UpgradeDraftSystem _upgradeDraftSystem = new UpgradeDraftSystem();
        private readonly MissionSystem _missionSystem = new MissionSystem();
        private readonly MissionRuleSystem _missionRuleSystem = new MissionRuleSystem();
        private readonly EconomySystem _economySystem = new EconomySystem();
        private readonly MetaProgressionSystem _metaProgressionSystem = new MetaProgressionSystem();
        private readonly PlayerController _playerController = new PlayerController();
        private readonly WeaponSystem _weaponSystem = new WeaponSystem();
        private readonly AbilitySystem _abilitySystem = new AbilitySystem();

        private VerticalSliceCatalog _catalog;
        private SaveProfile _profile;
        private SaveSystem _saveSystem;
        private AnalyticsService _analyticsService;
        private IAdsService _adsService;

        private ScreenState _screen = ScreenState.Boot;
        private DailyContract _dailyContract;
        private RunSession _runSession;
        private RunSummary _pendingSummary;
        private MissionEvaluation _pendingEvaluation;
        private RewardBreakdown _pendingRewards;
        private List<UnlockTrackEntry> _pendingUnlocks = new List<UnlockTrackEntry>();
        private MissionDef _selectedMission;
        private Vector2 _moduleScroll;
        private Vector2 _missionScroll;
        private int _selectedWeaponIndex;
        private float _bootTimer;
        private string _metaNotice = string.Empty;
        private float _metaNoticeTimer;
        private float _lastTapTime = -10f;
        private Vector2 _lastTapPosition;

        private Texture2D _pixel;
        private Texture2D _circle;
        private Vector2[] _starField;
        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _smallStyle;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            _saveSystem = new SaveSystem();
            _analyticsService = new AnalyticsService();
            _adsService = new MockAdsService();

            _catalog = CatalogProvider.Load();
            _profile = _saveSystem.Load();
            ProfileQueries.EnsureDefaultState(_profile, _catalog);
            _dailyContract = _missionSystem.GetDailyContract(_catalog.Missions, DateTime.Now.Date);

            BuildVisualAssets();
            TrackScreen(ScreenState.Boot);
        }

        private void Update()
        {
            _bootTimer += Time.deltaTime;
            if (_metaNoticeTimer > 0f)
            {
                _metaNoticeTimer -= Time.deltaTime;
                if (_metaNoticeTimer <= 0f)
                {
                    _metaNotice = string.Empty;
                }
            }

            if (_screen == ScreenState.Boot && _bootTimer >= 0.45f)
            {
                SetScreen(ScreenState.MetaHub);
            }

            if (_screen == ScreenState.Run && _runSession != null)
            {
                TickRun(Time.deltaTime);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (_screen == ScreenState.Run && _runSession != null)
                {
                    _runSession.Paused = true;
                }

                _saveSystem.Save(_profile);
            }
        }

        private void OnApplicationQuit()
        {
            _saveSystem.Save(_profile);
        }

        private void OnGUI()
        {
            EnsureGuiStyles();
            DrawBackground();

            switch (_screen)
            {
                case ScreenState.Boot:
                    DrawBoot();
                    break;
                case ScreenState.MetaHub:
                    DrawMetaHub();
                    break;
                case ScreenState.MissionSelect:
                    DrawMissionSelect();
                    break;
                case ScreenState.Run:
                    DrawRun();
                    break;
                case ScreenState.Results:
                    DrawResults();
                    break;
            }
        }

        private void EnsureGuiStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            BuildStyles();
        }

        private void DrawBoot()
        {
            var area = GetSafeArea();
            GUI.Label(new Rect(area.x, area.y + 220f, area.width, 70f), "COSMOSTAR", _titleStyle);
            GUI.Label(new Rect(area.x, area.y + 300f, area.width, 32f), "Neon portrait roguelite vertical slice", _subtitleStyle);
        }

        private void DrawMetaHub()
        {
            var area = GetSafeArea();
            DrawHeader("Meta Hub", "Build a fair run, then launch fast.");

            GUI.Label(new Rect(area.x + 28f, area.y + 110f, area.width * 0.48f, 28f), "Credits: " + _profile.SoftCurrency, _subtitleStyle);
            GUI.Label(new Rect(area.x + 28f, area.y + 140f, area.width * 0.48f, 28f), "Shards: " + _profile.ModuleShards + "   Streak: " + _profile.CurrentStreak, _bodyStyle);
            GUI.Label(new Rect(area.x + area.width * 0.52f, area.y + 110f, area.width * 0.4f, 28f), "Track XP: " + _profile.UnlockTrackXp, _subtitleStyle);
            GUI.Label(new Rect(area.x + area.width * 0.52f, area.y + 140f, area.width * 0.4f, 28f), "Daily: " + _dailyContract.Label, _bodyStyle);
            if (!string.IsNullOrEmpty(_metaNotice))
            {
                GUI.Label(new Rect(area.x + 28f, area.y + 160f, area.width - 56f, 24f), _metaNotice, _smallStyle);
            }

            DrawUnlockTrackCard(area);
            DrawWeaponSelector(area);
            DrawModules(area);

            if (!_profile.SeenFtue)
            {
                DrawFtueCard(area);
            }

            if (GUI.Button(new Rect(area.x + 28f, area.yMax - 96f, area.width - 56f, 58f), "Select Mission", _buttonStyle))
            {
                SetScreen(ScreenState.MissionSelect);
            }
        }

        private void DrawMissionSelect()
        {
            var area = GetSafeArea();
            DrawHeader("Mission Select", "Choose a contract and jump back into action.");

            if (GUI.Button(new Rect(area.x + 28f, area.y + 108f, 150f, 42f), "Back", _buttonStyle))
            {
                SetScreen(ScreenState.MetaHub);
            }

            var missions = GetMissionSelectOrder();
            var viewRect = new Rect(area.x + 20f, area.y + 170f, area.width - 40f, area.height - 210f);
            var contentRect = new Rect(0f, 0f, viewRect.width - 22f, 130f * missions.Count);
            _missionScroll = GUI.BeginScrollView(viewRect, _missionScroll, contentRect);

            for (var index = 0; index < missions.Count; index++)
            {
                var mission = missions[index];
                var progress = ProfileQueries.GetMissionProgress(_profile, mission.Id);
                var rules = _missionRuleSystem.Resolve(mission);
                var modifierText = mission.ModifierText + (rules.AnomalyKind == RunAnomalyKind.None ? string.Empty : " | " + rules.AnomalyLabel);
                var rect = new Rect(0f, index * 130f, contentRect.width, 114f);
                DrawCard(rect, mission.Id == _dailyContract.MissionId ? new Color(0f, 0.8f, 0.5f, 0.2f) : new Color(0f, 0f, 0f, 0.42f));
                GUI.Label(new Rect(rect.x + 18f, rect.y + 10f, rect.width - 160f, 28f), mission.DisplayName, _subtitleStyle);
                GUI.Label(new Rect(rect.x + 18f, rect.y + 42f, rect.width - 180f, 42f), mission.Description + " | " + modifierText, _smallStyle);
                GUI.Label(new Rect(rect.x + 18f, rect.y + 84f, 200f, 24f), "Stars: " + (progress == null ? 0 : progress.StarsEarned) + "   Reward: " + mission.Reward.SoftCurrency, _smallStyle);

                if (GUI.Button(new Rect(rect.width - 128f, rect.y + 28f, 112f, 54f), "Launch", _buttonStyle))
                {
                    StartMission(mission);
                }
            }

            GUI.EndScrollView();
        }

        private List<MissionDef> GetMissionSelectOrder()
        {
            var ordered = new List<MissionDef>(_catalog.Missions);
            ordered.Sort((a, b) =>
            {
                var aDaily = a.Id == _dailyContract.MissionId;
                var bDaily = b.Id == _dailyContract.MissionId;
                if (aDaily != bDaily)
                {
                    return aDaily ? -1 : 1;
                }

                var aProgress = ProfileQueries.GetMissionProgress(_profile, a.Id);
                var bProgress = ProfileQueries.GetMissionProgress(_profile, b.Id);
                var aStars = aProgress == null ? 0 : aProgress.StarsEarned;
                var bStars = bProgress == null ? 0 : bProgress.StarsEarned;
                if (aStars != bStars)
                {
                    return aStars.CompareTo(bStars);
                }

                return a.DifficultyRating.CompareTo(b.DifficultyRating);
            });

            return ordered;
        }

        private void DrawRun()
        {
            if (_runSession == null)
            {
                return;
            }

            DrawGameplayField();
            DrawRunHud();

            if (_runSession.DraftOpen)
            {
                DrawDraftOverlay();
            }
            else if (_runSession.AwaitingRewardedRevive)
            {
                DrawReviveOverlay();
            }
            else if (_runSession.TutorialOpen)
            {
                DrawTutorialOverlay();
            }
            else if (_runSession.Paused)
            {
                DrawPauseOverlay();
            }

            if (!string.IsNullOrEmpty(_runSession.RewardMessage))
            {
                GUI.Label(new Rect(0f, Screen.height - 120f, Screen.width, 28f), _runSession.RewardMessage, _subtitleStyle);
            }
        }

        private void DrawResults()
        {
            var area = GetSafeArea();
            DrawHeader("Results", "Push the next run with better tools.");

            DrawCard(new Rect(area.x + 18f, area.y + 110f, area.width - 36f, 314f), new Color(0f, 0f, 0f, 0.46f));
            GUI.Label(new Rect(area.x + 36f, area.y + 128f, area.width - 72f, 36f), _selectedMission.DisplayName, _subtitleStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 170f, area.width - 72f, 32f), "Outcome: " + (_pendingEvaluation.Completed ? "Complete" : "Failed"), _bodyStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 202f, area.width - 72f, 32f), "Stars: " + _pendingEvaluation.StarsEarned + "   Kills: " + _pendingSummary.Kills + "   Grazes: " + _pendingSummary.Grazes, _bodyStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 234f, area.width - 72f, 32f), "Credits: " + _pendingRewards.TotalSoftCurrency + "   Shards: " + _pendingRewards.TotalModuleShards + "   Track XP: " + _pendingRewards.TotalUnlockTrackXp, _bodyStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 266f, area.width - 72f, 32f), "Best Combo: x" + _pendingSummary.BestComboCount + "   Anomaly Events: " + _pendingSummary.AnomalyEventsTriggered, _bodyStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 298f, area.width - 72f, 32f), "Salvage +" + _pendingRewards.SalvageBonus + "   Graze +" + _pendingRewards.GrazeBonus + "   Anomaly +" + _pendingRewards.AnomalyBonus, _smallStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 322f, area.width - 72f, 24f), GetRewardCompositionLabel(), _smallStyle);

            var unlockText = _pendingUnlocks.Count == 0 ? "No new unlocks yet." : "Unlocked: " + string.Join(", ", GetUnlockLabels(_pendingUnlocks));
            GUI.Label(new Rect(area.x + 36f, area.y + 346f, area.width - 72f, 48f), unlockText, _smallStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 396f, area.width - 72f, 32f), "Next track: " + GetProjectedNextUnlockSummary(_pendingRewards), _smallStyle);

            if (_pendingRewards.BaseReward.DoubleRewardEligible && GUI.Button(new Rect(area.x + 28f, area.y + 430f, area.width - 56f, 58f), "Watch Rewarded Clip: Double Rewards", _buttonStyle))
            {
                string rewardMessage;
                if (_adsService.TryShowRewarded(RewardedPlacement.DoubleResults, out rewardMessage))
                {
                    _pendingRewards.Doubled = true;
                    CollectResults(false);
                }
            }

            if (GUI.Button(new Rect(area.x + 28f, area.y + 506f, area.width - 56f, 58f), _pendingRewards.Doubled ? "Collect Doubled Rewards" : "Collect Rewards", _buttonStyle))
            {
                CollectResults(false);
            }

            if (GUI.Button(new Rect(area.x + 28f, area.y + 580f, area.width - 56f, 58f), "Collect And Replay", _buttonStyle))
            {
                CollectResults(true);
            }
        }

        private string GetRewardCompositionLabel()
        {
            return "Base " + _pendingRewards.BaseReward.SoftCurrency
                   + "   Daily +" + _pendingRewards.DailyBonus
                   + "   Streak +" + _pendingRewards.StreakBonus
                   + "   Mastery +" + _pendingRewards.MasteryBonus;
        }

        private void DrawHeader(string title, string subtitle)
        {
            var area = GetSafeArea();
            GUI.Label(new Rect(area.x, area.y + 18f, area.width, 42f), title, _titleStyle);
            GUI.Label(new Rect(area.x, area.y + 62f, area.width, 28f), subtitle, _subtitleStyle);
        }

        private void DrawUnlockTrackCard(Rect area)
        {
            DrawCard(new Rect(area.x + 18f, area.y + 176f, area.width - 36f, 76f), new Color(0f, 0f, 0f, 0.42f));
            GUI.Label(new Rect(area.x + 32f, area.y + 184f, area.width - 64f, 24f), "Unlock Track", _subtitleStyle);
            GUI.Label(new Rect(area.x + 32f, area.y + 212f, area.width - 64f, 20f), GetNextUnlockSummary(), _smallStyle);
            GUI.Label(new Rect(area.x + area.width - 200f, area.y + 184f, 152f, 20f), "Slots " + ProfileQueries.GetEquippedModuleCount(_profile) + "/" + MetaProgressionSystem.MaxEquippedModules, _smallStyle);

            var progress = GetNextUnlockProgress();
            GUI.color = new Color(0.15f, 0.22f, 0.28f, 0.85f);
            GUI.DrawTexture(new Rect(area.x + 34f, area.y + 236f, area.width - 68f, 8f), _pixel);
            GUI.color = new Color(0.2f, 1f, 0.72f, 0.85f);
            GUI.DrawTexture(new Rect(area.x + 34f, area.y + 236f, (area.width - 68f) * progress, 8f), _pixel);
            GUI.color = Color.white;
        }

        private void DrawFtueCard(Rect area)
        {
            DrawCard(new Rect(area.x + 18f, area.yMax - 178f, area.width - 36f, 70f), new Color(0f, 0.18f, 0.18f, 0.5f));
            GUI.Label(new Rect(area.x + 28f, area.yMax - 168f, area.width - 56f, 20f), "First run tips", _subtitleStyle);
            GUI.Label(new Rect(area.x + 28f, area.yMax - 146f, area.width - 56f, 38f), "Drag anywhere to move, tap Dash to dodge heavy shots, and watch warning markers before they fire.", _smallStyle);
        }

        private void DrawWeaponSelector(Rect area)
        {
            DrawCard(new Rect(area.x + 18f, area.y + 266f, area.width - 36f, 120f), new Color(0f, 0f, 0f, 0.42f));
            GUI.Label(new Rect(area.x + 34f, area.y + 278f, area.width - 80f, 28f), "Weapon Family", _subtitleStyle);

            for (var index = 0; index < _catalog.Weapons.Count; index++)
            {
                var weapon = _catalog.Weapons[index];
                var rect = new Rect(area.x + 34f + index * ((area.width - 100f) / 3f), area.y + 318f, (area.width - 120f) / 3f, 46f);
                var label = index == _selectedWeaponIndex ? "[" + weapon.DisplayName + "]" : weapon.DisplayName;
                if (GUI.Button(rect, label, _buttonStyle))
                {
                    _selectedWeaponIndex = index;
                }
            }
        }

        private void DrawModules(Rect area)
        {
            DrawCard(new Rect(area.x + 18f, area.y + 404f, area.width - 36f, area.height - 522f), new Color(0f, 0f, 0f, 0.42f));
            GUI.Label(new Rect(area.x + 34f, area.y + 418f, area.width - 72f, 28f), "Modules", _subtitleStyle);
            GUI.Label(new Rect(area.x + 34f, area.y + 442f, area.width - 72f, 18f), "Equip up to " + MetaProgressionSystem.MaxEquippedModules + " modules before launch.", _smallStyle);

            var viewRect = new Rect(area.x + 28f, area.y + 454f, area.width - 56f, area.height - 574f);
            var contentRect = new Rect(0f, 0f, viewRect.width - 24f, _catalog.Modules.Count * 110f);
            _moduleScroll = GUI.BeginScrollView(viewRect, _moduleScroll, contentRect);

            for (var index = 0; index < _catalog.Modules.Count; index++)
            {
                var module = _catalog.Modules[index];
                var progress = ProfileQueries.GetModuleProgress(_profile, module.Id);
                var rowRect = new Rect(0f, index * 110f, contentRect.width, 96f);
                DrawCard(rowRect, progress != null && progress.Equipped ? new Color(0f, 0.95f, 0.52f, 0.17f) : new Color(1f, 1f, 1f, 0.05f));
                GUI.Label(new Rect(rowRect.x + 14f, rowRect.y + 10f, rowRect.width - 170f, 24f), module.DisplayName, _subtitleStyle);
                GUI.Label(new Rect(rowRect.x + 14f, rowRect.y + 36f, rowRect.width - 180f, 38f), module.Description, _smallStyle);
                GUI.Label(new Rect(rowRect.x + 14f, rowRect.y + 72f, 150f, 18f), "Lv " + (progress == null ? 0 : progress.Level) + "/" + module.MaxLevel, _smallStyle);

                if (progress != null && progress.Unlocked)
                {
                    if (GUI.Button(new Rect(rowRect.width - 132f, rowRect.y + 10f, 118f, 34f), progress.Equipped ? "Unequip" : "Equip", _buttonStyle))
                    {
                        var wasEquipped = progress.Equipped;
                        if (_metaProgressionSystem.ToggleEquip(_profile, module.Id))
                        {
                            _saveSystem.Save(_profile);
                            ShowMetaNotice(wasEquipped ? "Module disengaged." : "Module slotted.");
                        }
                        else
                        {
                            ShowMetaNotice("Module slots full. Unequip one first.");
                        }
                    }

                    var upgradeCost = module.UpgradeCost * progress.Level;
                    var upgradeLabel = progress.Level >= module.MaxLevel ? "Maxed" : "Upgrade " + upgradeCost + " shards";
                    if (GUI.Button(new Rect(rowRect.width - 132f, rowRect.y + 52f, 118f, 34f), upgradeLabel, _buttonStyle))
                    {
                        if (_metaProgressionSystem.TryUnlockOrUpgradeModule(_profile, module))
                        {
                            _saveSystem.Save(_profile);
                            ShowMetaNotice(module.DisplayName + " upgraded.");
                        }
                        else if (progress.Level < module.MaxLevel)
                        {
                            ShowMetaNotice("Not enough module shards.");
                        }
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(rowRect.width - 132f, rowRect.y + 28f, 118f, 40f), "Unlock " + module.UnlockCost, _buttonStyle))
                    {
                        if (_metaProgressionSystem.TryUnlockOrUpgradeModule(_profile, module))
                        {
                            _saveSystem.Save(_profile);
                            ShowMetaNotice(progress != null && progress.Equipped ? "Module unlocked and equipped." : "Module unlocked. Free a slot to equip it.");
                        }
                        else
                        {
                            ShowMetaNotice("Not enough credits for that module.");
                        }
                    }
                }
            }

            GUI.EndScrollView();
        }

        private void DrawGameplayField()
        {
            var fieldRect = GetGameplayFieldRect();
            DrawCard(fieldRect, new Color(0f, 0.05f, 0.08f, 0.65f));
            DrawGrid(fieldRect);
            DrawDefenseWarning(fieldRect);

            DrawCircle(ToScreen(_runSession.Player.Position, fieldRect), 34f, new Color(0.3f, 1f, 0.78f, _runSession.Player.InvulnerabilityTimer > 0f ? 0.55f : 0.9f));
            if (_runSession.Player.InvulnerabilityTimer > 0f)
            {
                var invulnPulse = 40f + Mathf.Sin(Time.time * 20f) * 4f;
                DrawCircle(ToScreen(_runSession.Player.Position, fieldRect), invulnPulse, new Color(0.34f, 1f, 0.92f, 0.22f));
            }

            DrawCircle(ToScreen(_runSession.Player.Position + new Vector2(0f, 0.015f), fieldRect), 14f, new Color(1f, 1f, 1f, 0.8f));

            for (var index = 0; index < _runSession.Pickups.Count; index++)
            {
                DrawCircle(ToScreen(_runSession.Pickups[index].Position, fieldRect), 12f, new Color(0.4f, 1f, 0.25f, 0.85f));
            }

            DrawAnomalyTelegraphs(fieldRect);
            DrawAttackTelegraphs(fieldRect);
            DrawCombatEffects(fieldRect);

            for (var index = 0; index < _runSession.Projectiles.Count; index++)
            {
                var projectile = _runSession.Projectiles[index];
                if (projectile.IsCritical)
                {
                    DrawCircle(ToScreen(projectile.Position, fieldRect), projectile.Radius + 7f, new Color(1f, 0.9f, 0.18f, 0.2f));
                }

                DrawCircle(ToScreen(projectile.Position, fieldRect), projectile.Radius, projectile.Color);
            }

            for (var index = 0; index < _runSession.Enemies.Count; index++)
            {
                var enemy = _runSession.Enemies[index];
                var color = ResolveEnemyColor(enemy.Def.Archetype);
                if (enemy.SlowTimer > 0f)
                {
                    color = Color.Lerp(color, Color.cyan, 0.45f);
                }

                var radius = enemy.Def.IsBoss ? 58f : enemy.Def.Archetype == EnemyArchetype.EliteWarden ? 34f : 22f;
                DrawCircle(ToScreen(enemy.Position, fieldRect), radius, color);
                DrawCircle(ToScreen(enemy.Position, fieldRect), Mathf.Max(8f, radius * (enemy.Hull / enemy.MaxHull)), new Color(1f, 1f, 1f, 0.12f));
            }

            DrawOffscreenEnemyIndicators(fieldRect);
        }

        private void DrawOffscreenEnemyIndicators(Rect fieldRect)
        {
            for (var index = 0; index < _runSession.Enemies.Count; index++)
            {
                var enemy = _runSession.Enemies[index];
                if (enemy.Position.x >= 0f && enemy.Position.x <= 1f && enemy.Position.y >= 0f && enemy.Position.y <= 1f)
                {
                    continue;
                }

                var clamped = new Vector2(Mathf.Clamp(enemy.Position.x, 0.04f, 0.96f), Mathf.Clamp(enemy.Position.y, 0.08f, 0.96f));
                var edge = ToScreen(clamped, fieldRect);
                var towardEnemy = new Vector2(enemy.Position.x - clamped.x, enemy.Position.y - clamped.y);
                var magnitude = Mathf.Max(0.0001f, towardEnemy.magnitude);
                towardEnemy /= magnitude;
                var tail = edge - towardEnemy * 26f;
                var color = enemy.Def.IsBoss ? new Color(1f, 0.3f, 0.3f, 0.9f) : new Color(1f, 0.75f, 0.3f, 0.8f);
                DrawLine(tail, edge, color, enemy.Def.IsBoss ? 6f : 4f);
                DrawCircle(edge, enemy.Def.IsBoss ? 10f : 7f, color);
            }
        }

        private void DrawAnomalyTelegraphs(Rect fieldRect)
        {
            for (var index = 0; index < _runSession.AnomalyTelegraphs.Count; index++)
            {
                var anomaly = _runSession.AnomalyTelegraphs[index];
                var progress = anomaly.TotalDuration <= 0f ? 1f : Mathf.Clamp01(1f - anomaly.RemainingDuration / anomaly.TotalDuration);
                var alpha = 0.18f + progress * 0.28f + Mathf.Sin(Time.time * 16f) * 0.05f;
                var color = anomaly.Kind == RunAnomalyKind.SolarFlare ? new Color(1f, 0.26f, 0.18f, alpha) : new Color(1f, 0.78f, 0.22f, alpha);

                if (anomaly.Shape == RunAnomalyShape.VerticalLane)
                {
                    var laneX = fieldRect.x + anomaly.Position.x * fieldRect.width;
                    var laneWidth = anomaly.Width * fieldRect.width;
                    GUI.color = color;
                    GUI.DrawTexture(new Rect(laneX - laneWidth * 0.5f, fieldRect.y, laneWidth, fieldRect.height), _pixel);
                    GUI.color = Color.white;
                    DrawLine(new Vector2(laneX, fieldRect.y + 10f), new Vector2(laneX, fieldRect.yMax - 10f), new Color(color.r, color.g, color.b, 0.62f), 4f + progress * 7f);
                    continue;
                }

                var radius = anomaly.Radius * Mathf.Min(fieldRect.width, fieldRect.height);
                var center = ToScreen(anomaly.Position, fieldRect);
                DrawCircle(center, radius * (0.82f + progress * 0.22f), color);
                DrawCircle(center, radius * 0.35f, new Color(color.r, color.g, color.b, 0.25f + progress * 0.3f));
            }
        }

        private void DrawAttackTelegraphs(Rect fieldRect)
        {
            for (var index = 0; index < _runSession.AttackTelegraphs.Count; index++)
            {
                var telegraph = _runSession.AttackTelegraphs[index];
                var direction = telegraph.Direction.sqrMagnitude > 0.0001f ? telegraph.Direction.normalized : Vector2.down;
                var progress = telegraph.TotalDuration <= 0f ? 1f : Mathf.Clamp01(1f - telegraph.RemainingDuration / telegraph.TotalDuration);
                var start = ToScreen(telegraph.Origin, fieldRect);
                var endpoint = telegraph.Origin + direction * (telegraph.IsBossShot ? 0.78f : 0.5f);
                var end = ToScreen(endpoint, fieldRect);
                var pulse = 0.45f + Mathf.Sin(Time.time * 18f) * 0.12f + progress * 0.3f;
                var color = telegraph.IsBossShot ? new Color(1f, 0.18f, 0.2f, pulse) : new Color(1f, 0.62f, 0.16f, pulse);

                DrawLine(start, end, color, telegraph.IsBossShot ? 10f : 7f);
                DrawCircle(start, telegraph.IsBossShot ? 19f : 13f, new Color(color.r, color.g, color.b, 0.26f + progress * 0.24f));
                DrawCircle(end, telegraph.IsBossShot ? 13f : 9f, new Color(color.r, color.g, color.b, 0.18f + progress * 0.2f));
            }
        }

        private void DrawDefenseWarning(Rect fieldRect)
        {
            var shieldRatio = _runSession.Player.MaxShield <= 0f ? 0f : _runSession.Player.Shield / _runSession.Player.MaxShield;
            var hullRatio = _runSession.Player.MaxHull <= 0f ? 0f : _runSession.Player.Hull / _runSession.Player.MaxHull;
            if (shieldRatio > 0.24f && hullRatio > 0.35f)
            {
                return;
            }

            var critical = hullRatio <= 0.28f || shieldRatio <= 0.01f;
            var alpha = (critical ? 0.34f : 0.22f) + Mathf.Sin(Time.time * 9f) * 0.08f;
            var color = critical ? new Color(1f, 0.12f, 0.18f, alpha) : new Color(1f, 0.62f, 0.18f, alpha);
            DrawRectBorder(fieldRect, color, critical ? 5f : 3f);
        }

        private void DrawCombatEffects(Rect fieldRect)
        {
            for (var index = 0; index < _runSession.CombatEffects.Count; index++)
            {
                var effect = _runSession.CombatEffects[index];
                var progress = effect.TotalDuration <= 0f ? 1f : Mathf.Clamp01(1f - effect.RemainingDuration / effect.TotalDuration);
                var color = new Color(effect.Color.r, effect.Color.g, effect.Color.b, effect.Color.a * (1f - progress));

                if (effect.IsLine)
                {
                    DrawLine(ToScreen(effect.Position, fieldRect), ToScreen(effect.TargetPosition, fieldRect), color, Mathf.Max(1f, effect.Width * (1f - progress * 0.4f)));
                }
                else
                {
                    DrawCircle(ToScreen(effect.Position, fieldRect), Mathf.Lerp(effect.StartRadius, effect.EndRadius, progress), color);
                }
            }
        }

        private void DrawRunHud()
        {
            var phaseLabel = _runSession.Director.Phase.ToString().ToUpperInvariant();
            GUI.Label(new Rect(18f, 14f, Screen.width - 36f, 32f), _selectedMission.DisplayName, _subtitleStyle);
            GUI.Label(new Rect(18f, 42f, Screen.width - 156f, 24f), "Phase: " + phaseLabel + "   " + _runSession.Director.ElapsedSeconds.ToString("0.0") + "s", _smallStyle);
            if (_runSession.BossStartSecond > 0f && _runSession.Director.Phase != RunPhase.Boss && _runSession.Director.Phase != RunPhase.Results)
            {
                var eta = Mathf.Max(0f, _runSession.BossStartSecond - _runSession.Director.ElapsedSeconds);
                GUI.Label(new Rect(Screen.width - 210f, 90f, 192f, 20f), "Boss ETA " + eta.ToString("0") + "s", _smallStyle);
            }
            GUI.Label(new Rect(Screen.width - 118f, 42f, 100f, 24f), "Rerolls " + _runSession.RerollsRemaining, _smallStyle);
            GUI.Label(new Rect(18f, 66f, 190f, 24f), "Hull " + Mathf.CeilToInt(_runSession.Player.Hull) + "   Shield " + Mathf.CeilToInt(_runSession.Player.Shield), _smallStyle);
            GUI.Label(new Rect(Screen.width - 190f, 66f, 172f, 24f), "Kills " + _runSession.Kills + "   Graze " + _runSession.Grazes, _smallStyle);
            var barrierLabel = _runSession.EmergencyBarrierUsed ? "Barrier spent" : "Barrier armed";
            GUI.Label(new Rect(18f, 84f, 160f, 20f), barrierLabel, _smallStyle);
            var comboRateBonus = Mathf.RoundToInt(Mathf.Min(40f, Mathf.Max(0f, _runSession.ComboCount - 1) * 3f));
            if (comboRateBonus > 0)
            {
                GUI.Label(new Rect(Screen.width - 190f, 84f, 172f, 20f), "Combo RoF +" + comboRateBonus + "%", _smallStyle);
            }
            DrawObjectiveTracker();
            DrawAnomalyStatus();
            DrawBossStatus();

            if (GUI.Button(new Rect(Screen.width - 112f, 10f, 94f, 28f), _runSession.Paused ? "Paused" : "Pause", _buttonStyle))
            {
                _runSession.Paused = true;
            }

            var dashLabel = _runSession.DashCooldownRemaining <= 0f ? "Dash Ready" : "Dash " + _runSession.DashCooldownRemaining.ToString("0.0") + "s";
            var dashEnabled = _runSession.DashCooldownRemaining <= 0f && !_runSession.DraftOpen && !_runSession.Paused && !_runSession.TutorialOpen && !_runSession.AwaitingRewardedRevive && !_runSession.Completed && !_runSession.Failed;
            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && dashEnabled;
            if (GUI.Button(GetDashButtonRect(), dashLabel, _buttonStyle))
            {
                _playerController.TryDash(_runSession);
            }

            GUI.enabled = previousEnabled;
            DrawSurgeControl();
            DrawComboTracker();
        }

        private void DrawComboTracker()
        {
            if (_runSession.ComboCount <= 1 || _runSession.ComboTimer <= 0f)
            {
                return;
            }

            var rect = new Rect(Screen.width * 0.5f - 62f, Screen.height - 74f, 124f, 48f);
            GUI.color = new Color(0f, 0f, 0f, 0.58f);
            GUI.DrawTexture(rect, _pixel);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x, rect.y + 3f, rect.width, 22f), "Combo x" + _runSession.ComboCount, _subtitleStyle);
            GUI.Label(new Rect(rect.x, rect.y + 24f, rect.width, 18f), "+" + Mathf.RoundToInt((GetComboSalvageMultiplier() - 1f) * 100f) + "% salvage", _smallStyle);
            DrawProgressBar(new Rect(rect.x + 8f, rect.yMax - 7f, rect.width - 16f, 4f), _runSession.ComboTimer / _runSession.ComboWindowSeconds, new Color(1f, 0.9f, 0.24f, 0.9f));
        }

        private void DrawSurgeControl()
        {
            var rect = GetSurgeButtonRect();
            var chargeRatio = Mathf.Clamp01(_runSession.ReactorCharge / _runSession.ReactorChargeRequired);
            var surgeReady = _runSession.ReactorCharge >= _runSession.ReactorChargeRequired;
            DrawProgressBar(new Rect(rect.x, rect.y - 9f, rect.width, 5f), chargeRatio, surgeReady ? new Color(0.32f, 0.95f, 1f, 0.92f) : new Color(0.22f, 0.55f, 0.72f, 0.8f));

            var label = surgeReady ? "Surge Ready" : "Surge " + Mathf.RoundToInt(chargeRatio * 100f) + "%";
            var enabled = surgeReady && !_runSession.DraftOpen && !_runSession.Paused && !_runSession.TutorialOpen && !_runSession.AwaitingRewardedRevive && !_runSession.Completed && !_runSession.Failed;
            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && enabled;
            if (GUI.Button(rect, label, _buttonStyle))
            {
                TryActivateReactorSurge();
            }

            GUI.enabled = previousEnabled;
        }

        private void DrawObjectiveTracker()
        {
            var rect = new Rect(18f, 92f, Screen.width - 36f, 30f);
            GUI.color = new Color(0f, 0f, 0f, 0.48f);
            GUI.DrawTexture(rect, _pixel);
            GUI.color = Color.white;

            GUI.Label(new Rect(rect.x + 8f, rect.y + 1f, rect.width - 16f, 18f), GetObjectiveLabel(), _smallStyle);
            DrawProgressBar(new Rect(rect.x + 8f, rect.yMax - 8f, rect.width - 16f, 5f), GetObjectiveProgress(), new Color(0.26f, 1f, 0.66f, 0.9f));
        }

        private void DrawBossStatus()
        {
            var boss = FindBossEnemy();
            if (boss == null)
            {
                return;
            }

            var rect = new Rect(18f, HasMissionAnomaly() ? 154f : 126f, Screen.width - 36f, 28f);
            var healthRatio = boss.MaxHull <= 0f ? 0f : Mathf.Clamp01(boss.Hull / boss.MaxHull);
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(rect, _pixel);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 8f, rect.y + 1f, rect.width - 16f, 18f), boss.Def.DisplayName + "   Phase " + boss.BossPhaseIndex + "   " + Mathf.CeilToInt(healthRatio * 100f) + "%", _smallStyle);
            DrawProgressBar(new Rect(rect.x + 8f, rect.yMax - 8f, rect.width - 16f, 5f), healthRatio, new Color(1f, 0.24f, 0.34f, 0.9f));
        }

        private void DrawAnomalyStatus()
        {
            if (!HasMissionAnomaly())
            {
                return;
            }

            var rect = new Rect(18f, 126f, Screen.width - 36f, 26f);
            var secondsRemaining = Mathf.Max(0f, _runSession.NextAnomalySecond - _runSession.Director.ElapsedSeconds);
            var activeCount = _runSession.AnomalyTelegraphs.Count;
            var label = activeCount > 0 ? _runSession.Rules.AnomalyLabel + " firing" : _runSession.Rules.AnomalyLabel + " in " + secondsRemaining.ToString("0") + "s";

            GUI.color = new Color(0f, 0f, 0f, 0.48f);
            GUI.DrawTexture(rect, _pixel);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 8f, rect.y + 1f, rect.width - 16f, 18f), label, _smallStyle);

            var interval = Mathf.Max(1f, _runSession.Rules.AnomalyIntervalSeconds);
            var progress = activeCount > 0 ? 1f : 1f - Mathf.Clamp01(secondsRemaining / interval);
            DrawProgressBar(new Rect(rect.x + 8f, rect.yMax - 7f, rect.width - 16f, 4f), progress, new Color(1f, 0.66f, 0.22f, 0.9f));
        }

        private void DrawPauseOverlay()
        {
            var overlay = new Rect(28f, Screen.height * 0.24f, Screen.width - 56f, 276f);
            DrawCard(overlay, new Color(0f, 0f, 0f, 0.82f));
            GUI.Label(new Rect(overlay.x, overlay.y + 18f, overlay.width, 36f), "Run Paused", _titleStyle);
            GUI.Label(new Rect(overlay.x + 22f, overlay.y + 66f, overlay.width - 44f, 42f), "Resume when you are ready, or cash out the attempt and go back to the hub.", _smallStyle);

            if (GUI.Button(new Rect(overlay.x + 20f, overlay.y + 132f, overlay.width - 40f, 42f), "Resume", _buttonStyle))
            {
                _runSession.Paused = false;
            }

            if (GUI.Button(new Rect(overlay.x + 20f, overlay.y + 180f, overlay.width - 40f, 42f), "Restart Mission", _buttonStyle))
            {
                RestartCurrentMission();
            }

            if (GUI.Button(new Rect(overlay.x + 20f, overlay.y + 228f, overlay.width - 40f, 42f), "Abandon Run", _buttonStyle))
            {
                _runSession.Paused = false;
                _runSession.AwaitingRewardedRevive = false;
                _runSession.Failed = true;
                FinishRun();
            }
        }

        private void DrawReviveOverlay()
        {
            var overlay = new Rect(28f, Screen.height * 0.24f, Screen.width - 56f, 270f);
            DrawCard(overlay, new Color(0f, 0f, 0f, 0.88f));
            GUI.Label(new Rect(overlay.x, overlay.y + 16f, overlay.width, 36f), "Reignite?", _titleStyle);
            GUI.Label(new Rect(overlay.x + 20f, overlay.y + 62f, overlay.width - 40f, 54f), "Your ship broke apart. Watch one rewarded clip to restore hull, purge nearby threats, and continue this run once.", _smallStyle);

            if (GUI.Button(new Rect(overlay.x + 20f, overlay.y + 138f, overlay.width - 40f, 48f), "Watch Rewarded Clip: Revive", _buttonStyle))
            {
                string rewardMessage;
                if (_adsService.TryShowRewarded(RewardedPlacement.Revive, out rewardMessage))
                {
                    AcceptRewardedRevive(rewardMessage);
                }
            }

            if (GUI.Button(new Rect(overlay.x + 20f, overlay.y + 196f, overlay.width - 40f, 48f), "End Run", _buttonStyle))
            {
                _runSession.AwaitingRewardedRevive = false;
                _runSession.Paused = false;
                _runSession.Failed = true;
                FinishRun();
            }
        }

        private void DrawTutorialOverlay()
        {
            var overlay = new Rect(22f, Screen.height * 0.19f, Screen.width - 44f, 332f);
            DrawCard(overlay, new Color(0f, 0.08f, 0.12f, 0.86f));
            GUI.Label(new Rect(overlay.x, overlay.y + 16f, overlay.width, 36f), "Pilot Brief", _titleStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 68f, overlay.width - 36f, 22f), "1. Drag anywhere on the screen to dodge.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 102f, overlay.width - 36f, 22f), "2. Tap Dash (or double-tap) to dodge; charge Surge.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 136f, overlay.width - 36f, 22f), "3. Warning lanes and anomaly markers show danger.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 170f, overlay.width - 36f, 22f), "4. Weapons auto-fire; focus on movement.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 204f, overlay.width - 36f, 22f), "5. After each phase, choose one upgrade.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 242f, overlay.width - 36f, 42f), "Keep the lane readable, survive the pressure, and push toward the Null Sovereign.", _smallStyle);

            if (GUI.Button(new Rect(overlay.x + 18f, overlay.y + 286f, overlay.width - 36f, 44f), "Launch Run", _buttonStyle))
            {
                _runSession.TutorialOpen = false;
                _runSession.Paused = false;
                _profile.SeenFtue = true;
                _saveSystem.Save(_profile);
            }
        }

        private void DrawDraftOverlay()
        {
            var overlay = new Rect(20f, Screen.height * 0.1f, Screen.width - 40f, Screen.height * 0.74f);
            DrawCard(overlay, new Color(0f, 0f, 0f, 0.82f));
            GUI.Label(new Rect(overlay.x, overlay.y + 18f, overlay.width, 36f), "Choose Your Upgrade", _titleStyle);
            DrawDraftBuildSummary(new Rect(overlay.x + 18f, overlay.y + 62f, overlay.width - 36f, 56f));

            for (var index = 0; index < _runSession.DraftChoices.Count; index++)
            {
                var choice = _runSession.DraftChoices[index];
                var card = new Rect(overlay.x + 18f, overlay.y + 132f + index * 88f, overlay.width - 36f, 76f);
                DrawCard(card, new Color(0f, 0.12f, 0.18f, 0.7f));
                GUI.Label(new Rect(card.x + 14f, card.y + 8f, card.width - 144f, 24f), choice.DisplayName + "  " + GetUpgradeStackLabel(choice), _subtitleStyle);
                GUI.Label(new Rect(card.x + 14f, card.y + 34f, card.width - 150f, 36f), choice.Description, _smallStyle);
                GUI.Label(new Rect(card.xMax - 126f, card.y + 8f, 106f, 20f), GetUpgradeDeltaLabel(choice), _smallStyle);
                if (GUI.Button(new Rect(card.xMax - 116f, card.y + 34f, 96f, 34f), "Take", _buttonStyle))
                {
                    ApplyDraftChoice(choice);
                }
            }

            if (_runSession.RerollsRemaining > 0 && GUI.Button(new Rect(overlay.x + 18f, overlay.yMax - 64f, overlay.width - 36f, 44f), "Free Reroll (" + _runSession.RerollsRemaining + ")", _buttonStyle))
            {
                _runSession.RerollsRemaining -= 1;
                _runSession.DraftChoices = _upgradeDraftSystem.GenerateChoices(_catalog.Upgrades, _runSession.Build, _profile.UnlockedAbilityIds, 3, new DefaultRandomSource());
            }
            else if (_runSession.RerollsRemaining <= 0 && GUI.Button(new Rect(overlay.x + 18f, overlay.yMax - 64f, overlay.width - 36f, 44f), "Rewarded Reroll", _buttonStyle))
            {
                string rewardMessage;
                if (_adsService.TryShowRewarded(RewardedPlacement.UpgradeReroll, out rewardMessage))
                {
                    _runSession.DraftChoices = _upgradeDraftSystem.GenerateChoices(_catalog.Upgrades, _runSession.Build, _profile.UnlockedAbilityIds, 3, new DefaultRandomSource());
                    ShowRunMessage(rewardMessage, 2.8f);
                }
            }
        }

        private void DrawDraftBuildSummary(Rect rect)
        {
            GUI.color = new Color(0f, 0.08f, 0.11f, 0.82f);
            GUI.DrawTexture(rect, _pixel);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 10f, rect.y + 4f, rect.width - 20f, 18f), "Current Build", _smallStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 22f, rect.width - 20f, 16f), GetBuildOffenseLabel(), _smallStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 38f, rect.width - 20f, 16f), GetBuildUtilityLabel(), _smallStyle);
        }

        private void ApplyDraftChoice(UpgradeDef choice)
        {
            var previousMaxShield = _runSession.Player.MaxShield;
            _upgradeDraftSystem.ApplyUpgrade(_runSession.Build, choice);
            RecalculatePlayerShieldMax();

            var gainedMaxShield = Mathf.Max(0f, _runSession.Player.MaxShield - previousMaxShield);
            if (gainedMaxShield > 0f)
            {
                _runSession.Player.Shield = Mathf.Min(_runSession.Player.MaxShield, _runSession.Player.Shield + gainedMaxShield);
            }

            if (_runSession.Build.ShieldRestore > 0f)
            {
                _runSession.Player.Shield = Mathf.Min(_runSession.Player.MaxShield, _runSession.Player.Shield + _runSession.Build.ShieldRestore);
                _runSession.Build.ShieldRestore = 0f;
            }

            _runSession.DraftOpen = false;
            _runSession.Director.ConsumeDraft();
            ShowRunMessage(choice.DisplayName + " equipped.", 2.8f);
        }

        private string GetBuildOffenseLabel()
        {
            return "DMG x" + _runSession.Build.DamageMultiplier.ToString("0.00") +
                "   Rate x" + _runSession.Build.FireRateMultiplier.ToString("0.00") +
                "   Crit " + Mathf.RoundToInt(_runSession.Weapon.CritChance * 100f) + "%" +
                "   Shots +" + _runSession.Build.BonusProjectiles;
        }

        private string GetBuildUtilityLabel()
        {
            return "Shield +" + Mathf.CeilToInt(_runSession.Build.BonusShield) +
                "   Speed x" + _runSession.Build.MoveSpeedMultiplier.ToString("0.00");
        }

        private string GetUpgradeStackLabel(UpgradeDef choice)
        {
            var currentStacks = _upgradeDraftSystem.GetStackCount(_runSession.Build, choice.Id);
            return "[" + currentStacks + "/" + choice.MaxStacks + "]";
        }

        private string GetUpgradeDeltaLabel(UpgradeDef choice)
        {
            switch (choice.EffectType)
            {
                case UpgradeEffectType.Damage:
                case UpgradeEffectType.FireRate:
                case UpgradeEffectType.MoveSpeed:
                case UpgradeEffectType.PickupRadius:
                case UpgradeEffectType.FrostChance:
                case UpgradeEffectType.ChainChance:
                    return "+" + Mathf.RoundToInt(choice.Magnitude * 100f) + "%";
                case UpgradeEffectType.ProjectileCount:
                case UpgradeEffectType.Piercing:
                case UpgradeEffectType.DroneCompanion:
                    return "+" + Mathf.RoundToInt(choice.Magnitude);
                case UpgradeEffectType.MaxShield:
                case UpgradeEffectType.RestoreShield:
                case UpgradeEffectType.OverclockBurst:
                    return "+" + Mathf.CeilToInt(choice.Magnitude);
                default:
                    return string.Empty;
            }
        }

        private void RecalculatePlayerShieldMax()
        {
            _runSession.Player.MaxShield = (_runSession.Ship.BaseShield + _runSession.Meta.BonusShield + _runSession.Build.BonusShield) * _runSession.Rules.StartingShieldMultiplier;
            _runSession.Player.Shield = Mathf.Min(_runSession.Player.Shield, _runSession.Player.MaxShield);
        }

        private static float ResolveBossStartSecond(List<WaveDef> missionWaves)
        {
            if (missionWaves == null || missionWaves.Count == 0)
            {
                return -1f;
            }

            for (var index = 0; index < missionWaves.Count; index++)
            {
                if (missionWaves[index].Phase != RunPhase.Boss)
                {
                    continue;
                }

                return index == 0 ? 0f : missionWaves[index - 1].EndSecond;
            }

            return -1f;
        }

        private void StartMission(MissionDef mission)
        {
            _selectedMission = mission;
            var weapon = _catalog.Weapons[Mathf.Clamp(_selectedWeaponIndex, 0, _catalog.Weapons.Count - 1)];
            var equippedModules = ProfileQueries.GetEquippedModuleIds(_profile);
            var meta = _metaProgressionSystem.BuildModifiers(_profile, _catalog, equippedModules);
            var missionRules = _missionRuleSystem.Resolve(mission);
            var missionWaves = _missionRuleSystem.CreateModifiedWaves(_catalog.Waves, missionRules);

            _runSession = new RunSession
            {
                Mission = mission,
                Ship = _catalog.Ship,
                Weapon = weapon,
                Meta = meta,
                Rules = missionRules,
                Director = new RunDirector(missionWaves),
                RerollsRemaining = meta.StartingRerolls,
                ReviveCharges = meta.ReviveCharges,
                NextAnomalySecond = missionRules.AnomalyFirstSecond,
                BossStartSecond = ResolveBossStartSecond(missionWaves),
                TutorialOpen = !_profile.SeenFtue,
                Paused = !_profile.SeenFtue
            };

            _runSession.Player.MaxHull = mission.DifficultyRating < 1.4f ? _catalog.Ship.BaseHull + meta.BonusHull : _catalog.Ship.BaseHull + meta.BonusHull - 5f;
            RecalculatePlayerShieldMax();
            _runSession.Player.Hull = _runSession.Player.MaxHull;
            _runSession.Player.Shield = _runSession.Player.MaxShield;
            _runSession.Player.Position = new Vector2(0.5f, 0.16f);
            _runSession.OverclockCooldown = 4f;
            _runSession.ReactorCharge = 25f;

            SetScreen(ScreenState.Run);
            _analyticsService.Track("mission_started", "run", "{\"mission\":\"" + mission.Id + "\",\"weapon\":\"" + weapon.Id + "\"}");
        }

        private void TickRun(float deltaTime)
        {
            if (_runSession.DraftOpen || _runSession.Paused || _runSession.TutorialOpen || _runSession.AwaitingRewardedRevive)
            {
                return;
            }

            TickRunMessage(deltaTime);
            TickCombo(deltaTime);
            var pointer = GetPointerTarget();
            _playerController.TickMovement(_runSession, pointer, deltaTime);
            HandleQuickDashInput();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _playerController.TryDash(_runSession);
            }

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                TryActivateReactorSurge();
            }

            var tick = _runSession.Director.Advance(deltaTime, _runSession.BossDefeated);
            if (tick.WaveChanged)
            {
                ShowRunMessage(GetPhaseMessage(tick.Phase), 2.8f);
            }

            TickRunAnomalies(deltaTime);

            _runSession.SpawnAccumulator += deltaTime * tick.SpawnRatePerSecond * _selectedMission.DifficultyRating;

            if (tick.Phase != RunPhase.Results)
            {
                while (_runSession.SpawnAccumulator >= 1f)
                {
                    _runSession.SpawnAccumulator -= 1f;
                    SpawnForWave(_runSession.Director.CurrentWave);
                }
            }

            _weaponSystem.TickPlayerFire(_runSession, deltaTime);
            _weaponSystem.TickProjectiles(_runSession, deltaTime);
            _abilitySystem.TickOverclock(_runSession, deltaTime);

            UpdateEnemies(deltaTime);
            UpdateAttackTelegraphs(deltaTime);
            UpdateCombatEffects(deltaTime);
            ResolveProjectileCollisions();
            UpdatePickups(deltaTime);

            if (tick.DraftPending && !_runSession.DraftOpen)
            {
                _runSession.DraftChoices = _upgradeDraftSystem.GenerateChoices(_catalog.Upgrades, _runSession.Build, _profile.UnlockedAbilityIds, 3, new DefaultRandomSource());
                _runSession.DraftOpen = true;
            }

            CheckMissionState();
        }

        private void UpdateEnemies(float deltaTime)
        {
            for (var index = _runSession.Enemies.Count - 1; index >= 0; index--)
            {
                var enemy = _runSession.Enemies[index];
                enemy.FireCooldown -= deltaTime;
                enemy.Oscillator += deltaTime;
                if (enemy.SlowTimer > 0f)
                {
                    enemy.SlowTimer -= deltaTime;
                }

                var speedFactor = enemy.SlowTimer > 0f ? 0.55f : 1f;
                switch (enemy.Def.Archetype)
                {
                    case EnemyArchetype.Scout:
                        enemy.Position += Vector2.down * enemy.Def.Speed * speedFactor * deltaTime;
                        break;
                    case EnemyArchetype.Miner:
                        enemy.Position += new Vector2(Mathf.Sin(enemy.Oscillator * 2.4f) * 0.08f, -enemy.Def.Speed) * speedFactor * deltaTime;
                        break;
                    case EnemyArchetype.Rammer:
                        var ramDirection = (_runSession.Player.Position - enemy.Position).normalized;
                        enemy.Position += ramDirection * enemy.Def.Speed * 1.2f * speedFactor * deltaTime;
                        break;
                    case EnemyArchetype.ShardCaster:
                        enemy.Position += new Vector2(Mathf.Sin(enemy.Oscillator) * 0.04f, -enemy.Def.Speed * 0.45f) * speedFactor * deltaTime;
                        if (enemy.FireCooldown <= 0f)
                        {
                            enemy.FireCooldown = enemy.Def.FireInterval;
                            var shotDirection = (_runSession.Player.Position - enemy.Position).normalized;
                            QueueAttackTelegraph(enemy, shotDirection, 0.26f, enemy.Def.TelegraphSeconds, false);
                        }
                        break;
                    case EnemyArchetype.EliteWarden:
                        enemy.Position += new Vector2(Mathf.Sin(enemy.Oscillator * 1.4f) * 0.06f, -enemy.Def.Speed * 0.2f) * speedFactor * deltaTime;
                        if (enemy.FireCooldown <= 0f)
                        {
                            enemy.FireCooldown = enemy.Def.FireInterval;
                            QueueAttackTelegraph(enemy, (_runSession.Player.Position - enemy.Position).normalized, 0.3f, enemy.Def.TelegraphSeconds, false);
                            QueueAttackTelegraph(enemy, Quaternion.Euler(0f, 0f, 18f) * Vector2.down, 0.24f, enemy.Def.TelegraphSeconds, false);
                            QueueAttackTelegraph(enemy, Quaternion.Euler(0f, 0f, -18f) * Vector2.down, 0.24f, enemy.Def.TelegraphSeconds, false);
                        }
                        break;
                    case EnemyArchetype.NullSovereign:
                        enemy.Position = new Vector2(0.5f + Mathf.Sin(enemy.Oscillator * 0.9f) * 0.22f, Mathf.Lerp(enemy.Position.y, 0.83f, deltaTime * 1.5f));
                        UpdateBossPhase(enemy);
                        break;
                }

                if (enemy.FireCooldown <= 0f && enemy.Def.IsBoss)
                {
                    FireBossPattern(enemy);
                }

                if (Vector2.Distance(enemy.Position, _runSession.Player.Position) <= (enemy.Def.IsBoss ? 0.11f : 0.06f))
                {
                    var applied = _playerController.ApplyDamage(_runSession, enemy.Def.ContactDamage);
                    if (enemy.Def.IsBoss || enemy.Def.Archetype == EnemyArchetype.Rammer)
                    {
                        enemy.Hull = 0f;
                    }
                    else
                    {
                        var push = (enemy.Position - _runSession.Player.Position).normalized;
                        enemy.Position += push * 0.08f;
                        enemy.FireCooldown = Mathf.Max(enemy.FireCooldown, 0.45f);
                        if (applied)
                        {
                            enemy.SlowTimer = Mathf.Max(enemy.SlowTimer, 0.45f);
                        }
                    }
                }

                if (enemy.Hull <= 0f)
                {
                    if (enemy.Def.IsBoss)
                    {
                        _runSession.BossDefeated = true;
                    }

                    _runSession.Kills += 1;
                    RegisterComboKill(enemy);
                    AddReactorCharge(enemy.Def.IsBoss ? _runSession.ReactorChargeRequired : 7f + enemy.Def.ScoreValue * 3f);
                    AddCombatPulse(enemy.Position, enemy.Def.IsBoss ? new Color(1f, 0.22f, 0.32f, 0.9f) : new Color(1f, 0.92f, 0.3f, 0.8f), enemy.Def.IsBoss ? 22f : 10f, enemy.Def.IsBoss ? 86f : 34f, enemy.Def.IsBoss ? 0.55f : 0.32f);
                    SpawnPickup(enemy.Position, enemy.Def.ScoreValue);
                    _runSession.Enemies.RemoveAt(index);
                    continue;
                }

                if (!enemy.Def.IsBoss && enemy.Position.y < -0.08f)
                {
                    _runSession.Enemies.RemoveAt(index);
                }
            }
        }

        private void QueueAttackTelegraph(EnemyState source, Vector2 direction, float speedScale, float duration, bool isBossShot)
        {
            if (source == null)
            {
                return;
            }

            var normalizedDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.down;
            var damage = source.Def.ContactDamage * 0.8f;
            if (duration <= 0f)
            {
                _weaponSystem.SpawnEnemyShot(_runSession, source.Position, damage, isBossShot, normalizedDirection, speedScale);
                return;
            }

            _runSession.AttackTelegraphs.Add(new AttackTelegraphState
            {
                Source = source,
                Origin = source.Position,
                Direction = normalizedDirection,
                Damage = damage,
                SpeedScale = speedScale,
                IsBossShot = isBossShot,
                TotalDuration = duration,
                RemainingDuration = duration
            });
        }

        private void UpdateAttackTelegraphs(float deltaTime)
        {
            for (var index = _runSession.AttackTelegraphs.Count - 1; index >= 0; index--)
            {
                var telegraph = _runSession.AttackTelegraphs[index];
                if (telegraph.Source == null || telegraph.Source.Hull <= 0f || !_runSession.Enemies.Contains(telegraph.Source))
                {
                    _runSession.AttackTelegraphs.RemoveAt(index);
                    continue;
                }

                telegraph.RemainingDuration -= deltaTime;
                if (telegraph.RemainingDuration > 0f)
                {
                    continue;
                }

                _weaponSystem.SpawnEnemyShot(_runSession, telegraph.Origin, telegraph.Damage, telegraph.IsBossShot, telegraph.Direction, telegraph.SpeedScale);
                _runSession.AttackTelegraphs.RemoveAt(index);
            }
        }

        private void RegisterComboKill(EnemyState enemy)
        {
            if (_runSession.ComboTimer <= 0f)
            {
                _runSession.ComboCount = 0;
            }

            _runSession.ComboCount += enemy.Def.IsBoss ? 3 : 1;
            _runSession.BestComboCount = Mathf.Max(_runSession.BestComboCount, _runSession.ComboCount);
            _runSession.ComboTimer = _runSession.ComboWindowSeconds;

            if (_runSession.ComboCount == 5 || _runSession.ComboCount == 10 || _runSession.ComboCount == 20)
            {
                ShowRunMessage("Combo x" + _runSession.ComboCount + " salvage flow +" + Mathf.RoundToInt((GetComboSalvageMultiplier() - 1f) * 100f) + "%.", 2.2f);
            }

            AddCombatPulse(enemy.Position, new Color(1f, 0.9f, 0.22f, 0.42f), 18f, Mathf.Min(70f, 26f + _runSession.ComboCount * 2f), 0.32f);
        }

        private void TickCombo(float deltaTime)
        {
            if (_runSession.ComboTimer <= 0f)
            {
                return;
            }

            _runSession.ComboTimer -= deltaTime;
            if (_runSession.ComboTimer <= 0f)
            {
                _runSession.ComboTimer = 0f;
                _runSession.ComboCount = 0;
            }
        }

        private float GetComboSalvageMultiplier()
        {
            if (_runSession == null)
            {
                return 1f;
            }

            return 1f + Mathf.Min(20, Mathf.Max(0, _runSession.ComboCount - 1)) * 0.05f;
        }

        private void RegisterProjectileGraze(ProjectileState projectile)
        {
            projectile.GrazedByPlayer = true;
            _runSession.Grazes += 1;
            AddReactorCharge(projectile.Radius >= 12f ? 8f : 4f);
            _runSession.DashCooldownRemaining = Mathf.Max(0f, _runSession.DashCooldownRemaining - (projectile.Radius >= 12f ? 0.22f : 0.12f));
            if (_runSession.ComboTimer > 0f)
            {
                _runSession.ComboTimer = Mathf.Min(_runSession.ComboWindowSeconds, _runSession.ComboTimer + 0.65f);
            }

            AddCombatLine(projectile.Position, _runSession.Player.Position, new Color(0.32f, 0.95f, 1f, 0.58f), 4f, 0.18f);
            AddCombatPulse(_runSession.Player.Position, new Color(0.32f, 0.95f, 1f, 0.62f), 12f, 34f, 0.26f);
            if (_runSession.Grazes % 5 == 0)
            {
                ShowRunMessage("Graze chain " + _runSession.Grazes + ". Reactor and dash recovering.", 2.1f);
            }
        }

        private bool TryActivateReactorSurge()
        {
            if (_runSession == null || _runSession.ReactorCharge < _runSession.ReactorChargeRequired || _runSession.DraftOpen || _runSession.Paused || _runSession.TutorialOpen || _runSession.AwaitingRewardedRevive || _runSession.Completed || _runSession.Failed)
            {
                return false;
            }

            var origin = _runSession.Player.Position;
            var radius = _runSession.ReactorSurgeRadius;
            var damage = _runSession.ReactorSurgeDamage + _runSession.Build.OverclockBurstDamage * 0.35f;
            var hitCount = 0;
            _runSession.ReactorCharge = 0f;

            AddCombatPulse(origin, new Color(0.32f, 0.95f, 1f, 0.92f), 28f, 180f, 0.58f);
            for (var enemyIndex = 0; enemyIndex < _runSession.Enemies.Count; enemyIndex++)
            {
                var enemy = _runSession.Enemies[enemyIndex];
                var distance = Vector2.Distance(origin, enemy.Position);
                if (distance > radius)
                {
                    continue;
                }

                var falloff = Mathf.Lerp(1f, 0.45f, distance / radius);
                enemy.Hull -= damage * falloff;
                enemy.SlowTimer = Mathf.Max(enemy.SlowTimer, 0.65f);
                hitCount += 1;
                AddCombatLine(origin, enemy.Position, new Color(0.32f, 0.95f, 1f, 0.82f), enemy.Def.IsBoss ? 9f : 6f, 0.24f);
                AddCombatPulse(enemy.Position, new Color(0.32f, 0.95f, 1f, 0.75f), enemy.Def.IsBoss ? 18f : 10f, enemy.Def.IsBoss ? 56f : 32f, 0.36f);
            }

            var purgedProjectiles = 0;
            for (var projectileIndex = _runSession.Projectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                if (!_runSession.Projectiles[projectileIndex].FromPlayer)
                {
                    purgedProjectiles += 1;
                    AddCombatPulse(_runSession.Projectiles[projectileIndex].Position, new Color(0.32f, 0.95f, 1f, 0.55f), 6f, 24f, 0.24f);
                    _runSession.Projectiles.RemoveAt(projectileIndex);
                }
            }

            if (_runSession.AttackTelegraphs.Count > 0)
            {
                purgedProjectiles += _runSession.AttackTelegraphs.Count;
                _runSession.AttackTelegraphs.Clear();
            }

            var collectedBySurge = 0;
            for (var pickupIndex = _runSession.Pickups.Count - 1; pickupIndex >= 0; pickupIndex--)
            {
                var pickup = _runSession.Pickups[pickupIndex];
                if (Vector2.Distance(origin, pickup.Position) > radius * 1.35f)
                {
                    continue;
                }

                CollectPickup(pickup);
                _runSession.Pickups.RemoveAt(pickupIndex);
                collectedBySurge += 1;
                AddCombatLine(origin, pickup.Position, new Color(0.52f, 1f, 0.46f, 0.65f), 3f, 0.2f);
            }

            var restoredShield = Mathf.Min(_runSession.Player.MaxShield * 0.25f, 7f + hitCount * 1.2f + collectedBySurge * 0.8f);
            _runSession.Player.Shield = Mathf.Min(_runSession.Player.MaxShield, _runSession.Player.Shield + restoredShield);

            ShowRunMessage("Reactor Surge hit " + hitCount + ", purged " + purgedProjectiles + ", absorbed " + collectedBySurge + ".", 2.6f);
            return true;
        }

        private void TickRunAnomalies(float deltaTime)
        {
            var rules = _runSession.Rules;
            if (rules.AnomalyKind != RunAnomalyKind.None && _runSession.NextAnomalySecond > 0f && _runSession.Director.Phase != RunPhase.Results && _runSession.Director.ElapsedSeconds >= _runSession.NextAnomalySecond)
            {
                TriggerRunAnomaly(rules);
                _runSession.AnomalyEventsTriggered += 1;
                _runSession.NextAnomalySecond += Mathf.Max(8f, rules.AnomalyIntervalSeconds);
            }

            UpdateAnomalyTelegraphs(deltaTime);
        }

        private void TriggerRunAnomaly(MissionRuleSet rules)
        {
            switch (rules.AnomalyKind)
            {
                case RunAnomalyKind.MeteorShower:
                    QueueMeteorShower(rules);
                    ShowRunMessage(rules.AnomalyLabel + " incoming.", 2.2f);
                    break;
                case RunAnomalyKind.SolarFlare:
                    QueueSolarFlare(rules);
                    ShowRunMessage(rules.AnomalyLabel + " charging lanes.", 2.2f);
                    break;
                case RunAnomalyKind.SalvageBloom:
                    SpawnSalvageBloom(rules);
                    ShowRunMessage(rules.AnomalyLabel + " drifting in.", 2.2f);
                    break;
            }
        }

        private void QueueMeteorShower(MissionRuleSet rules)
        {
            var count = Mathf.Max(1, rules.AnomalyCount);
            for (var index = 0; index < count; index++)
            {
                var target = index == 0
                    ? _runSession.Player.Position + new Vector2(UnityEngine.Random.Range(-0.08f, 0.08f), UnityEngine.Random.Range(-0.02f, 0.12f))
                    : new Vector2(UnityEngine.Random.Range(0.12f, 0.88f), UnityEngine.Random.Range(0.18f, 0.88f));

                QueueAnomalyTelegraph(RunAnomalyKind.MeteorShower, RunAnomalyShape.Circle, new Vector2(Mathf.Clamp01(target.x), Mathf.Clamp(target.y, 0.12f, 0.9f)), 0.085f, 0f, rules.AnomalyDamage, true, rules.AnomalyTelegraphSeconds);
            }
        }

        private void QueueSolarFlare(MissionRuleSet rules)
        {
            var count = Mathf.Max(1, rules.AnomalyCount);
            for (var index = 0; index < count; index++)
            {
                var laneX = index == 0 ? _runSession.Player.Position.x + UnityEngine.Random.Range(-0.06f, 0.06f) : UnityEngine.Random.Range(0.16f, 0.84f);
                QueueAnomalyTelegraph(RunAnomalyKind.SolarFlare, RunAnomalyShape.VerticalLane, new Vector2(Mathf.Clamp(laneX, 0.12f, 0.88f), 0.5f), 0f, 0.12f, rules.AnomalyDamage, true, rules.AnomalyTelegraphSeconds);
            }
        }

        private void SpawnSalvageBloom(MissionRuleSet rules)
        {
            var count = Mathf.Max(1, rules.AnomalyCount);
            for (var index = 0; index < count; index++)
            {
                var position = new Vector2(UnityEngine.Random.Range(0.12f, 0.88f), UnityEngine.Random.Range(0.24f, 0.86f));
                SpawnPickup(position, UnityEngine.Random.Range(1.8f, 3.4f));
                AddCombatPulse(position, new Color(0.42f, 1f, 0.28f, 0.58f), 12f, 42f, 0.45f);
            }
        }

        private void QueueAnomalyTelegraph(RunAnomalyKind kind, RunAnomalyShape shape, Vector2 position, float radius, float width, float damage, bool damagesEnemies, float duration)
        {
            var telegraphSeconds = Mathf.Max(0.35f, duration);
            _runSession.AnomalyTelegraphs.Add(new RunAnomalyTelegraphState
            {
                Kind = kind,
                Shape = shape,
                Position = position,
                Radius = radius,
                Width = width,
                Damage = damage,
                DamagesEnemies = damagesEnemies,
                TotalDuration = telegraphSeconds,
                RemainingDuration = telegraphSeconds
            });
        }

        private void UpdateAnomalyTelegraphs(float deltaTime)
        {
            for (var index = _runSession.AnomalyTelegraphs.Count - 1; index >= 0; index--)
            {
                var anomaly = _runSession.AnomalyTelegraphs[index];
                anomaly.RemainingDuration -= deltaTime;
                if (anomaly.RemainingDuration > 0f)
                {
                    continue;
                }

                ResolveAnomalyTelegraph(anomaly);
                _runSession.AnomalyTelegraphs.RemoveAt(index);
            }
        }

        private void ResolveAnomalyTelegraph(RunAnomalyTelegraphState anomaly)
        {
            if (anomaly.Shape == RunAnomalyShape.VerticalLane)
            {
                AddCombatLine(new Vector2(anomaly.Position.x, 0.08f), new Vector2(anomaly.Position.x, 0.95f), new Color(1f, 0.3f, 0.18f, 0.9f), 12f, 0.22f);
            }
            else
            {
                AddCombatPulse(anomaly.Position, new Color(1f, 0.72f, 0.22f, 0.9f), 18f, 78f, 0.38f);
            }

            if (IsInsideAnomaly(anomaly, _runSession.Player.Position))
            {
                _playerController.ApplyDamage(_runSession, anomaly.Damage);
                AddCombatPulse(_runSession.Player.Position, new Color(1f, 0.22f, 0.16f, 0.82f), 16f, 48f, 0.32f);
            }

            if (!anomaly.DamagesEnemies)
            {
                return;
            }

            for (var enemyIndex = 0; enemyIndex < _runSession.Enemies.Count; enemyIndex++)
            {
                var enemy = _runSession.Enemies[enemyIndex];
                if (!IsInsideAnomaly(anomaly, enemy.Position))
                {
                    continue;
                }

                enemy.Hull -= anomaly.Damage * (enemy.Def.IsBoss ? 0.55f : 1.35f);
                enemy.SlowTimer = Mathf.Max(enemy.SlowTimer, 0.55f);
                AddCombatPulse(enemy.Position, new Color(1f, 0.58f, 0.18f, 0.68f), enemy.Def.IsBoss ? 16f : 9f, enemy.Def.IsBoss ? 58f : 30f, 0.28f);
            }
        }

        private static bool IsInsideAnomaly(RunAnomalyTelegraphState anomaly, Vector2 position)
        {
            if (anomaly.Shape == RunAnomalyShape.VerticalLane)
            {
                return Mathf.Abs(position.x - anomaly.Position.x) <= anomaly.Width * 0.5f;
            }

            return Vector2.Distance(position, anomaly.Position) <= anomaly.Radius;
        }

        private void UpdateCombatEffects(float deltaTime)
        {
            for (var index = _runSession.CombatEffects.Count - 1; index >= 0; index--)
            {
                _runSession.CombatEffects[index].RemainingDuration -= deltaTime;
                if (_runSession.CombatEffects[index].RemainingDuration <= 0f)
                {
                    _runSession.CombatEffects.RemoveAt(index);
                }
            }
        }

        private void ResolveProjectileCollisions()
        {
            for (var projectileIndex = _runSession.Projectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                var projectile = _runSession.Projectiles[projectileIndex];
                if (projectile.FromPlayer)
                {
                    for (var enemyIndex = _runSession.Enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
                    {
                        var enemy = _runSession.Enemies[enemyIndex];
                        var hitRadius = enemy.Def.IsBoss ? 0.08f : 0.05f;
                        if (Vector2.Distance(projectile.Position, enemy.Position) <= hitRadius)
                        {
                            enemy.Hull -= projectile.Damage;
                            if (projectile.IsCritical)
                            {
                                AddCombatPulse(enemy.Position, new Color(1f, 0.88f, 0.18f, 0.92f), 12f, enemy.Def.IsBoss ? 44f : 30f, 0.28f);
                                ApplyCriticalShieldSiphon(projectile.Damage);
                            }
                            else
                            {
                                AddCombatPulse(enemy.Position, projectile.Color, 5f, enemy.Def.IsBoss ? 24f : 16f, 0.18f);
                            }

                            _abilitySystem.TryApplyHitEffects(_runSession, enemy);
                            if (projectile.RemainingPierce > 0)
                            {
                                projectile.RemainingPierce -= 1;
                            }
                            else
                            {
                                _runSession.Projectiles.RemoveAt(projectileIndex);
                            }

                            break;
                        }
                    }
                }
                else
                {
                    var distanceToPlayer = Vector2.Distance(projectile.Position, _runSession.Player.Position);
                    if (distanceToPlayer <= 0.04f)
                    {
                        _playerController.ApplyDamage(_runSession, projectile.Damage);
                        AddCombatPulse(_runSession.Player.Position, new Color(1f, 0.25f, 0.32f, 0.85f), 12f, 38f, 0.3f);
                        _runSession.Projectiles.RemoveAt(projectileIndex);
                    }
                    else if (!projectile.GrazedByPlayer && distanceToPlayer <= 0.09f)
                    {
                        RegisterProjectileGraze(projectile);
                    }
                }
            }
        }

        private void UpdatePickups(float deltaTime)
        {
            var magnetRadius = 0.08f + _runSession.Meta.PickupRadiusBonus + _runSession.Build.PickupRadiusBonus;
            for (var pickupIndex = _runSession.Pickups.Count - 1; pickupIndex >= 0; pickupIndex--)
            {
                var pickup = _runSession.Pickups[pickupIndex];
                var toPlayer = _runSession.Player.Position - pickup.Position;
                var distance = toPlayer.magnitude;
                if (distance <= magnetRadius)
                {
                    var pull = Mathf.Lerp(0.7f, 1.8f, 1f - Mathf.Clamp01(distance / Mathf.Max(0.0001f, magnetRadius)));
                    pickup.Position += toPlayer.normalized * deltaTime * pull;
                }
                else
                {
                    pickup.Position += Vector2.down * deltaTime * 0.03f;
                }

                pickup.Position = new Vector2(Mathf.Clamp01(pickup.Position.x), Mathf.Clamp(pickup.Position.y, 0.03f, 1.02f));
                var updatedDistance = Vector2.Distance(_runSession.Player.Position, pickup.Position);
                if (updatedDistance <= 0.04f)
                {
                    _runSession.PickupsCollected += 1;
                    CollectPickup(pickup);
                    _runSession.Pickups.RemoveAt(pickupIndex);
                }
            }
        }

        private void CollectPickup(PickupState pickup)
        {
            var salvage = Mathf.Max(1, Mathf.CeilToInt(pickup.Value * GetComboSalvageMultiplier()));
            _runSession.SalvageCollected += salvage;
            AddReactorCharge(4f + pickup.Value * 1.5f);
            AddCombatPulse(pickup.Position, _runSession.ComboCount > 1 ? new Color(1f, 0.9f, 0.22f, 0.85f) : new Color(0.42f, 1f, 0.28f, 0.85f), 10f, _runSession.ComboCount > 1 ? 46f : 36f, 0.35f);

            var maxShield = _runSession.Player.MaxShield;
            if (maxShield > 0f && _runSession.Player.Shield < maxShield)
            {
                var shieldBefore = _runSession.Player.Shield;
                _runSession.Player.Shield = Mathf.Min(maxShield, _runSession.Player.Shield + pickup.Value * 0.75f);
                if (_runSession.Player.Shield > shieldBefore)
                {
                    AddCombatPulse(_runSession.Player.Position, new Color(0.2f, 0.82f, 1f, 0.75f), 18f, 48f, 0.35f);
                }
            }
        }

        private void ApplyCriticalShieldSiphon(float projectileDamage)
        {
            if (_runSession.Player.MaxShield <= 0f)
            {
                return;
            }

            var siphon = Mathf.Min(4f, Mathf.Max(0.4f, projectileDamage * 0.08f));
            var before = _runSession.Player.Shield;
            _runSession.Player.Shield = Mathf.Min(_runSession.Player.MaxShield, _runSession.Player.Shield + siphon);
            if (_runSession.Player.Shield > before)
            {
                AddCombatPulse(_runSession.Player.Position, new Color(0.85f, 1f, 0.45f, 0.55f), 12f, 30f, 0.22f);
            }
        }

        private void CheckMissionState()
        {
            if (_runSession.Failed)
            {
                FinishRun();
                return;
            }

            switch (_selectedMission.ObjectiveKind)
            {
                case MissionObjectiveKind.SurviveTime:
                    _runSession.Completed = _runSession.Director.ElapsedSeconds >= _selectedMission.TargetDurationSeconds;
                    break;
                case MissionObjectiveKind.DefeatEnemies:
                    _runSession.Completed = _runSession.Kills >= _selectedMission.TargetValue;
                    break;
                case MissionObjectiveKind.DefeatBoss:
                    _runSession.Completed = _runSession.BossDefeated;
                    break;
                case MissionObjectiveKind.PreserveShield:
                    if (_runSession.Player.Shield <= 0f)
                    {
                        _runSession.Failed = true;
                        ShowRunMessage("Shield integrity lost. Contract failed.", 2.6f);
                    }

                    _runSession.Completed = _runSession.BossDefeated && _runSession.Player.Shield > 0f;
                    break;
            }

            if (_runSession.Completed || _runSession.BossDefeated)
            {
                FinishRun();
            }
        }

        private void RestartCurrentMission()
        {
            if (_selectedMission == null)
            {
                return;
            }

            StartMission(_selectedMission);
        }

        private void FinishRun()
        {
            _pendingSummary = new RunSummary
            {
                MissionId = _selectedMission.Id,
                Outcome = _runSession.Failed ? RunOutcome.Failed : RunOutcome.Completed,
                DurationSeconds = _runSession.Director.ElapsedSeconds,
                Kills = _runSession.Kills,
                BossDefeated = _runSession.BossDefeated,
                Revived = _runSession.Revived,
                EndingShieldRatio = _runSession.Player.MaxShield <= 0f ? 0f : _runSession.Player.Shield / _runSession.Player.MaxShield,
                PickupsCollected = _runSession.PickupsCollected,
                SalvageCollected = _runSession.SalvageCollected,
                Grazes = _runSession.Grazes,
                BestComboCount = _runSession.BestComboCount,
                AnomalyEventsTriggered = _runSession.AnomalyEventsTriggered
            };

            _pendingEvaluation = _missionSystem.Evaluate(_selectedMission, _pendingSummary);
            _pendingRewards = _economySystem.CalculateRewards(_selectedMission, _pendingEvaluation, _dailyContract, _profile, _runSession.Meta, _pendingSummary);
            _pendingUnlocks = PreviewUnlocks(_profile, _pendingRewards);
            _missionSystem.ApplyMissionProgress(_profile, _selectedMission, _pendingEvaluation, _pendingSummary);
            _runSession = null;
            SetScreen(ScreenState.Results);
        }

        private void CollectResults(bool replayMission)
        {
            _economySystem.ApplyReward(_profile, _pendingRewards, _pendingEvaluation);
            _pendingUnlocks = _metaProgressionSystem.CollectNewUnlocks(_profile, _catalog);
            _analyticsService.Track("mission_finished", "results", "{\"mission\":\"" + _selectedMission.Id + "\",\"complete\":" + (_pendingEvaluation.Completed ? "true" : "false") + "}");
            _saveSystem.Save(_profile);

            if (replayMission)
            {
                StartMission(_selectedMission);
                return;
            }

            SetScreen(ScreenState.MetaHub);
        }

        private void AcceptRewardedRevive(string rewardMessage)
        {
            _runSession.AwaitingRewardedRevive = false;
            _runSession.Paused = false;
            _runSession.RewardedReviveUsed = true;
            _runSession.Revived = true;
            _runSession.Player.Hull = _runSession.Player.MaxHull * 0.65f;
            _runSession.Player.Shield = _runSession.Player.MaxShield * 0.5f;
            _runSession.Player.InvulnerabilityTimer = 1.35f;
            _runSession.AttackTelegraphs.Clear();
            _runSession.AnomalyTelegraphs.Clear();
            _runSession.CombatEffects.Clear();
            ShowRunMessage(rewardMessage, 3.2f);

            for (var projectileIndex = _runSession.Projectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                if (!_runSession.Projectiles[projectileIndex].FromPlayer)
                {
                    _runSession.Projectiles.RemoveAt(projectileIndex);
                }
            }

            for (var enemyIndex = _runSession.Enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
            {
                if (Vector2.Distance(_runSession.Enemies[enemyIndex].Position, _runSession.Player.Position) <= 0.24f)
                {
                    _runSession.Enemies.RemoveAt(enemyIndex);
                }
            }
        }

        private void SpawnForWave(WaveDef wave)
        {
            if (wave == null || wave.SpawnArchetypes.Count == 0)
            {
                return;
            }

            if (wave.Phase == RunPhase.Boss)
            {
                if (_runSession.BossSpawned)
                {
                    return;
                }

                _runSession.BossSpawned = true;
                SpawnEnemy(EnemyArchetype.NullSovereign, new Vector2(0.5f, 0.95f));
                return;
            }

            var archetype = wave.SpawnArchetypes[UnityEngine.Random.Range(0, wave.SpawnArchetypes.Count)];
            SpawnEnemy(archetype, new Vector2(UnityEngine.Random.Range(0.1f, 0.9f), 1.05f));
        }

        private void SpawnEnemy(EnemyArchetype archetype, Vector2 position)
        {
            var def = FindEnemyDef(archetype);
            if (def == null)
            {
                return;
            }

            _runSession.Enemies.Add(new EnemyState
            {
                Def = def,
                Position = position,
                Hull = def.Hull * _selectedMission.DifficultyRating,
                MaxHull = def.Hull * _selectedMission.DifficultyRating,
                FireCooldown = def.FireInterval <= 0f ? 999f : def.FireInterval,
                Oscillator = UnityEngine.Random.value * 10f
            });
        }

        private EnemyDef FindEnemyDef(EnemyArchetype archetype)
        {
            for (var index = 0; index < _catalog.Enemies.Count; index++)
            {
                if (_catalog.Enemies[index].Archetype == archetype)
                {
                    return _catalog.Enemies[index];
                }
            }

            return null;
        }

        private void UpdateBossPhase(EnemyState boss)
        {
            var previousPhaseIndex = boss.BossPhaseIndex;
            var healthRatio = boss.Hull / boss.MaxHull;
            for (var index = 0; index < _catalog.BossPhases.Count; index++)
            {
                var phase = _catalog.BossPhases[index];
                if (healthRatio <= phase.TriggerHealthNormalized)
                {
                    boss.BossPhaseIndex = phase.PhaseIndex;
                }
            }

            if (boss.BossPhaseIndex != previousPhaseIndex)
            {
                ShowRunMessage("Null Sovereign phase " + boss.BossPhaseIndex + " escalated.", 2.8f);
            }
        }

        private void FireBossPattern(EnemyState boss)
        {
            BossPhaseDef phaseDef = _catalog.BossPhases[_catalog.BossPhases.Count - 1];
            for (var index = 0; index < _catalog.BossPhases.Count; index++)
            {
                if (_catalog.BossPhases[index].PhaseIndex == boss.BossPhaseIndex)
                {
                    phaseDef = _catalog.BossPhases[index];
                    break;
                }
            }

            boss.FireCooldown = phaseDef.VolleyInterval;
            for (var shotIndex = 0; shotIndex < phaseDef.VolleyCount; shotIndex++)
            {
                var spread = phaseDef.VolleyCount == 1 ? 0f : 50f;
                var step = phaseDef.VolleyCount == 1 ? 0f : spread / (phaseDef.VolleyCount - 1);
                var angle = -spread * 0.5f + step * shotIndex;
                var direction = Quaternion.Euler(0f, 0f, angle) * Vector2.down;
                QueueAttackTelegraph(boss, direction, phaseDef.ProjectileSpeed, phaseDef.TelegraphSeconds, true);
            }

            if (boss.BossPhaseIndex >= 3)
            {
                _playerController.ApplyDamage(_runSession, phaseDef.ArenaPulseDamage * 0.15f);
            }
        }

        private void SpawnPickup(Vector2 position, float value)
        {
            _runSession.Pickups.Add(new PickupState
            {
                Position = position,
                Value = value,
                Radius = 8f
            });
        }

        private void TickRunMessage(float deltaTime)
        {
            if (_runSession.RewardMessageTimer <= 0f)
            {
                return;
            }

            _runSession.RewardMessageTimer -= deltaTime;
            if (_runSession.RewardMessageTimer <= 0f)
            {
                _runSession.RewardMessage = string.Empty;
            }
        }

        private void ShowRunMessage(string message, float seconds)
        {
            if (_runSession == null)
            {
                return;
            }

            _runSession.RewardMessage = message;
            _runSession.RewardMessageTimer = seconds;
        }

        private void AddCombatPulse(Vector2 position, Color color, float startRadius, float endRadius, float duration)
        {
            _runSession.CombatEffects.Add(new CombatEffectState
            {
                Position = position,
                Color = color,
                StartRadius = startRadius,
                EndRadius = endRadius,
                TotalDuration = duration,
                RemainingDuration = duration
            });
        }

        private void AddCombatLine(Vector2 start, Vector2 end, Color color, float width, float duration)
        {
            _runSession.CombatEffects.Add(new CombatEffectState
            {
                Position = start,
                TargetPosition = end,
                Color = color,
                Width = width,
                TotalDuration = duration,
                RemainingDuration = duration,
                IsLine = true
            });
        }

        private void AddReactorCharge(float amount)
        {
            if (_runSession == null || _runSession.ReactorCharge >= _runSession.ReactorChargeRequired)
            {
                return;
            }

            _runSession.ReactorCharge = Mathf.Min(_runSession.ReactorChargeRequired, _runSession.ReactorCharge + amount);
        }

        private string GetPhaseMessage(RunPhase phase)
        {
            switch (phase)
            {
                case RunPhase.Escalation:
                    return "Escalation wave: Shard Casters entering.";
                case RunPhase.Elite:
                    return "Elite wave: Wardens deployed.";
                case RunPhase.Boss:
                    return "Boss signal locked. Null Sovereign inbound.";
                case RunPhase.Results:
                    return "Extraction window open.";
                default:
                    return "Breach corridor engaged.";
            }
        }

        private string GetObjectiveLabel()
        {
            switch (_selectedMission.ObjectiveKind)
            {
                case MissionObjectiveKind.SurviveTime:
                    return "Objective: Survive " + Mathf.FloorToInt(_runSession.Director.ElapsedSeconds) + "/" + Mathf.CeilToInt(_selectedMission.TargetDurationSeconds) + "s";
                case MissionObjectiveKind.DefeatEnemies:
                    return "Objective: Enemies " + _runSession.Kills + "/" + _selectedMission.TargetValue;
                case MissionObjectiveKind.DefeatBoss:
                    return _runSession.BossSpawned ? "Objective: Destroy boss" : "Objective: Reach boss";
                case MissionObjectiveKind.PreserveShield:
                    var shieldRatio = _runSession.Player.MaxShield <= 0f ? 0f : _runSession.Player.Shield / _runSession.Player.MaxShield;
                    return "Objective: Boss with " + Mathf.CeilToInt(_selectedMission.RequiredShieldRatio * 100f) + "% shield   Now " + Mathf.CeilToInt(shieldRatio * 100f) + "%";
                default:
                    return "Objective active";
            }
        }

        private float GetObjectiveProgress()
        {
            switch (_selectedMission.ObjectiveKind)
            {
                case MissionObjectiveKind.SurviveTime:
                    return Mathf.Clamp01(_runSession.Director.ElapsedSeconds / Mathf.Max(1f, _selectedMission.TargetDurationSeconds));
                case MissionObjectiveKind.DefeatEnemies:
                    return Mathf.Clamp01((float)_runSession.Kills / Mathf.Max(1, _selectedMission.TargetValue));
                case MissionObjectiveKind.DefeatBoss:
                case MissionObjectiveKind.PreserveShield:
                    if (_runSession.BossDefeated)
                    {
                        return 1f;
                    }

                    var boss = FindBossEnemy();
                    if (boss != null)
                    {
                        var healthRatio = boss.MaxHull <= 0f ? 1f : Mathf.Clamp01(boss.Hull / boss.MaxHull);
                        return 1f - healthRatio;
                    }

                    return Mathf.Clamp01(_runSession.Director.ElapsedSeconds / Mathf.Max(1f, _selectedMission.TargetDurationSeconds));
                default:
                    return 0f;
            }
        }

        private EnemyState FindBossEnemy()
        {
            for (var index = 0; index < _runSession.Enemies.Count; index++)
            {
                if (_runSession.Enemies[index].Def.IsBoss)
                {
                    return _runSession.Enemies[index];
                }
            }

            return null;
        }

        private bool HasMissionAnomaly()
        {
            return _runSession != null && _runSession.Rules != null && _runSession.Rules.AnomalyKind != RunAnomalyKind.None;
        }

        private void HandleQuickDashInput()
        {
            if (_runSession == null)
            {
                return;
            }

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    TryDashOnDoubleTap(touch.position);
                }

                return;
            }

            if (Input.GetMouseButtonUp(0))
            {
                TryDashOnDoubleTap(Input.mousePosition);
            }
        }

        private void TryDashOnDoubleTap(Vector2 screenPoint)
        {
            if (IsScreenPointInsideGuiRect(screenPoint, GetDashButtonRect()) || IsScreenPointInsideGuiRect(screenPoint, GetSurgeButtonRect()))
            {
                return;
            }

            var now = Time.unscaledTime;
            var withinTime = now - _lastTapTime <= 0.28f;
            var withinDistance = Vector2.Distance(screenPoint, _lastTapPosition) <= 90f;
            if (withinTime && withinDistance)
            {
                _playerController.TryDash(_runSession);
                _lastTapTime = -10f;
                return;
            }

            _lastTapTime = now;
            _lastTapPosition = screenPoint;
        }

        private Vector2 GetPointerTarget()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (IsScreenPointInsideGuiRect(touch.position, GetDashButtonRect()) || IsScreenPointInsideGuiRect(touch.position, GetSurgeButtonRect()))
                {
                    return _runSession.Player.Position;
                }

                return NormalizeScreenPoint(touch.position, GetGameplayFieldRect());
            }

            if (Input.GetMouseButton(0))
            {
                if (IsScreenPointInsideGuiRect(Input.mousePosition, GetDashButtonRect()) || IsScreenPointInsideGuiRect(Input.mousePosition, GetSurgeButtonRect()))
                {
                    return _runSession.Player.Position;
                }

                return NormalizeScreenPoint(Input.mousePosition, GetGameplayFieldRect());
            }

            return _runSession.Player.Position;
        }

        private Rect GetDashButtonRect()
        {
            return new Rect(Screen.width - 136f, Screen.height - 74f, 118f, 48f);
        }

        private Rect GetSurgeButtonRect()
        {
            return new Rect(18f, Screen.height - 74f, 118f, 48f);
        }

        private static bool IsScreenPointInsideGuiRect(Vector2 screenPoint, Rect guiRect)
        {
            return guiRect.Contains(new Vector2(screenPoint.x, Screen.height - screenPoint.y));
        }

        private Vector2 NormalizeScreenPoint(Vector2 screenPoint, Rect fieldRect)
        {
            return new Vector2(
                Mathf.Clamp01((screenPoint.x - fieldRect.xMin) / fieldRect.width),
                Mathf.Clamp01((screenPoint.y - (Screen.height - fieldRect.yMax)) / fieldRect.height));
        }

        private void SetScreen(ScreenState screen)
        {
            _screen = screen;
            TrackScreen(screen);
        }

        private void TrackScreen(ScreenState screen)
        {
            _analyticsService.Track("screen_view", screen.ToString().ToLowerInvariant(), "{}");
        }

        private void BuildVisualAssets()
        {
            _pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _pixel.SetPixel(0, 0, Color.white);
            _pixel.Apply();

            _circle = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (var x = 0; x < 64; x++)
            {
                for (var y = 0; y < 64; y++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(32f, 32f)) / 32f;
                    var alpha = Mathf.Clamp01(1f - distance);
                    _circle.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
                }
            }

            _circle.Apply();

            _starField = new Vector2[48];
            for (var index = 0; index < _starField.Length; index++)
            {
                _starField[index] = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
            }
        }

        private void BuildStyles()
        {
            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.fontSize = 28;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = new Color(0.52f, 1f, 0.78f);

            _subtitleStyle = new GUIStyle(GUI.skin.label);
            _subtitleStyle.alignment = TextAnchor.MiddleCenter;
            _subtitleStyle.fontSize = 18;
            _subtitleStyle.fontStyle = FontStyle.Bold;
            _subtitleStyle.normal.textColor = new Color(0.85f, 1f, 0.94f);

            _bodyStyle = new GUIStyle(GUI.skin.label);
            _bodyStyle.fontSize = 16;
            _bodyStyle.normal.textColor = Color.white;

            _smallStyle = new GUIStyle(GUI.skin.label);
            _smallStyle.fontSize = 13;
            _smallStyle.wordWrap = true;
            _smallStyle.normal.textColor = new Color(0.85f, 0.95f, 1f);

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 15;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.hover.textColor = Color.white;
            _buttonStyle.active.textColor = Color.white;

            _cardStyle = new GUIStyle(GUI.skin.box);
            _cardStyle.normal.background = _pixel;
        }

        private void DrawBackground()
        {
            GUI.color = new Color(0.03f, 0.03f, 0.06f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _pixel);
            GUI.color = new Color(0.05f, 0.1f, 0.18f, 0.35f);
            GUI.DrawTexture(new Rect(-80f, 140f, Screen.width * 0.7f, Screen.height * 0.45f), _circle);
            GUI.color = new Color(0.1f, 0.02f, 0.18f, 0.45f);
            GUI.DrawTexture(new Rect(Screen.width * 0.42f, Screen.height * 0.1f, Screen.width * 0.72f, Screen.height * 0.5f), _circle);

            for (var index = 0; index < _starField.Length; index++)
            {
                DrawCircle(new Vector2(_starField[index].x * Screen.width, (1f - _starField[index].y) * Screen.height), 2f + (index % 3), new Color(1f, 1f, 1f, 0.22f));
            }
        }

        private void DrawGrid(Rect fieldRect)
        {
            for (var lineIndex = 0; lineIndex <= 6; lineIndex++)
            {
                var y = Mathf.Lerp(fieldRect.y + 20f, fieldRect.yMax - 20f, lineIndex / 6f);
                DrawLine(new Vector2(fieldRect.x + 14f, y), new Vector2(fieldRect.xMax - 14f, y), new Color(0f, 1f, 0.72f, 0.08f), 1f);
            }

            for (var lineIndex = 0; lineIndex <= 4; lineIndex++)
            {
                var x = Mathf.Lerp(fieldRect.x + 20f, fieldRect.xMax - 20f, lineIndex / 4f);
                DrawLine(new Vector2(x, fieldRect.y + 14f), new Vector2(x, fieldRect.yMax - 14f), new Color(0.2f, 0.8f, 1f, 0.08f), 1f);
            }
        }

        private static Color ResolveEnemyColor(EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Miner:
                    return new Color(0.95f, 0.38f, 1f, 0.9f);
                case EnemyArchetype.Rammer:
                    return new Color(1f, 0.34f, 0.42f, 0.9f);
                case EnemyArchetype.ShardCaster:
                    return new Color(0.32f, 0.9f, 1f, 0.9f);
                case EnemyArchetype.EliteWarden:
                    return new Color(1f, 0.76f, 0.2f, 0.95f);
                case EnemyArchetype.NullSovereign:
                    return new Color(1f, 0.22f, 0.32f, 0.98f);
                default:
                    return new Color(0.55f, 0.32f, 1f, 0.9f);
            }
        }

        private void DrawCard(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _pixel);
            GUI.color = new Color(0.4f, 1f, 0.82f, 0.25f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), _pixel);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), _pixel);
            GUI.color = Color.white;
        }

        private void DrawCircle(Vector2 center, float radius, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f), _circle);
            GUI.color = Color.white;
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            var matrix = GUI.matrix;
            var angle = Vector3.Angle(end - start, Vector2.right);
            if (start.y > end.y)
            {
                angle = -angle;
            }

            GUI.color = color;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, (end - start).magnitude, width), _pixel);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }

        private void DrawProgressBar(Rect rect, float progress, Color fillColor)
        {
            GUI.color = new Color(0.08f, 0.12f, 0.16f, 0.88f);
            GUI.DrawTexture(rect, _pixel);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(progress), rect.height), _pixel);
            GUI.color = Color.white;
        }

        private void DrawRectBorder(Rect rect, Color color, float width)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, width), _pixel);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - width, rect.width, width), _pixel);
            GUI.DrawTexture(new Rect(rect.x, rect.y, width, rect.height), _pixel);
            GUI.DrawTexture(new Rect(rect.xMax - width, rect.y, width, rect.height), _pixel);
            GUI.color = Color.white;
        }

        private Vector2 ToScreen(Vector2 normalizedPosition, Rect fieldRect)
        {
            return new Vector2(
                fieldRect.x + normalizedPosition.x * fieldRect.width,
                fieldRect.yMax - normalizedPosition.y * fieldRect.height);
        }

        private Rect GetSafeArea()
        {
            return new Rect(0f, 0f, Screen.width, Screen.height);
        }

        private Rect GetGameplayFieldRect()
        {
            return new Rect(16f, 96f, Screen.width - 32f, Screen.height - 172f);
        }

        private string GetNextUnlockSummary()
        {
            for (var index = 0; index < _catalog.UnlockTrack.Count; index++)
            {
                var entry = _catalog.UnlockTrack[index];
                if (!_profile.ClaimedUnlockTrackIds.Contains(entry.Id))
                {
                    var remaining = Mathf.Max(0, entry.RequiredXp - _profile.UnlockTrackXp);
                    return entry.RewardLabel + " in " + remaining + " XP";
                }
            }

            return "Track complete";
        }

        private string GetProjectedNextUnlockSummary(RewardBreakdown rewards)
        {
            var projectedXp = _profile.UnlockTrackXp + rewards.TotalUnlockTrackXp;
            for (var index = 0; index < _catalog.UnlockTrack.Count; index++)
            {
                var entry = _catalog.UnlockTrack[index];
                if (_profile.ClaimedUnlockTrackIds.Contains(entry.Id))
                {
                    continue;
                }

                var remaining = Mathf.Max(0, entry.RequiredXp - projectedXp);
                return entry.RewardLabel + " in " + remaining + " XP";
            }

            return "Track complete";
        }

        private float GetNextUnlockProgress()
        {
            var previousXp = 0;
            for (var index = 0; index < _catalog.UnlockTrack.Count; index++)
            {
                var entry = _catalog.UnlockTrack[index];
                if (_profile.ClaimedUnlockTrackIds.Contains(entry.Id))
                {
                    previousXp = entry.RequiredXp;
                    continue;
                }

                var segment = Mathf.Max(1, entry.RequiredXp - previousXp);
                return Mathf.Clamp01((float)(_profile.UnlockTrackXp - previousXp) / segment);
            }

            return 1f;
        }

        private List<UnlockTrackEntry> PreviewUnlocks(SaveProfile profile, RewardBreakdown rewards)
        {
            var projectedXp = profile.UnlockTrackXp + rewards.TotalUnlockTrackXp;
            var preview = new List<UnlockTrackEntry>();

            for (var index = 0; index < _catalog.UnlockTrack.Count; index++)
            {
                var entry = _catalog.UnlockTrack[index];
                if (projectedXp >= entry.RequiredXp && !profile.ClaimedUnlockTrackIds.Contains(entry.Id))
                {
                    preview.Add(entry);
                }
            }

            return preview;
        }

        private static string[] GetUnlockLabels(List<UnlockTrackEntry> entries)
        {
            var labels = new string[entries.Count];
            for (var index = 0; index < entries.Count; index++)
            {
                labels[index] = entries[index].RewardLabel;
            }

            return labels;
        }

        private void ShowMetaNotice(string message)
        {
            _metaNotice = message;
            _metaNoticeTimer = 3.2f;
        }
    }
}
