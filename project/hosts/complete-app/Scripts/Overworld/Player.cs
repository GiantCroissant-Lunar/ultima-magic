using Godot;

namespace UltimaMagic.Overworld;

public partial class Player : CharacterBody2D
{
    private const float TileCenterOffset = 0.5f;
    private const string WalkableCustomDataKey = "walkable";

    [Export]
    public int TileSize { get; set; } = 32;

    [Export]
    public float MoveSpeed { get; set; } = 4.0f;

    public Vector2I TilePosition { get; private set; }
    public bool IsMoving { get; private set; }
    public int StepsTaken { get; private set; }

    private RayCast2D _rayCast = null!;
    private TileMapLayer _groundLayer = null!;
    private TileMapLayer _detailLayer = null!;
    private Tween? _moveTween;

    public override void _Ready()
    {
        _rayCast = GetNode<RayCast2D>("RayCast2D");
        _groundLayer = GetParent().GetNode<TileMapLayer>("TileMap/GroundLayer");
        _detailLayer = GetParent().GetNode<TileMapLayer>("TileMap/DetailLayer");

        TilePosition = WorldToTile(Position);
        Position = TileToWorld(TilePosition);
    }

    public override void _Process(double delta)
    {
        if (IsMoving)
        {
            return;
        }

        var direction = GetInputDirection();
        if (direction != Vector2I.Zero)
        {
            TryMove(direction);
        }
    }

    private Vector2I GetInputDirection()
    {
        var upPressed = Input.IsActionPressed("move_up");
        var downPressed = Input.IsActionPressed("move_down");
        if (upPressed != downPressed)
        {
            return upPressed ? Vector2I.Up : Vector2I.Down;
        }

        var leftPressed = Input.IsActionPressed("move_left");
        var rightPressed = Input.IsActionPressed("move_right");
        if (leftPressed != rightPressed)
        {
            return leftPressed ? Vector2I.Left : Vector2I.Right;
        }

        return Vector2I.Zero;
    }

    private void TryMove(Vector2I direction)
    {
        var targetTile = TilePosition + direction;
        if (!IsWithinMap(targetTile) || !IsWalkable(targetTile))
        {
            return;
        }

        _rayCast.TargetPosition = new Vector2(direction.X, direction.Y) * TileSize;
        _rayCast.ForceRaycastUpdate();
        if (_rayCast.IsColliding())
        {
            return;
        }

        MoveTo(targetTile);
    }

    private void MoveTo(Vector2I targetTile)
    {
        _moveTween?.Kill();

        IsMoving = true;
        TilePosition = targetTile;

        _moveTween = CreateTween();
        _moveTween
            .TweenProperty(this, "position", TileToWorld(targetTile), 1.0d / MoveSpeed)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _moveTween.TweenCallback(Callable.From(OnMoveFinished));
    }

    private void OnMoveFinished()
    {
        _moveTween = null;
        Position = TileToWorld(TilePosition);
        IsMoving = false;
        StepsTaken++;
    }

    private bool IsWithinMap(Vector2I tile)
    {
        return _groundLayer.GetCellSourceId(tile) != -1;
    }

    private bool IsWalkable(Vector2I tile)
    {
        return IsLayerWalkable(_groundLayer, tile) && IsLayerWalkable(_detailLayer, tile);
    }

    private static bool IsLayerWalkable(TileMapLayer layer, Vector2I tile)
    {
        var tileData = layer.GetCellTileData(tile);
        return tileData == null
            || (tileData.HasCustomData(WalkableCustomDataKey)
                && tileData.GetCustomData(WalkableCustomDataKey).AsBool());
    }

    private Vector2I WorldToTile(Vector2 worldPosition)
    {
        return new Vector2I(
            Mathf.RoundToInt((worldPosition.X - (TileSize * TileCenterOffset)) / TileSize),
            Mathf.RoundToInt((worldPosition.Y - (TileSize * TileCenterOffset)) / TileSize));
    }

    private Vector2 TileToWorld(Vector2I tilePosition)
    {
        return new Vector2(
            (tilePosition.X + TileCenterOffset) * TileSize,
            (tilePosition.Y + TileCenterOffset) * TileSize);
    }
}
