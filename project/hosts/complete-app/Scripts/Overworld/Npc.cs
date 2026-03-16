using System;
using Godot;
using UltimaMagic.Data;
using UltimaMagic.UI;

namespace UltimaMagic.Overworld;

public partial class Npc : CharacterBody2D, IInteractable
{
    private readonly RandomNumberGenerator _random = new();

    private Sprite2D _sprite = null!;
    private TileMapLayer _groundLayer = null!;
    private TileMapLayer _detailLayer = null!;
    private Tween? _moveTween;
    private Vector2I _tilePosition;
    private float _moveCooldown;
    private int _patrolIndex;
    private bool _playerNearby;

    [Export]
    public string NpcName { get; set; } = "Villager";

    [Export]
    public string DialogueId { get; set; } = string.Empty;

    [Export]
    public MovementPattern Pattern { get; set; } = MovementPattern.Stationary;

    public Vector2I[] PatrolWaypoints { get; set; } = [];

    [Export]
    public int TileSize { get; set; } = 32;

    [Export]
    public float MoveSpeed { get; set; } = 2.5f;

    [Export]
    public float MoveIntervalSeconds { get; set; } = 1.5f;

    [Export]
    public Color SpriteTint { get; set; } = Colors.White;

    public string InteractionPrompt => _playerNearby ? "Talk" : string.Empty;

    public enum MovementPattern
    {
        Stationary,
        Patrol,
        Random
    }

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _groundLayer = GetParent().GetNode<TileMapLayer>("TileMap/GroundLayer");
        _detailLayer = GetParent().GetNode<TileMapLayer>("TileMap/DetailLayer");

        var interactionArea = GetNode<Area2D>("InteractionArea");
        interactionArea.BodyEntered += OnInteractionAreaBodyEntered;
        interactionArea.BodyExited += OnInteractionAreaBodyExited;

        AddToGroup(OverworldGrid.InteractableGroup);
        AddToGroup(OverworldGrid.TileBlockerGroup);

        _sprite.Modulate = SpriteTint;
        _tilePosition = OverworldGrid.WorldToTile(Position, TileSize);
        Position = OverworldGrid.TileToWorld(_tilePosition, TileSize);
        _moveCooldown = MoveIntervalSeconds;
    }

    public override void _Process(double delta)
    {
        if (Pattern == MovementPattern.Stationary
            || _moveTween != null
            || DialogueBox.Instance?.IsOpen == true)
        {
            return;
        }

        _moveCooldown -= (float)delta;
        if (_moveCooldown > 0f)
        {
            return;
        }

        _moveCooldown = MoveIntervalSeconds;

        switch (Pattern)
        {
            case MovementPattern.Patrol:
                TryPatrolMove();
                break;
            case MovementPattern.Random:
                TryRandomMove();
                break;
        }
    }

    public void Interact(Player player)
    {
        var entry = DialogueDatabase.GetEntry(DialogueId);
        DialogueBox.Instance?.ShowDialogue(string.IsNullOrWhiteSpace(entry.Name) ? NpcName : entry.Name, entry.Lines);
    }

    private void TryPatrolMove()
    {
        if (PatrolWaypoints.Length < 2)
        {
            return;
        }

        if (_tilePosition == PatrolWaypoints[_patrolIndex])
        {
            _patrolIndex = (_patrolIndex + 1) % PatrolWaypoints.Length;
        }

        var targetTile = PatrolWaypoints[_patrolIndex];
        var delta = targetTile - _tilePosition;
        var direction = delta.X != 0
            ? new Vector2I(Math.Sign(delta.X), 0)
            : delta.Y != 0
                ? new Vector2I(0, Math.Sign(delta.Y))
                : Vector2I.Zero;

        TryMove(direction);
    }

    private void TryRandomMove()
    {
        var directions = new[]
        {
            Vector2I.Up,
            Vector2I.Down,
            Vector2I.Left,
            Vector2I.Right
        };

        var startIndex = _random.RandiRange(0, directions.Length - 1);
        for (var i = 0; i < directions.Length; i++)
        {
            var direction = directions[(startIndex + i) % directions.Length];
            if (TryMove(direction))
            {
                return;
            }
        }
    }

    private bool TryMove(Vector2I direction)
    {
        if (direction == Vector2I.Zero)
        {
            return false;
        }

        var targetTile = _tilePosition + direction;
        if (!OverworldGrid.IsWithinMap(_groundLayer, targetTile)
            || !OverworldGrid.IsWalkable(_groundLayer, _detailLayer, targetTile)
            || OverworldGrid.HasTileBlocker(GetTree(), targetTile, TileSize, this))
        {
            return false;
        }

        MoveTo(targetTile);
        return true;
    }

    private void MoveTo(Vector2I targetTile)
    {
        _moveTween = CreateTween();
        _tilePosition = targetTile;

        _moveTween
            .TweenProperty(this, "position", OverworldGrid.TileToWorld(targetTile, TileSize), 1.0d / MoveSpeed)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _moveTween.TweenCallback(Callable.From(OnMoveFinished));
    }

    private void OnMoveFinished()
    {
        _moveTween = null;
        Position = OverworldGrid.TileToWorld(_tilePosition, TileSize);
    }

    private void OnInteractionAreaBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            _playerNearby = true;
        }
    }

    private void OnInteractionAreaBodyExited(Node2D body)
    {
        if (body is Player)
        {
            _playerNearby = false;
        }
    }
}
