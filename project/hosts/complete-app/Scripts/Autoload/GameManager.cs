using System;
using System.Collections.Generic;
using Godot;
using UltimaMagic.Data;

namespace UltimaMagic.Autoload;

public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }
    private readonly List<string> _inventoryItems = [];

    public enum GameState
    {
        Overworld,
        Battle,
        Menu,
        Cutscene
    }

    public GameState CurrentState { get; set; } = GameState.Overworld;
    public bool IsInputEnabled { get; private set; } = true;
    public int PlayerHp { get; private set; } = 30;
    public int PlayerMp { get; private set; } = 10;
    public Vector2I OverworldReturnPosition { get; private set; } = Vector2I.Zero;
    public IReadOnlyList<string> InventoryItems => _inventoryItems;

    public override void _Ready()
    {
        Instance = this;
    }

    public void SetInputEnabled(bool enabled)
    {
        IsInputEnabled = enabled;
    }

    public PlayerStateSnapshot CreatePlayerStateSnapshot(Vector2I returnPosition)
    {
        OverworldReturnPosition = returnPosition;
        return new PlayerStateSnapshot
        {
            CurrentHp = PlayerHp,
            CurrentMp = PlayerMp,
            ReturnPosition = returnPosition
        };
    }

    public void ApplyBattleResult(BattleResult battleResult)
    {
        ArgumentNullException.ThrowIfNull(battleResult);

        PlayerHp = Math.Max(0, battleResult.RemainingHp);
        PlayerMp = Math.Max(0, battleResult.RemainingMp);
        OverworldReturnPosition = battleResult.PlayerReturnPosition;

        foreach (var item in battleResult.ItemsGained)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                _inventoryItems.Add(item);
            }
        }
    }

    public override void _Process(double delta)
    {
    }
}
