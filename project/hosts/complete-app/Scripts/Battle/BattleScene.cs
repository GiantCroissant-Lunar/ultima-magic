using System;
using Godot;
using UltimaMagic.Autoload;
using UltimaMagic.Data;

namespace UltimaMagic.Battle;

public partial class BattleScene : Node3D
{
    private const float BaseEnemyPixelSize = 0.02f;
    private const float ReferenceEnemyDistance = 4.0f;
    private const float MinDistanceThreshold = 0.01f;
    private const float MinDistanceScale = 0.85f;
    private const float MaxDistanceScale = 1.15f;
    private const float MillisecondsPerSecond = 1000.0f;
    private const float TargetPulseSpeed = 4.0f;
    private const float TargetScaleBoost = 0.08f;
    private const float DeathFadeDurationSeconds = 0.45f;
    private const float DeathDropDistance = 0.45f;
    private const float DeathFallAngleDegrees = 70.0f;

    private static readonly Vector3[] EnemySlotPositions =
    [
        new Vector3(-2.0f, 0.5f, -4.0f),
        new Vector3(-0.7f, 0.5f, -3.5f),
        new Vector3(0.7f, 0.5f, -3.5f),
        new Vector3(2.0f, 0.5f, -4.0f)
    ];

    private static readonly Color[] PreviewTints =
    [
        new Color(1.0f, 0.85f, 0.85f, 1.0f),
        new Color(0.9f, 1.0f, 0.85f, 1.0f),
        new Color(0.85f, 0.95f, 1.0f, 1.0f),
        new Color(1.0f, 0.92f, 0.75f, 1.0f)
    ];

    private Camera3D _camera = null!;
    private Sprite3D[] _enemySlots = Array.Empty<Sprite3D>();
    private EnemyVisualState[] _enemyStates = Array.Empty<EnemyVisualState>();
    private Texture2D? _defaultEnemyTexture;
    private int _targetedEnemyIndex = -1;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _enemySlots =
        [
            GetNode<Sprite3D>("EnemySlots/EnemySlot1"),
            GetNode<Sprite3D>("EnemySlots/EnemySlot2"),
            GetNode<Sprite3D>("EnemySlots/EnemySlot3"),
            GetNode<Sprite3D>("EnemySlots/EnemySlot4")
        ];
        _enemyStates = new EnemyVisualState[_enemySlots.Length];
        _defaultEnemyTexture = _enemySlots[0].Texture;

        for (var index = 0; index < _enemySlots.Length; index++)
        {
            _enemyStates[index] = new EnemyVisualState();
            ConfigureEnemySlot(index);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentState = GameManager.GameState.Battle;
        }

        var pendingEncounter = SceneManager.Instance?.ConsumePendingEncounter();
        if (pendingEncounter != null)
        {
            LoadEncounter(pendingEncounter);
        }
        else
        {
            LoadEnemies(CreatePreviewEnemies(4));
        }

        TargetEnemy(FindFirstActiveEnemyIndex());
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        var pulse = (Mathf.Sin((float)Time.GetTicksMsec() / MillisecondsPerSecond * TargetPulseSpeed) + 1.0f) * 0.5f;

        for (var index = 0; index < _enemySlots.Length; index++)
        {
            var slot = _enemySlots[index];
            if (!slot.Visible)
            {
                continue;
            }

            var state = _enemyStates[index];
            if (state.IsDefeated)
            {
                UpdateDeathAnimation(index, slot, state, delta);
                continue;
            }

            slot.Position = state.BasePosition;
            slot.Rotation = Vector3.Zero;

            if (index == _targetedEnemyIndex)
            {
                slot.Scale = state.BaseScale * (1.0f + TargetScaleBoost * pulse);
                slot.Modulate = state.BaseTint.Lerp(new Color(1.0f, 0.93f, 0.62f, 1.0f), 0.35f + 0.2f * pulse);
            }
            else
            {
                slot.Scale = state.BaseScale;
                slot.Modulate = state.BaseTint;
            }
        }
    }

    public void LoadEnemies(EnemyBattleData[] enemies)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        for (var index = 0; index < enemies.Length; index++)
        {
            if (enemies[index] == null)
            {
                throw new ArgumentException($"Enemy at index {index} must not be null.", nameof(enemies));
            }
        }

        for (var index = 0; index < _enemySlots.Length; index++)
        {
            var slot = _enemySlots[index];
            var state = _enemyStates[index];

            if (index < enemies.Length)
            {
                var enemy = enemies[index];
                slot.Texture = enemy.Sprite ?? _defaultEnemyTexture;
                slot.Visible = slot.Texture != null;
                slot.Position = state.BasePosition;
                slot.Rotation = Vector3.Zero;
                slot.Scale = state.BaseScale;
                slot.Modulate = enemy.Tint;

                state.BaseTint = enemy.Tint;
                state.IsDefeated = false;
                state.DeathProgress = 0.0f;
            }
            else
            {
                HideEnemySlot(index);
            }
        }

        TargetEnemy(FindFirstActiveEnemyIndex());
    }

    public void LoadEncounter(EncounterResult encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);

        var encounterEnemyCount = encounter.EnemyCount > 0 ? encounter.EnemyCount : encounter.EnemyTypes.Length;
        var clampedCount = Math.Clamp(encounterEnemyCount, 0, _enemySlots.Length);
        var enemies = new EnemyBattleData[clampedCount];

        for (var index = 0; index < clampedCount; index++)
        {
            var enemyName = encounter.EnemyTypes.Length == 0
                ? $"Enemy {index + 1}"
                : encounter.EnemyTypes[index % encounter.EnemyTypes.Length];

            enemies[index] = new EnemyBattleData
            {
                Name = enemyName,
                Sprite = _defaultEnemyTexture,
                Tint = ResolveEnemyTint(enemyName, index)
            };
        }

        LoadEnemies(enemies);
    }

    public void TargetEnemy(int index)
    {
        _targetedEnemyIndex = index >= 0 && index < _enemySlots.Length && _enemySlots[index].Visible && !_enemyStates[index].IsDefeated
            ? index
            : FindFirstActiveEnemyIndex();
    }

    public void OnEnemyDefeated(int index)
    {
        if (index < 0 || index >= _enemySlots.Length)
        {
            return;
        }

        var slot = _enemySlots[index];
        var state = _enemyStates[index];
        if (!slot.Visible || state.IsDefeated)
        {
            return;
        }

        state.IsDefeated = true;
        state.DeathProgress = 0.0f;

        if (_targetedEnemyIndex == index)
        {
            _targetedEnemyIndex = FindNextActiveEnemyIndex(index);
        }
    }

    private void ConfigureEnemySlot(int index)
    {
        var slot = _enemySlots[index];
        slot.Position = EnemySlotPositions[index];
        slot.Visible = false;
        slot.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        slot.Shaded = false;
        slot.DoubleSided = true;
        slot.AlphaCut = SpriteBase3D.AlphaCutMode.Discard;
        slot.PixelSize = BaseEnemyPixelSize;

        var state = _enemyStates[index];
        state.BasePosition = slot.Position;
        state.BaseScale = ComputeDistanceScale(slot);
        state.BaseTint = Colors.White;
    }

    private Vector3 ComputeDistanceScale(Node3D slot)
    {
        var distance = _camera.GlobalPosition.DistanceTo(slot.GlobalPosition);
        var scaleFactor = Mathf.Clamp(ReferenceEnemyDistance / Mathf.Max(distance, MinDistanceThreshold), MinDistanceScale, MaxDistanceScale);
        return Vector3.One * scaleFactor;
    }

    private void UpdateDeathAnimation(int index, Sprite3D slot, EnemyVisualState state, double delta)
    {
        state.DeathProgress = Mathf.Min(1.0f, state.DeathProgress + (float)delta / DeathFadeDurationSeconds);
        slot.Position = state.BasePosition + Vector3.Down * DeathDropDistance * state.DeathProgress;
        slot.Rotation = new Vector3(0.0f, 0.0f, -Mathf.DegToRad(DeathFallAngleDegrees * state.DeathProgress));
        slot.Scale = state.BaseScale;
        slot.Modulate = new Color(state.BaseTint.R, state.BaseTint.G, state.BaseTint.B, 1.0f - state.DeathProgress);

        if (state.DeathProgress >= 1.0f)
        {
            HideEnemySlot(index);
            if (_targetedEnemyIndex < 0)
            {
                _targetedEnemyIndex = FindFirstActiveEnemyIndex();
            }
        }
    }

    private void HideEnemySlot(int index)
    {
        if (index < 0 || index >= _enemySlots.Length)
        {
            return;
        }

        _enemySlots[index].Visible = false;
        _enemySlots[index].Modulate = Colors.Transparent;
        _enemySlots[index].Rotation = Vector3.Zero;
        _enemySlots[index].Position = _enemyStates[index].BasePosition;
        _enemySlots[index].Scale = _enemyStates[index].BaseScale;
        _enemyStates[index].IsDefeated = false;
        _enemyStates[index].DeathProgress = 0.0f;
    }

    private int FindFirstActiveEnemyIndex()
    {
        for (var index = 0; index < _enemySlots.Length; index++)
        {
            if (_enemySlots[index].Visible && !_enemyStates[index].IsDefeated)
            {
                return index;
            }
        }

        return -1;
    }

    private int FindNextActiveEnemyIndex(int defeatedIndex)
    {
        for (var offset = 1; offset <= _enemySlots.Length; offset++)
        {
            var candidateIndex = (defeatedIndex + offset) % _enemySlots.Length;
            if (_enemySlots[candidateIndex].Visible && !_enemyStates[candidateIndex].IsDefeated)
            {
                return candidateIndex;
            }
        }

        return -1;
    }

    private EnemyBattleData[] CreatePreviewEnemies(int count)
    {
        var previewCount = Math.Clamp(count, 0, _enemySlots.Length);
        var previewEnemies = new EnemyBattleData[previewCount];

        for (var index = 0; index < previewEnemies.Length; index++)
        {
            previewEnemies[index] = new EnemyBattleData
            {
                Name = $"Preview Enemy {index + 1}",
                Sprite = _defaultEnemyTexture,
                Tint = PreviewTints[index % PreviewTints.Length]
            };
        }

        return previewEnemies;
    }

    private static Color ResolveEnemyTint(string enemyName, int index)
    {
        if (string.IsNullOrWhiteSpace(enemyName))
        {
            return PreviewTints[index % PreviewTints.Length];
        }

        var hash = Math.Abs(enemyName.GetHashCode() % PreviewTints.Length);
        return PreviewTints[hash];
    }

    private sealed class EnemyVisualState
    {
        public Vector3 BasePosition { get; set; } = Vector3.Zero;

        public Vector3 BaseScale { get; set; } = Vector3.One;

        public Color BaseTint { get; set; } = Colors.White;

        public bool IsDefeated { get; set; }

        public float DeathProgress { get; set; }
    }
}
