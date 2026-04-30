using System;

[Serializable]
public class MetaState
{
    public int Credits;
    public int Cores;
    public int Prisms;
    public int TechTreeLevel;
}

public class MetaProgressionService
{
    private readonly SaveService saveService;
    public MetaState State { get; private set; }

    public MetaProgressionService(SaveService saveService)
    {
        this.saveService = saveService;
        State = saveService.LoadMeta() ?? new MetaState();
    }

    public void AddRunRewards(int credits, int cores, int prisms)
    {
        State.Credits += credits;
        State.Cores += cores;
        State.Prisms += prisms;
        saveService.SaveMeta(State);
    }

    public bool TrySpendCredits(int amount)
    {
        if (State.Credits < amount) return false;

        State.Credits -= amount;
        saveService.SaveMeta(State);
        return true;
    }
}
