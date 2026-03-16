using Godot;
using UltimaMagic.UI;

namespace UltimaMagic.Overworld;

public partial class Overworld : Node2D
{
    private const int MapWidth = 40;
    private const int MapHeight = 30;
    private const int TerrainSourceId = 0;

    private static readonly Vector2I GrassTile = new(0, 0);
    private static readonly Vector2I WaterTile = new(1, 0);
    private static readonly Vector2I MountainTile = new(2, 0);
    private static readonly Vector2I ForestTile = new(3, 0);
    private static readonly Vector2I PathTile = new(4, 0);
    private static readonly Vector2I TownTile = new(5, 0);

    private const string NpcScenePath = "res://Scenes/Overworld/Npc.tscn";
    private const string SignScenePath = "res://Scenes/Overworld/Sign.tscn";
    private const string ChestScenePath = "res://Scenes/Overworld/Chest.tscn";
    private const string DialogueBoxScenePath = "res://Scenes/UI/DialogueBox.tscn";
    private const int TileSize = 32;

    private TileMapLayer _groundLayer = null!;
    private TileMapLayer _detailLayer = null!;

    public override void _Ready()
    {
        _groundLayer = GetNode<TileMapLayer>("TileMap/GroundLayer");
        _detailLayer = GetNode<TileMapLayer>("TileMap/DetailLayer");

        BuildMap();
        PopulateScene();
    }

    private void BuildMap()
    {
        _groundLayer.Clear();
        _detailLayer.Clear();

        FillGround(GrassTile);
        PlaceWaterFeatures();
        PlaceRoads();
        PlaceTown();
        PlaceForest();
        PlaceMountains();
    }

    private void PopulateScene()
    {
        var uiLayer = new CanvasLayer
        {
            Name = "UI"
        };
        uiLayer.AddChild(ResourceLoader.Load<PackedScene>(DialogueBoxScenePath).Instantiate<DialogueBox>());
        AddChild(uiLayer);

        AddChild(CreateNpc("Village Elder", "elder", new Vector2I(27, 7), new Color(0.94f, 0.94f, 1.0f, 1.0f), Npc.MovementPattern.Stationary));
        AddChild(CreateNpc("Road Merchant", "merchant", new Vector2I(24, 15), new Color(0.74f, 1.0f, 0.74f, 1.0f), Npc.MovementPattern.Patrol, new Vector2I(24, 15), new Vector2I(28, 15)));
        AddChild(CreateNpc("Town Scout", "scout", new Vector2I(29, 8), new Color(1.0f, 0.82f, 0.82f, 1.0f), Npc.MovementPattern.Random));

        AddChild(CreateSign("Town Sign", "town_sign", new Vector2I(25, 7)));
        AddChild(CreateChest(new Vector2I(10, 8), "forest_chest_closed", "forest_chest_opened", "Healing Herb"));
    }

    private static Npc CreateNpc(string npcName, string dialogueId, Vector2I tile, Color spriteTint, Npc.MovementPattern pattern, params Vector2I[] patrolWaypoints)
    {
        var npc = ResourceLoader.Load<PackedScene>(NpcScenePath).Instantiate<Npc>();
        npc.Name = npcName.Replace(" ", string.Empty);
        npc.NpcName = npcName;
        npc.DialogueId = dialogueId;
        npc.Pattern = pattern;
        npc.PatrolWaypoints = patrolWaypoints;
        npc.SpriteTint = spriteTint;
        npc.Position = OverworldGrid.TileToWorld(tile, TileSize);
        return npc;
    }

    private static Sign CreateSign(string displayName, string dialogueId, Vector2I tile)
    {
        var sign = ResourceLoader.Load<PackedScene>(SignScenePath).Instantiate<Sign>();
        sign.Name = displayName.Replace(" ", string.Empty);
        sign.DisplayName = displayName;
        sign.DialogueId = dialogueId;
        sign.Position = OverworldGrid.TileToWorld(tile, TileSize);
        return sign;
    }

    private static Chest CreateChest(Vector2I tile, string closedDialogueId, string openedDialogueId, string itemName)
    {
        var chest = ResourceLoader.Load<PackedScene>(ChestScenePath).Instantiate<Chest>();
        chest.Name = "ForestChest";
        chest.ClosedDialogueId = closedDialogueId;
        chest.OpenedDialogueId = openedDialogueId;
        chest.ItemName = itemName;
        chest.Position = OverworldGrid.TileToWorld(tile, TileSize);
        return chest;
    }

    private void FillGround(Vector2I atlasCoords)
    {
        for (var y = 0; y < MapHeight; y++)
        {
            for (var x = 0; x < MapWidth; x++)
            {
                _groundLayer.SetCell(new Vector2I(x, y), TerrainSourceId, atlasCoords);
            }
        }
    }

    private void PlaceWaterFeatures()
    {
        FillGroundRect(new Rect2I(1, 20, 9, 8), WaterTile);
        FillGroundRect(new Rect2I(33, 0, 4, 12), WaterTile);
        FillGroundRect(new Rect2I(31, 10, 2, 3), WaterTile);
    }

    private void PlaceRoads()
    {
        FillGroundRect(new Rect2I(4, 15, 32, 2), PathTile);
        FillGroundRect(new Rect2I(28, 5, 2, 19), PathTile);
        FillGroundRect(new Rect2I(14, 6, 2, 10), PathTile);
    }

    private void PlaceTown()
    {
        FillGroundRect(new Rect2I(26, 6, 6, 4), PathTile);
        FillDetailRect(new Rect2I(27, 6, 4, 3), TownTile);
    }

    private void PlaceForest()
    {
        FillDetailRect(new Rect2I(5, 4, 8, 7), ForestTile);
        FillDetailRect(new Rect2I(8, 11, 4, 3), ForestTile);
        CarveDetailRect(new Rect2I(7, 6, 2, 2));
        CarveDetailRect(new Rect2I(10, 8, 2, 2));
    }

    private void PlaceMountains()
    {
        FillDetailRect(new Rect2I(18, 21, 8, 3), MountainTile);
        FillDetailRect(new Rect2I(23, 18, 9, 3), MountainTile);
        FillDetailRect(new Rect2I(29, 16, 6, 2), MountainTile);
    }

    private void FillGroundRect(Rect2I area, Vector2I atlasCoords)
    {
        for (var y = area.Position.Y; y < area.End.Y; y++)
        {
            for (var x = area.Position.X; x < area.End.X; x++)
            {
                _groundLayer.SetCell(new Vector2I(x, y), TerrainSourceId, atlasCoords);
            }
        }
    }

    private void FillDetailRect(Rect2I area, Vector2I atlasCoords)
    {
        for (var y = area.Position.Y; y < area.End.Y; y++)
        {
            for (var x = area.Position.X; x < area.End.X; x++)
            {
                _detailLayer.SetCell(new Vector2I(x, y), TerrainSourceId, atlasCoords);
            }
        }
    }

    private void CarveDetailRect(Rect2I area)
    {
        for (var y = area.Position.Y; y < area.End.Y; y++)
        {
            for (var x = area.Position.X; x < area.End.X; x++)
            {
                _detailLayer.EraseCell(new Vector2I(x, y));
            }
        }
    }
}
