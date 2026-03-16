using System;
using Godot;
using UltimaMagic.Data;

namespace UltimaMagic.Autoload;

public partial class SceneManager : Node
{
    private const string BattleScenePath = "res://Scenes/Battle/BattleScene.tscn";
    private const int MaxEncounterManagerConnectionAttempts = 10;
    private const double EncounterManagerRetryDelaySeconds = 1.0d;

    public static SceneManager? Instance { get; private set; }
    public EncounterResult? PendingEncounter { get; private set; }

    private Godot.Timer? _encounterManagerRetryTimer;
    private bool _encounterManagerConnected;
    private int _encounterManagerConnectionAttempts;
    private bool _encounterManagerMissingLogged;

    public override void _Ready()
    {
        Instance = this;
        _encounterManagerRetryTimer = new Godot.Timer
        {
            WaitTime = EncounterManagerRetryDelaySeconds,
            OneShot = true
        };
        _encounterManagerRetryTimer.Timeout += OnEncounterManagerRetryTimeout;
        AddChild(_encounterManagerRetryTimer);
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentState = GameManager.GameState.Battle;
        }
        CallDeferred(nameof(ChangeScene), BattleScenePath);
    }

    public EncounterResult? ConsumePendingEncounter()
    {
        var encounter = PendingEncounter;
        PendingEncounter = null;
        return encounter;
    }

    private void ConnectEncounterManager()
    {
        if (_encounterManagerConnected)
        {
            return;
        }

        if (EncounterManager.Instance == null)
        {
            _encounterManagerConnectionAttempts++;
            if (_encounterManagerConnectionAttempts >= MaxEncounterManagerConnectionAttempts)
            {
                if (!_encounterManagerMissingLogged)
                {
                    GD.PushError("SceneManager could not connect to EncounterManager. Verify the EncounterManager autoload is configured correctly.");
                    _encounterManagerMissingLogged = true;
                }

                _encounterManagerConnectionAttempts = 0;
                ScheduleEncounterManagerReconnect();
                return;
            }

            CallDeferred(MethodName.ConnectEncounterManager);
            return;
        }

        EncounterManager.Instance.EncounterTriggered += OnEncounterTriggered;
        _encounterManagerConnected = true;
        _encounterManagerConnectionAttempts = 0;
        _encounterManagerRetryTimer?.Stop();
    }

    private void OnEncounterTriggered(EncounterResult encounter)
    {
        TransitionToBattle(encounter);
    }

    private void ScheduleEncounterManagerReconnect()
    {
        if (_encounterManagerRetryTimer == null || _encounterManagerRetryTimer.TimeLeft > 0.0d)
        {
            return;
        }

        _encounterManagerRetryTimer.Start();
    }

    private void OnEncounterManagerRetryTimeout()
    {
        ConnectEncounterManager();
    }
}
