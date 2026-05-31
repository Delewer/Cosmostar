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

        private readonly NeonRunGameplaySystem _gameplay = new NeonRunGameplaySystem();
        private readonly NeonEquipmentSystem _equipment = new NeonEquipmentSystem();
        private readonly List<SpriteRenderer> _enemyViews = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _projectileViews = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _xpViews = new List<SpriteRenderer>();
        private readonly List<LineRenderer> _trailViews = new List<LineRenderer>();
        private readonly List<Button> _upgradeButtons = new List<Button>();

        private NeonSkySurvivorsCatalog _catalog = null!;
        private NeonSaveProfile _profile = null!;
        private NeonRunState _run = null!;
        private Camera _camera = null!;
        private Sprite _sprite = null!;
        private Text _hudText = null!;
        private Text _messageText = null!;
        private Text _statusText = null!;
        private Button _dashButton = null!;
        private GameObject _garagePanel = null!;
        private Text _garageTitleText = null!;
        private Text _garageStatsText = null!;
        private GameObject _resultsPanel = null!;
        private Text _resultsTitleText = null!;
        private Text _resultsStatsText = null!;
        private GameObject _upgradePanel = null!;
        private bool _paused;
        private bool _resultApplied;
        private int _lastRewardCoins;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.orientation = ScreenOrientation.Portrait;

            _catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            _profile = new NeonSaveProfile();
            _equipment.EnsureStartingProfile(_profile, _catalog);
            _sprite = CreateSprite();

            EnsureCamera();
            EnsureEventSystem();
            CreatePools();
            CreateHud();
            ShowGarage();
        }

        private void Update()
        {
            if (_run == null)
            {
                return;
            }

            HandleTouchInput();

            if (!_paused && _run.Status == NeonRunStatus.Running)
            {
                _gameplay.Tick(_run, _catalog, Mathf.Min(Time.deltaTime, 0.05f));
            }

            RenderRun();
            UpdateHud();
        }

        private void StartRun()
        {
            _run = _gameplay.StartRun(_profile, _catalog);
            _paused = false;
            _resultApplied = false;
            _lastRewardCoins = 0;
            _garagePanel.SetActive(false);
            _resultsPanel.SetActive(false);
            SetRunHudVisible(true);
            _statusText.text = string.Empty;
            _messageText.text = "Survive 10 minutes. Bosses at 3:00, 6:00, 7:30, 8:45, 10:00.";
            UpdateUpgradeChoices(false);
        }

        private void ShowGarage()
        {
            _run = null!;
            _paused = true;
            _resultApplied = false;
            HideRuntimeViews();
            UpdateUpgradeChoices(false);
            SetRunHudVisible(false);
            _resultsPanel.SetActive(false);
            _garagePanel.SetActive(true);
            UpdateGaragePanel();
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

                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    SetMovementTarget(touch.position);
                }

                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                SetMovementTarget(Input.mousePosition);
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
                Handheld.Vibrate();
            }
        }

        private void RenderRun()
        {
            RenderEnemies();
            RenderProjectiles();
            RenderXp();
            RenderTrails();
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
                view.color = enemy.IsBoss ? (enemy.IsMiniBoss ? new Color(1f, 0.62f, 0.18f) : new Color(1f, 0.18f, 0.82f)) : new Color(1f, 0.24f, 0.36f);
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
                view.transform.localScale = Vector3.one * 0.12f;
                view.color = new Color(0.38f, 1f, 0.52f);
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
                view.gameObject.SetActive(true);
                view.positionCount = 2;
                view.SetPosition(0, ToWorld(trail.Start));
                view.SetPosition(1, ToWorld(trail.End));
            }
        }

        private void UpdateHud()
        {
            var player = _run.Player;
            var hpPercent = Mathf.Clamp01(player.Stats.CurrentHP / player.Stats.MaxHP);
            var xpPercent = Mathf.Clamp01(player.XP / player.XPToNextLevel);
            _hudText.text = FormatTime(_run.ElapsedSeconds) + "\n"
                + "HP " + Mathf.RoundToInt(hpPercent * 100f) + "%  XP " + Mathf.RoundToInt(xpPercent * 100f) + "%\n"
                + "Lv " + player.Level + "  Coins " + player.CoinsCollected + "  Dash " + (player.DashCooldownRemaining <= 0f ? "READY" : player.DashCooldownRemaining.ToString("0.0"));

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
                _statusText.text = string.Empty;
                UpdateUpgradeChoices(false);
            }

            _dashButton.interactable = _run.Status == NeonRunStatus.Running && player.DashCooldownRemaining <= 0f;
        }

        private void ShowResults(string title)
        {
            if (!_resultApplied)
            {
                _lastRewardCoins = CalculateRunReward(_run);
                _profile.PlayerCoins += _lastRewardCoins;
                _profile.CompletedRuns += 1;
                _profile.BestSurvivalTime = Mathf.Max(_profile.BestSurvivalTime, _run.ElapsedSeconds);
                _profile.BossesDefeated += _run.BossesKilled + _run.MiniBossesKilled;
                _resultApplied = true;
            }

            SetRunHudVisible(false);
            _resultsPanel.SetActive(true);
            _resultsTitleText.text = title;
            _resultsStatsText.text = "Time " + FormatTime(_run.ElapsedSeconds) + "  Best " + FormatTime(_profile.BestSurvivalTime) + "\n"
                + "Kills " + _run.EnemiesKilled + "  Bosses " + (_run.BossesKilled + _run.MiniBossesKilled) + "\n"
                + "Coins +" + _lastRewardCoins + "  Total " + _profile.PlayerCoins + "\n"
                + "Runs " + _profile.CompletedRuns;
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
                var label = button.GetComponentInChildren<Text>();
                label.text = choice.Name + "\n" + choice.Description + "\nLv " + (_run.Build.GetLevel(choice.Id) + 1) + "/" + choice.MaxLevel;
            }
        }

        private void SelectUpgradeChoice(int index)
        {
            if (_run.Status != NeonRunStatus.LevelUpDraft || index >= _run.DraftChoices.Count)
            {
                return;
            }

            var choice = _run.DraftChoices[index];
            if (_gameplay.ApplyUpgradeChoice(_run, choice))
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
            CreateSpritePool("Enemies", MaxEnemyViews, _enemyViews, 1);
            CreateSpritePool("Projectiles", MaxProjectileViews, _projectileViews, 2);
            CreateSpritePool("XP", MaxXpViews, _xpViews, 3);

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

        private void CreateSpritePool(string name, int count, List<SpriteRenderer> target, int sortingOrder)
        {
            var root = new GameObject(name + " Pool");
            for (var index = 0; index < count; index++)
            {
                var item = new GameObject(name + " " + index);
                item.transform.SetParent(root.transform, false);
                var renderer = item.AddComponent<SpriteRenderer>();
                renderer.sprite = _sprite;
                renderer.sortingOrder = sortingOrder;
                item.SetActive(false);
                target.Add(renderer);
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

            _hudText = CreateText(canvasObject.transform, "HUD", new Vector2(32f, -32f), new Vector2(0f, 1f), new Vector2(0f, 1f), TextAnchor.UpperLeft, 34, new Color(0.75f, 1f, 1f));
            _messageText = CreateText(canvasObject.transform, "Message", new Vector2(0f, -170f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 34, new Color(1f, 0.82f, 0.28f));
            _statusText = CreateText(canvasObject.transform, "Status", new Vector2(0f, 0f), new Vector2(0f, 0.48f), new Vector2(1f, 0.48f), TextAnchor.MiddleCenter, 42, Color.white);
            _dashButton = CreateButton(canvasObject.transform, "Dash", new Vector2(-210f, 150f), TryDash);
            CreateUpgradePanel(canvasObject.transform);
            CreateGaragePanel(canvasObject.transform);
            CreateResultsPanel(canvasObject.transform);
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

            _garageTitleText = CreateText(_garagePanel.transform, "Garage Title", new Vector2(0f, -90f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 52, new Color(0.68f, 1f, 1f));
            _garageTitleText.rectTransform.sizeDelta = new Vector2(-80f, 190f);

            _garageStatsText = CreateText(_garagePanel.transform, "Garage Stats", new Vector2(0f, -250f), new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperCenter, 30, Color.white);
            _garageStatsText.rectTransform.sizeDelta = new Vector2(-120f, 1040f);

            var startRunButton = CreateButton(_garagePanel.transform, "Start Run", new Vector2(0f, 150f), StartRun);
            startRunButton.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, 120f);
            startRunButton.GetComponent<Image>().color = new Color(0.02f, 0.42f, 0.48f, 0.96f);

            _garagePanel.SetActive(false);
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
                label.alignment = TextAnchor.MiddleCenter;
                _upgradeButtons.Add(button);
            }

            _upgradePanel.SetActive(false);
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment, int fontSize, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(-64f, 140f);

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
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
            _dashButton.gameObject.SetActive(visible);
        }

        private void UpdateGaragePanel()
        {
            var stats = _equipment.CalculateStats(_profile, _catalog);
            _garageTitleText.text = "NEON SKY SURVIVORS\nGARAGE";
            _garageStatsText.text = "Coins " + _profile.PlayerCoins + "  Runs " + _profile.CompletedRuns + "  Best " + FormatTime(_profile.BestSurvivalTime) + "\n"
                + "ATK " + stats.AttackDamage.ToString("0") + "  Fire " + stats.FireRate.ToString("0.0") + "  Speed " + stats.MovementSpeed.ToString("0.0") + "\n"
                + "HP " + stats.MaxHP.ToString("0") + "  Armor " + stats.Armor.ToString("0") + "  Dash " + stats.DashCooldown.ToString("0.0") + "s\n\n"
                + "Equipment\n"
                + FormatEquippedSlot(NeonEquipmentSlot.Weapon) + "\n"
                + FormatEquippedSlot(NeonEquipmentSlot.Wings) + "\n"
                + FormatEquippedSlot(NeonEquipmentSlot.Engine) + "\n"
                + FormatEquippedSlot(NeonEquipmentSlot.Hull) + "\n"
                + FormatEquippedSlot(NeonEquipmentSlot.Core) + "\n"
                + FormatEquippedSlot(NeonEquipmentSlot.Radar);
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
            for (var index = 0; index < _trailViews.Count; index++)
            {
                _trailViews[index].gameObject.SetActive(false);
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
    }
}
