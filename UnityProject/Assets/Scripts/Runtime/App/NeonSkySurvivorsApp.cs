using System;
using System.Collections.Generic;
using NeonSkySurvivors.Core.Design;
using NeonSkySurvivors.Core.Models;
using NeonSkySurvivors.Core.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonSkySurvivors.Runtime.App
{
    public sealed class NeonSkySurvivorsApp : MonoBehaviour
    {
        private const float ArenaHalfWidth = 3f;
        private const float ArenaHalfHeight = 5.35f;
        private const int MaxEnemyViews = 128;
        private const int MaxProjectileViews = 80;
        private const int MaxXpViews = 80;
        private const int MaxChestViews = 4;
        private const int MaxTrailViews = 24;
        private const int MaxZapViews = 12;
        private const int MaxOrbitViews = 6;
        private const int GridVerticalLines = 9;
        private const int GridHorizontalLines = 13;
        private const float GridScrollSpeed = 1.5f;
        private const float GridTop = 6f;
        private const float GridBottom = -6f;
        private const int StarCount = 46;
        private const int MaxParticles = 160;
        private const int MaxBossTelegraphCircles = 8;

        // Screen-shake state
        private float _shakeRemaining;
        private float _shakeAmplitude;
        private Vector3 _cameraBasePosition;

        // Hit-stop state
        private float _hitStopRemaining;

        // Double-tap dash detection (optional input mode)
        private const float DoubleTapWindow = 0.28f;
        private float _lastTapTime = -1f;
        private bool _pointerWasDown;

        // Boss telegraph circles (danger zone rings)
        private readonly List<LineRenderer> _telegraphCircles = new List<LineRenderer>();
        private readonly List<float> _telegraphRadii = new List<float>();

        private readonly NeonRunGameplaySystem _gameplay = new NeonRunGameplaySystem();
        private readonly NeonEquipmentSystem _equipment = new NeonEquipmentSystem();
        private readonly NeonAudioService _audio = new NeonAudioService();
        private readonly List<SpriteRenderer> _enemyViews = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _projectileViews = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _xpViews = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _chestViews = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _orbitViews = new List<SpriteRenderer>();
        private readonly List<LineRenderer> _trailViews = new List<LineRenderer>();
        private readonly List<LineRenderer> _zapViews = new List<LineRenderer>();
        private readonly List<Button> _upgradeButtons = new List<Button>();
        private readonly List<LineRenderer> _gridLines = new List<LineRenderer>();
        private readonly List<float> _gridLineY = new List<float>();
        private readonly List<Transform> _stars = new List<Transform>();
        private readonly List<float> _starSpeeds = new List<float>();
        private readonly List<NeonParticleView> _particles = new List<NeonParticleView>();
        private readonly Dictionary<NeonRunEnemyState, EnemyDeathSnapshot> _enemySnapshots = new Dictionary<NeonRunEnemyState, EnemyDeathSnapshot>();
        private readonly HashSet<NeonRunEnemyState> _enemyAfterTick = new HashSet<NeonRunEnemyState>();
        private int _particleCursor;

        private NeonSkySurvivorsCatalog _catalog = null!;
        private NeonSaveProfile _profile = null!;
        private NeonRunState _run = null!;
        private Camera _camera = null!;
        private Sprite _sprite = null!;
        private Transform _playerRoot = null!;
        private SpriteRenderer _playerBody = null!;
        private SpriteRenderer _playerNose = null!;
        private SpriteRenderer _playerWingLeft = null!;
        private SpriteRenderer _playerWingRight = null!;
        private Text _timerText = null!;
        private Text _waveText = null!;
        private Text _killsText = null!;
        private Text _messageText = null!;
        private Text _statusText = null!;
        private Image _hpBarFill = null!;
        private Image _xpBarFill = null!;
        private Button _dashButton = null!;
        private Button _specialButton = null!;
        private Image _specialRing = null!;
        private Image _dashRing = null!;
        private Button _pauseButton = null!;
        private Text _pauseLabel = null!;
        private GameObject _bossBarRoot = null!;
        private Image _bossBarFill = null!;
        private Text _bossBarText = null!;
        private GameObject _garagePanel = null!;
        private Text _garageTitleText = null!;
        private Text _garageStatsText = null!;
        private Text _sectorLabel = null!;
        private Text _garageDetailText = null!;
        private RectTransform _inventoryContent = null!;
        private Button _equipButton = null!;
        private Button _unequipButton = null!;
        private Button _upgradeButton = null!;
        private Button _mergeButton = null!;
        private readonly List<GameObject> _inventoryCards = new List<GameObject>();
        private string _selectedInstanceId = string.Empty;
        private NeonEquipmentSlot? _selectedSlotFilter;
        private readonly Dictionary<NeonEquipmentSlot, Button> _slotFilterButtons = new Dictionary<NeonEquipmentSlot, Button>();
        private readonly Dictionary<NeonEquipmentSlot, NeonCutRect> _slotBorders = new Dictionary<NeonEquipmentSlot, NeonCutRect>();
        private readonly Dictionary<NeonEquipmentSlot, Image> _slotIcons = new Dictionary<NeonEquipmentSlot, Image>();
        private GameObject _resultsPanel = null!;
        private Text _resultsTitleText = null!;
        private Text _resultsStatsText = null!;   // repurposed: items-found list
        private Text _resultsTimeText = null!;    // hero TIME SURVIVED value
        private readonly Text[] _resultsStatValues = new Text[6];
        private readonly Text[] _pauseStatValues = new Text[6];
        private readonly Text[] _menuMissionTexts = new Text[3];
        private readonly NeonCutRect[] _menuMissionBorders = new NeonCutRect[3];
        private GameObject _upgradePanel = null!;
        private readonly List<Image> _upgradeButtonIcons = new List<Image>();
        private readonly List<NeonPolyGraphic> _upgradeMedallions = new List<NeonPolyGraphic>();
        private readonly List<Button> _upgradeBanishButtons = new List<Button>();
        private Button _rerollButton = null!;
        private Text _menuCoinsChip = null!;
        private Text _menuRankChip = null!;
        private readonly List<GameObject> _buffChips = new List<GameObject>();
        private readonly List<NeonPolyGraphic> _buffChipBgs = new List<NeonPolyGraphic>();
        private readonly List<Image> _buffChipIcons = new List<Image>();
        private int _lastBuffSignature = -1;
        private Dictionary<string, NeonUpgradeCategory>? _upgradeCatById;
        private bool _paused;
        private bool _resultApplied;
        private int _lastRewardCoins;
        private int _lastRewardItems;
        private GameObject _mainMenuPanel = null!;
        private Text _mainMenuStatsText = null!;
        private GameObject _settingsPanel = null!;
        private Text _settingsMusicText = null!;
        private Text _settingsSfxText = null!;
        private Text _settingsVibrationText = null!;
        private Text _settingsDashModeText = null!;
        private Text _settingsReducedMotionText = null!;
        private bool _settingsFromMainMenu;
        private GameObject _pauseMenuPanel = null!;
        private readonly List<string> _lastRewardItemList = new List<string>();
        private GameObject _missionsPanel = null!;
        private RectTransform _missionsContent = null!;
        private GameObject _achievementsPanel = null!;
        private RectTransform _achievementsContent = null!;

        private NeonRunStatus _prevStatus;
        private int _prevEnemiesKilled;
        private int _prevPlayerProjectiles;
        private int _prevBossCount;
        private float _prevHP;
        private float _prevXP;
        private string _prevWarning = string.Empty;
        private int _prevEvolutionCount;
        private float _xpSoundCooldown;
        private float _damageSoundCooldown;

        // Ambient combat backdrop (shown behind main-menu / garage)
        private bool _ambientActive;
        private float _ambientPhase;
        private const int AmbientShipCount = 6;
        private readonly Vector2[] _ambientPositions = new Vector2[AmbientShipCount];
        private readonly Vector2[] _ambientVelocities = new Vector2[AmbientShipCount];
        private readonly float[] _ambientSpinRates = new float[AmbientShipCount];
        private readonly float[] _ambientAngles = new float[AmbientShipCount];
        private readonly List<SpriteRenderer> _ambientViews = new List<SpriteRenderer>();

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.orientation = ScreenOrientation.Portrait;

            _catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            _profile = NeonSaveService.Load() ?? new NeonSaveProfile();
            _equipment.EnsureStartingProfile(_profile, _catalog);
            NeonSaveService.Save(_profile);
            _sprite = CreateSprite();

            EnsureCamera();
            _cameraBasePosition = _camera.transform.position;
            EnsureEventSystem();
            CreateNeonBackground();
            CreatePlayerView();
            CreateAmbientLayer();
            CreatePools();
            CreateTelegraphCirclePool();
            CreateParticlePool();
            CreateHud();
            _audio.Initialize(transform);
            ApplyAudioSettings();
            ShowMainMenu();
        }

        private void Update()
        {
            var dt = Mathf.Min(Time.deltaTime, 0.05f);

            // Grid and starfield always scroll — gives menu screens a living background.
            UpdateNeonBackground(dt);

            if (_ambientActive)
            {
                TickAmbient(dt);
            }

            if (_run == null)
            {
                return;
            }

            HandleTouchInput();

            // Advance hit-stop timer — restores timescale once done.
            if (_hitStopRemaining > 0f)
            {
                _hitStopRemaining -= Time.unscaledDeltaTime;
                if (_hitStopRemaining <= 0f) Time.timeScale = 1f;
            }

            if (!_paused && _run.Status == NeonRunStatus.Running)
            {
                CapturePreTickEnemies();
                _gameplay.Tick(_run, _catalog, dt);
                SpawnDeathBursts();
            }

            RenderRun();
            UpdateParticles(dt);
            UpdateScreenShake(dt);
            UpdateAudio(dt);
            UpdateHud();
        }

        private void StartRun()
        {
            HideAmbient();
            var sector = ClampSelectedSector();
            _run = _gameplay.StartRun(_profile, _catalog, sector);
            // Pilot-rank bonuses: extra rerolls and banishes from milestone perks.
            _run.RerollsRemaining  += PilotExtraRerolls(_profile.Meta.AccountLevel);
            _run.BanishesRemaining += PilotExtraBanishes(_profile.Meta.AccountLevel);
            _paused = false;
            _pauseLabel.text = "II";
            _resultApplied = false;
            _lastRewardCoins = 0;
            _lastRewardItems = 0;
            _garagePanel.SetActive(false);
            _resultsPanel.SetActive(false);
            _pauseMenuPanel.SetActive(false);
            _mainMenuPanel.SetActive(false);
            _settingsPanel.SetActive(false);
            _missionsPanel.SetActive(false);
            _achievementsPanel.SetActive(false);
            SetRunHudVisible(true);
            _statusText.text = string.Empty;
            _messageText.text = (sector > 1 ? "SECTOR " + sector + " — " : string.Empty)
                + "Survive 10 minutes. Bosses at 3:00, 6:00, 7:30, 8:45, 10:00.";
            UpdateUpgradeChoices(false);
            ResetAudioTrackers();
            _audio.SetMusic("normal");
        }

        private int MaxUnlockedSector => Mathf.Min(NeonRunGameplaySystem.MaxSector, _profile.HighestSectorCleared + 1);

        private int ClampSelectedSector()
        {
            _profile.SelectedSector = Mathf.Clamp(_profile.SelectedSector, 1, MaxUnlockedSector);
            return _profile.SelectedSector;
        }

        private void CycleSector(int delta)
        {
            _profile.SelectedSector = Mathf.Clamp(_profile.SelectedSector + delta, 1, MaxUnlockedSector);
            NeonSaveService.Save(_profile);
            UpdateSectorLabel();
        }

        private void UpdateSectorLabel()
        {
            var sector = ClampSelectedSector();
            if (MaxUnlockedSector == 1)
            {
                _sectorLabel.text = "SECTOR 1 — win a run to unlock Sector 2";
                return;
            }

            var rewardPercent = Mathf.RoundToInt((NeonRunGameplaySystem.SectorRewardScale(sector) - 1f) * 100f);
            _sectorLabel.text = sector == 1
                ? "SECTOR 1 / " + MaxUnlockedSector + " — base difficulty"
                : "SECTOR " + sector + " / " + MaxUnlockedSector + " — tougher enemies, +" + rewardPercent + "% rewards";
        }

        private void ShowGarage()
        {
            _run = null!;
            _paused = true;
            _resultApplied = false;
            _selectedSlotFilter = null;
            HideRuntimeViews();
            UpdateUpgradeChoices(false);
            SetRunHudVisible(false);
            _resultsPanel.SetActive(false);
            _mainMenuPanel.SetActive(false);
            _settingsPanel.SetActive(false);
            _pauseMenuPanel.SetActive(false);
            _missionsPanel.SetActive(false);
            _achievementsPanel.SetActive(false);
            _garagePanel.SetActive(true);
            UpdateGaragePanel();
            _audio.StopMusic();
            ShowAmbient();
        }

        private void ShowMainMenu()
        {
            _run = null!;
            _paused = true;
            _resultApplied = false;
            HideRuntimeViews();
            SetRunHudVisible(false);
            _garagePanel.SetActive(false);
            _settingsPanel.SetActive(false);
            _pauseMenuPanel.SetActive(false);
            _resultsPanel.SetActive(false);
            _missionsPanel.SetActive(false);
            _achievementsPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
            UpdateMainMenuPanel();
            _audio.StopMusic();
            ShowAmbient();
        }

        private void UpdateMainMenuPanel()
        {
            _menuCoinsChip.text = "◎ " + _profile.PlayerCoins;
            _menuRankChip.text = "LV " + _profile.Meta.AccountLevel;
            _mainMenuStatsText.text = "BEST " + FormatTime(_profile.BestSurvivalTime)
                + "   ·   RUNS " + _profile.CompletedRuns
                + "   ·   BOSSES " + _profile.BossesDefeated;

            RefreshDailyMissions();
            for (var i = 0; i < 3; i++)
            {
                if (i >= _profile.Meta.DailyMissions.Count) break;
                var mission = _profile.Meta.DailyMissions[i];
                Color borderColor;
                string statusLine;
                if (mission.Claimed)
                {
                    borderColor = NeonUITheme.Uncommon;
                    statusLine = "✓ CLAIMED";
                }
                else if (mission.Progress >= mission.Target)
                {
                    borderColor = NeonUITheme.Cyan;
                    statusLine = "COMPLETE";
                }
                else
                {
                    borderColor = NeonUITheme.Line2;
                    statusLine = mission.Progress + " / " + mission.Target;
                }
                _menuMissionTexts[i].text = mission.Name + "\n" + statusLine;
                _menuMissionBorders[i].BorderColor = borderColor;
                _menuMissionBorders[i].Refresh();
            }
        }

        private void ShowSettings(bool fromMainMenu)
        {
            _settingsFromMainMenu = fromMainMenu;
            _garagePanel.SetActive(false);
            _mainMenuPanel.SetActive(false);
            _settingsPanel.SetActive(true);
            UpdateSettingsPanel();
        }

        private void HideSettings()
        {
            _settingsPanel.SetActive(false);
            if (_settingsFromMainMenu)
            {
                ShowMainMenu();
            }
            else
            {
                ShowGarage();
            }
        }

        private void UpdateSettingsPanel()
        {
            _settingsMusicText.text = "Music  " + Mathf.RoundToInt(_profile.MusicVolume * 100f) + "%";
            _settingsSfxText.text = "SFX  " + Mathf.RoundToInt(_profile.SfxVolume * 100f) + "%";
            _settingsVibrationText.text = "Vibration  " + (_profile.VibrationEnabled ? "ON" : "OFF");
            _settingsDashModeText.text = "Dash  " + (_profile.DoubleTapDashEnabled ? "DOUBLE-TAP" : "BUTTON");
            _settingsReducedMotionText.text = "Reduced Motion  " + (_profile.ReducedMotionEnabled ? "ON" : "OFF");
        }

        private void ApplyAudioSettings()
        {
            _audio.MusicVolume = _profile.MusicVolume;
            _audio.SfxVolume = _profile.SfxVolume;
        }

        private void HandleTouchInput()
        {
            if (_run.Status != NeonRunStatus.Running)
            {
                return;
            }

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                if (touch.phase == TouchPhase.Began)
                {
                    RegisterTapForDoubleTapDash();
                }

                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    SetMovementTarget(touch.position);
                }

                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                _pointerWasDown = Input.GetMouseButton(0);
                return;
            }

            // Editor / mouse fallback: rising edge of the pointer counts as a tap.
            var pointerDown = Input.GetMouseButton(0);
            if (pointerDown && !_pointerWasDown)
            {
                RegisterTapForDoubleTapDash();
            }
            _pointerWasDown = pointerDown;

            if (pointerDown)
            {
                SetMovementTarget(Input.mousePosition);
            }
        }

        private void RegisterTapForDoubleTapDash()
        {
            if (!_profile.DoubleTapDashEnabled)
            {
                return;
            }

            var now = Time.unscaledTime;
            if (_lastTapTime >= 0f && now - _lastTapTime <= DoubleTapWindow)
            {
                TryDash();
                _lastTapTime = -1f;
            }
            else
            {
                _lastTapTime = now;
            }
        }

        private void SetMovementTarget(Vector2 screenPosition)
        {
            var world = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));
            _gameplay.SetMovementTarget(_run, new NeonVector2(world.x / ArenaHalfWidth, world.y / ArenaHalfHeight));
        }

        private void TryDash()
        {
            if (_run != null && _gameplay.TryDash(_run))
            {
                _audio.PlayDash();
                if (_profile.VibrationEnabled) Handheld.Vibrate();
            }
        }

        private void RenderRun()
        {
            RenderEnemies();
            RenderProjectiles();
            RenderXp();
            RenderEvolutionChests();
            RenderTrails();
            RenderTeslaZaps();
            RenderOrbitBlades();
            RenderPlayer();
            RenderBossTelegraphs();
        }

        private void RenderOrbitBlades()
        {
            HideAll(_orbitViews);
            if (_run.Status != NeonRunStatus.Running && _run.Status != NeonRunStatus.LevelUpDraft)
            {
                return;
            }

            var count = Mathf.Min(_run.OrbitBlades.Count, _orbitViews.Count);
            for (var index = 0; index < count; index++)
            {
                var view = _orbitViews[index];
                view.gameObject.SetActive(true);
                view.transform.position = ToWorld(_run.OrbitBlades[index]);
                view.transform.localScale = Vector3.one * 0.16f;
                view.color = new Color(0.7f, 0.5f, 1f);
            }
        }

        private void RenderPlayer()
        {
            var player = _run.Player;
            var active = _run.Status == NeonRunStatus.Running || _run.Status == NeonRunStatus.LevelUpDraft;
            _playerRoot.gameObject.SetActive(active);
            if (!active)
            {
                return;
            }

            _playerRoot.position = ToWorld(player.Position);

            var direction = player.LastMoveDirection;
            if (direction.SqrMagnitude > 0.0001f)
            {
                var angle = Mathf.Atan2(direction.Y, direction.X) * Mathf.Rad2Deg - 90f;
                _playerRoot.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Bright pulsing flash while dashing / briefly invulnerable.
            var invulnerable = player.InvulnerabilityRemaining > 0f;
            var bodyColor = new Color(0.25f, 0.85f, 1f);
            if (invulnerable)
            {
                var pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 28f);
                bodyColor = Color.Lerp(bodyColor, Color.white, pulse);
            }

            _playerBody.color = bodyColor;
            _playerNose.color = new Color(0.7f, 1f, 1f);
            _playerWingLeft.color = new Color(0.45f, 0.6f, 1f);
            _playerWingRight.color = _playerWingLeft.color;
        }

        private void RenderEnemies()
        {
            HideAll(_enemyViews);
            var count = Mathf.Min(_run.Enemies.Count, _enemyViews.Count);
            for (var index = 0; index < count; index++)
            {
                var enemy = _run.Enemies[index];
                var view = _enemyViews[index];
                view.gameObject.SetActive(true);
                view.transform.position = ToWorld(enemy.Position);
                view.transform.localScale = Vector3.one * ResolveEnemySize(enemy);
                view.sprite = enemy.IsBoss ? NeonSpriteFactory.Boss : NeonSpriteFactory.GetEnemy(enemy.Behavior);

                if (enemy.IsBoss)
                {
                    // Phase-based boss pulse: darker when HP is low.
                    var hpFraction = enemy.MaxHP > 0f ? enemy.HP / enemy.MaxHP : 1f;
                    var pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 4f);
                    view.color = enemy.IsMiniBoss
                        ? new Color(1f, 0.62f * pulse, 0.18f)
                        : Color.Lerp(new Color(1f, 0.18f, 0.82f), new Color(1f, 0.65f, 0.1f), 1f - hpFraction) * pulse;
                }
                else
                {
                    view.color = ResolveEnemyColor(enemy.Behavior);
                    // Slight pulsing on shield drones
                    if (enemy.Behavior == NeonEnemyBehaviorType.Tank)
                    {
                        var shield = 0.7f + 0.3f * Mathf.Sin(Time.time * 3f);
                        view.color = new Color(0.28f * shield, 0.72f * shield, 1f * shield);
                    }
                }

                // Attack-ready flash on shooters/mine-carriers
                if (!enemy.IsBoss && enemy.AttackCooldownRemaining <= 0.18f && enemy.AttackCooldownRemaining >= 0f &&
                    (enemy.Behavior == NeonEnemyBehaviorType.Shooter || enemy.Behavior == NeonEnemyBehaviorType.MineCarrier))
                {
                    view.color = Color.white;
                }
            }
        }

        private static Color ResolveEnemyColor(NeonEnemyBehaviorType behavior)
        {
            switch (behavior)
            {
                case NeonEnemyBehaviorType.FastChaser: return new Color(1f, 0.35f, 0.72f);   // hot pink
                case NeonEnemyBehaviorType.Shooter:    return new Color(1f, 0.65f, 0.1f);    // amber
                case NeonEnemyBehaviorType.Tank:       return new Color(0.28f, 0.72f, 1f);   // steel blue
                case NeonEnemyBehaviorType.MineCarrier: return new Color(0.35f, 0.85f, 0.3f); // toxic green
                case NeonEnemyBehaviorType.Splitter:   return new Color(0.6f, 0.28f, 1f);    // violet
                default:                               return new Color(1f, 0.24f, 0.36f);   // red (Chaser)
            }
        }

        private void RenderProjectiles()
        {
            HideAll(_projectileViews);
            var count = Mathf.Min(_run.Projectiles.Count, _projectileViews.Count);
            for (var index = 0; index < count; index++)
            {
                var projectile = _run.Projectiles[index];
                var view = _projectileViews[index];
                view.gameObject.SetActive(true);
                view.transform.position = ToWorld(projectile.Position);

                if (projectile.FromPlayer)
                {
                    view.sprite = NeonSpriteFactory.Projectile;
                    view.transform.localScale = Vector3.one * 0.12f;
                    view.color = new Color(0.38f, 1f, 0.52f);
                }
                else if (projectile.IsMine)
                {
                    // Pulsing orange hazard so mines read as a telegraphed danger.
                    var pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 10f);
                    view.sprite = NeonSpriteFactory.Mine;
                    view.transform.localScale = Vector3.one * 0.2f;
                    view.color = new Color(1f, 0.55f * pulse, 0.12f);
                }
                else
                {
                    view.sprite = NeonSpriteFactory.Projectile;
                    view.transform.localScale = Vector3.one * 0.14f;
                    view.color = new Color(1f, 0.4f, 0.25f);
                }
            }
        }

        private void RenderXp()
        {
            HideAll(_xpViews);
            var count = Mathf.Min(_run.XpShards.Count, _xpViews.Count);
            for (var index = 0; index < count; index++)
            {
                var shard = _run.XpShards[index];
                var view = _xpViews[index];
                view.gameObject.SetActive(true);
                view.transform.position = ToWorld(shard.Position);
                view.transform.localScale = Vector3.one * 0.11f;
                view.color = new Color(0.23f, 1f, 0.78f);
            }
        }

        private void RenderEvolutionChests()
        {
            HideAll(_chestViews);
            var count = Mathf.Min(_run.EvolutionChests.Count, _chestViews.Count);
            for (var index = 0; index < count; index++)
            {
                var chest = _run.EvolutionChests[index];
                var view = _chestViews[index];
                view.gameObject.SetActive(true);
                view.transform.position = ToWorld(chest.Position);
                // Gold pulsing star so the boss-drop chest reads as a special pickup;
                // it fades in its final 3 seconds before despawning.
                var pulse = 0.22f + 0.05f * Mathf.Sin(Time.time * 6f);
                view.transform.localScale = Vector3.one * pulse;
                var alpha = Mathf.Clamp01(chest.RemainingLife / 3f);
                view.color = new Color(1f, 0.82f, 0.2f, Mathf.Lerp(0.35f, 1f, alpha));
            }
        }

        private void RenderTrails()
        {
            for (var index = 0; index < _trailViews.Count; index++)
            {
                var view = _trailViews[index];
                if (index >= _run.DashTrails.Count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                var trail = _run.DashTrails[index];
                var lifeRatio = Mathf.Clamp01(trail.RemainingLifetime / 1.5f);
                var alpha = lifeRatio * 0.9f;
                var width = Mathf.Lerp(0.06f, 0.28f, lifeRatio);

                view.gameObject.SetActive(true);
                view.positionCount = 2;
                view.SetPosition(0, ToWorld(trail.Start));
                view.SetPosition(1, ToWorld(trail.End));
                view.startWidth = width;
                view.endWidth = width * 0.3f;
                view.startColor = new Color(0.36f, 0.95f, 1f, alpha);
                view.endColor = new Color(0.75f, 0.38f, 1f, alpha * 0.35f);
            }
        }

        private void RenderTeslaZaps()
        {
            for (var index = 0; index < _zapViews.Count; index++)
            {
                var view = _zapViews[index];
                if (index >= _run.TeslaZaps.Count)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                var zap = _run.TeslaZaps[index];
                var alpha = Mathf.Clamp01(zap.RemainingLife / 0.22f);
                view.gameObject.SetActive(true);
                view.positionCount = 2;
                view.SetPosition(0, ToWorld(zap.Start));
                view.SetPosition(1, ToWorld(zap.End));
                view.startColor = new Color(0.65f, 0.92f, 1f, 0.95f * alpha);
                view.endColor = new Color(1f, 1f, 1f, 0.8f * alpha);
            }
        }

        private void UpdateHud()
        {
            var player = _run.Player;
            var hpPercent = Mathf.Clamp01(player.Stats.CurrentHP / player.Stats.MaxHP);
            var xpPercent = Mathf.Clamp01(player.XP / player.XPToNextLevel);
            _hpBarFill.fillAmount = hpPercent;
            _hpBarFill.color = Color.Lerp(new Color(1f, 0.28f, 0.3f), new Color(0.3f, 1f, 0.6f), hpPercent);
            _xpBarFill.fillAmount = xpPercent;
            _timerText.text = FormatTime(_run.ElapsedSeconds);
            var wave = Mathf.FloorToInt(_run.ElapsedSeconds / 60f) + 1;
            _waveText.text = (_run.Sector > 1 ? "S" + _run.Sector + " · " : string.Empty) + "WAVE " + wave + " · LV " + player.Level;
            _killsText.text = "☠ " + _run.EnemiesKilled + "\n◎ " + player.CoinsCollected;
            UpdateBuffChips();

            if (!string.IsNullOrWhiteSpace(_run.LastWarning))
            {
                _messageText.text = _run.LastWarning;
            }

            if (_run.Status == NeonRunStatus.LevelUpDraft)
            {
                _statusText.text = "LEVEL UP";
                UpdateUpgradeChoices(true);
            }
            else if (_run.Status == NeonRunStatus.GameOver)
            {
                _statusText.text = "GAME OVER";
                UpdateUpgradeChoices(false);
                ShowResults("GAME OVER");
            }
            else if (_run.Status == NeonRunStatus.Victory)
            {
                _statusText.text = "MISSION COMPLETE";
                UpdateUpgradeChoices(false);
                ShowResults("MISSION COMPLETE");
            }
            else
            {
                _statusText.text = _paused ? "PAUSED" : string.Empty;
                UpdateUpgradeChoices(false);
            }

            UpdateBossBar();
            var specialReady = player.SpecialCharge >= player.SpecialChargeMax;
            _specialRing.fillAmount = Mathf.Clamp01(player.SpecialCharge / player.SpecialChargeMax);
            _specialRing.color = specialReady ? NeonUITheme.CyanSoft : NeonUITheme.Magenta;
            _specialButton.interactable = specialReady && !_paused && _run.Status == NeonRunStatus.Running;

            var dashMax = player.Stats.DashCooldown;
            var dashReady = player.DashCooldownRemaining <= 0f;
            _dashRing.fillAmount = dashMax > 0.01f ? Mathf.Clamp01(1f - player.DashCooldownRemaining / dashMax) : 1f;
            _dashRing.color = dashReady ? NeonUITheme.Cyan : NeonUITheme.Alpha(NeonUITheme.Cyan, 0.5f);
            _dashButton.interactable = !_paused && _run.Status == NeonRunStatus.Running && dashReady;
            _pauseButton.interactable = _run.Status == NeonRunStatus.Running;
        }

        private void ShowResults(string title)
        {
            if (!_resultApplied)
            {
                _lastRewardCoins = CalculateRunReward(_run);
                _lastRewardItems = GrantRunRewardItems(_run);
                _profile.PlayerCoins += _lastRewardCoins;
                _profile.CompletedRuns += 1;
                _profile.BestSurvivalTime = Mathf.Max(_profile.BestSurvivalTime, _run.ElapsedSeconds);
                _profile.BossesDefeated += _run.BossesKilled + _run.MiniBossesKilled;
                if (_run.Status == NeonRunStatus.Victory)
                {
                    // Beating a sector unlocks the next difficulty tier.
                    _profile.HighestSectorCleared = Mathf.Max(_profile.HighestSectorCleared, _run.Sector);
                }
                // Accumulate lifetime stats
                _profile.LifetimeEnemiesKilled += _run.EnemiesKilled;
                _profile.LifetimeBossesKilled  += _run.BossesKilled + _run.MiniBossesKilled;
                _profile.LifetimeTimePlayed    += _run.ElapsedSeconds;
                _resultApplied = true;
                UpdateMissionProgressFromRun(_run);
                CheckAchievements(_run);
                NeonSaveService.Save(_profile);
            }

            SetRunHudVisible(false);
            _resultsPanel.SetActive(true);
            _resultsTitleText.text = title;
            _resultsTimeText.text = FormatTime(_run.ElapsedSeconds);
            _resultsStatValues[0].text = _run.EnemiesKilled.ToString();
            _resultsStatValues[1].text = (_run.BossesKilled + _run.MiniBossesKilled).ToString();
            _resultsStatValues[2].text = "+" + _lastRewardCoins;
            _resultsStatValues[3].text = _profile.PlayerCoins.ToString();
            _resultsStatValues[4].text = FormatTime(_profile.BestSurvivalTime);
            _resultsStatValues[5].text = _profile.CompletedRuns.ToString();
            _resultsStatsText.text = _lastRewardItemList.Count > 0
                ? "SALVAGE: " + string.Join(" · ", _lastRewardItemList)
                : "No items recovered.";
        }

        private int GrantRunRewardItems(NeonRunState run)
        {
            _lastRewardItemList.Clear();
            var dropped = 0;

            // Quantum Sensor (Radar): boss rewards improved — better drop odds and rarity bias.
            var boosted = run.ActiveEquipmentEffects.Contains("boss_reward_boost");
            var miniDropChance = boosted ? 0.75f : 0.6f;
            var bossDropChance = boosted ? 0.85f : 0.7f;
            var rarityUpChance = boosted ? 0.65f : 0.5f;
            var epicChance = boosted ? 0.4f : 0.25f;

            for (var index = 0; index < run.MiniBossesKilled; index++)
            {
                if (UnityEngine.Random.value < miniDropChance)
                {
                    var rarity = UnityEngine.Random.value < rarityUpChance ? NeonEquipmentRarity.Uncommon : NeonEquipmentRarity.Common;
                    var item = GrantRandomItemTracked(rarity);
                    if (item != null) { _lastRewardItemList.Add(item + " [" + rarity + "]"); dropped++; }
                }
            }

            for (var index = 0; index < run.BossesKilled; index++)
            {
                if (UnityEngine.Random.value < bossDropChance)
                {
                    var rarity = UnityEngine.Random.value < rarityUpChance ? NeonEquipmentRarity.Rare : NeonEquipmentRarity.Uncommon;
                    var item = GrantRandomItemTracked(rarity);
                    if (item != null) { _lastRewardItemList.Add(item + " [" + rarity + "]"); dropped++; }
                }
            }

            if (run.Status == NeonRunStatus.Victory)
            {
                var rarity = UnityEngine.Random.value < epicChance ? NeonEquipmentRarity.Epic : NeonEquipmentRarity.Rare;
                var item = GrantRandomItemTracked(rarity);
                if (item != null) { _lastRewardItemList.Add(item + " [" + rarity + "] *"); dropped++; }
            }

            return dropped;
        }

        private string GrantRandomItemTracked(NeonEquipmentRarity rarity)
        {
            if (_catalog.Equipment.Count == 0) return null!;
            var definition = _catalog.Equipment[UnityEngine.Random.Range(0, _catalog.Equipment.Count)];
            _profile.OwnedEquipmentItems.Add(new NeonOwnedEquipmentItem
            {
                InstanceID = definition.ItemID + "_drop_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                ItemID = definition.ItemID,
                Rarity = rarity,
                Level = 1
            });
            return definition.Name;
        }

        private void UpdateUpgradeChoices(bool visible)
        {
            _upgradePanel.SetActive(visible);
            if (!visible)
            {
                return;
            }

            for (var index = 0; index < _upgradeButtons.Count; index++)
            {
                var hasChoice = index < _run.DraftChoices.Count;
                var button = _upgradeButtons[index];
                button.gameObject.SetActive(hasChoice);
                if (!hasChoice)
                {
                    continue;
                }

                var choice = _run.DraftChoices[index];
                var categoryColor = ResolveUpgradeCategoryColor(choice.Category);
                AccentButton(button, categoryColor);

                if (index < _upgradeMedallions.Count)
                {
                    var medallion = _upgradeMedallions[index];
                    medallion.color = NeonUITheme.Mix(categoryColor, 0.16f, NeonUITheme.Bg1);
                    medallion.BorderColor = categoryColor;
                    medallion.Refresh();
                }

                if (index < _upgradeButtonIcons.Count)
                {
                    var icon = _upgradeButtonIcons[index];
                    icon.sprite = NeonSpriteFactory.GetUpgradeIcon(choice.Category);
                    icon.color  = categoryColor;
                }

                var label = button.GetComponentInChildren<Text>();
                label.color = categoryColor;
                label.text = choice.Name + "  [" + choice.Category + "]\n" + choice.Description + "\nLv " + (_run.Build.GetLevel(choice.Id) + 1) + "/" + choice.MaxLevel;
            }

            // Reroll / banish charges (matches the design's REROLL·N / BANISH buttons).
            for (var i = 0; i < _upgradeBanishButtons.Count; i++)
            {
                var active = i < _run.DraftChoices.Count && _run.BanishesRemaining > 0;
                _upgradeBanishButtons[i].gameObject.SetActive(active);
            }
            _rerollButton.interactable = _run.RerollsRemaining > 0;
            var rerollLabel = _rerollButton.GetComponentInChildren<Text>();
            if (rerollLabel != null) rerollLabel.text = "REROLL · " + _run.RerollsRemaining;
        }

        private static Color ResolveUpgradeCategoryColor(NeonUpgradeCategory category)
        {
            switch (category)
            {
                case NeonUpgradeCategory.Weapon:
                    return new Color(1f, 0.5f, 0.4f);
                case NeonUpgradeCategory.Passive:
                    return new Color(0.45f, 0.8f, 1f);
                case NeonUpgradeCategory.Trail:
                    return new Color(0.7f, 0.55f, 1f);
                case NeonUpgradeCategory.Defense:
                    return new Color(0.45f, 1f, 0.65f);
                case NeonUpgradeCategory.Special:
                    return new Color(1f, 0.82f, 0.32f);
                default:
                    return Color.white;
            }
        }

        private void SelectUpgradeChoice(int index)
        {
            if (_run.Status != NeonRunStatus.LevelUpDraft || index >= _run.DraftChoices.Count)
            {
                return;
            }

            var choice = _run.DraftChoices[index];
            if (_gameplay.ApplyUpgradeChoice(_run, _catalog, choice))
            {
                _messageText.text = "Installed: " + choice.Name;
                UpdateUpgradeChoices(false);
            }
        }

        private void EnsureCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var cameraObject = new GameObject("Neon Mobile Camera");
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            _camera.orthographic = true;
            _camera.orthographicSize = ArenaHalfHeight;
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _camera.backgroundColor = new Color(0.015f, 0.02f, 0.06f);
            _camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void CreatePools()
        {
            CreateSpritePool("Enemies",    MaxEnemyViews,      _enemyViews,      1, null);
            CreateSpritePool("Projectiles",MaxProjectileViews, _projectileViews, 2, null);
            CreateSpritePool("XP",         MaxXpViews,         _xpViews,         3, NeonSpriteFactory.XpShard);
            CreateSpritePool("Chests",     MaxChestViews,      _chestViews,      3, NeonSpriteFactory.XpShard);
            CreateSpritePool("Orbit",      MaxOrbitViews,      _orbitViews,      4, NeonSpriteFactory.OrbitBlade);

            var trailRoot = new GameObject("Dash Trail Pool");
            for (var index = 0; index < MaxTrailViews; index++)
            {
                var trailObject = new GameObject("Trail " + index);
                trailObject.transform.SetParent(trailRoot.transform, false);
                var line = trailObject.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = new Color(0.36f, 0.95f, 1f, 0.75f);
                line.endColor = new Color(0.75f, 0.38f, 1f, 0.25f);
                line.startWidth = 0.16f;
                line.endWidth = 0.04f;
                line.positionCount = 2;
                trailObject.SetActive(false);
                _trailViews.Add(line);
            }

            var zapRoot = new GameObject("Tesla Zap Pool");
            for (var index = 0; index < MaxZapViews; index++)
            {
                var zapObject = new GameObject("Zap " + index);
                zapObject.transform.SetParent(zapRoot.transform, false);
                var line = zapObject.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = new Color(0.65f, 0.92f, 1f, 0.95f);
                line.endColor = new Color(1f, 1f, 1f, 0.8f);
                line.startWidth = 0.05f;
                line.endWidth = 0.03f;
                line.positionCount = 2;
                line.sortingOrder = 5;
                zapObject.SetActive(false);
                _zapViews.Add(line);
            }
        }

        private void CreateSpritePool(string name, int count, List<SpriteRenderer> target, int sortingOrder, Sprite? fixedSprite)
        {
            var root = new GameObject(name + " Pool");
            for (var index = 0; index < count; index++)
            {
                var item = new GameObject(name + " " + index);
                item.transform.SetParent(root.transform, false);
                var renderer = item.AddComponent<SpriteRenderer>();
                renderer.sprite = fixedSprite != null ? fixedSprite : _sprite;
                renderer.sortingOrder = sortingOrder;
                item.SetActive(false);
                target.Add(renderer);
            }
        }

        private void CreateParticlePool()
        {
            var root = new GameObject("Neon Particles");
            for (var index = 0; index < MaxParticles; index++)
            {
                var particleObject = new GameObject("Particle " + index);
                particleObject.transform.SetParent(root.transform, false);
                var renderer = particleObject.AddComponent<SpriteRenderer>();
                renderer.sprite = _sprite;
                renderer.sortingOrder = 6;
                particleObject.SetActive(false);
                _particles.Add(new NeonParticleView
                {
                    Transform = particleObject.transform,
                    Renderer = renderer
                });
            }
        }

        private void SpawnBurst(NeonVector2 position, Color color, int count, float speed, float size, float life)
        {
            var world = ToWorld(position);
            for (var spawned = 0; spawned < count; spawned++)
            {
                var particle = _particles[_particleCursor];
                _particleCursor = (_particleCursor + 1) % _particles.Count;

                var angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                var magnitude = speed * UnityEngine.Random.Range(0.45f, 1f);
                particle.Velocity = new Vector3(Mathf.Cos(angle) * magnitude, Mathf.Sin(angle) * magnitude, 0f);
                particle.MaxLife = life * UnityEngine.Random.Range(0.7f, 1f);
                particle.Life = particle.MaxLife;
                particle.BaseColor = color;
                particle.BaseSize = size * UnityEngine.Random.Range(0.7f, 1.2f);

                particle.Transform.position = world;
                particle.Transform.localScale = Vector3.one * particle.BaseSize;
                particle.Renderer.color = color;
                particle.Transform.gameObject.SetActive(true);
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            for (var index = 0; index < _particles.Count; index++)
            {
                var particle = _particles[index];
                if (particle.Life <= 0f)
                {
                    continue;
                }

                particle.Life -= deltaTime;
                if (particle.Life <= 0f)
                {
                    particle.Transform.gameObject.SetActive(false);
                    continue;
                }

                particle.Transform.position += particle.Velocity * deltaTime;
                particle.Velocity *= Mathf.Clamp01(1f - 3.5f * deltaTime);

                var t = particle.Life / particle.MaxLife;
                particle.Transform.localScale = Vector3.one * particle.BaseSize * (0.35f + 0.65f * t);
                var color = particle.BaseColor;
                color.a = particle.BaseColor.a * t;
                particle.Renderer.color = color;
            }
        }

        private void CapturePreTickEnemies()
        {
            _enemySnapshots.Clear();
            for (var index = 0; index < _run.Enemies.Count; index++)
            {
                var enemy = _run.Enemies[index];
                _enemySnapshots[enemy] = new EnemyDeathSnapshot
                {
                    Position = enemy.Position,
                    IsBoss = enemy.IsBoss,
                    IsMiniBoss = enemy.IsMiniBoss
                };
            }
        }

        private void SpawnDeathBursts()
        {
            _enemyAfterTick.Clear();
            for (var index = 0; index < _run.Enemies.Count; index++)
            {
                _enemyAfterTick.Add(_run.Enemies[index]);
            }

            var reduced = _profile.ReducedMotionEnabled;
            foreach (var pair in _enemySnapshots)
            {
                if (_enemyAfterTick.Contains(pair.Key))
                {
                    continue;
                }

                var snapshot = pair.Value;
                if (snapshot.IsBoss && !snapshot.IsMiniBoss)
                {
                    // Always show boss death even in reduced-motion mode (just fewer particles).
                    SpawnBurst(snapshot.Position, new Color(1f, 0.3f, 0.85f, 0.95f), reduced ? 8 : 22, 4.2f, 0.16f, 0.6f);
                }
                else if (snapshot.IsMiniBoss)
                {
                    SpawnBurst(snapshot.Position, new Color(1f, 0.65f, 0.2f, 0.95f), reduced ? 5 : 16, 3.6f, 0.13f, 0.5f);
                }
                else if (!reduced)
                {
                    SpawnBurst(snapshot.Position, new Color(1f, 0.4f, 0.45f, 0.9f), 7, 3f, 0.08f, 0.4f);
                }
            }
        }

        private void CreatePlayerView()
        {
            var rootObject = new GameObject("Player Plane");
            _playerRoot = rootObject.transform;
            _playerRoot.position = Vector3.zero;

            _playerBody      = CreatePlayerSprite("Body",       new Vector3(0f,     0f,     0f), new Vector3(0.22f, 0.34f, 1f), 4, NeonSpriteFactory.PlayerBody);
            _playerNose      = CreatePlayerSprite("Nose",       new Vector3(0f,     0.2f,   0f), new Vector3(0.12f, 0.16f, 1f), 5, NeonSpriteFactory.PlayerNose);
            _playerWingLeft  = CreatePlayerSprite("Wing Left",  new Vector3(-0.17f,-0.05f,  0f), new Vector3(0.12f, 0.2f,  1f), 4, NeonSpriteFactory.PlayerWing);
            _playerWingRight = CreatePlayerSprite("Wing Right", new Vector3( 0.17f,-0.05f,  0f), new Vector3(0.12f, 0.2f,  1f), 4, NeonSpriteFactory.PlayerWing);
            _playerWingLeft.transform.localRotation  = Quaternion.Euler(0f, 0f,  28f);
            _playerWingRight.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);

            rootObject.SetActive(false);
        }

        private SpriteRenderer CreatePlayerSprite(string name, Vector3 localPosition, Vector3 localScale, int sortingOrder, Sprite sprite)
        {
            var spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(_playerRoot, false);
            spriteObject.transform.localPosition = localPosition;
            spriteObject.transform.localScale    = localScale;
            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite       = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void CreateNeonBackground()
        {
            var root = new GameObject("Neon Background");
            var lineMaterial = new Material(Shader.Find("Sprites/Default"));

            for (var index = 0; index < GridVerticalLines; index++)
            {
                var x = Mathf.Lerp(-3.4f, 3.4f, index / (float)(GridVerticalLines - 1));
                CreateBackgroundLine(root.transform, lineMaterial, new Vector3(x, GridBottom, 3f), new Vector3(x, GridTop, 3f), new Color(0.2f, 0.85f, 1f, 0.1f));
            }

            var step = (GridTop - GridBottom) / (GridHorizontalLines - 1);
            for (var index = 0; index < GridHorizontalLines; index++)
            {
                var y = GridBottom + index * step;
                var line = CreateBackgroundLine(root.transform, lineMaterial, new Vector3(-3.4f, y, 3f), new Vector3(3.4f, y, 3f), new Color(0.6f, 0.3f, 1f, 0.13f));
                _gridLines.Add(line);
                _gridLineY.Add(y);
            }

            var starRoot = new GameObject("Stars");
            starRoot.transform.SetParent(root.transform, false);
            for (var index = 0; index < StarCount; index++)
            {
                var star = new GameObject("Star " + index);
                star.transform.SetParent(starRoot.transform, false);
                star.transform.position = new Vector3(UnityEngine.Random.Range(-3.2f, 3.2f), UnityEngine.Random.Range(GridBottom, GridTop), 4f);
                var size = UnityEngine.Random.Range(0.03f, 0.09f);
                star.transform.localScale = new Vector3(size, size, 1f);
                var renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = _sprite;
                renderer.sortingOrder = -9;
                var shade = UnityEngine.Random.Range(0.5f, 1f);
                renderer.color = new Color(0.6f * shade, 0.95f * shade, shade, UnityEngine.Random.Range(0.35f, 0.85f));
                _stars.Add(star.transform);
                _starSpeeds.Add(UnityEngine.Random.Range(0.5f, 2.1f));
            }
        }

        private static LineRenderer CreateBackgroundLine(Transform parent, Material material, Vector3 start, Vector3 end, Color color)
        {
            var lineObject = new GameObject("Grid Line");
            lineObject.transform.SetParent(parent, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.material = material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.startColor = color;
            line.endColor = color;
            line.sortingOrder = -8;
            return line;
        }

        private void UpdateNeonBackground(float deltaTime)
        {
            var span = GridTop - GridBottom;
            for (var index = 0; index < _gridLines.Count; index++)
            {
                var y = _gridLineY[index] - GridScrollSpeed * deltaTime;
                if (y < GridBottom)
                {
                    y += span;
                }

                _gridLineY[index] = y;
                var line = _gridLines[index];
                line.SetPosition(0, new Vector3(-3.4f, y, 3f));
                line.SetPosition(1, new Vector3(3.4f, y, 3f));
            }

            for (var index = 0; index < _stars.Count; index++)
            {
                var star = _stars[index];
                var position = star.position;
                position.y -= _starSpeeds[index] * deltaTime;
                if (position.y < GridBottom)
                {
                    position.y = GridTop;
                    position.x = UnityEngine.Random.Range(-3.2f, 3.2f);
                }

                star.position = position;
            }
        }

        private void CreateHud()
        {
            var canvasObject = new GameObject("Mobile HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;

            // Safe-area root: edge-anchored run HUD lives here so notches / punch-holes /
            // gesture bars never clip the timer, bars, or touch buttons. Full-screen modal
            // panels stay on the raw canvas (they are centered and notch-safe by design).
            var safeAreaObject = new GameObject("Safe Area", typeof(RectTransform), typeof(NeonSafeArea));
            safeAreaObject.transform.SetParent(canvasObject.transform, false);
            var safeRect = safeAreaObject.GetComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.offsetMin = Vector2.zero;
            safeRect.offsetMax = Vector2.zero;
            var safeArea = safeAreaObject.transform;

            // Top tactical row: timer centered, wave/level beneath, kills top-right.
            _timerText = CreateDisplayText(safeArea, "Timer", new Vector2(0f, -14f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 44, NeonUITheme.TextCyan);
            _timerText.rectTransform.sizeDelta = new Vector2(-220f, 60f);
            AddTextGlow(_timerText, NeonUITheme.Cyan, 3f);
            _waveText = CreateText(safeArea, "Wave", new Vector2(0f, -74f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 20, NeonUITheme.TextMute);
            _killsText = CreateText(safeArea, "Kills", new Vector2(-22f, -22f), new Vector2(1f, 1f), new Vector2(1f, 1f), TextAnchor.UpperRight, 24, NeonUITheme.Teal);
            _killsText.rectTransform.pivot = new Vector2(1f, 1f);
            _killsText.rectTransform.sizeDelta = new Vector2(260f, 70f);

            _hpBarFill = CreateBar(safeArea, "HP Bar", -116f, 26f, new Color(0.3f, 1f, 0.6f));
            _xpBarFill = CreateBar(safeArea, "XP Bar", -148f, 14f, new Color(0.23f, 1f, 0.78f));
            _messageText = CreateText(safeArea, "Message", new Vector2(0f, -188f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 30, new Color(1f, 0.82f, 0.28f));
            _statusText = CreateText(canvasObject.transform, "Status", new Vector2(0f, 0f), new Vector2(0f, 0.48f), new Vector2(1f, 0.48f), TextAnchor.MiddleCenter, 42, Color.white);
            _dashButton = CreateRoundButton(safeArea, "Dash", NeonUITheme.Cyan, "»", "DASH", new Vector2(-34f, 44f), 150f, TryDash, out _dashRing);
            CreateSpecialButton(safeArea);
            CreatePauseButton(safeArea);
            CreateBuffChipPool(safeArea);
            CreateBossBar(safeArea);
            CreateUpgradePanel(canvasObject.transform);
            CreateGaragePanel(canvasObject.transform);
            CreateResultsPanel(canvasObject.transform);
            CreateMainMenuPanel(canvasObject.transform);
            CreateSettingsPanel(canvasObject.transform);
            CreatePauseMenuPanel(canvasObject.transform);
            CreateMissionsPanel(canvasObject.transform);
            CreateAchievementsPanel(canvasObject.transform);
        }

        private void CreateSpecialButton(Transform parent)
        {
            // Special "NOVA" — round magenta button; the radial ring is the charge meter.
            _specialButton = CreateRoundButton(parent, "Special", NeonUITheme.Magenta, "NOVA", string.Empty, new Vector2(-34f, 218f), 150f, ActivateSpecial, out _specialRing);
            _specialRing.fillAmount = 0f;
        }

        private void ActivateSpecial()
        {
            if (_run != null && _gameplay.TryActivateSpecial(_run))
            {
                _audio.PlaySpecial();
                SpawnBurst(_run.Player.Position, new Color(0.4f, 0.9f, 1f, 0.95f), 28, 5f, 0.18f, 0.6f);
                TriggerScreenShake(0.12f, 0.35f);
                TriggerHitStop(0.08f);
                if (_profile.VibrationEnabled) Handheld.Vibrate();
            }
        }

        private void CreatePauseButton(Transform parent)
        {
            // Small round ghost button, top-left of the tactical HUD.
            var go = new GameObject("Pause", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(64f, 64f);

            var disc = go.GetComponent<Image>();
            disc.sprite = NeonSpriteFactory.UiDisc;
            disc.color = NeonUITheme.Mix(NeonUITheme.TextDim, 0.12f, NeonUITheme.Bg1);

            _pauseButton = go.GetComponent<Button>();
            _pauseButton.targetGraphic = disc;
            _pauseButton.onClick.AddListener(ShowPauseMenu);

            AddRingChild(go.transform, "Border", NeonUITheme.Alpha(NeonUITheme.TextDim, 0.5f), false, out _);

            _pauseLabel = CreateText(go.transform, "Pause Glyph", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 30, NeonUITheme.TextDim, NeonUITheme.UiBold);
            _pauseLabel.rectTransform.offsetMin = Vector2.zero;
            _pauseLabel.rectTransform.offsetMax = Vector2.zero;
            _pauseLabel.raycastTarget = false;
            _pauseLabel.text = "II";
        }

        private void CreateBuffChipPool(Transform parent)
        {
            // Vertical stack of active-upgrade chips, top-left under the pause button.
            for (var i = 0; i < 5; i++)
            {
                var go = new GameObject("Buff " + i, typeof(RectTransform), typeof(NeonPolyGraphic));
                go.transform.SetParent(parent, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(22f, -100f - i * 60f);
                rect.sizeDelta = new Vector2(50f, 50f);
                var bg = go.GetComponent<NeonPolyGraphic>();
                bg.BorderThickness = 1.5f;
                bg.raycastTarget = false;

                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(go.transform, false);
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(28f, 28f);
                var icon = iconObj.GetComponent<Image>();
                icon.raycastTarget = false;

                go.SetActive(false);
                _buffChips.Add(go);
                _buffChipBgs.Add(bg);
                _buffChipIcons.Add(icon);
            }
        }

        private void UpdateBuffChips()
        {
            var levels = _run.Build.UpgradeLevels;
            if (levels.Count == _lastBuffSignature)
            {
                return; // only rebuild when the set of owned upgrades changes
            }
            _lastBuffSignature = levels.Count;

            if (_upgradeCatById == null)
            {
                _upgradeCatById = new Dictionary<string, NeonUpgradeCategory>();
                foreach (var def in _catalog.Upgrades) _upgradeCatById[def.Id] = def.Category;
            }

            var slot = 0;
            foreach (var kv in levels)
            {
                if (slot >= _buffChips.Count) break;
                if (!_upgradeCatById.TryGetValue(kv.Key, out var category)) continue;
                var color = ResolveUpgradeCategoryColor(category);
                _buffChipBgs[slot].color = NeonUITheme.Mix(color, 0.16f, NeonUITheme.Bg1);
                _buffChipBgs[slot].BorderColor = color;
                _buffChipBgs[slot].Refresh();
                _buffChipIcons[slot].sprite = NeonSpriteFactory.GetUpgradeIcon(category);
                _buffChipIcons[slot].color = color;
                _buffChips[slot].SetActive(true);
                slot++;
            }
            for (; slot < _buffChips.Count; slot++) _buffChips[slot].SetActive(false);
        }

        private void HideBuffChips()
        {
            _lastBuffSignature = -1;
            foreach (var chip in _buffChips) chip.SetActive(false);
        }

        private Image CreateBar(Transform parent, string name, float topOffset, float height, Color fillColor)
        {
            var background = new GameObject(name, typeof(RectTransform), typeof(Image));
            background.transform.SetParent(parent, false);
            var rect = background.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, topOffset);
            rect.sizeDelta = new Vector2(-64f, height);
            background.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.85f);

            var fillObject = new GameObject(name + " Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(background.transform, false);
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            fillRect.pivot = new Vector2(0f, 0.5f);

            var fill = fillObject.GetComponent<Image>();
            fill.sprite = _sprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            fill.color = fillColor;
            return fill;
        }

        private void CreateBossBar(Transform parent)
        {
            _bossBarRoot = new GameObject("Boss Bar", typeof(RectTransform), typeof(Image));
            _bossBarRoot.transform.SetParent(parent, false);
            var rootRect = _bossBarRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -285f);
            rootRect.sizeDelta = new Vector2(760f, 60f);
            _bossBarRoot.GetComponent<Image>().color = NeonUITheme.Alpha(NeonUITheme.Mix(NeonUITheme.Magenta, 0.10f, NeonUITheme.Bg1), 0.92f);
            StylePanelCut(_bossBarRoot, NeonUITheme.Magenta, 2f);

            var fillObject = new GameObject("Boss Bar Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(_bossBarRoot.transform, false);
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            _bossBarFill = fillObject.GetComponent<Image>();
            _bossBarFill.sprite = _sprite;
            _bossBarFill.type = Image.Type.Filled;
            _bossBarFill.fillMethod = Image.FillMethod.Horizontal;
            _bossBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _bossBarFill.fillAmount = 1f;
            _bossBarFill.color = new Color(1f, 0.2f, 0.55f, 0.95f);

            _bossBarText = CreateText(_bossBarRoot.transform, "Boss Bar Label", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 24, Color.white);
            _bossBarText.rectTransform.offsetMin = Vector2.zero;
            _bossBarText.rectTransform.offsetMax = Vector2.zero;

            _bossBarRoot.SetActive(false);
        }

        private void ShowPauseMenu()
        {
            if (_run == null || _run.Status != NeonRunStatus.Running)
            {
                return;
            }

            if (_pauseMenuPanel.activeSelf)
            {
                ResumePausedRun();
                return;
            }

            _paused = true;
            _pauseLabel.text = "II";
            _pauseStatValues[0].text = FormatTime(_run.ElapsedSeconds);
            _pauseStatValues[1].text = _run.EnemiesKilled.ToString();
            _pauseStatValues[2].text = _run.Player.Level.ToString();
            _pauseStatValues[3].text = _run.Player.CoinsCollected.ToString();
            _pauseStatValues[4].text = (_run.BossesKilled + _run.MiniBossesKilled).ToString();
            _pauseStatValues[5].text = Mathf.CeilToInt(_run.Player.Stats.CurrentHP) + "/" + Mathf.CeilToInt(_run.Player.Stats.MaxHP);
            _pauseMenuPanel.SetActive(true);
        }

        private void ResumePausedRun()
        {
            _paused = false;
            _pauseLabel.text = "II";
            _pauseMenuPanel.SetActive(false);
        }

        private void RestartRun()
        {
            _pauseMenuPanel.SetActive(false);
            StartRun();
        }

        private void ReturnToGarage()
        {
            _pauseMenuPanel.SetActive(false);
            ShowGarage();
        }

        private NeonRunEnemyState FindActiveBoss()
        {
            NeonRunEnemyState best = null!;
            for (var index = 0; index < _run.Enemies.Count; index++)
            {
                var enemy = _run.Enemies[index];
                if (!enemy.IsBoss)
                {
                    continue;
                }

                if (best == null || enemy.MaxHP > best.MaxHP)
                {
                    best = enemy;
                }
            }

            return best;
        }

        private void UpdateBossBar()
        {
            var boss = FindActiveBoss();
            if (boss == null || _run.Status != NeonRunStatus.Running)
            {
                _bossBarRoot.SetActive(false);
                return;
            }

            _bossBarRoot.SetActive(true);
            var fraction = boss.MaxHP > 0f ? Mathf.Clamp01(boss.HP / boss.MaxHP) : 0f;
            _bossBarFill.fillAmount = fraction;
            _bossBarFill.color = boss.IsMiniBoss ? new Color(1f, 0.62f, 0.18f, 0.95f) : new Color(1f, 0.2f, 0.55f, 0.95f);
            _bossBarText.text = (boss.IsMiniBoss ? "MINI-BOSS  " : "BOSS  ") + Mathf.CeilToInt(boss.HP) + " / " + Mathf.CeilToInt(boss.MaxHP);
        }

        private void ResetAudioTrackers()
        {
            _prevStatus = NeonRunStatus.Running;
            _prevEnemiesKilled = 0;
            _prevPlayerProjectiles = 0;
            _prevBossCount = 0;
            _prevHP = _run.Player.Stats.CurrentHP;
            _prevXP = _run.Player.XP;
            _prevWarning = string.Empty;
            _prevEvolutionCount = 0;
            _xpSoundCooldown = 0f;
            _damageSoundCooldown = 0f;
        }

        private void UpdateAudio(float deltaTime)
        {
            // Status transitions fire regardless of pause state.
            if (_run.Status != _prevStatus)
            {
                if (_run.Status == NeonRunStatus.GameOver)
                {
                    _audio.PlayGameOver();
                    _audio.StopMusic();
                }
                else if (_run.Status == NeonRunStatus.Victory)
                {
                    _audio.PlayVictory();
                    _audio.StopMusic();
                }
                else if (_run.Status == NeonRunStatus.LevelUpDraft)
                {
                    _audio.PlayLevelUp();
                }

                _prevStatus = _run.Status;
            }

            if (_paused || _run.Status != NeonRunStatus.Running)
            {
                return;
            }

            _xpSoundCooldown = Mathf.Max(0f, _xpSoundCooldown - deltaTime);
            _damageSoundCooldown = Mathf.Max(0f, _damageSoundCooldown - deltaTime);

            var player = _run.Player;

            var playerProjectiles = 0;
            for (var index = 0; index < _run.Projectiles.Count; index++)
            {
                if (_run.Projectiles[index].FromPlayer)
                {
                    playerProjectiles++;
                }
            }

            if (playerProjectiles > _prevPlayerProjectiles)
            {
                _audio.PlayShoot();
            }

            _prevPlayerProjectiles = playerProjectiles;

            if (_run.EnemiesKilled > _prevEnemiesKilled)
            {
                _audio.PlayEnemyDeath();
                _prevEnemiesKilled = _run.EnemiesKilled;
            }

            if (player.Stats.CurrentHP < _prevHP - 0.01f && _damageSoundCooldown <= 0f)
            {
                _audio.PlayPlayerDamage();
                SpawnBurst(player.Position, new Color(1f, 0.3f, 0.3f, 0.95f), 10, 3.2f, 0.1f, 0.4f);
                TriggerScreenShake(0.08f, 0.25f);
                _damageSoundCooldown = 0.25f;
            }

            _prevHP = player.Stats.CurrentHP;

            if (player.XP > _prevXP + 0.01f && _xpSoundCooldown <= 0f)
            {
                _audio.PlayXp();
                _xpSoundCooldown = 0.09f;
            }

            _prevXP = player.XP;

            var bossCount = 0;
            for (var index = 0; index < _run.Enemies.Count; index++)
            {
                if (_run.Enemies[index].IsBoss)
                {
                    bossCount++;
                }
            }

            if (bossCount > _prevBossCount)
            {
                _audio.PlayBossSpawn();
                TriggerScreenShake(0.18f, 0.5f);
                TriggerHitStop(0.06f);
            }

            _prevBossCount = bossCount;

            if (!string.IsNullOrWhiteSpace(_run.LastWarning) && _run.LastWarning != _prevWarning)
            {
                _audio.PlayWarning();
                _prevWarning = _run.LastWarning;
            }

            // Evolution unlocked (draft pick or boss evolution chest): celebrate it.
            if (_run.Build.EvolvedWeapons.Count > _prevEvolutionCount)
            {
                _audio.PlayLevelUp();
                _messageText.text = "WEAPON EVOLVED!";
                SpawnBurst(player.Position, new Color(1f, 0.82f, 0.2f, 0.95f), 18, 4f, 0.12f, 0.6f);
                _prevEvolutionCount = _run.Build.EvolvedWeapons.Count;
            }

            // Use "final" music mode when the final boss (Eclipse Core — highest HP boss) is alive.
            var musicMode = "normal";
            if (bossCount > 0)
            {
                var activeBoss = FindActiveBoss();
                musicMode = activeBoss != null && !activeBoss.IsMiniBoss && activeBoss.MaxHP >= 10000f ? "final" : "boss";
            }
            _audio.SetMusic(musicMode);
        }

        private void CreateGaragePanel(Transform parent)
        {
            _garagePanel = new GameObject("Garage", typeof(RectTransform), typeof(Image));
            _garagePanel.transform.SetParent(parent, false);
            var rect = _garagePanel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = _garagePanel.GetComponent<Image>();
            image.color = new Color(0.01f, 0.025f, 0.055f, 0.98f);

            _garageTitleText = CreateDisplayText(_garagePanel.transform, "Garage Title", new Vector2(0f, -40f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 36, NeonUITheme.TextCyan);
            _garageTitleText.rectTransform.sizeDelta = new Vector2(-80f, 120f);
            AddTextGlow(_garageTitleText, NeonUITheme.Cyan);

            _garageStatsText = CreateText(_garagePanel.transform, "Garage Stats", new Vector2(0f, -170f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 26, Color.white);
            _garageStatsText.rectTransform.sizeDelta = new Vector2(-100f, 80f);

            CreateSlotArrangement(_garagePanel.transform);

            // Scrollable inventory grid occupies the middle band of the screen.
            _inventoryContent = CreateInventoryScroll(_garagePanel.transform, new Vector2(0.04f, 0.345f), new Vector2(0.96f, 0.74f));

            _garageDetailText = CreateText(_garagePanel.transform, "Garage Detail", new Vector2(0f, 0f), new Vector2(0.04f, 0.255f), new Vector2(0.96f, 0.34f), TextAnchor.MiddleCenter, 26, new Color(0.85f, 0.95f, 1f));
            _garageDetailText.rectTransform.offsetMin = Vector2.zero;
            _garageDetailText.rectTransform.offsetMax = Vector2.zero;

            // Action row: Equip / Unequip / Upgrade / Merge.
            _equipButton = CreateActionButton(_garagePanel.transform, "Equip", 0.045f, 0.255f, 0.245f, EquipSelected);
            _unequipButton = CreateActionButton(_garagePanel.transform, "Unequip", 0.265f, 0.255f, 0.485f, UnequipSelected);
            _upgradeButton = CreateActionButton(_garagePanel.transform, "Upgrade", 0.515f, 0.255f, 0.735f, UpgradeSelected);
            _mergeButton = CreateActionButton(_garagePanel.transform, "Merge x3", 0.755f, 0.255f, 0.955f, MergeSelected);

            var settingsBtn = CreateButton(_garagePanel.transform, "Settings", new Vector2(0f, 240f), () => ShowSettings(false));
            settingsBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 90f);
            GhostButton(settingsBtn);
            settingsBtn.GetComponentInChildren<Text>().fontSize = 26;

            var missionsBtn = CreateButton(_garagePanel.transform, "Missions", new Vector2(290f, 240f), ShowMissions);
            missionsBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280f, 90f);
            AccentButton(missionsBtn, NeonUITheme.Uncommon);
            missionsBtn.GetComponentInChildren<Text>().fontSize = 26;

            var achievementsBtn = CreateButton(_garagePanel.transform, "Achievements", new Vector2(290f, 360f), ShowAchievements);
            achievementsBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280f, 90f);
            AccentButton(achievementsBtn, NeonUITheme.Legendary);
            achievementsBtn.GetComponentInChildren<Text>().fontSize = 22;

            var backToMenuBtn = CreateButton(_garagePanel.transform, "< Menu", new Vector2(-290f, 240f), ShowMainMenu);
            backToMenuBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 90f);
            GhostButton(backToMenuBtn);
            backToMenuBtn.GetComponentInChildren<Text>().fontSize = 24;

            var startRunButton = CreateButton(_garagePanel.transform, "Start Run", new Vector2(0f, 120f), StartRun);
            startRunButton.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 120f);
            PrimaryButton(startRunButton);

            // Sector difficulty selector flanks the Start Run button.
            var sectorDown = CreateButton(_garagePanel.transform, "◀", new Vector2(-360f, 120f), () => CycleSector(-1));
            sectorDown.GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 120f);
            GhostButton(sectorDown);

            var sectorUp = CreateButton(_garagePanel.transform, "▶", new Vector2(360f, 120f), () => CycleSector(1));
            sectorUp.GetComponent<RectTransform>().sizeDelta = new Vector2(110f, 120f);
            GhostButton(sectorUp);

            _sectorLabel = CreateText(_garagePanel.transform, "Sector Label", new Vector2(0f, 38f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), TextAnchor.MiddleCenter, 24, NeonUITheme.TextCyan);
            _sectorLabel.rectTransform.sizeDelta = new Vector2(900f, 44f);

            _garagePanel.SetActive(false);
        }

        private void CreateSlotArrangement(Transform parent)
        {
            var panel = new GameObject("Slot Arrangement", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.04f, 0.74f);
            panelRect.anchorMax = new Vector2(0.96f, 0.895f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = NeonUITheme.Alpha(NeonUITheme.Bg1, 0.6f);
            StylePanelCut(panel, NeonUITheme.Line2);

            NeonEquipmentSlot[] row0 = { NeonEquipmentSlot.Wings, NeonEquipmentSlot.Weapon, NeonEquipmentSlot.Engine };
            NeonEquipmentSlot[] row1 = { NeonEquipmentSlot.Hull, NeonEquipmentSlot.Core, NeonEquipmentSlot.Radar };

            CreateSlotRow(panel.transform, row0, 0);
            CreateSlotRow(panel.transform, row1, 1);
        }

        private void CreateSlotRow(Transform parent, NeonEquipmentSlot[] slots, int rowIndex)
        {
            for (var col = 0; col < slots.Length; col++)
            {
                var slot = slots[col];
                var capturedSlot = slot;
                var colFrac = col / 3f;
                var rowFrac = rowIndex == 0 ? 0.5f : 0f;

                var btnObject = new GameObject(slot + " Slot Btn", typeof(RectTransform), typeof(Image), typeof(Button));
                btnObject.transform.SetParent(parent, false);
                var rect = btnObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(colFrac + 0.01f, rowFrac + 0.03f);
                rect.anchorMax = new Vector2(colFrac + 0.325f, rowFrac + 0.47f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var image = btnObject.GetComponent<Image>();
                image.sprite = NeonSpriteFactory.UiCutPanel;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 1f;
                image.color = NeonUITheme.Mix(NeonUITheme.Cyan, 0.10f, NeonUITheme.Bg1);
                btnObject.GetComponent<Button>().onClick.AddListener(() => FilterInventoryBySlot(capturedSlot));

                // hexagonal slot-type icon (top), name label (bottom)
                var iconObj = new GameObject(slot + " Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(btnObject.transform, false);
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 1f);
                iconRect.anchorMax = new Vector2(0.5f, 1f);
                iconRect.pivot = new Vector2(0.5f, 1f);
                iconRect.anchoredPosition = new Vector2(0f, -10f);
                iconRect.sizeDelta = new Vector2(40f, 40f);
                var iconImg = iconObj.GetComponent<Image>();
                iconImg.sprite = NeonSpriteFactory.GetIcon(slot);
                iconImg.raycastTarget = false;
                _slotIcons[slot] = iconImg;

                var label = CreateText(btnObject.transform, slot + " Lbl", new Vector2(0f, 8f), new Vector2(0f, 0f), new Vector2(1f, 0f), TextAnchor.LowerCenter, 18, Color.white);
                label.rectTransform.sizeDelta = new Vector2(-6f, 56f);
                label.raycastTarget = false;

                // rarity-colored neon border overlay
                var borderObj = new GameObject("Slot Border", typeof(RectTransform), typeof(NeonCutRect));
                borderObj.transform.SetParent(btnObject.transform, false);
                var borderRect = borderObj.GetComponent<RectTransform>();
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = Vector2.zero;
                borderRect.offsetMax = Vector2.zero;
                var borderCut = borderObj.GetComponent<NeonCutRect>();
                borderCut.CutSize = NeonSpriteFactory.CutPanelCorner;
                borderCut.CutTL = borderCut.CutTR = borderCut.CutBR = borderCut.CutBL = true;
                borderCut.color = new Color(0f, 0f, 0f, 0f);
                borderCut.BorderColor = NeonUITheme.Line2;
                borderCut.BorderThickness = 1.5f;
                borderCut.raycastTarget = false;
                _slotBorders[slot] = borderCut;

                _slotFilterButtons[slot] = btnObject.GetComponent<Button>();
            }
        }

        private void FilterInventoryBySlot(NeonEquipmentSlot slot)
        {
            if (_selectedSlotFilter == slot)
            {
                _selectedSlotFilter = null; // toggle off → show all
            }
            else
            {
                _selectedSlotFilter = slot;
            }

            RebuildInventoryCards();
            UpdateSlotButtons();
            UpdateGarageActions();
        }

        private void UpdateSlotButtons()
        {
            foreach (var pair in _slotFilterButtons)
            {
                var slot = pair.Key;
                var button = pair.Value;
                var equippedId = GetEquippedItemId(slot);
                var ownedItem = FindOwnedItem(equippedId);
                var def = FindEquipmentDef(equippedId);
                var isFiltered = _selectedSlotFilter == slot;

                var rarityColor = ownedItem != null ? ResolveRarityColor(ownedItem.Rarity) : NeonUITheme.Line2;
                var bg = isFiltered
                    ? NeonUITheme.Mix(rarityColor, 0.30f, NeonUITheme.Bg1)
                    : NeonUITheme.Mix(rarityColor, 0.10f, NeonUITheme.Bg1);
                button.GetComponent<Image>().color = bg;

                if (_slotBorders.TryGetValue(slot, out var border))
                {
                    border.BorderColor = isFiltered ? rarityColor : NeonUITheme.Mix(rarityColor, 0.7f, NeonUITheme.Line2);
                    border.BorderThickness = isFiltered ? 2.5f : 1.5f;
                    border.Refresh();
                }
                if (_slotIcons.TryGetValue(slot, out var icon))
                {
                    icon.color = ownedItem != null ? rarityColor : NeonUITheme.TextMute;
                }

                var label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    var itemName = def != null ? TruncateName(def.Name, 10) : "Empty";
                    label.text = slot.ToString() + "\n" + itemName + (isFiltered ? " ▼" : "");
                    label.color = ownedItem != null ? rarityColor : NeonUITheme.TextMute;
                }
            }
        }

        private static string TruncateName(string name, int maxLen)
        {
            return name.Length <= maxLen ? name : name.Substring(0, maxLen - 1) + "…";
        }

        private RectTransform CreateInventoryScroll(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var scrollObject = new GameObject("Inventory Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(parent, false);
            var scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = anchorMin;
            scrollRect.anchorMax = anchorMax;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scrollObject.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.1f, 0.85f);

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            var viewportRect = viewportObject.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(10f, 10f);
            viewportRect.offsetMax = new Vector2(-10f, -10f);
            viewportObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);
            var contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var grid = contentObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(470f, 150f);
            grid.spacing = new Vector2(16f, 16f);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            var fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            return contentRect;
        }

        private Button CreateActionButton(Transform parent, string label, float anchorMinX, float anchorMinY, float anchorMaxX, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label + " Action", typeof(RectTransform), typeof(NeonCutRect), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorMinX, anchorMinY - 0.05f);
            rect.anchorMax = new Vector2(anchorMaxX, anchorMinY);
            rect.offsetMin = new Vector2(6f, 6f);
            rect.offsetMax = new Vector2(-6f, -6f);

            var cut = buttonObject.GetComponent<NeonCutRect>();
            cut.CutSize = 10f;
            cut.BorderThickness = 1.5f;
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = cut;
            button.onClick.AddListener(action);
            GhostButton(button);

            var text = CreateText(buttonObject.transform, label + " Label", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 26, NeonUITheme.TextDim);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.text = label;
            return button;
        }

        private void CreateResultsPanel(Transform parent)
        {
            _resultsPanel = new GameObject("Run Results", typeof(RectTransform), typeof(Image));
            _resultsPanel.transform.SetParent(parent, false);
            var rect = _resultsPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(940f, 760f);

            var image = _resultsPanel.GetComponent<Image>();
            image.color = new Color(0.015f, 0.035f, 0.07f, 0.97f);
            StylePanelCut(_resultsPanel, NeonUITheme.Mix(NeonUITheme.Cyan, 0.5f, NeonUITheme.Line2), 2f);

            _resultsTitleText = CreateDisplayText(_resultsPanel.transform, "Results Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 44, NeonUITheme.TextRed);
            AddTextGlow(_resultsTitleText, NeonUITheme.Red, 3f);

            // Hero TIME SURVIVED card.
            var heroVal = CreateStatCell(_resultsPanel.transform, new Vector2(0f, 220f), new Vector2(640f, 130f), "TIME SURVIVED", NeonUITheme.TextCyan);
            heroVal.fontSize = 56;
            AddTextGlow(heroVal, NeonUITheme.Cyan, 2.5f);
            _resultsTimeText = heroVal;

            // 2×3 stat grid.
            string[] labels = { "KILLS", "BOSSES", "COINS +", "TOTAL", "BEST", "RUNS" };
            Color[] accents = { NeonUITheme.Teal, NeonUITheme.Magenta, NeonUITheme.Legendary, NeonUITheme.Legendary, NeonUITheme.Cyan, NeonUITheme.Text };
            for (var i = 0; i < 6; i++)
            {
                var col = i % 2;
                var row = i / 2;
                var x = col == 0 ? -218f : 218f;
                var y = 100f - row * 104f;
                _resultsStatValues[i] = CreateStatCell(_resultsPanel.transform, new Vector2(x, y), new Vector2(420f, 92f), labels[i], accents[i]);
            }

            // Items-found list (salvage).
            _resultsStatsText = CreateText(_resultsPanel.transform, "Results Items", new Vector2(0f, 198f), new Vector2(0f, 0f), new Vector2(1f, 0f), TextAnchor.LowerCenter, 22, NeonUITheme.Legendary);
            _resultsStatsText.rectTransform.sizeDelta = new Vector2(-120f, 44f);

            var garageButton = CreateButton(_resultsPanel.transform, "Garage", new Vector2(0f, 70f), ShowGarage);
            garageButton.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 112f);
            PrimaryButton(garageButton);

            _resultsPanel.SetActive(false);
        }

        private void CreateUpgradePanel(Transform parent)
        {
            _upgradePanel = new GameObject("Upgrade Choices", typeof(RectTransform), typeof(Image));
            _upgradePanel.transform.SetParent(parent, false);
            var rect = _upgradePanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -90f);
            rect.sizeDelta = new Vector2(960f, 650f);

            var image = _upgradePanel.GetComponent<Image>();
            image.color = new Color(0.015f, 0.035f, 0.07f, 0.96f);
            StylePanelCut(_upgradePanel, NeonUITheme.Mix(NeonUITheme.Cyan, 0.5f, NeonUITheme.Line2), 2f);

            var title = CreateDisplayText(_upgradePanel.transform, "Upgrade Title", new Vector2(0f, -38f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 32, NeonUITheme.TextCyan);
            title.text = "LEVEL UP";
            AddTextGlow(title, NeonUITheme.Cyan);

            for (var index = 0; index < 3; index++)
            {
                var capturedIndex = index;
                var button = CreateButton(_upgradePanel.transform, "Upgrade " + (index + 1), new Vector2(0f, 430f - index * 170f), () => SelectUpgradeChoice(capturedIndex));
                var buttonRect = button.GetComponent<RectTransform>();
                buttonRect.sizeDelta = new Vector2(860f, 140f);


                var label = button.GetComponentInChildren<Text>();
                label.fontSize = 26;
                label.alignment = TextAnchor.MiddleLeft;
                label.rectTransform.offsetMin = new Vector2(120f, 8f);
                label.rectTransform.offsetMax = new Vector2(-12f, -8f);

                // Hex icon-medallion on the left (color set per choice in UpdateUpgradeChoices)
                var medallionObj = new GameObject("Upgrade Medallion", typeof(RectTransform), typeof(NeonPolyGraphic));
                medallionObj.transform.SetParent(button.transform, false);
                var medRect = medallionObj.GetComponent<RectTransform>();
                medRect.anchorMin = new Vector2(0f, 0.5f);
                medRect.anchorMax = new Vector2(0f, 0.5f);
                medRect.anchoredPosition = new Vector2(64f, 0f);
                medRect.sizeDelta = new Vector2(84f, 84f);
                var medallion = medallionObj.GetComponent<NeonPolyGraphic>();
                medallion.BorderThickness = 2f;
                medallion.raycastTarget = false;
                _upgradeMedallions.Add(medallion);

                var iconObj = new GameObject("Upgrade Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(button.transform, false);
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(64f, 0f);
                iconRect.sizeDelta = new Vector2(40f, 40f);
                var iconImage = iconObj.GetComponent<Image>();
                iconImage.raycastTarget = false;
                _upgradeButtonIcons.Add(iconImage);

                // Per-card banish button (top-right ✕).
                var banishBtn = CreateButton(button.transform, "Banish " + (index + 1), Vector2.zero, () => BanishUpgradeChoice(capturedIndex));
                var banishRect = banishBtn.GetComponent<RectTransform>();
                banishRect.anchorMin = new Vector2(1f, 1f);
                banishRect.anchorMax = new Vector2(1f, 1f);
                banishRect.pivot = new Vector2(1f, 1f);
                banishRect.anchoredPosition = new Vector2(-8f, -8f);
                banishRect.sizeDelta = new Vector2(56f, 56f);
                DangerButton(banishBtn);
                var banishLabel = banishBtn.GetComponentInChildren<Text>();
                banishLabel.text = "✕";
                banishLabel.fontSize = 30;
                _upgradeBanishButtons.Add(banishBtn);

                _upgradeButtons.Add(button);
            }

            // Reroll button at the bottom of the panel.
            _rerollButton = CreateButton(_upgradePanel.transform, "Reroll", new Vector2(0f, -270f), RerollDraftChoices);
            _rerollButton.GetComponent<RectTransform>().sizeDelta = new Vector2(420f, 96f);
            GhostButton(_rerollButton);

            _upgradePanel.SetActive(false);
        }

        private void RerollDraftChoices()
        {
            if (_run != null && _gameplay.RerollDraft(_run, _catalog))
            {
                _audio.PlayLevelUp();
                UpdateUpgradeChoices(true);
            }
        }

        private void BanishUpgradeChoice(int index)
        {
            if (_run == null || index >= _run.DraftChoices.Count) return;
            if (_gameplay.BanishUpgrade(_run, _catalog, _run.DraftChoices[index]))
            {
                _audio.PlayLevelUp();
                UpdateUpgradeChoices(true);
            }
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment, int fontSize, Color color, Font? font = null)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(-64f, 140f);

            var text = textObject.GetComponent<Text>();
            text.font = font != null ? font : NeonUITheme.Ui;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        /// <summary>Orbitron display title — used for logo/headline text per the design.</summary>
        private static Text CreateDisplayText(Transform parent, string name, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment, int fontSize, Color color)
        {
            return CreateText(parent, name, anchoredPosition, anchorMin, anchorMax, alignment, fontSize, color, NeonUITheme.Display);
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(NeonCutRect), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(340f, 112f);

            var cut = buttonObject.GetComponent<NeonCutRect>();
            cut.CutSize = 12f;
            cut.BorderThickness = 2f;

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = cut;
            button.onClick.AddListener(action);
            GhostButton(button); // default look; callers re-style as needed

            var text = CreateText(buttonObject.transform, label + " Label", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 34, NeonUITheme.Text);
            text.rectTransform.sizeDelta = Vector2.zero;
            text.text = label;
            return button;
        }

        /// <summary>Small cut-corner neon chip (resource counter / tag). Returns the label Text for updates.</summary>
        private Text CreateChip(Transform parent, string name, string text, Vector2 anchoredPosition, Vector2 anchor, Color accent, float width = 170f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(NeonCutRect));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(width, 50f);
            var cut = go.GetComponent<NeonCutRect>();
            cut.CutSize = 6f;
            cut.BorderThickness = 1.5f;
            cut.color = NeonUITheme.Mix(accent, 0.12f, NeonUITheme.Bg2);
            cut.BorderColor = NeonUITheme.Mix(accent, 0.55f, NeonUITheme.Line2);

            var label = CreateText(go.transform, name + " Lbl", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 24, accent);
            label.rectTransform.offsetMin = new Vector2(8f, 0f);
            label.rectTransform.offsetMax = new Vector2(-8f, 0f);
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>Cut-corner stat cell (label on top, big value below). Returns the value Text.</summary>
        private Text CreateStatCell(Transform parent, Vector2 center, Vector2 size, string label, Color accent)
        {
            var go = new GameObject("Stat " + label, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = center;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = NeonUITheme.Bg1;
            StylePanelCut(go, NeonUITheme.Line2);

            var lab = CreateText(go.transform, label + " L", new Vector2(0f, -10f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 18, NeonUITheme.TextMute);
            lab.rectTransform.sizeDelta = new Vector2(-12f, 30f);
            lab.text = label;
            var val = CreateText(go.transform, label + " V", new Vector2(0f, 8f), new Vector2(0f, 0f), new Vector2(1f, 0f), TextAnchor.LowerCenter, 30, accent);
            val.rectTransform.sizeDelta = new Vector2(-12f, size.y * 0.6f);
            val.font = NeonUITheme.UiBold;
            return val;
        }

        /// <summary>Approximate neon glow on legacy Text via a soft Outline in the accent color.</summary>
        private static void AddTextGlow(Text text, Color color, float distance = 2.5f)
        {
            var outline = text.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(color.r, color.g, color.b, 0.85f);
            outline.effectDistance = new Vector2(distance, distance);
            outline.useGraphicAlpha = false;
        }

        /// <summary>Builds a cyan vector plane (body + nose + wings) from the sprite factory for menu/garage heroes.</summary>
        private void CreatePlaneHero(Transform parent, Vector2 anchoredPosition, float size, Color tint)
        {
            var root = new GameObject("Plane Hero", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(size, size);

            void Part(string n, Sprite sprite, Vector2 anchor, Vector2 sz, Color c)
            {
                var go = new GameObject(n, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(root.transform, false);
                var r = go.GetComponent<RectTransform>();
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = anchor;
                r.sizeDelta = sz;
                var img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.color = c;
                img.raycastTarget = false;
            }

            var wing = NeonSpriteFactory.PlayerWing;
            Part("WingL", wing, new Vector2(-size * 0.24f, -size * 0.04f), new Vector2(size * 0.5f, size * 0.5f), NeonUITheme.Alpha(tint, 0.9f));
            var wr = new GameObject("WingR", typeof(RectTransform), typeof(Image));
            wr.transform.SetParent(root.transform, false);
            var wrr = wr.GetComponent<RectTransform>();
            wrr.anchorMin = wrr.anchorMax = new Vector2(0.5f, 0.5f);
            wrr.anchoredPosition = new Vector2(size * 0.24f, -size * 0.04f);
            wrr.sizeDelta = new Vector2(size * 0.5f, size * 0.5f);
            wrr.localScale = new Vector3(-1f, 1f, 1f);
            var wri = wr.GetComponent<Image>();
            wri.sprite = wing; wri.color = NeonUITheme.Alpha(tint, 0.9f); wri.raycastTarget = false;

            Part("Body", NeonSpriteFactory.PlayerBody, Vector2.zero, new Vector2(size * 0.62f, size * 0.92f), tint);
            Part("Nose", NeonSpriteFactory.PlayerNose, new Vector2(0f, size * 0.18f), new Vector2(size * 0.5f, size * 0.6f), NeonUITheme.CyanSoft);
        }

        // ── Panel styling ───────────────────────────────────────────────
        /// <summary>
        /// Give an existing Image-backed panel cut corners (9-sliced octagon fill) plus a
        /// neon border overlay, without disturbing call sites that set the Image color.
        /// </summary>
        private static void StylePanelCut(GameObject panel, Color border, float borderThickness = 1.5f)
        {
            var image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = NeonSpriteFactory.UiCutPanel;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 1f;
            }

            var b = new GameObject("Border", typeof(RectTransform), typeof(NeonCutRect));
            b.transform.SetParent(panel.transform, false);
            var br = b.GetComponent<RectTransform>();
            br.anchorMin = Vector2.zero;
            br.anchorMax = Vector2.one;
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;
            var cut = b.GetComponent<NeonCutRect>();
            cut.CutSize = NeonSpriteFactory.CutPanelCorner;
            cut.CutTL = cut.CutTR = cut.CutBR = cut.CutBL = true;
            cut.color = new Color(0f, 0f, 0f, 0f); // border only
            cut.BorderColor = border;
            cut.BorderThickness = borderThickness;
            cut.raycastTarget = false;
            b.transform.SetAsLastSibling();
        }

        // ── Neon button styling ─────────────────────────────────────────
        private static NeonCutRect? ButtonCut(Button b) => b.targetGraphic as NeonCutRect;

        private static void StyleButtonColors(Button b, Color fill, Color border, Color textColor)
        {
            var cut = ButtonCut(b);
            if (cut != null)
            {
                cut.color = fill;
                cut.BorderColor = border;
                cut.Refresh();
            }
            var label = b.GetComponentInChildren<Text>();
            if (label != null) label.color = textColor;
        }

        private static void GhostButton(Button b) =>
            StyleButtonColors(b, NeonUITheme.Alpha(NeonUITheme.Bg2, 0.6f), NeonUITheme.Line2, NeonUITheme.TextDim);

        private static void PrimaryButton(Button b) =>
            StyleButtonColors(b, NeonUITheme.Mix(NeonUITheme.Cyan, 0.20f, NeonUITheme.Bg2), NeonUITheme.Cyan, NeonUITheme.TextCyan);

        private static void MagentaButton(Button b) =>
            StyleButtonColors(b, NeonUITheme.Mix(NeonUITheme.Magenta, 0.20f, NeonUITheme.Bg2), NeonUITheme.Magenta, NeonUITheme.TextMagenta);

        private static void DangerButton(Button b) =>
            StyleButtonColors(b, NeonUITheme.Mix(NeonUITheme.Red, 0.12f, NeonUITheme.Bg2), NeonUITheme.Red, new Color(1f, 0.9f, 0.91f));

        private static void AccentButton(Button b, Color accent) =>
            StyleButtonColors(b, NeonUITheme.Mix(accent, 0.18f, NeonUITheme.Bg2), accent, NeonUITheme.Text);

        /// <summary>
        /// Round neon HUD action button (dash / special): a dark disc, a dim full
        /// border ring, a bright radial charge/cooldown ring, and a centered glyph.
        /// Returns the Button; the radial ring Image is emitted via <paramref name="ring"/>.
        /// </summary>
        private Button CreateRoundButton(Transform parent, string name, Color accent, string glyph, string subLabel, Vector2 anchoredPosition, float size, UnityEngine.Events.UnityAction action, out Image ring)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(size, size);

            var disc = go.GetComponent<Image>();
            disc.sprite = NeonSpriteFactory.UiDisc;
            disc.color = NeonUITheme.Mix(accent, 0.20f, NeonUITheme.Bg1);

            var button = go.GetComponent<Button>();
            button.targetGraphic = disc;
            button.onClick.AddListener(action);

            AddRingChild(go.transform, "Border", NeonUITheme.Alpha(accent, 0.35f), false, out _);
            AddRingChild(go.transform, "Charge", accent, true, out ring);

            var glyphSize = Mathf.RoundToInt(size * (string.IsNullOrEmpty(subLabel) ? 0.42f : 0.34f));
            var label = CreateText(go.transform, name + " Glyph", new Vector2(0f, string.IsNullOrEmpty(subLabel) ? 0f : size * 0.08f), Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, glyphSize, accent, NeonUITheme.UiBold);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.text = glyph;
            label.raycastTarget = false;

            if (!string.IsNullOrEmpty(subLabel))
            {
                var sub = CreateText(go.transform, name + " Sub", new Vector2(0f, -size * 0.20f), Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, Mathf.RoundToInt(size * 0.13f), accent);
                sub.rectTransform.offsetMin = Vector2.zero;
                sub.rectTransform.offsetMax = Vector2.zero;
                sub.text = subLabel;
                sub.raycastTarget = false;
            }
            return button;
        }

        private static void AddRingChild(Transform parent, string name, Color color, bool radial, out Image ring)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var r = obj.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            ring = obj.GetComponent<Image>();
            ring.sprite = NeonSpriteFactory.UiRing;
            ring.color = color;
            ring.raycastTarget = false;
            if (radial)
            {
                ring.type = Image.Type.Filled;
                ring.fillMethod = Image.FillMethod.Radial360;
                ring.fillOrigin = (int)Image.Origin360.Top;
                ring.fillClockwise = true;
                ring.fillAmount = 1f;
            }
        }

        private static Sprite CreateSprite()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        private void SetRunHudVisible(bool visible)
        {
            _timerText.gameObject.SetActive(visible);
            _waveText.gameObject.SetActive(visible);
            _killsText.gameObject.SetActive(visible);
            _messageText.gameObject.SetActive(visible);
            _statusText.gameObject.SetActive(visible);
            _hpBarFill.transform.parent.gameObject.SetActive(visible);
            _xpBarFill.transform.parent.gameObject.SetActive(visible);
            _dashButton.gameObject.SetActive(visible);
            _specialButton.gameObject.SetActive(visible);
            _pauseButton.gameObject.SetActive(visible);
            if (!visible)
            {
                _bossBarRoot.SetActive(false);
                _pauseMenuPanel.SetActive(false);
                HideBuffChips();
            }
        }

        private void UpdateGaragePanel()
        {
            var stats = _equipment.CalculateStats(_profile, _catalog);
            _garageTitleText.text = "NEON SKY SURVIVORS — GARAGE";
            _garageStatsText.text = "Coins " + _profile.PlayerCoins
                + "   Runs " + _profile.CompletedRuns
                + "   Best " + FormatTime(_profile.BestSurvivalTime)
                + "   Lv " + _profile.Meta.AccountLevel
                + "\nATK " + stats.AttackDamage.ToString("0")
                + "  Fire " + stats.FireRate.ToString("0.0")
                + "  Speed " + stats.MovementSpeed.ToString("0.0")
                + "  HP " + stats.MaxHP.ToString("0")
                + "  Armor " + stats.Armor.ToString("0")
                + "  Dash " + stats.DashCooldown.ToString("0.0") + "s";

            UpdateSlotButtons();
            RebuildInventoryCards();
            UpdateGarageActions();
            UpdateSectorLabel();
        }

        private void RebuildInventoryCards()
        {
            for (var index = 0; index < _inventoryCards.Count; index++)
            {
                var card = _inventoryCards[index];
                card.transform.SetParent(null, false);
                Destroy(card);
            }

            _inventoryCards.Clear();

            for (var index = 0; index < _profile.OwnedEquipmentItems.Count; index++)
            {
                var owned = _profile.OwnedEquipmentItems[index];
                var definition = FindEquipmentDef(owned.ItemID);
                if (definition == null)
                {
                    continue;
                }

                if (_selectedSlotFilter.HasValue && definition.SlotType != _selectedSlotFilter.Value)
                {
                    continue;
                }

                _inventoryCards.Add(CreateInventoryCard(owned, definition));
            }
        }

        private GameObject CreateInventoryCard(NeonOwnedEquipmentItem owned, NeonEquipmentItemDef definition)
        {
            var cardObject = new GameObject("Card " + owned.InstanceID, typeof(RectTransform), typeof(Image), typeof(Button));
            cardObject.transform.SetParent(_inventoryContent, false);

            var isSelected = owned.InstanceID == _selectedInstanceId;
            var isEquipped = GetEquippedItemId(definition.SlotType) == owned.ItemID;
            var rarityColor = ResolveRarityColor(owned.Rarity);

            var image = cardObject.GetComponent<Image>();
            image.sprite = NeonSpriteFactory.UiCutPanel;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = isSelected
                ? NeonUITheme.Mix(rarityColor, 0.34f, NeonUITheme.Bg1)
                : NeonUITheme.Mix(rarityColor, 0.14f, NeonUITheme.Bg1);

            var capturedId = owned.InstanceID;
            cardObject.GetComponent<Button>().onClick.AddListener(() => SelectInventoryItem(capturedId));

            // rarity neon border (brighter when selected)
            var cardBorderObj = new GameObject("Card Border", typeof(RectTransform), typeof(NeonCutRect));
            cardBorderObj.transform.SetParent(cardObject.transform, false);
            var cbRect = cardBorderObj.GetComponent<RectTransform>();
            cbRect.anchorMin = Vector2.zero;
            cbRect.anchorMax = Vector2.one;
            cbRect.offsetMin = Vector2.zero;
            cbRect.offsetMax = Vector2.zero;
            var cardBorder = cardBorderObj.GetComponent<NeonCutRect>();
            cardBorder.CutSize = NeonSpriteFactory.CutPanelCorner;
            cardBorder.CutTL = cardBorder.CutTR = cardBorder.CutBR = cardBorder.CutBL = true;
            cardBorder.color = new Color(0f, 0f, 0f, 0f);
            cardBorder.BorderColor = rarityColor;
            cardBorder.BorderThickness = isSelected ? 2.5f : 1.5f;
            cardBorder.raycastTarget = false;

            // Slot icon — top-right corner
            var iconObj = new GameObject("Slot Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(cardObject.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(1f, 1f);
            iconRect.anchorMax = new Vector2(1f, 1f);
            iconRect.anchoredPosition = new Vector2(-16f, -16f);
            iconRect.sizeDelta = new Vector2(30f, 30f);
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = NeonSpriteFactory.GetIcon(definition.SlotType);
            iconImg.color  = rarityColor * 0.85f;

            var label = CreateText(cardObject.transform, "Card Label", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 24, rarityColor);
            label.rectTransform.offsetMin = new Vector2(12f, 8f);
            label.rectTransform.offsetMax = new Vector2(-12f, -8f);
            label.text = definition.Name + (isEquipped ? "  [E]" : string.Empty) + "\n"
                + owned.Rarity + "  Lv " + owned.Level + "/" + NeonEquipmentSystem.MvpMaxEquipmentLevel;

            return cardObject;
        }

        private void SelectInventoryItem(string instanceId)
        {
            _selectedInstanceId = instanceId;
            RebuildInventoryCards();
            UpdateGarageActions();
        }

        private void UpdateGarageActions()
        {
            var owned = FindOwnedInstance(_selectedInstanceId);
            if (owned == null)
            {
                _garageDetailText.text = "Tap an item to equip, upgrade, or merge.";
                SetActionState(_equipButton, "Equip", false);
                SetActionState(_unequipButton, "Unequip", false);
                SetActionState(_upgradeButton, "Upgrade", false);
                SetActionState(_mergeButton, "Merge x3", false);
                return;
            }

            var definition = FindEquipmentDef(owned.ItemID);
            var isEquipped = definition != null && GetEquippedItemId(definition.SlotType) == owned.ItemID;
            var duplicates = _equipment.CountDuplicates(_profile, owned.ItemID, owned.Rarity);
            var canMerge = owned.Rarity < NeonEquipmentRarity.Mythic && duplicates >= NeonEquipmentSystem.RequiredDuplicatesForMerge;
            var hasUpgradeCost = _equipment.TryGetUpgradeCost(_profile, _catalog, owned.InstanceID, out var upgradeCost);
            var canUpgrade = hasUpgradeCost && _profile.PlayerCoins >= upgradeCost;

            _garageDetailText.text = (definition != null ? definition.Name : owned.ItemID) + " · " + owned.Rarity + " Lv " + owned.Level
                + (string.IsNullOrWhiteSpace(definition?.SpecialEffect) ? string.Empty : "\n" + definition!.SpecialEffect)
                + "\nOwned at this rarity: " + duplicates;

            SetActionState(_equipButton, "Equip", definition != null && !isEquipped);
            SetActionState(_unequipButton, "Unequip", isEquipped);
            SetActionState(_upgradeButton, hasUpgradeCost ? "Upgrade " + upgradeCost : "Maxed", canUpgrade);
            SetActionState(_mergeButton, "Merge x3", canMerge);
        }

        private static void SetActionState(Button button, string label, bool interactable)
        {
            button.interactable = interactable;
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
                text.color = interactable ? Color.white : new Color(0.5f, 0.55f, 0.6f);
            }
        }

        private void EquipSelected()
        {
            if (_equipment.TryEquip(_profile, _catalog, _selectedInstanceId))
            {
                PersistAndRefreshGarage();
            }
        }

        private void UnequipSelected()
        {
            var owned = FindOwnedInstance(_selectedInstanceId);
            var definition = owned == null ? null : FindEquipmentDef(owned.ItemID);
            if (definition != null && _equipment.TryUnequip(_profile, definition.SlotType))
            {
                PersistAndRefreshGarage();
            }
        }

        private void UpgradeSelected()
        {
            if (_equipment.TryUpgrade(_profile, _catalog, _selectedInstanceId))
            {
                PersistAndRefreshGarage();
            }
        }

        private void MergeSelected()
        {
            var owned = FindOwnedInstance(_selectedInstanceId);
            if (owned == null)
            {
                return;
            }

            if (_equipment.TryMergeDuplicates(_profile, owned.ItemID, owned.Rarity, out var mergedItem) && mergedItem != null)
            {
                _selectedInstanceId = mergedItem.InstanceID;
                PersistAndRefreshGarage();
            }
        }

        private void PersistAndRefreshGarage()
        {
            NeonSaveService.Save(_profile);
            UpdateGaragePanel();
        }

        private NeonOwnedEquipmentItem FindOwnedInstance(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return null!;
            }

            for (var index = 0; index < _profile.OwnedEquipmentItems.Count; index++)
            {
                if (_profile.OwnedEquipmentItems[index].InstanceID == instanceId)
                {
                    return _profile.OwnedEquipmentItems[index];
                }
            }

            return null!;
        }

        private static Color ResolveRarityColor(NeonEquipmentRarity rarity)
        {
            switch (rarity)
            {
                case NeonEquipmentRarity.Uncommon:
                    return new Color(0.42f, 1f, 0.5f);
                case NeonEquipmentRarity.Rare:
                    return new Color(0.4f, 0.68f, 1f);
                case NeonEquipmentRarity.Epic:
                    return new Color(0.78f, 0.46f, 1f);
                case NeonEquipmentRarity.Legendary:
                    return new Color(1f, 0.82f, 0.28f);
                case NeonEquipmentRarity.Mythic:
                    return new Color(1f, 0.34f, 0.34f);
                default:
                    return new Color(0.78f, 0.82f, 0.86f);
            }
        }

        private int CalculateRunReward(NeonRunState run)
        {
            var rewards = _catalog.Rewards;
            var survivalMinutes = Mathf.FloorToInt(run.ElapsedSeconds / 60f);
            // Quantum Sensor (Radar): boss rewards improved — +25% boss/mini-boss coin bonuses.
            var bossBonusScale = run.ActiveEquipmentEffects.Contains("boss_reward_boost") ? 1.25f : 1f;
            var coins = rewards.BaseCoins
                + run.Player.CoinsCollected
                + run.EnemiesKilled * rewards.CoinPerKill
                + run.BossesKilled * rewards.BossCoinBonus * bossBonusScale
                + run.MiniBossesKilled * rewards.MiniBossCoinBonus * bossBonusScale
                + survivalMinutes * rewards.SurvivalMinuteCoins;

            return Mathf.Max(0, Mathf.RoundToInt(coins * run.Player.Stats.CoinBonus * NeonRunGameplaySystem.SectorRewardScale(run.Sector)));
        }

        private void CreateMissionsPanel(Transform parent)
        {
            _missionsPanel = new GameObject("Missions Panel", typeof(RectTransform), typeof(Image));
            _missionsPanel.transform.SetParent(parent, false);
            var panelRect = _missionsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            _missionsPanel.GetComponent<Image>().color = new Color(0.01f, 0.025f, 0.055f, 0.98f);

            CreateDisplayText(_missionsPanel.transform, "Missions Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 38, NeonUITheme.Uncommon)
                .text = "DAILY MISSIONS";

            // Scrollable list
            var scrollObject = new GameObject("Missions Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(_missionsPanel.transform, false);
            var scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.04f, 0.18f);
            scrollRect.anchorMax = new Vector2(0.96f, 0.84f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scrollObject.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.1f, 0.85f);

            var viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObj.transform.SetParent(scrollObject.transform, false);
            var vpRect = viewportObj.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = new Vector2(10f, 10f);
            vpRect.offsetMax = new Vector2(-10f, -10f);
            viewportObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewportObj.GetComponent<Mask>().showMaskGraphic = false;

            var contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(viewportObj.transform, false);
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            contentObj.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = vpRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            _missionsContent = contentRect;

            var backButton = CreateButton(_missionsPanel.transform, "Back", new Vector2(0f, 120f), HideMissions);
            backButton.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 110f);
            GhostButton(backButton);

            _missionsPanel.SetActive(false);
            _achievementsPanel.SetActive(false);
        }

        private void ShowMissions()
        {
            RefreshDailyMissions();
            RefreshWeeklyMission();
            _garagePanel.SetActive(false);
            _missionsPanel.SetActive(true);
            RebuildMissionCards();
        }

        private void HideMissions()
        {
            _missionsPanel.SetActive(false);
            _achievementsPanel.SetActive(false);
            ShowGarage();
        }

        private void ShowAchievements()
        {
            _garagePanel.SetActive(false);
            _achievementsPanel.SetActive(true);
            RebuildAchievementCards();
        }

        private void HideAchievements()
        {
            _achievementsPanel.SetActive(false);
            ShowGarage();
        }

        private void RebuildAchievementCards()
        {
            for (var i = _achievementsContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_achievementsContent.GetChild(i).gameObject);
            }
            // Stats summary card
            CreateStatsCard();
            foreach (var def in Achievements)
            {
                var unlocked = _profile.UnlockedAchievements.Contains(def.Id);
                CreateAchievementCard(def, unlocked);
            }
        }

        private void CreateStatsCard()
        {
            var card = new GameObject("Stats Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_achievementsContent, false);
            var layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 180f;
            layoutElem.flexibleWidth = 1f;
            card.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.10f, 0.92f);
            StylePanelCut(card, NeonUITheme.Mix(NeonUITheme.Cyan, 0.4f, NeonUITheme.Line2));

            var unlocked = _profile.UnlockedAchievements.Count;
            var total    = Achievements.Length;
            CreateDisplayText(card.transform, "Stats Title", new Vector2(0f, -28f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 28, NeonUITheme.TextCyan)
                .text = "PILOT CODEX";
            CreateText(card.transform, "Stats Body", new Vector2(0f, -75f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 22, NeonUITheme.Text)
                .text = "Enemies: " + _profile.LifetimeEnemiesKilled
                    + "   Bosses: " + _profile.LifetimeBossesKilled
                    + "   Runs: " + _profile.CompletedRuns;
            CreateText(card.transform, "Stats Body2", new Vector2(0f, -112f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 22, NeonUITheme.Text)
                .text = "Time Played: " + FormatTime(_profile.LifetimeTimePlayed)
                    + "   Best: " + FormatTime(_profile.BestSurvivalTime);
            CreateText(card.transform, "Stats Achiev", new Vector2(0f, -148f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 20, NeonUITheme.TextMute)
                .text = "Achievements: " + unlocked + " / " + total;
        }

        private void CreateAchievementCard(AchievementDef def, bool unlocked)
        {
            var card = new GameObject("Achiev " + def.Id, typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_achievementsContent, false);
            var layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 140f;
            layoutElem.flexibleWidth = 1f;
            card.GetComponent<Image>().color = unlocked
                ? new Color(0.10f, 0.08f, 0.02f, 0.92f)
                : new Color(0.04f, 0.04f, 0.08f, 0.88f);
            StylePanelCut(card, unlocked ? NeonUITheme.Legendary : NeonUITheme.Line2);

            var nameText = unlocked ? "★  " + def.Name : "?  " + def.Name;
            var statusText = unlocked ? "UNLOCKED" : "Locked";
            var textColor = unlocked ? NeonUITheme.Legendary : NeonUITheme.TextDim;
            var infoText = CreateText(card.transform, "Achiev Info", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleLeft, 26, textColor);
            infoText.rectTransform.offsetMin = new Vector2(18f, 8f);
            infoText.rectTransform.offsetMax = new Vector2(-12f, -8f);
            infoText.text = nameText + "\n<size=20>" + def.Desc + "</size>\n<size=18><color=#888>" + statusText + "</color></size>";
        }

        private void CreateAchievementsPanel(Transform parent)
        {
            _achievementsPanel = new GameObject("Achievements Panel", typeof(RectTransform), typeof(Image));
            _achievementsPanel.transform.SetParent(parent, false);
            var panelRect = _achievementsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            _achievementsPanel.GetComponent<Image>().color = new Color(0.01f, 0.025f, 0.055f, 0.98f);

            CreateDisplayText(_achievementsPanel.transform, "Achievements Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 38, NeonUITheme.Legendary)
                .text = "ACHIEVEMENTS";

            // Scrollable content area
            var scrollObject = new GameObject("Achievements Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollObject.transform.SetParent(_achievementsPanel.transform, false);
            scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            var scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0.12f);
            scrollRect.anchorMax = new Vector2(1f, 0.90f);
            scrollRect.offsetMin = new Vector2(20f, 0f);
            scrollRect.offsetMax = new Vector2(-20f, -90f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            var vpRect = viewport.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var vLayout = content.GetComponent<VerticalLayoutGroup>();
            vLayout.spacing = 18f;
            vLayout.padding = new RectOffset(0, 0, 8, 8);
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _achievementsContent = contentRect;

            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = vpRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            var backButton = CreateButton(_achievementsPanel.transform, "Back", new Vector2(0f, 120f), HideAchievements);
            backButton.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 110f);
            GhostButton(backButton);

            _achievementsPanel.SetActive(false);
        }

        // ── Pilot milestone reward track ─────────────────────────────────────────
        // level → (coinBonus, rankTitle, runPerkKey)
        // rankTitle shown in the Missions panel pilot card.
        // runPerkKey applied to the next run via _run.RerollsRemaining/BanishesRemaining.
        private static readonly (int Level, int CoinBonus, string Title, string PerkKey)[] AccountMilestones =
        {
            (2,  50,  "Pilot",       "bonus_reroll"),
            (5,  100, "Ace",         "bonus_banish"),
            (10, 200, "Veteran",     "bonus_reroll"),
            (15, 350, "Commander",   "bonus_banish"),
            (20, 600, "Legend",      "bonus_reroll"),
        };

        private static int AccountXpPerLevel(int level) => 100 * level;

        private static string PilotTitle(int level)
        {
            for (var i = AccountMilestones.Length - 1; i >= 0; i--)
            {
                if (level >= AccountMilestones[i].Level)
                    return AccountMilestones[i].Title;
            }
            return "Cadet";
        }

        private static int PilotExtraRerolls(int level)
        {
            var count = 0;
            foreach (var m in AccountMilestones)
            {
                if (level >= m.Level && m.PerkKey == "bonus_reroll") count++;
            }
            return count;
        }

        private static int PilotExtraBanishes(int level)
        {
            var count = 0;
            foreach (var m in AccountMilestones)
            {
                if (level >= m.Level && m.PerkKey == "bonus_banish") count++;
            }
            return count;
        }

        private static void GrantMilestoneIfDue(NeonSaveProfile profile)
        {
            foreach (var m in AccountMilestones)
            {
                if (profile.Meta.AccountLevel == m.Level
                    && !profile.Meta.ClaimedMilestoneLevels.Contains(m.Level))
                {
                    profile.Meta.ClaimedMilestoneLevels.Add(m.Level);
                    profile.PlayerCoins += m.CoinBonus;
                }
            }
        }

        // ── Achievement definitions ───────────────────────────────────────────────
        private sealed class AchievementDef
        {
            public string Id   = string.Empty;
            public string Name = string.Empty;
            public string Desc = string.Empty;
        }

        private static readonly AchievementDef[] Achievements =
        {
            new AchievementDef { Id = "first_run",      Name = "First Flight",       Desc = "Complete your first run." },
            new AchievementDef { Id = "run_10",          Name = "Seasoned",            Desc = "Complete 10 runs." },
            new AchievementDef { Id = "kill_1000",       Name = "Thousand Kills",      Desc = "Destroy 1,000 enemies total." },
            new AchievementDef { Id = "boss_5",          Name = "Boss Slayer",         Desc = "Defeat 5 bosses total." },
            new AchievementDef { Id = "survive_10min",   Name = "Ironclad",            Desc = "Survive 10 minutes in a single run." },
            new AchievementDef { Id = "reach_lv5",       Name = "Veteran Pilot",       Desc = "Reach Account Level 5." },
            new AchievementDef { Id = "sector_3",        Name = "Sector 3 Cleared",    Desc = "Win a run on Sector 3 or higher." },
            new AchievementDef { Id = "sector_8",        Name = "Apex Pilot",          Desc = "Win a run on Sector 8." },
            new AchievementDef { Id = "evolution_3",     Name = "Triple Evolution",    Desc = "Evolve 3 weapons in a single run." },
            new AchievementDef { Id = "all_weapons",     Name = "Full Arsenal",        Desc = "Max out 3 different weapon upgrades in one run." },
        };

        private bool TryUnlock(string id)
        {
            if (_profile.UnlockedAchievements.Contains(id)) return false;
            _profile.UnlockedAchievements.Add(id);
            return true;
        }

        private void CheckAchievements(NeonRunState run)
        {
            if (_profile.CompletedRuns >= 1)  TryUnlock("first_run");
            if (_profile.CompletedRuns >= 10) TryUnlock("run_10");
            if (_profile.LifetimeEnemiesKilled >= 1000) TryUnlock("kill_1000");
            if (_profile.LifetimeBossesKilled >= 5)     TryUnlock("boss_5");
            if (run.ElapsedSeconds >= 600f)              TryUnlock("survive_10min");
            if (_profile.Meta.AccountLevel >= 5)        TryUnlock("reach_lv5");
            if (_profile.HighestSectorCleared >= 3)     TryUnlock("sector_3");
            if (_profile.HighestSectorCleared >= 8)     TryUnlock("sector_8");
            if (run.Build.EvolvedWeapons.Count >= 3)    TryUnlock("evolution_3");

            // "all_weapons": at least 3 weapon upgrades at max level (L5) in this run.
            var maxedWeapons = 0;
            foreach (var kv in run.Build.UpgradeLevels)
            {
                var upgradeId = kv.Key;
                var def = _catalog.Upgrades.Find(u => u.Id == upgradeId);
                if (def != null && def.Category == NeonUpgradeCategory.Weapon && kv.Value >= 5)
                    maxedWeapons++;
            }
            if (maxedWeapons >= 3) TryUnlock("all_weapons");
        }

        private static readonly NeonMissionState[] MissionTemplates =
        {
            new NeonMissionState { Id = "kill30", Name = "Exterminator", Description = "Kill 30 enemies in one run", Metric = "kills", Target = 30, RewardCoins = 40, RewardAccountXP = 20 },
            new NeonMissionState { Id = "kill100", Name = "Slaughter", Description = "Kill 100 enemies in one run", Metric = "kills", Target = 100, RewardCoins = 80, RewardAccountXP = 40 },
            new NeonMissionState { Id = "survive3", Name = "Survivor", Description = "Survive 3 minutes", Metric = "survive", Target = 3, RewardCoins = 30, RewardAccountXP = 15 },
            new NeonMissionState { Id = "survive6", Name = "Veteran", Description = "Survive 6 minutes", Metric = "survive", Target = 6, RewardCoins = 60, RewardAccountXP = 30 },
            new NeonMissionState { Id = "boss1", Name = "Boss Hunter", Description = "Defeat a major boss", Metric = "bosses", Target = 1, RewardCoins = 50, RewardAccountXP = 25 },
            new NeonMissionState { Id = "complete1", Name = "Full Run", Description = "Complete a full 10-minute run", Metric = "runs", Target = 1, RewardCoins = 100, RewardAccountXP = 50 },
        };

        // Weekly missions have longer targets and bigger rewards than dailies.
        private static readonly NeonMissionState[] WeeklyTemplates =
        {
            new NeonMissionState { Id = "w_kill500", Name = "Annihilator", Description = "Kill 500 enemies across runs", Metric = "kills_total", Target = 500, RewardCoins = 300, RewardAccountXP = 120 },
            new NeonMissionState { Id = "w_boss10",  Name = "Boss Slayer",  Description = "Defeat 10 bosses",             Metric = "bosses_total", Target = 10,  RewardCoins = 250, RewardAccountXP = 100 },
            new NeonMissionState { Id = "w_survive30", Name = "Ironclad",   Description = "Survive 30 minutes total",     Metric = "survive_total", Target = 30,  RewardCoins = 280, RewardAccountXP = 110 },
            new NeonMissionState { Id = "w_runs5",   Name = "Road Warrior", Description = "Complete 5 full runs",         Metric = "runs_total",   Target = 5,   RewardCoins = 350, RewardAccountXP = 140 },
            new NeonMissionState { Id = "w_sector3", Name = "Sector Storm", Description = "Win a run on Sector 3 or higher", Metric = "sector_win", Target = 3,  RewardCoins = 400, RewardAccountXP = 160 },
        };

        private void RefreshDailyMissions()
        {
            var today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (_profile.Meta.DailyMissions.Count == 3 && _profile.Meta.DailyMissionDate == today)
            {
                return; // still current day
            }

            // New day — pick 3 missions using the day's numeric value as a deterministic seed
            _profile.Meta.DailyMissions.Clear();
            var seed = (int)(System.DateTime.UtcNow - new System.DateTime(2024, 1, 1)).TotalDays;
            var rng = new System.Random(seed);
            var indices = new System.Collections.Generic.List<int> { 0, 1, 2, 3, 4, 5 };
            for (var pick = 0; pick < 3 && indices.Count > 0; pick++)
            {
                var chosen = rng.Next(indices.Count);
                var template = MissionTemplates[indices[chosen]];
                indices.RemoveAt(chosen);
                _profile.Meta.DailyMissions.Add(new NeonMissionState
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Metric = template.Metric,
                    Target = template.Target,
                    Progress = 0,
                    Claimed = false,
                    RewardCoins = template.RewardCoins,
                    RewardAccountXP = template.RewardAccountXP
                });
            }

            _profile.Meta.DailyMissionDate = today;
            NeonSaveService.Save(_profile);
        }

        private static string CurrentWeekKey()
        {
            // ISO week: year + week-of-year to give a unique key per calendar week.
            var now = System.DateTime.UtcNow;
            var week = System.Globalization.ISOWeek.GetWeekOfYear(now);
            return now.Year + "-W" + week.ToString("00");
        }

        private void RefreshWeeklyMission()
        {
            var weekKey = CurrentWeekKey();
            if (_profile.Meta.WeeklyMissionDate == weekKey
                && !string.IsNullOrEmpty(_profile.Meta.WeeklyMission.Id))
            {
                return; // already have this week's mission
            }

            var weekNum = int.Parse(weekKey.Substring(weekKey.IndexOf('W') + 1));
            var template = WeeklyTemplates[weekNum % WeeklyTemplates.Length];
            _profile.Meta.WeeklyMission = new NeonMissionState
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Metric = template.Metric,
                Target = template.Target,
                Progress = 0,
                Claimed = false,
                RewardCoins = template.RewardCoins,
                RewardAccountXP = template.RewardAccountXP
            };
            _profile.Meta.WeeklyMissionDate = weekKey;
            NeonSaveService.Save(_profile);
        }

        private void UpdateMissionProgressFromRun(NeonRunState run)
        {
            var changed = false;

            // Daily mission progress
            foreach (var mission in _profile.Meta.DailyMissions)
            {
                if (mission.Claimed) continue;
                var prev = mission.Progress;
                ApplyRunMetric(mission, run);
                if (mission.Progress != prev) changed = true;
            }

            // Weekly mission progress (accumulates across runs for _total metrics)
            var weekly = _profile.Meta.WeeklyMission;
            if (weekly != null && !weekly.Claimed && !string.IsNullOrEmpty(weekly.Id))
            {
                var prev = weekly.Progress;
                switch (weekly.Metric)
                {
                    case "kills_total":   weekly.Progress += run.EnemiesKilled; break;
                    case "bosses_total":  weekly.Progress += run.BossesKilled + run.MiniBossesKilled; break;
                    case "survive_total": weekly.Progress += Mathf.FloorToInt(run.ElapsedSeconds / 60f); break;
                    case "runs_total":
                        if (run.Status == NeonRunStatus.Victory) weekly.Progress += 1;
                        break;
                    case "sector_win":
                        if (run.Status == NeonRunStatus.Victory && run.Sector >= weekly.Target)
                            weekly.Progress = weekly.Target;
                        break;
                }
                if (weekly.Progress != prev) changed = true;
            }

            if (changed) NeonSaveService.Save(_profile);
        }

        private static void ApplyRunMetric(NeonMissionState mission, NeonRunState run)
        {
            switch (mission.Metric)
            {
                case "kills":
                    mission.Progress = Mathf.Max(mission.Progress, run.EnemiesKilled);
                    break;
                case "survive":
                    mission.Progress = Mathf.Max(mission.Progress, Mathf.FloorToInt(run.ElapsedSeconds / 60f));
                    break;
                case "bosses":
                    mission.Progress = Mathf.Max(mission.Progress, run.BossesKilled);
                    break;
                case "runs":
                    if (run.Status == NeonRunStatus.Victory)
                        mission.Progress = Mathf.Max(mission.Progress, 1);
                    break;
            }
        }

        private void ClaimMission(int index)
        {
            if (index < 0 || index >= _profile.Meta.DailyMissions.Count) return;
            var mission = _profile.Meta.DailyMissions[index];
            if (mission.Claimed || mission.Progress < mission.Target) return;

            mission.Claimed = true;
            _profile.PlayerCoins += mission.RewardCoins;
            _profile.Meta.AccountXP += mission.RewardAccountXP;

            // Level up account if threshold reached; check for milestone rewards.
            while (_profile.Meta.AccountXP >= AccountXpPerLevel(_profile.Meta.AccountLevel))
            {
                _profile.Meta.AccountXP -= AccountXpPerLevel(_profile.Meta.AccountLevel);
                _profile.Meta.AccountLevel += 1;
                _profile.PlayerCoins += 30 * _profile.Meta.AccountLevel;
                GrantMilestoneIfDue(_profile);
            }

            NeonSaveService.Save(_profile);
            RebuildMissionCards();
            UpdateGaragePanel();
        }

        private void RebuildMissionCards()
        {
            // Destroy old children of _missionsContent
            for (var i = _missionsContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_missionsContent.GetChild(i).gameObject);
            }

            CreatePilotRankCard();

            // Weekly mission (above dailies)
            if (!string.IsNullOrEmpty(_profile.Meta.WeeklyMission?.Id))
            {
                CreateWeeklyMissionCard(_profile.Meta.WeeklyMission);
            }

            for (var index = 0; index < _profile.Meta.DailyMissions.Count; index++)
            {
                var mission = _profile.Meta.DailyMissions[index];
                CreateMissionCard(mission, index);
            }
        }

        private void CreatePilotRankCard()
        {
            var card = new GameObject("Pilot Rank", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_missionsContent, false);
            var layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 180f;
            layoutElem.flexibleWidth = 1f;
            card.GetComponent<Image>().color = new Color(0.02f, 0.06f, 0.12f, 0.92f);
            StylePanelCut(card, NeonUITheme.Mix(NeonUITheme.Amber, 0.5f, NeonUITheme.Line2));

            var level = _profile.Meta.AccountLevel;
            var xp = _profile.Meta.AccountXP;
            var xpNeeded = AccountXpPerLevel(level);
            var title = PilotTitle(level);
            var extraRerolls = PilotExtraRerolls(level);
            var extraBanishes = PilotExtraBanishes(level);

            CreateDisplayText(card.transform, "Rank Title", new Vector2(0f, -28f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 32, NeonUITheme.Amber)
                .text = "PILOT RANK — " + title.ToUpperInvariant();

            CreateText(card.transform, "Rank Sub", new Vector2(0f, -72f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 22, NeonUITheme.TextMute)
                .text = "LV " + level + "   XP " + xp + " / " + xpNeeded;

            // Perks line
            var perksText = "PERKS: ";
            perksText += extraRerolls > 0 ? "+" + extraRerolls + " Reroll" + (extraRerolls > 1 ? "s" : "") + "/run   " : "";
            perksText += extraBanishes > 0 ? "+" + extraBanishes + " Banish" + (extraBanishes > 1 ? "es" : "") + "/run   " : "";
            if (perksText == "PERKS: ") perksText += "none yet";
            CreateText(card.transform, "Perks", new Vector2(0f, -105f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 20, NeonUITheme.TextDim)
                .text = perksText;

            // Next milestone hint
            string nextHint = "MAX LEVEL";
            foreach (var m in AccountMilestones)
            {
                if (level < m.Level)
                {
                    nextHint = "LV " + m.Level + " → " + m.Title + " · +" + m.CoinBonus + " ◎";
                    break;
                }
            }
            CreateText(card.transform, "Next", new Vector2(0f, -138f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 18, NeonUITheme.Amber)
                .text = "NEXT: " + nextHint;
        }

        private void CreateMissionCard(NeonMissionState mission, int index)
        {
            var card = new GameObject("Mission " + mission.Id, typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_missionsContent, false);

            var layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 200f;
            layoutElem.flexibleWidth = 1f;

            var complete = mission.Progress >= mission.Target;
            var claimed = mission.Claimed;
            var cardColor = claimed
                ? new Color(0.06f, 0.2f, 0.08f, 0.8f)
                : complete
                    ? new Color(0.04f, 0.3f, 0.12f, 0.95f)
                    : new Color(0.05f, 0.1f, 0.14f, 0.92f);
            card.GetComponent<Image>().color = cardColor;
            StylePanelCut(card, complete ? NeonUITheme.Uncommon : NeonUITheme.Line2);

            var progressText = mission.Target > 1
                ? Mathf.Min(mission.Progress, mission.Target) + "/" + mission.Target
                : (complete ? "Done" : "0/" + mission.Target);

            var statusSuffix = claimed ? "  ✓ Claimed" : complete ? "  — COMPLETE!" : "  [" + progressText + "]";
            var infoText = CreateText(card.transform, "Mission Info", new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(0.68f, 1f), TextAnchor.MiddleLeft, 26, complete && !claimed ? new Color(0.6f, 1f, 0.65f) : Color.white);
            infoText.rectTransform.offsetMin = new Vector2(18f, 8f);
            infoText.rectTransform.offsetMax = new Vector2(-8f, -8f);
            infoText.text = mission.Name + statusSuffix + "\n" + mission.Description + "\nReward: " + mission.RewardCoins + " coins  +" + mission.RewardAccountXP + " XP";

            if (complete && !claimed)
            {
                var capturedIndex = index;
                var claimBtn = CreateButton(card.transform, "Claim!", new Vector2(0f, 0f), () => ClaimMission(capturedIndex));
                var claimRect = claimBtn.GetComponent<RectTransform>();
                claimRect.anchorMin = new Vector2(0.7f, 0.15f);
                claimRect.anchorMax = new Vector2(0.97f, 0.85f);
                claimRect.offsetMin = Vector2.zero;
                claimRect.offsetMax = Vector2.zero;
                claimRect.sizeDelta = Vector2.zero;
                AccentButton(claimBtn, NeonUITheme.Uncommon);
                claimBtn.GetComponentInChildren<Text>().fontSize = 30;
            }
        }

        private void CreateWeeklyMissionCard(NeonMissionState mission)
        {
            var card = new GameObject("Weekly " + mission.Id, typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_missionsContent, false);
            var layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 200f;
            layoutElem.flexibleWidth = 1f;

            var complete = mission.Progress >= mission.Target;
            var claimed = mission.Claimed;
            var cardColor = claimed
                ? new Color(0.10f, 0.14f, 0.04f, 0.85f)
                : complete
                    ? new Color(0.14f, 0.10f, 0.04f, 0.95f)
                    : new Color(0.06f, 0.06f, 0.14f, 0.92f);
            card.GetComponent<Image>().color = cardColor;
            StylePanelCut(card, complete ? NeonUITheme.Amber : NeonUITheme.Mix(NeonUITheme.Amber, 0.4f, NeonUITheme.Line2));

            var progressText = mission.Target > 1
                ? Mathf.Min(mission.Progress, mission.Target) + "/" + mission.Target
                : (complete ? "Done" : "0/" + mission.Target);
            var statusSuffix = claimed ? "  ✓ Claimed" : complete ? "  — COMPLETE!" : "  [" + progressText + "]";

            var infoText = CreateText(card.transform, "Weekly Info", Vector2.zero, Vector2.zero, new Vector2(0.68f, 1f), TextAnchor.MiddleLeft, 26, complete && !claimed ? NeonUITheme.Amber : NeonUITheme.Text);
            infoText.rectTransform.offsetMin = new Vector2(18f, 8f);
            infoText.rectTransform.offsetMax = new Vector2(-8f, -8f);
            infoText.text = "⚡ WEEKLY: " + mission.Name + statusSuffix + "\n" + mission.Description + "\nReward: " + mission.RewardCoins + " coins  +" + mission.RewardAccountXP + " XP";

            if (complete && !claimed)
            {
                var claimBtn = CreateButton(card.transform, "Claim!", Vector2.zero, ClaimWeeklyMission);
                var claimRect = claimBtn.GetComponent<RectTransform>();
                claimRect.anchorMin = new Vector2(0.7f, 0.15f);
                claimRect.anchorMax = new Vector2(0.97f, 0.85f);
                claimRect.offsetMin = Vector2.zero;
                claimRect.offsetMax = Vector2.zero;
                claimRect.sizeDelta = Vector2.zero;
                AccentButton(claimBtn, NeonUITheme.Amber);
                claimBtn.GetComponentInChildren<Text>().fontSize = 30;
            }
        }

        private void ClaimWeeklyMission()
        {
            var mission = _profile.Meta.WeeklyMission;
            if (mission == null || mission.Claimed || mission.Progress < mission.Target) return;

            mission.Claimed = true;
            _profile.PlayerCoins += mission.RewardCoins;
            _profile.Meta.AccountXP += mission.RewardAccountXP;

            while (_profile.Meta.AccountXP >= AccountXpPerLevel(_profile.Meta.AccountLevel))
            {
                _profile.Meta.AccountXP -= AccountXpPerLevel(_profile.Meta.AccountLevel);
                _profile.Meta.AccountLevel += 1;
                _profile.PlayerCoins += 30 * _profile.Meta.AccountLevel;
                GrantMilestoneIfDue(_profile);
            }

            NeonSaveService.Save(_profile);
            RebuildMissionCards();
            UpdateGaragePanel();
        }

        private string FormatEquippedSlot(NeonEquipmentSlot slot)
        {
            var itemId = GetEquippedItemId(slot);
            var ownedItem = FindOwnedItem(itemId);
            var definition = FindEquipmentDef(itemId);
            if (ownedItem == null || definition == null)
            {
                return slot + ": Empty";
            }

            return slot + ": " + definition.Name + " Lv " + ownedItem.Level + " " + ownedItem.Rarity;
        }

        private NeonOwnedEquipmentItem FindOwnedItem(string itemId)
        {
            for (var index = 0; index < _profile.OwnedEquipmentItems.Count; index++)
            {
                var item = _profile.OwnedEquipmentItems[index];
                if (item.ItemID == itemId)
                {
                    return item;
                }
            }

            return null!;
        }

        private NeonEquipmentItemDef FindEquipmentDef(string itemId)
        {
            for (var index = 0; index < _catalog.Equipment.Count; index++)
            {
                var item = _catalog.Equipment[index];
                if (item.ItemID == itemId)
                {
                    return item;
                }
            }

            return null!;
        }

        private string GetEquippedItemId(NeonEquipmentSlot slot)
        {
            switch (slot)
            {
                case NeonEquipmentSlot.Weapon:
                    return _profile.EquippedWeaponItemID;
                case NeonEquipmentSlot.Wings:
                    return _profile.EquippedWingsItemID;
                case NeonEquipmentSlot.Engine:
                    return _profile.EquippedEngineItemID;
                case NeonEquipmentSlot.Hull:
                    return _profile.EquippedHullItemID;
                case NeonEquipmentSlot.Core:
                    return _profile.EquippedCoreItemID;
                case NeonEquipmentSlot.Radar:
                    return _profile.EquippedRadarItemID;
                default:
                    return string.Empty;
            }
        }

        private void HideRuntimeViews()
        {
            HideAll(_enemyViews);
            HideAll(_projectileViews);
            HideAll(_xpViews);
            HideAll(_orbitViews);
            HideAll(_chestViews);
            for (var index = 0; index < _trailViews.Count; index++)
            {
                _trailViews[index].gameObject.SetActive(false);
            }

            for (var index = 0; index < _zapViews.Count; index++)
            {
                _zapViews[index].gameObject.SetActive(false);
            }

            if (_playerRoot != null)
            {
                _playerRoot.gameObject.SetActive(false);
            }

            for (var index = 0; index < _particles.Count; index++)
            {
                _particles[index].Life = 0f;
                _particles[index].Transform.gameObject.SetActive(false);
            }
        }

        private static void HideAll(List<SpriteRenderer> views)
        {
            for (var index = 0; index < views.Count; index++)
            {
                views[index].gameObject.SetActive(false);
            }
        }

        private void CreateAmbientLayer()
        {
            var types = new[]
            {
                NeonEnemyBehaviorType.Chaser,
                NeonEnemyBehaviorType.Shooter,
                NeonEnemyBehaviorType.FastChaser,
                NeonEnemyBehaviorType.MineCarrier,
                NeonEnemyBehaviorType.Chaser,
                NeonEnemyBehaviorType.Splitter,
            };
            var alphas  = new[] { 0.28f, 0.22f, 0.25f, 0.20f, 0.18f, 0.22f };
            var scales  = new[] { 0.55f, 0.60f, 0.48f, 0.65f, 0.42f, 0.52f };
            var rng = new System.Random(7337);
            for (var i = 0; i < AmbientShipCount; i++)
            {
                var go = new GameObject("Ambient Ship " + i);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = NeonSpriteFactory.GetEnemy(types[i]);
                var c = ResolveEnemyColor(types[i]);
                sr.color = new Color(c.r, c.g, c.b, alphas[i]);
                sr.sortingOrder = 1;
                go.transform.localScale = Vector3.one * scales[i];
                _ambientViews.Add(sr);

                _ambientPositions[i] = new Vector2(
                    (float)(rng.NextDouble() * 2.0 - 1.0) * ArenaHalfWidth  * 0.85f,
                    (float)(rng.NextDouble() * 2.0 - 1.0) * ArenaHalfHeight * 0.85f
                );
                var angle = (float)(rng.NextDouble() * System.Math.PI * 2.0);
                var speed = 0.32f + (float)(rng.NextDouble() * 0.28f);
                _ambientVelocities[i] = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                _ambientSpinRates[i]  = (i % 2 == 0 ? 1f : -1f) * (18f + (float)(rng.NextDouble() * 22f));
                _ambientAngles[i]     = (float)(rng.NextDouble() * 360.0);
                go.SetActive(false);
            }
        }

        private void ShowAmbient()
        {
            if (_ambientActive) return;
            _ambientActive = true;
            _playerRoot.gameObject.SetActive(true);
            var dimCyan = new Color(0.25f, 0.85f, 1f, 0.36f);
            _playerBody.color      = dimCyan;
            _playerNose.color      = new Color(0.7f, 1f, 1f, 0.36f);
            _playerWingLeft.color  = new Color(0.45f, 0.6f, 1f, 0.36f);
            _playerWingRight.color = _playerWingLeft.color;
            for (var i = 0; i < _ambientViews.Count; i++)
            {
                _ambientViews[i].gameObject.SetActive(true);
            }
        }

        private void HideAmbient()
        {
            if (!_ambientActive) return;
            _ambientActive = false;
            _playerRoot.gameObject.SetActive(false);
            for (var i = 0; i < _ambientViews.Count; i++)
            {
                _ambientViews[i].gameObject.SetActive(false);
            }
        }

        private void TickAmbient(float dt)
        {
            // Player traces a slow figure-8 (Lissajous: x=sin(t), y=sin(2t)*0.5)
            _ambientPhase += dt * 0.28f;
            var px = Mathf.Sin(_ambientPhase) * ArenaHalfWidth * 0.55f;
            var py = Mathf.Sin(_ambientPhase * 2f) * ArenaHalfHeight * 0.28f;
            _playerRoot.position = new Vector3(px, py, 0f);
            var vx = Mathf.Cos(_ambientPhase)         * ArenaHalfWidth  * 0.55f;
            var vy = Mathf.Cos(_ambientPhase * 2f) * 2f * ArenaHalfHeight * 0.28f;
            if (vx * vx + vy * vy > 0.001f)
            {
                _playerRoot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vx, vy) * Mathf.Rad2Deg);
            }

            for (var i = 0; i < _ambientViews.Count; i++)
            {
                var pos = _ambientPositions[i];
                pos.x += _ambientVelocities[i].x * dt;
                pos.y += _ambientVelocities[i].y * dt;

                const float mx = ArenaHalfWidth  + 0.8f;
                const float my = ArenaHalfHeight + 0.8f;
                if (pos.x >  mx) pos.x = -mx;
                if (pos.x < -mx) pos.x =  mx;
                if (pos.y >  my) pos.y = -my;
                if (pos.y < -my) pos.y =  my;

                _ambientPositions[i] = pos;
                _ambientAngles[i] += _ambientSpinRates[i] * dt;
                _ambientViews[i].transform.position = new Vector3(pos.x, pos.y, 0f);
                _ambientViews[i].transform.rotation = Quaternion.Euler(0f, 0f, _ambientAngles[i]);
            }
        }

        private static Vector3 ToWorld(NeonVector2 position)
        {
            return new Vector3(position.X * ArenaHalfWidth, position.Y * ArenaHalfHeight, 0f);
        }

        private static float ResolveEnemySize(NeonRunEnemyState enemy)
        {
            if (enemy.IsBoss)
            {
                return enemy.IsMiniBoss ? 0.48f : 0.78f;
            }

            return 0.22f;
        }

        private static string FormatTime(float seconds)
        {
            var clamped = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (clamped / 60).ToString("00") + ":" + (clamped % 60).ToString("00");
        }

        private void CreateMainMenuPanel(Transform parent)
        {
            _mainMenuPanel = new GameObject("Main Menu", typeof(RectTransform), typeof(Image));
            _mainMenuPanel.transform.SetParent(parent, false);
            var rect = _mainMenuPanel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            // Translucent so the live neon grid/starfield reads through, per the design.
            _mainMenuPanel.GetComponent<Image>().color = NeonUITheme.Alpha(NeonUITheme.Bg, 0.82f);

            // Top resource counters (coins left, account rank right).
            _menuCoinsChip = CreateChip(_mainMenuPanel.transform, "Coins Chip", "◎ 0", new Vector2(20f, -20f), new Vector2(0f, 1f), NeonUITheme.Legendary);
            _menuRankChip = CreateChip(_mainMenuPanel.transform, "Rank Chip", "LV 1", new Vector2(-20f, -20f), new Vector2(1f, 1f), NeonUITheme.Cyan, 130f);

            var kicker = CreateText(_mainMenuPanel.transform, "Menu Kicker", new Vector2(0f, -96f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 22, NeonUITheme.Cyan);
            kicker.text = "ENDLESS · WAVE 0";

            var titleNeon = CreateDisplayText(_mainMenuPanel.transform, "Menu Title", new Vector2(0f, -120f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 78, NeonUITheme.TextCyan);
            titleNeon.text = "NEON SKY";
            AddTextGlow(titleNeon, NeonUITheme.Cyan, 3f);

            var titleSub = CreateText(_mainMenuPanel.transform, "Menu Title Sub", new Vector2(0f, -218f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 34, NeonUITheme.TextDim);
            titleSub.text = "S U R V I V O R S";
            titleSub.font = NeonUITheme.UiBold;

            // Plane hero with a cyan→purple trail behind it.
            var trail = new GameObject("Hero Trail", typeof(RectTransform), typeof(Image));
            trail.transform.SetParent(_mainMenuPanel.transform, false);
            var trailRect = trail.GetComponent<RectTransform>();
            trailRect.anchorMin = trailRect.anchorMax = new Vector2(0.5f, 0.5f);
            trailRect.anchoredPosition = new Vector2(0f, -70f);
            trailRect.sizeDelta = new Vector2(16f, 180f);
            var trailImg = trail.GetComponent<Image>();
            trailImg.sprite = NeonSpriteFactory.Blank;
            trailImg.color = NeonUITheme.Alpha(NeonUITheme.Purple, 0.5f);
            trailImg.raycastTarget = false;
            CreatePlaneHero(_mainMenuPanel.transform, new Vector2(0f, 10f), 230f, NeonUITheme.Cyan);

            _mainMenuStatsText = CreateText(_mainMenuPanel.transform, "Menu Stats", new Vector2(0f, 150f), new Vector2(0f, 0f), new Vector2(1f, 0f), TextAnchor.LowerCenter, 26, NeonUITheme.TextMute);

            // Daily-ops strip: today's three missions as small status cards above PLAY.
            var dailyKicker = CreateText(_mainMenuPanel.transform, "Daily Kicker", new Vector2(0f, 642f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), TextAnchor.MiddleCenter, 20, NeonUITheme.TextMute);
            dailyKicker.rectTransform.sizeDelta = new Vector2(400f, 32f);
            dailyKicker.text = "— DAILY OPS —";

            for (var missionIndex = 0; missionIndex < 3; missionIndex++)
            {
                var cell = new GameObject("Daily Mission " + missionIndex, typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(_mainMenuPanel.transform, false);
                var cellRect = cell.GetComponent<RectTransform>();
                cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 0f);
                cellRect.anchoredPosition = new Vector2((missionIndex - 1) * 345f, 565f);
                cellRect.sizeDelta = new Vector2(330f, 92f);
                cell.GetComponent<Image>().color = NeonUITheme.Alpha(NeonUITheme.Bg1, 0.85f);
                StylePanelCut(cell, NeonUITheme.Line2);
                _menuMissionBorders[missionIndex] = cell.transform.Find("Border").GetComponent<NeonCutRect>();

                var label = CreateText(cell.transform, "Mission Label", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 21, NeonUITheme.Text);
                label.rectTransform.offsetMin = new Vector2(8f, 6f);
                label.rectTransform.offsetMax = new Vector2(-8f, -6f);
                _menuMissionTexts[missionIndex] = label;
            }

            var playButton = CreateButton(_mainMenuPanel.transform, "PLAY", new Vector2(0f, 400f), ShowGarage);
            playButton.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 130f);
            PrimaryButton(playButton);
            playButton.GetComponentInChildren<Text>().fontSize = 42;

            var settingsButton = CreateButton(_mainMenuPanel.transform, "SETTINGS", new Vector2(0f, 240f), () => ShowSettings(true));
            settingsButton.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 110f);
            GhostButton(settingsButton);

            _mainMenuPanel.SetActive(false);
        }

        private void CreateSettingsPanel(Transform parent)
        {
            _settingsPanel = new GameObject("Settings", typeof(RectTransform), typeof(Image));
            _settingsPanel.transform.SetParent(parent, false);
            var rect = _settingsPanel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _settingsPanel.GetComponent<Image>().color = new Color(0.01f, 0.025f, 0.055f, 0.98f);

            var titleText = CreateDisplayText(_settingsPanel.transform, "Settings Title", new Vector2(0f, -80f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 40, NeonUITheme.TextCyan);
            AddTextGlow(titleText, NeonUITheme.Cyan);
            titleText.text = "SETTINGS";

            // Music volume row — 10% steps for finer control
            _settingsMusicText = CreateText(_settingsPanel.transform, "Music Label", new Vector2(0f, 260f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 34, Color.white);
            _settingsMusicText.rectTransform.sizeDelta = new Vector2(500f, 70f);

            CreateSmallButton(_settingsPanel.transform, "-", new Vector2(-260f, 260f), () => AdjustMusicVolume(-0.1f));
            CreateSmallButton(_settingsPanel.transform, "+", new Vector2(260f, 260f), () => AdjustMusicVolume(0.1f));

            // SFX volume row — 10% steps
            _settingsSfxText = CreateText(_settingsPanel.transform, "SFX Label", new Vector2(0f, 140f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 34, Color.white);
            _settingsSfxText.rectTransform.sizeDelta = new Vector2(500f, 70f);

            CreateSmallButton(_settingsPanel.transform, "-", new Vector2(-260f, 140f), () => AdjustSfxVolume(-0.1f));
            CreateSmallButton(_settingsPanel.transform, "+", new Vector2(260f, 140f), () => AdjustSfxVolume(0.1f));

            // Vibration row
            _settingsVibrationText = CreateText(_settingsPanel.transform, "Vibration Label", new Vector2(0f, 30f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 34, Color.white);
            _settingsVibrationText.rectTransform.sizeDelta = new Vector2(500f, 70f);
            _settingsVibrationText.rectTransform.anchoredPosition = new Vector2(0f, 30f);

            CreateSmallButton(_settingsPanel.transform, "Toggle", new Vector2(0f, -40f), ToggleVibration)
                .GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 80f);

            // Dash mode row (button vs. double-tap)
            _settingsDashModeText = CreateText(_settingsPanel.transform, "Dash Mode Label", new Vector2(0f, -150f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 34, Color.white);
            _settingsDashModeText.rectTransform.sizeDelta = new Vector2(560f, 70f);
            _settingsDashModeText.rectTransform.anchoredPosition = new Vector2(0f, -150f);

            CreateSmallButton(_settingsPanel.transform, "Toggle", new Vector2(0f, -220f), ToggleDashMode)
                .GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 80f);

            // Reduced Motion row — disables screen shake and particle bursts
            _settingsReducedMotionText = CreateText(_settingsPanel.transform, "Reduced Motion Label", new Vector2(0f, -330f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 34, Color.white);
            _settingsReducedMotionText.rectTransform.sizeDelta = new Vector2(580f, 70f);
            _settingsReducedMotionText.rectTransform.anchoredPosition = new Vector2(0f, -330f);

            CreateSmallButton(_settingsPanel.transform, "Toggle", new Vector2(0f, -400f), ToggleReducedMotion)
                .GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 80f);

            var backButton = CreateButton(_settingsPanel.transform, "Back", new Vector2(0f, -510f), HideSettings);
            backButton.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 110f);
            GhostButton(backButton);

            _settingsPanel.SetActive(false);
        }

        private Button CreateSmallButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label + " Btn", typeof(RectTransform), typeof(NeonCutRect), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(130f, 90f);
            var cut = buttonObject.GetComponent<NeonCutRect>();
            cut.CutSize = 8f;
            cut.BorderThickness = 1.5f;
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = cut;
            button.onClick.AddListener(action);
            StyleButtonColors(button, NeonUITheme.Mix(NeonUITheme.Cyan, 0.14f, NeonUITheme.Bg2), NeonUITheme.Mix(NeonUITheme.Cyan, 0.6f, NeonUITheme.Line2), NeonUITheme.Text);
            var text = CreateText(buttonObject.transform, label + " Lbl", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 36, NeonUITheme.Text);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.text = label;
            return button;
        }

        private void AdjustMusicVolume(float delta)
        {
            _profile.MusicVolume = Mathf.Clamp01(Mathf.Round((_profile.MusicVolume + delta) * 10f) / 10f);
            ApplyAudioSettings();
            NeonSaveService.Save(_profile);
            UpdateSettingsPanel();
        }

        private void AdjustSfxVolume(float delta)
        {
            _profile.SfxVolume = Mathf.Clamp01(Mathf.Round((_profile.SfxVolume + delta) * 10f) / 10f);
            ApplyAudioSettings();
            NeonSaveService.Save(_profile);
            UpdateSettingsPanel();
        }

        private void ToggleReducedMotion()
        {
            _profile.ReducedMotionEnabled = !_profile.ReducedMotionEnabled;
            NeonSaveService.Save(_profile);
            UpdateSettingsPanel();
        }

        private void ToggleVibration()
        {
            _profile.VibrationEnabled = !_profile.VibrationEnabled;
            NeonSaveService.Save(_profile);
            UpdateSettingsPanel();
        }

        private void ToggleDashMode()
        {
            _profile.DoubleTapDashEnabled = !_profile.DoubleTapDashEnabled;
            _lastTapTime = -1f;
            NeonSaveService.Save(_profile);
            UpdateSettingsPanel();
        }

        private void CreatePauseMenuPanel(Transform parent)
        {
            _pauseMenuPanel = new GameObject("Pause Menu", typeof(RectTransform), typeof(Image));
            _pauseMenuPanel.transform.SetParent(parent, false);
            var rect = _pauseMenuPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(860f, 680f);
            _pauseMenuPanel.GetComponent<Image>().color = new Color(0.01f, 0.03f, 0.07f, 0.97f);
            StylePanelCut(_pauseMenuPanel, NeonUITheme.Mix(NeonUITheme.Cyan, 0.5f, NeonUITheme.Line2), 2f);

            var title = CreateDisplayText(_pauseMenuPanel.transform, "Pause Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 44, NeonUITheme.TextCyan);
            title.text = "PAUSED";
            AddTextGlow(title, NeonUITheme.Cyan, 3f);

            // 2×3 run-snapshot stat grid in the empty middle band of the card.
            var pauseLabels = new[] { "TIME", "KILLS", "LEVEL", "COINS", "BOSSES", "HULL" };
            var pauseAccents = new[] { NeonUITheme.Cyan, NeonUITheme.Magenta, NeonUITheme.Amber, NeonUITheme.Legendary, NeonUITheme.Purple, NeonUITheme.Uncommon };
            for (var statIndex = 0; statIndex < 6; statIndex++)
            {
                var col = statIndex % 3;
                var row = statIndex / 3;
                var center = new Vector2((col - 1) * 285f, row == 0 ? 90f : -45f);
                _pauseStatValues[statIndex] = CreateStatCell(_pauseMenuPanel.transform, center, new Vector2(260f, 120f), pauseLabels[statIndex], pauseAccents[statIndex]);
            }

            var resumeButton = CreateButton(_pauseMenuPanel.transform, "RESUME", new Vector2(0f, 150f), ResumePausedRun);
            resumeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 120f);
            PrimaryButton(resumeButton);
            resumeButton.GetComponentInChildren<Text>().fontSize = 38;

            var restartButton = CreateButton(_pauseMenuPanel.transform, "RESTART", new Vector2(0f, 0f), RestartRun);
            restartButton.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 110f);
            AccentButton(restartButton, NeonUITheme.Amber);
            restartButton.GetComponentInChildren<Text>().fontSize = 36;

            var quitButton = CreateButton(_pauseMenuPanel.transform, "QUIT TO GARAGE", new Vector2(0f, -140f), ReturnToGarage);
            quitButton.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 110f);
            DangerButton(quitButton);
            quitButton.GetComponentInChildren<Text>().fontSize = 32;

            _pauseMenuPanel.SetActive(false);
        }

        // ── Screen shake & hit-stop ───────────────────────────────────────────

        private void TriggerScreenShake(float amplitude, float duration)
        {
            _shakeAmplitude = Mathf.Max(_shakeAmplitude, amplitude);
            _shakeRemaining = Mathf.Max(_shakeRemaining, duration);
        }

        private void TriggerHitStop(float duration)
        {
            if (_profile.ReducedMotionEnabled) return;
            if (duration > _hitStopRemaining)
            {
                _hitStopRemaining = duration;
                Time.timeScale = 0.05f;
            }
        }

        private void UpdateScreenShake(float deltaTime)
        {
            if (_shakeRemaining <= 0f || _profile.ReducedMotionEnabled)
            {
                _camera.transform.position = _cameraBasePosition;
                _shakeAmplitude = 0f;
                _shakeRemaining = 0f;
                return;
            }

            _shakeRemaining -= deltaTime;
            var decay = _shakeRemaining > 0f ? _shakeRemaining : 0f;
            var magnitude = _shakeAmplitude * decay;
            var offset = new Vector3(
                UnityEngine.Random.Range(-magnitude, magnitude),
                UnityEngine.Random.Range(-magnitude, magnitude),
                0f);
            _camera.transform.position = _cameraBasePosition + offset;
            if (_shakeRemaining <= 0f) _shakeAmplitude = 0f;
        }

        // ── Boss telegraph circles ─────────────────────────────────────────────

        private void CreateTelegraphCirclePool()
        {
            var root = new GameObject("Boss Telegraphs");
            var mat = new Material(Shader.Find("Sprites/Default"));
            for (var index = 0; index < MaxBossTelegraphCircles; index++)
            {
                var obj = new GameObject("Telegraph " + index);
                obj.transform.SetParent(root.transform, false);
                var line = obj.AddComponent<LineRenderer>();
                line.material = mat;
                line.useWorldSpace = true;
                line.loop = true;
                line.positionCount = 32;
                line.startWidth = 0.06f;
                line.endWidth = 0.06f;
                line.sortingOrder = 3;
                obj.SetActive(false);
                _telegraphCircles.Add(line);
                _telegraphRadii.Add(0f);
            }
        }

        private void RenderBossTelegraphs()
        {
            // Hide all first.
            for (var index = 0; index < _telegraphCircles.Count; index++)
            {
                _telegraphCircles[index].gameObject.SetActive(false);
            }

            if (_run == null || (_run.Status != NeonRunStatus.Running && _run.Status != NeonRunStatus.LevelUpDraft))
            {
                return;
            }

            var circleIndex = 0;
            for (var enemyIndex = 0; enemyIndex < _run.Enemies.Count && circleIndex < _telegraphCircles.Count; enemyIndex++)
            {
                var enemy = _run.Enemies[enemyIndex];
                if (!enemy.IsBoss) continue;

                // Show an attack-ready telegraph ring as the cooldown nears zero.
                var readiness = enemy.AttackCooldownRemaining;
                if (readiness > 0.9f || readiness < 0f) continue; // not about to fire

                var t = 1f - Mathf.Clamp01(readiness / 0.9f); // 0→1 as attack approaches
                var radius = (enemy.IsMiniBoss ? 1.2f : 1.8f) * (0.5f + 0.5f * t);
                var alpha = t * 0.75f;
                var col = enemy.IsMiniBoss
                    ? new Color(1f, 0.65f, 0.15f, alpha)
                    : new Color(0.9f, 0.15f, 0.75f, alpha);

                DrawTelegraphCircle(_telegraphCircles[circleIndex], ToWorld(enemy.Position), radius, col);
                _telegraphCircles[circleIndex].gameObject.SetActive(true);
                circleIndex++;
            }
        }

        private static void DrawTelegraphCircle(LineRenderer line, Vector3 center, float radius, Color color)
        {
            const int segments = 32;
            line.positionCount = segments;
            line.startColor = color;
            line.endColor = color;
            for (var i = 0; i < segments; i++)
            {
                var angle = i / (float)segments * Mathf.PI * 2f;
                line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
        }

        private struct EnemyDeathSnapshot
        {
            public NeonVector2 Position;
            public bool IsBoss;
            public bool IsMiniBoss;
        }

        private sealed class NeonParticleView
        {
            public Transform Transform = null!;
            public SpriteRenderer Renderer = null!;
            public Vector3 Velocity;
            public float Life;
            public float MaxLife;
            public Color BaseColor;
            public float BaseSize;
        }
    }
}
