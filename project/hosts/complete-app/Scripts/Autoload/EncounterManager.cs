using System;
using System.Linq;
using Godot;
using UltimaMagic.Data;
using UltimaMagic.Overworld;

namespace UltimaMagic.Autoload;

public partial class EncounterManager : Node
{
    [Signal]
    public delegate void EncounterTriggeredEventHandler(EncounterResult encounter);

    public static EncounterManager? Instance { get; private set; }

    [Export]
    public float BaseEncounterRate { get; set; } = 0.05f;

    [Export]
    public int GuaranteedSafeSteps { get; set; } = 5;

    [Export]
    public float DrySpellScalingFactor { get; set; } = 20.0f;

    public float CurrentEncounterProbability { get; private set; }
    public int StepsSinceLastEncounter => _stepsSinceLastEncounter;
    public string CurrentZoneName { get; private set; } = "None";

    private readonly RandomNumberGenerator _rng = new();
    private int _stepsSinceLastEncounter;
    private Player? _player;
    private TileMapLayer? _groundLayer;
    private TileMapLayer? _detailLayer;
    private EncounterZone[] _zones = Array.Empty<EncounterZone>();
    private Control? _debugPanel;
    private Label? _debugLabel;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _rng.Randomize();
        SetProcessUnhandledInput(true);
        CreateDebugOverlay();
        UpdateDebugOverlay();
    }

    public override void _ExitTree()
    {
        if (ReferenceEquals(Instance, this))
        {
            Instance = null;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.F1 })
        {
            ToggleDebugOverlay();
        }
    }

    public void RegisterOverworld(Player player, TileMapLayer groundLayer, TileMapLayer detailLayer, EncounterZone[] zones)
    {
        if (_player != null && GodotObject.IsInstanceValid(_player))
        {
            _player.StepTaken -= OnPlayerStep;
        }

        _player = player;
        _groundLayer = groundLayer;
        _detailLayer = detailLayer;
        _zones = zones
            .Where(zone => zone != null)
            .OrderByDescending(zone => zone.Priority)
            .ToArray();
        _stepsSinceLastEncounter = 0;

        CurrentZoneName = ResolveCurrentZone(player.TilePosition)?.ZoneName ?? "None";
        CurrentEncounterProbability = 0.0f;
        _player.StepTaken += OnPlayerStep;
        UpdateDebugOverlay();
    }

    private void OnPlayerStep(Vector2I tilePosition)
    {
        _stepsSinceLastEncounter++;

        var zone = ResolveCurrentZone(tilePosition);
        CurrentZoneName = zone?.ZoneName ?? "None";
        CurrentEncounterProbability = CalculateEncounterProbability(zone, tilePosition);

        if (CurrentEncounterProbability > 0.0f && zone?.EncounterData != null && _rng.Randf() < CurrentEncounterProbability)
        {
            var encounter = RollEncounter(zone, tilePosition);
            if (encounter != null)
            {
                _stepsSinceLastEncounter = 0;
                CurrentEncounterProbability = 0.0f;
                EmitSignal(SignalName.EncounterTriggered, encounter);
            }
        }

        UpdateDebugOverlay();
    }

    private float CalculateEncounterProbability(EncounterZone? zone, Vector2I tilePosition)
    {
        if (_groundLayer == null || _detailLayer == null || zone?.EncounterData == null)
        {
            return 0.0f;
        }

        if (_stepsSinceLastEncounter <= GuaranteedSafeSteps)
        {
            return 0.0f;
        }

        var tileEncounterMultiplier = OverworldGrid.GetEncounterMultiplier(_groundLayer, _detailLayer, tilePosition);
        if (tileEncounterMultiplier <= 0.0f || zone.BaseEncounterRate <= 0.0f)
        {
            return 0.0f;
        }

        var extraSteps = _stepsSinceLastEncounter - GuaranteedSafeSteps;
        var probability = zone.BaseEncounterRate
            * tileEncounterMultiplier
            * (1.0f + extraSteps / Mathf.Max(DrySpellScalingFactor, 1.0f));

        return Mathf.Clamp(probability, 0.0f, 1.0f);
    }

    private EncounterResult? RollEncounter(EncounterZone zone, Vector2I tilePosition)
    {
        if (_groundLayer == null || _detailLayer == null || zone.EncounterData == null)
        {
            return null;
        }

        var possibleGroups = zone.EncounterData.PossibleGroups
            .Where(group => group != null && group.Weight > 0.0f && group.EnemyTypes.Length > 0)
            .ToArray();

        if (possibleGroups.Length == 0)
        {
            return null;
        }

        var totalWeight = possibleGroups.Sum(group => group.Weight);
        var weightRoll = _rng.Randf() * totalWeight;
        var selectedGroup = possibleGroups[^1];

        foreach (var group in possibleGroups)
        {
            if (weightRoll < group.Weight)
            {
                selectedGroup = group;
                break;
            }

            weightRoll -= group.Weight;
        }

        var minCount = Mathf.Max(selectedGroup.MinCount, 1);
        var maxCount = Mathf.Max(selectedGroup.MaxCount, minCount);
        var enemyCount = _rng.RandiRange(minCount, maxCount);
        var enemies = new string[enemyCount];

        for (var index = 0; index < enemyCount; index++)
        {
            enemies[index] = selectedGroup.EnemyTypes[_rng.RandiRange(0, selectedGroup.EnemyTypes.Length - 1)];
        }

        return new EncounterResult
        {
            ZoneName = zone.ZoneName,
            TerrainType = OverworldGrid.GetTileType(_groundLayer, _detailLayer, tilePosition),
            EnemyTypes = enemies,
            EnemyCount = enemyCount,
            PlayerReturnPosition = tilePosition
        };
    }

    private EncounterZone? ResolveCurrentZone(Vector2I tilePosition)
    {
        foreach (var zone in _zones)
        {
            if (zone.Contains(tilePosition))
            {
                return zone;
            }
        }

        return null;
    }

    private void CreateDebugOverlay()
    {
        var canvasLayer = new CanvasLayer
        {
            Name = "EncounterDebugCanvas"
        };

        _debugPanel = new PanelContainer
        {
            Name = "EncounterDebugPanel",
            Visible = false,
            Position = new Vector2(12.0f, 12.0f),
            CustomMinimumSize = new Vector2(280.0f, 0.0f)
        };

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 10);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_right", 10);
        margin.AddThemeConstantOverride("margin_bottom", 8);

        _debugLabel = new Label();
        margin.AddChild(_debugLabel);
        _debugPanel.AddChild(margin);
        canvasLayer.AddChild(_debugPanel);
        AddChild(canvasLayer);
    }

    private void ToggleDebugOverlay()
    {
        if (_debugPanel == null)
        {
            return;
        }

        _debugPanel.Visible = !_debugPanel.Visible;
        UpdateDebugOverlay();
    }

    private void UpdateDebugOverlay()
    {
        if (_debugLabel == null)
        {
            return;
        }

        _debugLabel.Text = $"Encounter chance: {CurrentEncounterProbability * 100.0f:0.0}%\n"
            + $"Steps since last encounter: {_stepsSinceLastEncounter}\n"
            + $"Encounter zone: {CurrentZoneName}";
    }
}
