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
        private const int MaxTrailViews = 24;
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
        private readonly List<SpriteRenderer> _orbitViews = new List<SpriteRenderer>();
        private readonly List<LineRenderer> _trailViews = new List<LineRenderer>();
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
        private Text _hudText = null!;
        private Text _messageText = null!;
        private Text _statusText = null!;
        private Image _hpBarFill = null!;
        private Image _xpBarFill = null!;
        private Button _dashButton = null!;
        private Button _specialButton = null!;
        private Image _specialFill = null!;
        private Button _pauseButton = null!;
        private Text _pauseLabel = null!;
        private GameObject _bossBarRoot = null!;
        private Image _bossBarFill = null!;
        private Text _bossBarText = null!;
        private GameObject _garagePanel = null!;
        private Text _garageTitleText = null!;
        private Text _garageStatsText = null!;
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
        private GameObject _resultsPanel = null!;
        private Text _resultsTitleText = null!;
        private Text _resultsStatsText = null!;
        private GameObject _upgradePanel = null!;
        private readonly List<Image> _upgradeButtonIcons = new List<Image>();
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
        private bool _settingsFromMainMenu;
        private GameObject _pauseMenuPanel = null!;
        private readonly List<string> _lastRewardItemList = new List<string>();
        private GameObject _missionsPanel = null!;
        private RectTransform _missionsContent = null!;

        private NeonRunStatus _prevStatus;
        private int _prevEnemiesKilled;
        private int _prevPlayerProjectiles;
        private int _prevBossCount;
        private float _prevHP;
        private float _prevXP;
        private string _prevWarning = string.Empty;
        private float _xpSoundCooldown;
        private float _damageSoundCooldown;

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

            var dt = Mathf.Min(Time.deltaTime, 0.05f);

            if (!_paused && _run.Status == NeonRunStatus.Running)
            {
                CapturePreTickEnemies();
                _gameplay.Tick(_run, _catalog, dt);
                SpawnDeathBursts();
            }

            RenderRun();
            UpdateNeonBackground(dt);
            UpdateParticles(dt);
            UpdateScreenShake(dt);
            UpdateAudio(dt);
            UpdateHud();
        }

        private void StartRun()
        {
            _run = _gameplay.StartRun(_profile, _catalog);
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
            SetRunHudVisible(true);
            _statusText.text = string.Empty;
            _messageText.text = "Survive 10 minutes. Bosses at 3:00, 6:00, 7:30, 8:45, 10:00.";
            UpdateUpgradeChoices(false);
            ResetAudioTrackers();
            _audio.SetMusic("normal");
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
            _garagePanel.SetActive(true);
            UpdateGaragePanel();
            _audio.StopMusic();
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
            _mainMenuPanel.SetActive(true);
            UpdateMainMenuPanel();
            _audio.StopMusic();
        }

        private void UpdateMainMenuPanel()
        {
            _mainMenuStatsText.text = "Best Run: " + FormatTime(_profile.BestSurvivalTime)
                + "   Runs: " + _profile.CompletedRuns
                + "   Coins: " + _profile.PlayerCoins;
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
            RenderTrails();
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

        private void UpdateHud()
        {
            var player = _run.Player;
            var hpPercent = Mathf.Clamp01(player.Stats.CurrentHP / player.Stats.MaxHP);
            var xpPercent = Mathf.Clamp01(player.XP / player.XPToNextLevel);
            _hpBarFill.fillAmount = hpPercent;
            _hpBarFill.color = Color.Lerp(new Color(1f, 0.28f, 0.3f), new Color(0.3f, 1f, 0.6f), hpPercent);
            _xpBarFill.fillAmount = xpPercent;
            _hudText.text = FormatTime(_run.ElapsedSeconds) + "   Lv " + player.Level + "\n"
                + "HP " + Mathf.CeilToInt(player.Stats.CurrentHP) + "/" + Mathf.CeilToInt(player.Stats.MaxHP) + "   Coins " + player.CoinsCollected + "\n"
                + "Dash " + (player.DashCooldownRemaining <= 0f ? "READY" : player.DashCooldownRemaining.ToString("0.0"));

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
            _specialFill.fillAmount = Mathf.Clamp01(player.SpecialCharge / player.SpecialChargeMax);
            _specialFill.color = specialReady ? new Color(0.4f, 1f, 1f, 0.95f) : new Color(1f, 0.3f, 0.85f, 0.7f);
            _specialButton.interactable = specialReady && !_paused && _run.Status == NeonRunStatus.Running;
            _dashButton.interactable = !_paused && _run.Status == NeonRunStatus.Running && player.DashCooldownRemaining <= 0f;
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
                _resultApplied = true;
                UpdateMissionProgressFromRun(_run);
                NeonSaveService.Save(_profile);
            }

            SetRunHudVisible(false);
            _resultsPanel.SetActive(true);
            _resultsTitleText.text = title;
            var itemsText = _lastRewardItemList.Count > 0
                ? "\nItems Found:\n" + string.Join("\n", _lastRewardItemList)
                : "\nNo items dropped.";
            _resultsStatsText.text = "Time " + FormatTime(_run.ElapsedSeconds) + "  Best " + FormatTime(_profile.BestSurvivalTime) + "\n"
                + "Kills " + _run.EnemiesKilled + "  Bosses " + (_run.BossesKilled + _run.MiniBossesKilled) + "\n"
                + "Coins +" + _lastRewardCoins + "  Total " + _profile.PlayerCoins + "\n"
                + "Runs " + _profile.CompletedRuns
                + itemsText;
        }

        private int GrantRunRewardItems(NeonRunState run)
        {
            _lastRewardItemList.Clear();
            var dropped = 0;

            for (var index = 0; index < run.MiniBossesKilled; index++)
            {
                if (UnityEngine.Random.value < 0.6f)
                {
                    var rarity = UnityEngine.Random.value < 0.5f ? NeonEquipmentRarity.Common : NeonEquipmentRarity.Uncommon;
                    var item = GrantRandomItemTracked(rarity);
                    if (item != null) { _lastRewardItemList.Add(item + " [" + rarity + "]"); dropped++; }
                }
            }

            for (var index = 0; index < run.BossesKilled; index++)
            {
                if (UnityEngine.Random.value < 0.7f)
                {
                    var rarity = UnityEngine.Random.value < 0.5f ? NeonEquipmentRarity.Uncommon : NeonEquipmentRarity.Rare;
                    var item = GrantRandomItemTracked(rarity);
                    if (item != null) { _lastRewardItemList.Add(item + " [" + rarity + "]"); dropped++; }
                }
            }

            if (run.Status == NeonRunStatus.Victory)
            {
                var rarity = UnityEngine.Random.value < 0.25f ? NeonEquipmentRarity.Epic : NeonEquipmentRarity.Rare;
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
                button.GetComponent<Image>().color = new Color(categoryColor.r * 0.28f, categoryColor.g * 0.28f, categoryColor.b * 0.28f, 0.98f);

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

            foreach (var pair in _enemySnapshots)
            {
                if (_enemyAfterTick.Contains(pair.Key))
                {
                    continue;
                }

                var snapshot = pair.Value;
                if (snapshot.IsBoss && !snapshot.IsMiniBoss)
                {
                    SpawnBurst(snapshot.Position, new Color(1f, 0.3f, 0.85f, 0.95f), 22, 4.2f, 0.16f, 0.6f);
                }
                else if (snapshot.IsMiniBoss)
                {
                    SpawnBurst(snapshot.Position, new Color(1f, 0.65f, 0.2f, 0.95f), 16, 3.6f, 0.13f, 0.5f);
                }
                else
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

            _hpBarFill = CreateBar(safeArea, "HP Bar", -44f, 26f, new Color(0.3f, 1f, 0.6f));
            _xpBarFill = CreateBar(safeArea, "XP Bar", -76f, 16f, new Color(0.23f, 1f, 0.78f));
            _hudText = CreateText(safeArea, "HUD", new Vector2(32f, -100f), new Vector2(0f, 1f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 34, new Color(0.75f, 1f, 1f));
            _messageText = CreateText(safeArea, "Message", new Vector2(0f, -205f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 34, new Color(1f, 0.82f, 0.28f));
            _statusText = CreateText(canvasObject.transform, "Status", new Vector2(0f, 0f), new Vector2(0f, 0.48f), new Vector2(1f, 0.48f), TextAnchor.MiddleCenter, 42, Color.white);
            _dashButton = CreateButton(safeArea, "Dash", new Vector2(-210f, 150f), TryDash);
            CreateSpecialButton(safeArea);
            CreatePauseButton(safeArea);
            CreateBossBar(safeArea);
            CreateUpgradePanel(canvasObject.transform);
            CreateGaragePanel(canvasObject.transform);
            CreateResultsPanel(canvasObject.transform);
            CreateMainMenuPanel(canvasObject.transform);
            CreateSettingsPanel(canvasObject.transform);
            CreatePauseMenuPanel(canvasObject.transform);
            CreateMissionsPanel(canvasObject.transform);
        }

        private void CreateSpecialButton(Transform parent)
        {
            _specialButton = CreateButton(parent, "SPECIAL", new Vector2(210f, 150f), ActivateSpecial);
            _specialButton.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.18f, 0.92f);

            // Charge fill behind the label acts as the "Special ability charge" indicator.
            var fillObject = new GameObject("Special Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(_specialButton.transform, false);
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);
            fillRect.pivot = new Vector2(0f, 0.5f);

            _specialFill = fillObject.GetComponent<Image>();
            _specialFill.sprite = _sprite;
            _specialFill.type = Image.Type.Filled;
            _specialFill.fillMethod = Image.FillMethod.Horizontal;
            _specialFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _specialFill.fillAmount = 0f;
            _specialFill.color = new Color(1f, 0.3f, 0.85f, 0.85f);
            fillObject.transform.SetSiblingIndex(0); // render behind the label
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
            _pauseButton = CreateButton(parent, "Pause", new Vector2(0f, 0f), ShowPauseMenu);
            var rect = _pauseButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -28f);
            rect.sizeDelta = new Vector2(150f, 96f);
            _pauseButton.GetComponent<Image>().color = new Color(0.06f, 0.16f, 0.24f, 0.92f);
            _pauseLabel = _pauseButton.GetComponentInChildren<Text>();
            _pauseLabel.text = "II";
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
            rootRect.sizeDelta = new Vector2(760f, 44f);
            _bossBarRoot.GetComponent<Image>().color = new Color(0.05f, 0.02f, 0.08f, 0.85f);

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

            _garageTitleText = CreateText(_garagePanel.transform, "Garage Title", new Vector2(0f, -40f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 46, new Color(0.68f, 1f, 1f));
            _garageTitleText.rectTransform.sizeDelta = new Vector2(-80f, 120f);

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
            settingsBtn.GetComponent<Image>().color = new Color(0.08f, 0.18f, 0.28f, 0.92f);
            settingsBtn.GetComponentInChildren<Text>().fontSize = 26;

            var missionsBtn = CreateButton(_garagePanel.transform, "Missions", new Vector2(290f, 240f), ShowMissions);
            missionsBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(280f, 90f);
            missionsBtn.GetComponent<Image>().color = new Color(0.14f, 0.22f, 0.1f, 0.92f);
            missionsBtn.GetComponentInChildren<Text>().fontSize = 26;

            var backToMenuBtn = CreateButton(_garagePanel.transform, "< Menu", new Vector2(-290f, 240f), ShowMainMenu);
            backToMenuBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 90f);
            backToMenuBtn.GetComponent<Image>().color = new Color(0.08f, 0.14f, 0.22f, 0.92f);
            backToMenuBtn.GetComponentInChildren<Text>().fontSize = 24;

            var startRunButton = CreateButton(_garagePanel.transform, "Start Run", new Vector2(0f, 120f), StartRun);
            startRunButton.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 120f);
            startRunButton.GetComponent<Image>().color = new Color(0.02f, 0.42f, 0.48f, 0.96f);

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
            panel.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.1f, 0.6f);

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
                image.color = new Color(0.06f, 0.14f, 0.2f, 0.95f);
                btnObject.GetComponent<Button>().onClick.AddListener(() => FilterInventoryBySlot(capturedSlot));

                var label = CreateText(btnObject.transform, slot + " Lbl", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 20, Color.white);
                label.rectTransform.offsetMin = new Vector2(4f, 2f);
                label.rectTransform.offsetMax = new Vector2(-4f, -2f);

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

                var rarityColor = ownedItem != null ? ResolveRarityColor(ownedItem.Rarity) : new Color(0.4f, 0.45f, 0.5f);
                var bg = isFiltered
                    ? new Color(rarityColor.r * 0.45f + 0.08f, rarityColor.g * 0.45f + 0.08f, rarityColor.b * 0.45f + 0.08f, 0.98f)
                    : new Color(rarityColor.r * 0.15f + 0.03f, rarityColor.g * 0.15f + 0.03f, rarityColor.b * 0.15f + 0.03f, 0.92f);
                button.GetComponent<Image>().color = bg;

                var label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    var itemName = def != null ? TruncateName(def.Name, 10) : "Empty";
                    label.text = slot.ToString() + "\n" + itemName + (isFiltered ? " ▼" : "");
                    label.color = ownedItem != null ? rarityColor : new Color(0.5f, 0.55f, 0.6f);
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
            var buttonObject = new GameObject(label + " Action", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorMinX, anchorMinY - 0.05f);
            rect.anchorMax = new Vector2(anchorMaxX, anchorMinY);
            rect.offsetMin = new Vector2(6f, 6f);
            rect.offsetMax = new Vector2(-6f, -6f);

            buttonObject.GetComponent<Image>().color = new Color(0.06f, 0.16f, 0.24f, 0.95f);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);

            var text = CreateText(buttonObject.transform, label + " Label", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 26, Color.white);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.text = label;
            return buttonObject.GetComponent<Button>();
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

            _resultsTitleText = CreateText(_resultsPanel.transform, "Results Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 48, new Color(1f, 0.82f, 0.28f));
            _resultsStatsText = CreateText(_resultsPanel.transform, "Results Stats", new Vector2(0f, -190f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 34, Color.white);
            _resultsStatsText.rectTransform.sizeDelta = new Vector2(-120f, 420f);

            var garageButton = CreateButton(_resultsPanel.transform, "Garage", new Vector2(0f, 70f), ShowGarage);
            garageButton.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 112f);
            garageButton.GetComponent<Image>().color = new Color(0.02f, 0.42f, 0.48f, 0.96f);

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

            var title = CreateText(_upgradePanel.transform, "Upgrade Title", new Vector2(0f, -38f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 38, new Color(0.72f, 1f, 1f));
            title.text = "Choose upgrade";

            for (var index = 0; index < 3; index++)
            {
                var capturedIndex = index;
                var button = CreateButton(_upgradePanel.transform, "Upgrade " + (index + 1), new Vector2(0f, 430f - index * 170f), () => SelectUpgradeChoice(capturedIndex));
                var buttonRect = button.GetComponent<RectTransform>();
                buttonRect.sizeDelta = new Vector2(860f, 140f);

                var buttonImage = button.GetComponent<Image>();
                buttonImage.color = new Color(0.05f, 0.18f, 0.25f, 0.98f);

                var label = button.GetComponentInChildren<Text>();
                label.fontSize = 26;
                label.alignment = TextAnchor.MiddleLeft;
                label.rectTransform.offsetMin = new Vector2(64f, 8f);
                label.rectTransform.offsetMax = new Vector2(-8f, -8f);

                // Small category icon on the left side of each upgrade card
                var iconObj = new GameObject("Upgrade Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(button.transform, false);
                var iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(32f, 0f);
                iconRect.sizeDelta = new Vector2(40f, 40f);
                _upgradeButtonIcons.Add(iconObj.GetComponent<Image>());

                _upgradeButtons.Add(button);
            }

            _upgradePanel.SetActive(false);
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
            var buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(340f, 112f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.06f, 0.16f, 0.24f, 0.92f);

            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);

            var text = CreateText(buttonObject.transform, label + " Label", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 34, Color.white);
            text.rectTransform.sizeDelta = Vector2.zero;
            text.text = label;
            return button;
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
            _hudText.gameObject.SetActive(visible);
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
            }
        }

        private void UpdateGaragePanel()
        {
            var stats = _equipment.CalculateStats(_profile, _catalog);
            _garageTitleText.text = "NEON SKY SURVIVORS — GARAGE";
            _garageStatsText.text = "Coins " + _profile.PlayerCoins
                + "   Runs " + _profile.CompletedRuns
                + "   Best " + FormatTime(_profile.BestSurvivalTime)
                + "   Lv " + _profile.AccountLevel
                + "\nATK " + stats.AttackDamage.ToString("0")
                + "  Fire " + stats.FireRate.ToString("0.0")
                + "  Speed " + stats.MovementSpeed.ToString("0.0")
                + "  HP " + stats.MaxHP.ToString("0")
                + "  Armor " + stats.Armor.ToString("0")
                + "  Dash " + stats.DashCooldown.ToString("0.0") + "s";

            UpdateSlotButtons();
            RebuildInventoryCards();
            UpdateGarageActions();
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
            image.color = isSelected
                ? new Color(rarityColor.r * 0.5f + 0.1f, rarityColor.g * 0.5f + 0.1f, rarityColor.b * 0.5f + 0.1f, 0.98f)
                : new Color(rarityColor.r * 0.22f, rarityColor.g * 0.22f, rarityColor.b * 0.22f, 0.92f);

            var capturedId = owned.InstanceID;
            cardObject.GetComponent<Button>().onClick.AddListener(() => SelectInventoryItem(capturedId));

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
            var coins = rewards.BaseCoins
                + run.Player.CoinsCollected
                + run.EnemiesKilled * rewards.CoinPerKill
                + run.BossesKilled * rewards.BossCoinBonus
                + run.MiniBossesKilled * rewards.MiniBossCoinBonus
                + survivalMinutes * rewards.SurvivalMinuteCoins;

            return Mathf.Max(0, Mathf.RoundToInt(coins * run.Player.Stats.CoinBonus));
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

            CreateText(_missionsPanel.transform, "Missions Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 48, new Color(0.5f, 1f, 0.6f))
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
            backButton.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.95f);

            _missionsPanel.SetActive(false);
        }

        private void ShowMissions()
        {
            RefreshDailyMissions();
            _garagePanel.SetActive(false);
            _missionsPanel.SetActive(true);
            RebuildMissionCards();
        }

        private void HideMissions()
        {
            _missionsPanel.SetActive(false);
            ShowGarage();
        }

        private static readonly NeonActiveMission[] MissionTemplates =
        {
            new NeonActiveMission { MissionId = "kill30", Name = "Exterminator", Description = "Kill 30 enemies in one run", Type = NeonMissionType.KillEnemies, TargetCount = 30, RewardCoins = 40, RewardAccountXP = 20 },
            new NeonActiveMission { MissionId = "kill100", Name = "Slaughter", Description = "Kill 100 enemies in one run", Type = NeonMissionType.KillEnemies, TargetCount = 100, RewardCoins = 80, RewardAccountXP = 40 },
            new NeonActiveMission { MissionId = "survive3", Name = "Survivor", Description = "Survive 3 minutes", Type = NeonMissionType.SurviveMinutes, TargetCount = 3, RewardCoins = 30, RewardAccountXP = 15 },
            new NeonActiveMission { MissionId = "survive6", Name = "Veteran", Description = "Survive 6 minutes", Type = NeonMissionType.SurviveMinutes, TargetCount = 6, RewardCoins = 60, RewardAccountXP = 30 },
            new NeonActiveMission { MissionId = "boss1", Name = "Boss Hunter", Description = "Defeat a major boss", Type = NeonMissionType.DefeatBoss, TargetCount = 1, RewardCoins = 50, RewardAccountXP = 25 },
            new NeonActiveMission { MissionId = "complete1", Name = "Full Run", Description = "Complete a full 10-minute run", Type = NeonMissionType.CompleteRun, TargetCount = 1, RewardCoins = 100, RewardAccountXP = 50 },
        };

        private void RefreshDailyMissions()
        {
            const long TicksPerDay = 864000000000L;
            var now = System.DateTime.UtcNow.Ticks;
            var daysSinceEpoch = now / TicksPerDay;
            var lastReset = _profile.MissionLastResetTicks / TicksPerDay;

            if (_profile.ActiveMissions.Count == 3 && daysSinceEpoch == lastReset)
            {
                return; // still current day
            }

            // New day — pick 3 missions using the day number as a deterministic seed
            _profile.ActiveMissions.Clear();
            var rng = new System.Random((int)daysSinceEpoch);
            var indices = new System.Collections.Generic.List<int> { 0, 1, 2, 3, 4, 5 };
            for (var pick = 0; pick < 3 && indices.Count > 0; pick++)
            {
                var chosen = rng.Next(indices.Count);
                var template = MissionTemplates[indices[chosen]];
                indices.RemoveAt(chosen);
                _profile.ActiveMissions.Add(new NeonActiveMission
                {
                    MissionId = template.MissionId,
                    Name = template.Name,
                    Description = template.Description,
                    Type = template.Type,
                    TargetCount = template.TargetCount,
                    CurrentCount = 0,
                    Claimed = false,
                    RewardCoins = template.RewardCoins,
                    RewardAccountXP = template.RewardAccountXP
                });
            }

            _profile.MissionLastResetTicks = daysSinceEpoch * TicksPerDay;
            NeonSaveService.Save(_profile);
        }

        private void UpdateMissionProgressFromRun(NeonRunState run)
        {
            var changed = false;
            foreach (var mission in _profile.ActiveMissions)
            {
                if (mission.Claimed) continue;
                var prev = mission.CurrentCount;
                switch (mission.Type)
                {
                    case NeonMissionType.KillEnemies:
                        mission.CurrentCount = Mathf.Max(mission.CurrentCount, run.EnemiesKilled);
                        break;
                    case NeonMissionType.SurviveMinutes:
                        mission.CurrentCount = Mathf.Max(mission.CurrentCount, Mathf.FloorToInt(run.ElapsedSeconds / 60f));
                        break;
                    case NeonMissionType.DefeatBoss:
                        mission.CurrentCount = Mathf.Max(mission.CurrentCount, run.BossesKilled);
                        break;
                    case NeonMissionType.CompleteRun:
                        if (run.Status == NeonRunStatus.Victory)
                            mission.CurrentCount = Mathf.Max(mission.CurrentCount, 1);
                        break;
                }

                if (mission.CurrentCount != prev) changed = true;
            }

            if (changed) NeonSaveService.Save(_profile);
        }

        private void ClaimMission(int index)
        {
            if (index < 0 || index >= _profile.ActiveMissions.Count) return;
            var mission = _profile.ActiveMissions[index];
            if (mission.Claimed || mission.CurrentCount < mission.TargetCount) return;

            mission.Claimed = true;
            _profile.PlayerCoins += mission.RewardCoins;
            _profile.AccountXP += mission.RewardAccountXP;

            // Level up account if threshold reached: threshold = 100 * current level
            while (_profile.AccountXP >= 100 * _profile.AccountLevel)
            {
                _profile.AccountXP -= 100 * _profile.AccountLevel;
                _profile.AccountLevel += 1;
                _profile.PlayerCoins += 30 * _profile.AccountLevel; // level-up coin bonus
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

            for (var index = 0; index < _profile.ActiveMissions.Count; index++)
            {
                var mission = _profile.ActiveMissions[index];
                CreateMissionCard(mission, index);
            }
        }

        private void CreateMissionCard(NeonActiveMission mission, int index)
        {
            var card = new GameObject("Mission " + mission.MissionId, typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_missionsContent, false);

            var layoutElem = card.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 200f;
            layoutElem.flexibleWidth = 1f;

            var complete = mission.CurrentCount >= mission.TargetCount;
            var claimed = mission.Claimed;
            var cardColor = claimed
                ? new Color(0.06f, 0.2f, 0.08f, 0.8f)
                : complete
                    ? new Color(0.04f, 0.3f, 0.12f, 0.95f)
                    : new Color(0.05f, 0.1f, 0.14f, 0.92f);
            card.GetComponent<Image>().color = cardColor;

            var progressText = mission.TargetCount > 1
                ? Mathf.Min(mission.CurrentCount, mission.TargetCount) + "/" + mission.TargetCount
                : (complete ? "Done" : "0/" + mission.TargetCount);

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
                claimBtn.GetComponent<Image>().color = new Color(0.08f, 0.5f, 0.18f, 0.96f);
                claimBtn.GetComponentInChildren<Text>().fontSize = 30;
            }
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
            for (var index = 0; index < _trailViews.Count; index++)
            {
                _trailViews[index].gameObject.SetActive(false);
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
            _mainMenuPanel.GetComponent<Image>().color = new Color(0.01f, 0.025f, 0.055f, 0.98f);

            var titleText = CreateText(_mainMenuPanel.transform, "Menu Title", new Vector2(0f, -120f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 64, new Color(0.5f, 1f, 1f));
            titleText.text = "NEON SKY\nSURVIVORS";

            var taglineText = CreateText(_mainMenuPanel.transform, "Menu Tagline", new Vector2(0f, -310f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 30, new Color(0.6f, 0.9f, 1f, 0.7f));
            taglineText.text = "Survive. Upgrade. Evolve.";

            _mainMenuStatsText = CreateText(_mainMenuPanel.transform, "Menu Stats", new Vector2(0f, -400f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 26, new Color(0.65f, 0.75f, 0.85f));

            var playButton = CreateButton(_mainMenuPanel.transform, "PLAY", new Vector2(0f, 400f), ShowGarage);
            playButton.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 130f);
            playButton.GetComponent<Image>().color = new Color(0.02f, 0.42f, 0.48f, 0.96f);
            playButton.GetComponentInChildren<Text>().fontSize = 42;

            var settingsButton = CreateButton(_mainMenuPanel.transform, "SETTINGS", new Vector2(0f, 240f), () => ShowSettings(true));
            settingsButton.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 110f);
            settingsButton.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.95f);

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

            var titleText = CreateText(_settingsPanel.transform, "Settings Title", new Vector2(0f, -80f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 52, new Color(0.68f, 1f, 1f));
            titleText.text = "SETTINGS";

            // Music volume row
            _settingsMusicText = CreateText(_settingsPanel.transform, "Music Label", new Vector2(0f, 260f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 34, Color.white);
            _settingsMusicText.rectTransform.sizeDelta = new Vector2(500f, 70f);

            CreateSmallButton(_settingsPanel.transform, "-", new Vector2(-260f, 260f), () => AdjustMusicVolume(-0.25f));
            CreateSmallButton(_settingsPanel.transform, "+", new Vector2(260f, 260f), () => AdjustMusicVolume(0.25f));

            // SFX volume row
            _settingsSfxText = CreateText(_settingsPanel.transform, "SFX Label", new Vector2(0f, 140f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, 34, Color.white);
            _settingsSfxText.rectTransform.sizeDelta = new Vector2(500f, 70f);

            CreateSmallButton(_settingsPanel.transform, "-", new Vector2(-260f, 140f), () => AdjustSfxVolume(-0.25f));
            CreateSmallButton(_settingsPanel.transform, "+", new Vector2(260f, 140f), () => AdjustSfxVolume(0.25f));

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

            var backButton = CreateButton(_settingsPanel.transform, "Back", new Vector2(0f, -360f), HideSettings);
            backButton.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 110f);
            backButton.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.95f);

            _settingsPanel.SetActive(false);
        }

        private Button CreateSmallButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label + " Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(130f, 90f);
            buttonObject.GetComponent<Image>().color = new Color(0.1f, 0.22f, 0.34f, 0.95f);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
            var text = CreateText(buttonObject.transform, label + " Lbl", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 36, Color.white);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.text = label;
            return buttonObject.GetComponent<Button>();
        }

        private void AdjustMusicVolume(float delta)
        {
            _profile.MusicVolume = Mathf.Clamp01(Mathf.Round((_profile.MusicVolume + delta) * 4f) / 4f);
            ApplyAudioSettings();
            NeonSaveService.Save(_profile);
            UpdateSettingsPanel();
        }

        private void AdjustSfxVolume(float delta)
        {
            _profile.SfxVolume = Mathf.Clamp01(Mathf.Round((_profile.SfxVolume + delta) * 4f) / 4f);
            ApplyAudioSettings();
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

            var title = CreateText(_pauseMenuPanel.transform, "Pause Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 52, new Color(0.72f, 1f, 1f));
            title.text = "PAUSED";

            var resumeButton = CreateButton(_pauseMenuPanel.transform, "RESUME", new Vector2(0f, 150f), ResumePausedRun);
            resumeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 120f);
            resumeButton.GetComponent<Image>().color = new Color(0.02f, 0.42f, 0.48f, 0.96f);
            resumeButton.GetComponentInChildren<Text>().fontSize = 38;

            var restartButton = CreateButton(_pauseMenuPanel.transform, "RESTART", new Vector2(0f, 0f), RestartRun);
            restartButton.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 110f);
            restartButton.GetComponent<Image>().color = new Color(0.28f, 0.18f, 0.06f, 0.96f);
            restartButton.GetComponentInChildren<Text>().fontSize = 36;

            var quitButton = CreateButton(_pauseMenuPanel.transform, "QUIT TO GARAGE", new Vector2(0f, -140f), ReturnToGarage);
            quitButton.GetComponent<RectTransform>().sizeDelta = new Vector2(660f, 110f);
            quitButton.GetComponent<Image>().color = new Color(0.18f, 0.08f, 0.08f, 0.96f);
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
            if (duration > _hitStopRemaining)
            {
                _hitStopRemaining = duration;
                Time.timeScale = 0.05f;
            }
        }

        private void UpdateScreenShake(float deltaTime)
        {
            if (_shakeRemaining <= 0f)
            {
                _camera.transform.position = _cameraBasePosition;
                _shakeAmplitude = 0f;
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

        // ── Garage slot arrangement ──────────────────────────────────────────

        private void CreateSlotArrangement(Transform parent)
        {
            var panel = new GameObject("Slot Arrangement", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.04f, 0.74f);
            panelRect.anchorMax = new Vector2(0.96f, 0.895f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.1f, 0.6f);

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
                btnObject.GetComponent<Image>().color = new Color(0.06f, 0.14f, 0.2f, 0.95f);
                btnObject.GetComponent<Button>().onClick.AddListener(() => FilterInventoryBySlot(capturedSlot));

                var label = CreateText(btnObject.transform, slot + " Lbl", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 20, Color.white);
                label.rectTransform.offsetMin = new Vector2(4f, 2f);
                label.rectTransform.offsetMax = new Vector2(-4f, -2f);
                _slotFilterButtons[slot] = btnObject.GetComponent<Button>();
            }
        }

        private void FilterInventoryBySlot(NeonEquipmentSlot slot)
        {
            _selectedSlotFilter = _selectedSlotFilter == slot ? (NeonEquipmentSlot?)null : slot;
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
                var rarityColor = ownedItem != null ? ResolveRarityColor(ownedItem.Rarity) : new Color(0.4f, 0.45f, 0.5f);
                button.GetComponent<Image>().color = isFiltered
                    ? new Color(rarityColor.r * 0.45f + 0.08f, rarityColor.g * 0.45f + 0.08f, rarityColor.b * 0.45f + 0.08f, 0.98f)
                    : new Color(rarityColor.r * 0.15f + 0.03f, rarityColor.g * 0.15f + 0.03f, rarityColor.b * 0.15f + 0.03f, 0.92f);
                var label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    var itemName = def != null ? TruncateName(def.Name, 10) : "Empty";
                    label.text = slot.ToString() + "\n" + itemName + (isFiltered ? " v" : string.Empty);
                    label.color = ownedItem != null ? rarityColor : new Color(0.5f, 0.55f, 0.6f);
                }
            }
        }

        private static string TruncateName(string name, int maxLen)
        {
            return name.Length <= maxLen ? name : name.Substring(0, maxLen - 1) + "~";
        }

        // ── Missions panel ────────────────────────────────────────────────────

        private static readonly NeonActiveMission[] MissionTemplates =
        {
            new NeonActiveMission { MissionId = "kill30",   Name = "Exterminator", Description = "Kill 30 enemies in one run",     Type = NeonMissionType.KillEnemies,    TargetCount = 30,  RewardCoins = 40,  RewardAccountXP = 20 },
            new NeonActiveMission { MissionId = "kill100",  Name = "Slaughter",    Description = "Kill 100 enemies in one run",    Type = NeonMissionType.KillEnemies,    TargetCount = 100, RewardCoins = 80,  RewardAccountXP = 40 },
            new NeonActiveMission { MissionId = "survive3", Name = "Survivor",     Description = "Survive 3 minutes",             Type = NeonMissionType.SurviveMinutes, TargetCount = 3,   RewardCoins = 30,  RewardAccountXP = 15 },
            new NeonActiveMission { MissionId = "survive6", Name = "Veteran",      Description = "Survive 6 minutes",             Type = NeonMissionType.SurviveMinutes, TargetCount = 6,   RewardCoins = 60,  RewardAccountXP = 30 },
            new NeonActiveMission { MissionId = "boss1",    Name = "Boss Hunter",  Description = "Defeat a major boss",           Type = NeonMissionType.DefeatBoss,     TargetCount = 1,   RewardCoins = 50,  RewardAccountXP = 25 },
            new NeonActiveMission { MissionId = "run1",     Name = "Full Run",     Description = "Complete a full 10-minute run", Type = NeonMissionType.CompleteRun,    TargetCount = 1,   RewardCoins = 100, RewardAccountXP = 50 },
        };

        private void RefreshDailyMissions()
        {
            const long TicksPerDay = 864000000000L;
            var daysSinceEpoch = System.DateTime.UtcNow.Ticks / TicksPerDay;
            if (_profile.ActiveMissions.Count == 3 && _profile.MissionLastResetTicks / TicksPerDay == daysSinceEpoch) return;

            _profile.ActiveMissions.Clear();
            var rng = new System.Random((int)daysSinceEpoch);
            var indices = new System.Collections.Generic.List<int> { 0, 1, 2, 3, 4, 5 };
            for (var pick = 0; pick < 3 && indices.Count > 0; pick++)
            {
                var chosen = rng.Next(indices.Count);
                var t = MissionTemplates[indices[chosen]];
                indices.RemoveAt(chosen);
                _profile.ActiveMissions.Add(new NeonActiveMission
                {
                    MissionId = t.MissionId, Name = t.Name, Description = t.Description,
                    Type = t.Type, TargetCount = t.TargetCount, CurrentCount = 0, Claimed = false,
                    RewardCoins = t.RewardCoins, RewardAccountXP = t.RewardAccountXP
                });
            }
            _profile.MissionLastResetTicks = daysSinceEpoch * TicksPerDay;
            NeonSaveService.Save(_profile);
        }

        private void UpdateMissionProgressFromRun(NeonRunState run)
        {
            var changed = false;
            for (var i = 0; i < _profile.ActiveMissions.Count; i++)
            {
                var m = _profile.ActiveMissions[i];
                if (m.Claimed) continue;
                var prev = m.CurrentCount;
                switch (m.Type)
                {
                    case NeonMissionType.KillEnemies:    m.CurrentCount = Mathf.Max(m.CurrentCount, run.EnemiesKilled); break;
                    case NeonMissionType.SurviveMinutes: m.CurrentCount = Mathf.Max(m.CurrentCount, Mathf.FloorToInt(run.ElapsedSeconds / 60f)); break;
                    case NeonMissionType.DefeatBoss:     m.CurrentCount = Mathf.Max(m.CurrentCount, run.BossesKilled); break;
                    case NeonMissionType.CompleteRun:    if (run.Status == NeonRunStatus.Victory) m.CurrentCount = Mathf.Max(m.CurrentCount, 1); break;
                }
                if (m.CurrentCount != prev) changed = true;
            }
            if (changed) NeonSaveService.Save(_profile);
        }

        private void ClaimMission(int index)
        {
            if (index < 0 || index >= _profile.ActiveMissions.Count) return;
            var m = _profile.ActiveMissions[index];
            if (m.Claimed || m.CurrentCount < m.TargetCount) return;
            m.Claimed = true;
            _profile.PlayerCoins += m.RewardCoins;
            _profile.AccountXP += m.RewardAccountXP;
            while (_profile.AccountXP >= 100 * _profile.AccountLevel)
            {
                _profile.AccountXP -= 100 * _profile.AccountLevel;
                _profile.AccountLevel += 1;
                _profile.PlayerCoins += 30 * _profile.AccountLevel;
            }
            NeonSaveService.Save(_profile);
            RebuildMissionCards();
            UpdateGaragePanel();
        }

        private void ShowMissions()
        {
            RefreshDailyMissions();
            _garagePanel.SetActive(false);
            _missionsPanel.SetActive(true);
            RebuildMissionCards();
        }

        private void HideMissions()
        {
            _missionsPanel.SetActive(false);
            ShowGarage();
        }

        private void RebuildMissionCards()
        {
            for (var i = _missionsContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_missionsContent.GetChild(i).gameObject);
            }

            var header = new GameObject("Acct Header", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            header.transform.SetParent(_missionsContent, false);
            header.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.16f, 0.9f);
            header.GetComponent<LayoutElement>().preferredHeight = 100f;
            var acctLabel = CreateText(header.transform, "Acct Lbl", Vector2.zero, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 28, new Color(0.9f, 0.9f, 0.5f));
            acctLabel.rectTransform.offsetMin = new Vector2(12f, 4f);
            acctLabel.rectTransform.offsetMax = new Vector2(-12f, -4f);
            acctLabel.text = "Account Level " + _profile.AccountLevel + "   XP " + _profile.AccountXP + " / " + (100 * _profile.AccountLevel);

            for (var index = 0; index < _profile.ActiveMissions.Count; index++)
            {
                CreateMissionCard(_profile.ActiveMissions[index], index);
            }
        }

        private void CreateMissionCard(NeonActiveMission mission, int index)
        {
            var card = new GameObject("Mission " + mission.MissionId, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(_missionsContent, false);
            card.GetComponent<LayoutElement>().preferredHeight = 200f;

            var complete = mission.CurrentCount >= mission.TargetCount;
            card.GetComponent<Image>().color = mission.Claimed
                ? new Color(0.06f, 0.2f, 0.08f, 0.8f)
                : complete ? new Color(0.04f, 0.3f, 0.12f, 0.95f) : new Color(0.05f, 0.1f, 0.14f, 0.92f);

            var progress = Mathf.Min(mission.CurrentCount, mission.TargetCount) + "/" + mission.TargetCount;
            var statusSuffix = mission.Claimed ? "  Done" : complete ? "  COMPLETE!" : "  [" + progress + "]";
            var infoText = CreateText(card.transform, "Mission Info", Vector2.zero, new Vector2(0f, 0f), new Vector2(0.68f, 1f), TextAnchor.MiddleLeft, 26, complete && !mission.Claimed ? new Color(0.6f, 1f, 0.65f) : Color.white);
            infoText.rectTransform.offsetMin = new Vector2(18f, 8f);
            infoText.rectTransform.offsetMax = new Vector2(-8f, -8f);
            infoText.text = mission.Name + statusSuffix + "\n" + mission.Description + "\n+" + mission.RewardCoins + " coins  +" + mission.RewardAccountXP + " XP";

            if (complete && !mission.Claimed)
            {
                var capturedIndex = index;
                var claimBtn = CreateButton(card.transform, "Claim!", new Vector2(0f, 0f), () => ClaimMission(capturedIndex));
                var claimRect = claimBtn.GetComponent<RectTransform>();
                claimRect.anchorMin = new Vector2(0.7f, 0.15f);
                claimRect.anchorMax = new Vector2(0.97f, 0.85f);
                claimRect.offsetMin = Vector2.zero;
                claimRect.offsetMax = Vector2.zero;
                claimRect.sizeDelta = Vector2.zero;
                claimBtn.GetComponent<Image>().color = new Color(0.08f, 0.5f, 0.18f, 0.96f);
                claimBtn.GetComponentInChildren<Text>().fontSize = 30;
            }
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

            CreateText(_missionsPanel.transform, "Missions Title", new Vector2(0f, -60f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 48, new Color(0.5f, 1f, 0.6f))
                .text = "DAILY MISSIONS";

            var scrollObject = new GameObject("Missions Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(_missionsPanel.transform, false);
            var scrollRectComp = scrollObject.GetComponent<RectTransform>();
            scrollRectComp.anchorMin = new Vector2(0.04f, 0.18f);
            scrollRectComp.anchorMax = new Vector2(0.96f, 0.84f);
            scrollRectComp.offsetMin = Vector2.zero;
            scrollRectComp.offsetMax = Vector2.zero;
            scrollObject.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.1f, 0.85f);

            var vpObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            vpObj.transform.SetParent(scrollObject.transform, false);
            var vpRect = vpObj.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = new Vector2(10f, 10f);
            vpRect.offsetMax = new Vector2(-10f, -10f);
            vpObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            vpObj.GetComponent<Mask>().showMaskGraphic = false;

            var contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObj.transform.SetParent(vpObj.transform, false);
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
            backButton.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.95f);
            _missionsPanel.SetActive(false);
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
