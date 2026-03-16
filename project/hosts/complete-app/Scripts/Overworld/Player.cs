using Godot;
using UltimaMagic.UI;

namespace UltimaMagic.Overworld;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void StepTakenEventHandler(Vector2I tilePosition);

    [Export]
    public int TileSize { get; set; } = 32;

    [Export]
    public float MoveSpeed { get; set; } = 4.0f;

    public Vector2I TilePosition { get; private set; }
    public Vector2I FacingDirection { get; private set; } = Vector2I.Down;
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
        TileSize = OverworldGrid.ResolveTileSize(_groundLayer, TileSize);

        AddToGroup(OverworldGrid.TileBlockerGroup);

        TilePosition = OverworldGrid.WorldToTile(Position, TileSize);
        Position = OverworldGrid.TileToWorld(TilePosition, TileSize);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("interact"))
        {
            if (DialogueBox.Instance?.IsOpen == true)
            {
                DialogueBox.Instance.AdvanceLine();
            }
            else
            {
                TryInteract();
            }

            return;
        }

        if (IsMoving || DialogueBox.Instance?.IsOpen == true)
        {
            return;
        }

        var direction = GetInputDirection();
        if (direction != Vector2I.Zero)
        {
            FacingDirection = direction;
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
        if (!OverworldGrid.IsWithinMap(_groundLayer, targetTile)
            || !OverworldGrid.IsWalkable(_groundLayer, _detailLayer, targetTile)
            || OverworldGrid.HasTileBlocker(GetTree(), targetTile, TileSize))
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
            .TweenProperty(this, "position", OverworldGrid.TileToWorld(targetTile, TileSize), 1.0d / MoveSpeed)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        _moveTween.TweenCallback(Callable.From(OnMoveFinished));
    }

    private void OnMoveFinished()
    {
        _moveTween = null;
        Position = OverworldGrid.TileToWorld(TilePosition, TileSize);
        IsMoving = false;
        StepsTaken++;
        EmitSignal(SignalName.StepTaken, TilePosition);
    }

    private void TryInteract()
    {
        var targetTile = TilePosition + FacingDirection;
        OverworldGrid.FindInteractable(GetTree(), targetTile, TileSize)?.Interact(this);
    }
}
