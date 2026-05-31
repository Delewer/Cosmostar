using NeonSkySurvivors.Core.Design;
using NeonSkySurvivors.Core.Models;
using NeonSkySurvivors.Core.Systems;
using Xunit;

namespace NeonSkySurvivors.Core.Tests
{
    public sealed class NeonRunGameplaySystemTests
    {
        [Fact]
        public void StartRun_SeedsPlayerPlaneWithGarageStats()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var profile = new NeonSaveProfile();
            var gameplay = new NeonRunGameplaySystem();

            var run = gameplay.StartRun(profile, catalog);

            Assert.Equal(NeonRunStatus.Running, run.Status);
            Assert.Equal(6, profile.OwnedEquipmentItems.Count);
            Assert.True(run.Player.Stats.AttackDamage > catalog.BasePlayerStats.AttackDamage);
            Assert.Equal(run.Player.Stats.MaxHP, run.Player.Stats.CurrentHP);
        }

        [Fact]
        public void MovementAndDash_MovePlaneAndCreateDamagingTrail()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var gameplay = new NeonRunGameplaySystem();
            var run = gameplay.StartRun(new NeonSaveProfile(), catalog);

            gameplay.SetMovementTarget(run, new NeonVector2(0.5f, 0.5f));
            gameplay.Tick(run, catalog, 0.5f);
            var movedPosition = run.Player.Position;
            var dashed = gameplay.TryDash(run);

            Assert.True(movedPosition.SqrMagnitude > 0f);
            Assert.True(dashed);
            Assert.True(run.Player.DashCooldownRemaining > 0f);
            Assert.True(run.Player.InvulnerabilityRemaining > 0f);
            Assert.Single(run.DashTrails);
            Assert.True(run.DashTrails[0].DamagePerSecond > 0f);
        }

        [Fact]
        public void AutoFire_DamagesEnemyAndDropsXpShardOnKill()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var gameplay = new NeonRunGameplaySystem();
            var run = gameplay.StartRun(new NeonSaveProfile(), catalog);
            run.Enemies.Add(new NeonRunEnemyState
            {
                EnemyID = "chaser_drone",
                Position = new NeonVector2(0f, 0.12f),
                HP = 5f,
                MaxHP = 5f,
                ContactDamage = 10f,
                Speed = 0f,
                XPDrop = 1
            });

            gameplay.Tick(run, catalog, 0.1f);

            Assert.Equal(1, run.EnemiesKilled);
            Assert.Empty(run.Enemies);
            Assert.Single(run.XpShards);
        }

        [Fact]
        public void XpCollection_OpensThreeCardLevelUpDraft()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var gameplay = new NeonRunGameplaySystem();
            var run = gameplay.StartRun(new NeonSaveProfile(), catalog);
            run.XpShards.Add(new NeonXpShardState { Position = run.Player.Position, XPValue = 99f });

            gameplay.Tick(run, catalog, 0.1f);

            Assert.Equal(NeonRunStatus.LevelUpDraft, run.Status);
            Assert.Equal(2, run.Player.Level);
            Assert.Equal(3, run.DraftChoices.Count);
        }

        [Fact]
        public void TimelineTick_SpawnsBossAtThreeMinutes()
        {
            var catalog = NeonSkySurvivorsBlueprints.CreateMvpCatalog();
            var gameplay = new NeonRunGameplaySystem();
            var run = gameplay.StartRun(new NeonSaveProfile(), catalog);
            run.ElapsedSeconds = 179.9f;

            gameplay.Tick(run, catalog, 0.2f);

            Assert.Contains(run.Enemies, enemy => enemy.IsBoss && enemy.EnemyID == "sky_reaper");
            Assert.Contains("SKY REAPER", run.LastWarning);
        }
    }
}
