using System.IO;
using UnityEngine;

public class SaveService
{
    private const string MetaFile = "meta.json";

    public void SaveMeta(MetaState state)
    {
        string path = Path.Combine(Application.persistentDataPath, MetaFile);
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(path, json);
    }

    public MetaState LoadMeta()
    {
        string path = Path.Combine(Application.persistentDataPath, MetaFile);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<MetaState>(json);
    }
}
