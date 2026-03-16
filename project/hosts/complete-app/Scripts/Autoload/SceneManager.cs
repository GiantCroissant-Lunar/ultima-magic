using System;
using Godot;
using UltimaMagic.Data;

namespace UltimaMagic.Autoload;

public partial class SceneManager : Node
{
    public static SceneManager? Instance { get; private set; }
    public EncounterResult? PendingEncounter { get; private set; }

    private bool _encounterManagerConnected;

    public override void _Ready()
    {
        Instance = this;
        CallDeferred(MethodName.ConnectEncounterManager);
    }

    public void ChangeScene(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            throw new ArgumentException("Scene path must not be null or empty.", nameof(scenePath));
        }

        if (!ResourceLoader.Exists(scenePath))
        {
            throw new ArgumentException($"Scene path does not exist: {scenePath}", nameof(scenePath));
        }

        var result = GetTree().ChangeSceneToFile(scenePath);
        if (result != Error.Ok)
        {
            throw new InvalidOperationException($"Failed to change scene to '{scenePath}': {result}");
        }
    }

    public void TransitionToBattle(EncounterResult encounterData)
    {
        PendingEncounter = encounterData;
        GD.Print($"Encounter triggered in zone '{encounterData.ZoneName}' on terrain '{encounterData.TerrainType}' with {encounterData.EnemyCount} enemies.");
    }

    private void ConnectEncounterManager()
    {
        if (_encounterManagerConnected)
        {
            return;
        }

        if (EncounterManager.Instance == null)
        {
            CallDeferred(MethodName.ConnectEncounterManager);
            return;
        }

        EncounterManager.Instance.EncounterTriggered += OnEncounterTriggered;
        _encounterManagerConnected = true;
    }

    private void OnEncounterTriggered(EncounterResult encounter)
    {
        TransitionToBattle(encounter);
    }
}
