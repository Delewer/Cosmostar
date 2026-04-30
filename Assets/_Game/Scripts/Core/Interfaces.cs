using System;

public interface IInitializable
{
    void Initialize();
}

public interface ITickable
{
    void Tick(float deltaTime);
}

public interface IDamageable
{
    void TakeDamage(in DamageContext context);
    bool IsAlive { get; }
}

public interface IUpgradeable
{
    void ApplyUpgrade(UpgradeData upgrade);
}

public interface ISaveable
{
    object CaptureState();
    void RestoreState(object state);
}
