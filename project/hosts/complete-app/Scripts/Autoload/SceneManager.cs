using System;
using System.Threading.Tasks;
using Godot;
using UltimaMagic.Data;
using UltimaMagic.Overworld;

namespace UltimaMagic.Autoload;

public partial class SceneManager : Node
{
    private const string BattleScenePath = "res://Scenes/Battle/BattleScene.tscn";
    private const string OverworldScenePath = "res://Scenes/Overworld/Overworld.tscn";
    private const int MaxEncounterManagerConnectionAttempts = 10;
    private const double EncounterManagerRetryDelaySeconds = 1.0d;
    private const double FadeDurationSeconds = 0.5d;
    private const int TransitionLayer = 100;

    public static SceneManager? Instance { get; private set; }
    public BattleTransitionData? PendingBattleTransition { get; private set; }

    private Godot.Timer? _encounterManagerRetryTimer;
    private ColorRect _fadeRect = null!;
    private bool _encounterManagerConnected;
    private int _encounterManagerConnectionAttempts;
    private bool _encounterManagerMissingLogged;
    private bool _isTransitioning;

    public override void _Ready()
    {
        Instance = this;
        SetupTransitionOverlay();

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

    public async void TransitionToBattle(EncounterResult encounterData)
    {
        ArgumentNullException.ThrowIfNull(encounterData);

        if (_isTransitioning)
        {
            return;
        }

        var playerState = GameManager.Instance?.CreatePlayerStateSnapshot(encounterData.PlayerReturnPosition)
            ?? new PlayerStateSnapshot { ReturnPosition = encounterData.PlayerReturnPosition };

        PendingBattleTransition = new BattleTransitionData
        {
            Encounter = encounterData,
            PlayerState = playerState
        };

        GD.Print($"Encounter triggered in zone '{encounterData.ZoneName}' on terrain '{encounterData.TerrainType}' with {encounterData.EnemyCount} enemies.");
        await RunSceneTransition(BattleScenePath, GameManager.GameState.Battle, null);
    }

    public async void TransitionToOverworld(BattleResult battleResult)
    {
        ArgumentNullException.ThrowIfNull(battleResult);

        if (_isTransitioning)
        {
            return;
        }

        await RunSceneTransition(
            OverworldScenePath,
            GameManager.GameState.Overworld,
            () => RestoreOverworldState(battleResult));
    }

    public BattleTransitionData? ConsumePendingBattleTransition()
    {
        var transitionData = PendingBattleTransition;
        PendingBattleTransition = null;
        return transitionData;
    }

    private void SetupTransitionOverlay()
    {
        var canvasLayer = new CanvasLayer
        {
            Layer = TransitionLayer
        };

        _fadeRect = new ColorRect
        {
            Name = "SceneFadeRect",
            Color = new Color(0.0f, 0.0f, 0.0f, 0.0f),
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        canvasLayer.AddChild(_fadeRect);
        AddChild(canvasLayer);
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

    private async Task RunSceneTransition(string scenePath, GameManager.GameState targetState, Action? postSceneChange)
    {
        if (_isTransitioning)
        {
            return;
        }

        _isTransitioning = true;
        GameManager.Instance?.SetInputEnabled(false);

        try
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CurrentState = targetState;
            }

            await FadeToAsync(1.0f);
            ChangeScene(scenePath);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            postSceneChange?.Invoke();
            await FadeToAsync(0.0f);
        }
        finally
        {
            GameManager.Instance?.SetInputEnabled(true);
            _isTransitioning = false;
        }
    }

    private async Task FadeToAsync(float targetAlpha)
    {
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", targetAlpha, FadeDurationSeconds);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private void RestoreOverworldState(BattleResult battleResult)
    {
        GameManager.Instance?.ApplyBattleResult(battleResult);

        var player = GetTree().CurrentScene?.GetNodeOrNull<Player>("Player");
        player?.SnapToTile(battleResult.PlayerReturnPosition);

        EncounterManager.Instance?.ResetEncounterCounter();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentState = GameManager.GameState.Overworld;
        }
    }
}
