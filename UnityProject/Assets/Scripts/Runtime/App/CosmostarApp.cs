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
            BuildStyles();
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
            GUI.Label(new Rect(area.x + 28f, area.y + 140f, area.width * 0.48f, 28f), "Streak: " + _profile.CurrentStreak, _bodyStyle);
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

            var viewRect = new Rect(area.x + 20f, area.y + 170f, area.width - 40f, area.height - 210f);
            var contentRect = new Rect(0f, 0f, viewRect.width - 22f, 130f * _catalog.Missions.Count);
            _missionScroll = GUI.BeginScrollView(viewRect, _missionScroll, contentRect);

            for (var index = 0; index < _catalog.Missions.Count; index++)
            {
                var mission = _catalog.Missions[index];
                var progress = ProfileQueries.GetMissionProgress(_profile, mission.Id);
                var rect = new Rect(0f, index * 130f, contentRect.width, 114f);
                DrawCard(rect, mission.Id == _dailyContract.MissionId ? new Color(0f, 0.8f, 0.5f, 0.2f) : new Color(0f, 0f, 0f, 0.42f));
                GUI.Label(new Rect(rect.x + 18f, rect.y + 10f, rect.width - 160f, 28f), mission.DisplayName, _subtitleStyle);
                GUI.Label(new Rect(rect.x + 18f, rect.y + 42f, rect.width - 180f, 42f), mission.Description + " | " + mission.ModifierText, _smallStyle);
                GUI.Label(new Rect(rect.x + 18f, rect.y + 84f, 200f, 24f), "Stars: " + (progress == null ? 0 : progress.StarsEarned) + "   Reward: " + mission.Reward.SoftCurrency, _smallStyle);

                if (GUI.Button(new Rect(rect.width - 128f, rect.y + 28f, 112f, 54f), "Launch", _buttonStyle))
                {
                    StartMission(mission);
                }
            }

            GUI.EndScrollView();
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

            DrawCard(new Rect(area.x + 18f, area.y + 110f, area.width - 36f, 290f), new Color(0f, 0f, 0f, 0.46f));
            GUI.Label(new Rect(area.x + 36f, area.y + 128f, area.width - 72f, 36f), _selectedMission.DisplayName, _subtitleStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 170f, area.width - 72f, 32f), "Outcome: " + (_pendingEvaluation.Completed ? "Complete" : "Failed"), _bodyStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 202f, area.width - 72f, 32f), "Stars: " + _pendingEvaluation.StarsEarned + "   Kills: " + _pendingSummary.Kills, _bodyStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 234f, area.width - 72f, 32f), "Credits: " + _pendingRewards.TotalSoftCurrency + "   Shards: " + _pendingRewards.TotalModuleShards, _bodyStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 266f, area.width - 72f, 32f), "Track XP: " + _pendingRewards.TotalUnlockTrackXp + "   Streak bonus: " + _pendingRewards.StreakBonus, _bodyStyle);

            var unlockText = _pendingUnlocks.Count == 0 ? "No new unlocks yet." : "Unlocked: " + string.Join(", ", GetUnlockLabels(_pendingUnlocks));
            GUI.Label(new Rect(area.x + 36f, area.y + 312f, area.width - 72f, 58f), unlockText, _smallStyle);
            GUI.Label(new Rect(area.x + 36f, area.y + 368f, area.width - 72f, 48f), "Next track: " + GetProjectedNextUnlockSummary(_pendingRewards), _smallStyle);

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
            GUI.Label(new Rect(area.x + 28f, area.yMax - 146f, area.width - 56f, 38f), "Drag anywhere to move. Auto-fire is always active. Between phases, pick one upgrade and keep the lane alive.", _smallStyle);
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

                    var upgradeLabel = progress.Level >= module.MaxLevel ? "Maxed" : "Upgrade " + (module.UpgradeCost * progress.Level);
                    if (GUI.Button(new Rect(rowRect.width - 132f, rowRect.y + 52f, 118f, 34f), upgradeLabel, _buttonStyle))
                    {
                        if (_metaProgressionSystem.TryUnlockOrUpgradeModule(_profile, module))
                        {
                            _saveSystem.Save(_profile);
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

            DrawCircle(ToScreen(_runSession.Player.Position, fieldRect), 34f, new Color(0.3f, 1f, 0.78f, _runSession.Player.InvulnerabilityTimer > 0f ? 0.55f : 0.9f));
            DrawCircle(ToScreen(_runSession.Player.Position + new Vector2(0f, 0.015f), fieldRect), 14f, new Color(1f, 1f, 1f, 0.8f));

            for (var index = 0; index < _runSession.Pickups.Count; index++)
            {
                DrawCircle(ToScreen(_runSession.Pickups[index].Position, fieldRect), 12f, new Color(0.4f, 1f, 0.25f, 0.85f));
            }

            for (var index = 0; index < _runSession.Projectiles.Count; index++)
            {
                var projectile = _runSession.Projectiles[index];
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
        }

        private void DrawRunHud()
        {
            var phaseLabel = _runSession.Director.Phase.ToString().ToUpperInvariant();
            GUI.Label(new Rect(18f, 14f, Screen.width - 36f, 32f), _selectedMission.DisplayName, _subtitleStyle);
            GUI.Label(new Rect(18f, 42f, Screen.width - 36f, 24f), "Phase: " + phaseLabel + "   Time: " + _runSession.Director.ElapsedSeconds.ToString("0.0") + "s", _smallStyle);
            GUI.Label(new Rect(18f, 66f, 260f, 24f), "Hull " + Mathf.CeilToInt(_runSession.Player.Hull) + "/" + Mathf.CeilToInt(_runSession.Player.MaxHull) + "   Shield " + Mathf.CeilToInt(_runSession.Player.Shield), _smallStyle);
            GUI.Label(new Rect(Screen.width - 210f, 42f, 190f, 24f), "Kills " + _runSession.Kills + "   Rerolls " + _runSession.RerollsRemaining, _smallStyle);

            if (GUI.Button(new Rect(Screen.width - 112f, 10f, 94f, 28f), _runSession.Paused ? "Paused" : "Pause", _buttonStyle))
            {
                _runSession.Paused = true;
            }
        }

        private void DrawPauseOverlay()
        {
            var overlay = new Rect(28f, Screen.height * 0.28f, Screen.width - 56f, 220f);
            DrawCard(overlay, new Color(0f, 0f, 0f, 0.82f));
            GUI.Label(new Rect(overlay.x, overlay.y + 18f, overlay.width, 36f), "Run Paused", _titleStyle);
            GUI.Label(new Rect(overlay.x + 22f, overlay.y + 66f, overlay.width - 44f, 42f), "Resume when you are ready, or cash out the attempt and go back to the hub.", _smallStyle);

            if (GUI.Button(new Rect(overlay.x + 20f, overlay.y + 132f, overlay.width - 40f, 42f), "Resume", _buttonStyle))
            {
                _runSession.Paused = false;
            }

            if (GUI.Button(new Rect(overlay.x + 20f, overlay.y + 180f, overlay.width - 40f, 42f), "Abandon Run", _buttonStyle))
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
            var overlay = new Rect(22f, Screen.height * 0.21f, Screen.width - 44f, 290f);
            DrawCard(overlay, new Color(0f, 0.08f, 0.12f, 0.86f));
            GUI.Label(new Rect(overlay.x, overlay.y + 16f, overlay.width, 36f), "Pilot Brief", _titleStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 68f, overlay.width - 36f, 22f), "1. Drag anywhere on the screen to dodge.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 102f, overlay.width - 36f, 22f), "2. Weapons auto-fire; focus on movement.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 136f, overlay.width - 36f, 22f), "3. After each phase, choose one upgrade.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 170f, overlay.width - 36f, 22f), "4. Optional rewarded revive appears once if you crash.", _bodyStyle);
            GUI.Label(new Rect(overlay.x + 18f, overlay.y + 208f, overlay.width - 36f, 42f), "Keep the lane readable, survive the pressure, and push toward the Null Sovereign.", _smallStyle);

            if (GUI.Button(new Rect(overlay.x + 18f, overlay.y + 244f, overlay.width - 36f, 44f), "Launch Run", _buttonStyle))
            {
                _runSession.TutorialOpen = false;
                _runSession.Paused = false;
                _profile.SeenFtue = true;
                _saveSystem.Save(_profile);
            }
        }

        private void DrawDraftOverlay()
        {
            var overlay = new Rect(24f, Screen.height * 0.22f, Screen.width - 48f, Screen.height * 0.5f);
            DrawCard(overlay, new Color(0f, 0f, 0f, 0.82f));
            GUI.Label(new Rect(overlay.x, overlay.y + 18f, overlay.width, 36f), "Choose Your Upgrade", _titleStyle);

            for (var index = 0; index < _runSession.DraftChoices.Count; index++)
            {
                var choice = _runSession.DraftChoices[index];
                var card = new Rect(overlay.x + 18f, overlay.y + 80f + index * 108f, overlay.width - 36f, 92f);
                DrawCard(card, new Color(0f, 0.12f, 0.18f, 0.7f));
                GUI.Label(new Rect(card.x + 18f, card.y + 10f, card.width - 36f, 28f), choice.DisplayName, _subtitleStyle);
                GUI.Label(new Rect(card.x + 18f, card.y + 40f, card.width - 160f, 42f), choice.Description, _smallStyle);
                if (GUI.Button(new Rect(card.xMax - 116f, card.y + 22f, 96f, 48f), "Take", _buttonStyle))
                {
                    _upgradeDraftSystem.ApplyUpgrade(_runSession.Build, choice);
                    if (_runSession.Build.ShieldRestore > 0f)
                    {
                        _runSession.Player.Shield = Mathf.Min(_runSession.Player.MaxShield + _runSession.Build.BonusShield, _runSession.Player.Shield + _runSession.Build.ShieldRestore);
                        _runSession.Build.ShieldRestore = 0f;
                    }

                    _runSession.Player.MaxShield = _runSession.Ship.BaseShield + _runSession.Meta.BonusShield + _runSession.Build.BonusShield;
                    _runSession.DraftOpen = false;
                    _runSession.Director.ConsumeDraft();
                    _runSession.RewardMessage = choice.DisplayName + " equipped.";
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
                    _runSession.RewardMessage = rewardMessage;
                }
            }
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
                TutorialOpen = !_profile.SeenFtue,
                Paused = !_profile.SeenFtue
            };

            _runSession.Player.MaxHull = mission.DifficultyRating < 1.4f ? _catalog.Ship.BaseHull + meta.BonusHull : _catalog.Ship.BaseHull + meta.BonusHull - 5f;
            _runSession.Player.MaxShield = (_catalog.Ship.BaseShield + meta.BonusShield) * missionRules.StartingShieldMultiplier;
            _runSession.Player.Hull = _runSession.Player.MaxHull;
            _runSession.Player.Shield = _runSession.Player.MaxShield;
            _runSession.Player.Position = new Vector2(0.5f, 0.16f);
            _runSession.OverclockCooldown = 4f;

            SetScreen(ScreenState.Run);
            _analyticsService.Track("mission_started", "run", "{\"mission\":\"" + mission.Id + "\",\"weapon\":\"" + weapon.Id + "\"}");
        }

        private void TickRun(float deltaTime)
        {
            if (_runSession.DraftOpen || _runSession.Paused || _runSession.TutorialOpen || _runSession.AwaitingRewardedRevive)
            {
                return;
            }

            var pointer = GetPointerTarget();
            _playerController.TickMovement(_runSession, pointer, deltaTime);

            var tick = _runSession.Director.Advance(deltaTime, _runSession.BossDefeated);
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
                            _weaponSystem.SpawnEnemyShot(_runSession, enemy, shotDirection, 0.26f);
                        }
                        break;
                    case EnemyArchetype.EliteWarden:
                        enemy.Position += new Vector2(Mathf.Sin(enemy.Oscillator * 1.4f) * 0.06f, -enemy.Def.Speed * 0.2f) * speedFactor * deltaTime;
                        if (enemy.FireCooldown <= 0f)
                        {
                            enemy.FireCooldown = enemy.Def.FireInterval;
                            _weaponSystem.SpawnEnemyShot(_runSession, enemy, (_runSession.Player.Position - enemy.Position).normalized, 0.3f);
                            _weaponSystem.SpawnEnemyShot(_runSession, enemy, Quaternion.Euler(0f, 0f, 18f) * Vector2.down, 0.24f);
                            _weaponSystem.SpawnEnemyShot(_runSession, enemy, Quaternion.Euler(0f, 0f, -18f) * Vector2.down, 0.24f);
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
                    _playerController.ApplyDamage(_runSession, enemy.Def.ContactDamage);
                    enemy.Hull = 0f;
                }

                if (enemy.Hull <= 0f)
                {
                    if (enemy.Def.IsBoss)
                    {
                        _runSession.BossDefeated = true;
                    }

                    _runSession.Kills += 1;
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
                    if (Vector2.Distance(projectile.Position, _runSession.Player.Position) <= 0.04f)
                    {
                        _playerController.ApplyDamage(_runSession, projectile.Damage);
                        _runSession.Projectiles.RemoveAt(projectileIndex);
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
                var distance = Vector2.Distance(_runSession.Player.Position, pickup.Position);
                if (distance <= magnetRadius)
                {
                    var direction = (_runSession.Player.Position - pickup.Position).normalized;
                    pickup.Position += direction * deltaTime * 0.65f;
                }

                if (distance <= 0.035f)
                {
                    _runSession.PickupsCollected += 1;
                    _runSession.Pickups.RemoveAt(pickupIndex);
                }
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
                case MissionObjectiveKind.PreserveShield:
                    _runSession.Completed = _runSession.BossDefeated;
                    break;
            }

            if (_runSession.Completed || _runSession.BossDefeated)
            {
                FinishRun();
            }
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
                PickupsCollected = _runSession.PickupsCollected
            };

            _pendingEvaluation = _missionSystem.Evaluate(_selectedMission, _pendingSummary);
            _pendingRewards = _economySystem.CalculateRewards(_selectedMission, _pendingEvaluation, _dailyContract, _profile, _runSession.Meta);
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
            _runSession.RewardMessage = rewardMessage;

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
            var healthRatio = boss.Hull / boss.MaxHull;
            for (var index = 0; index < _catalog.BossPhases.Count; index++)
            {
                var phase = _catalog.BossPhases[index];
                if (healthRatio <= phase.TriggerHealthNormalized)
                {
                    boss.BossPhaseIndex = phase.PhaseIndex;
                }
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
                _weaponSystem.SpawnEnemyShot(_runSession, boss, direction, phaseDef.ProjectileSpeed);
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

        private Vector2 GetPointerTarget()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                return NormalizeScreenPoint(touch.position, GetGameplayFieldRect());
            }

            if (Input.GetMouseButton(0))
            {
                return NormalizeScreenPoint(Input.mousePosition, GetGameplayFieldRect());
            }

            return _runSession.Player.Position;
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
