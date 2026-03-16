using Godot;

namespace UltimaMagic.Overworld;

public partial class Overworld : Node2D
{
    private const int MapWidth = 40;
    private const int MapHeight = 30;
    private const int TileSize = 32;
    private const int TerrainSourceId = 0;

    private static readonly Vector2I GrassTile = new(0, 0);
    private static readonly Vector2I WaterTile = new(1, 0);
    private static readonly Vector2I MountainTile = new(2, 0);
    private static readonly Vector2I ForestTile = new(3, 0);
    private static readonly Vector2I PathTile = new(4, 0);
    private static readonly Vector2I TownTile = new(5, 0);
    private static readonly Vector2I PlayerStartTile = new(6, 15);

    private TileMapLayer _groundLayer = null!;
    private TileMapLayer _detailLayer = null!;
    private Node2D _player = null!;

    public override void _Ready()
    {
        _groundLayer = GetNode<TileMapLayer>("TileMap/GroundLayer");
        _detailLayer = GetNode<TileMapLayer>("TileMap/DetailLayer");
        _player = GetNode<Node2D>("Player");

        BuildMap();
        PositionPlayer(PlayerStartTile);
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

        _groundLayer.UpdateInternals();
        _detailLayer.UpdateInternals();
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
        FillGroundRect(new Rect2I(34, 10, 3, 3), WaterTile);
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

    private void PositionPlayer(Vector2I tilePosition)
    {
        _player.Position = new Vector2(
            (tilePosition.X + 0.5f) * TileSize,
            (tilePosition.Y + 0.5f) * TileSize);
    }
}
