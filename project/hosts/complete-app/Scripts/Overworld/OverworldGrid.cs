using Godot;

namespace UltimaMagic.Overworld;

public static class OverworldGrid
{
    public const int DefaultTileSize = 32;
    public const float TileCenterOffset = 0.5f;
    public const string WalkableCustomDataKey = "walkable";
    public const string InteractableGroup = "interactable";
    public const string TileBlockerGroup = "tile_blocker";

    public static Vector2 TileToWorld(Vector2I tilePosition, int tileSize)
    {
        return new Vector2(
            (tilePosition.X + TileCenterOffset) * tileSize,
            (tilePosition.Y + TileCenterOffset) * tileSize);
    }

    public static Vector2I WorldToTile(Vector2 worldPosition, int tileSize)
    {
        return new Vector2I(
            Mathf.RoundToInt((worldPosition.X - (tileSize * TileCenterOffset)) / tileSize),
            Mathf.RoundToInt((worldPosition.Y - (tileSize * TileCenterOffset)) / tileSize));
    }

    public static bool IsWithinMap(TileMapLayer groundLayer, Vector2I tile)
    {
        return groundLayer.GetCellSourceId(tile) != -1;
    }

    public static int ResolveTileSize(TileMapLayer layer, int fallbackTileSize = DefaultTileSize)
    {
        var tileSet = layer.TileSet;
        if (tileSet == null)
        {
            GD.PushError($"TileMapLayer '{layer.Name}' is missing a TileSet. Falling back to tile size {fallbackTileSize}.");
            return fallbackTileSize;
        }

        var tileSize = tileSet.TileSize;
        if (tileSize.X <= 0 || tileSize.Y <= 0)
        {
            GD.PushError($"TileSet '{tileSet.ResourcePath}' has invalid tile size {tileSize}. Falling back to tile size {fallbackTileSize}.");
            return fallbackTileSize;
        }

        if (tileSize.X != tileSize.Y)
        {
            GD.PushWarning($"TileSet '{tileSet.ResourcePath}' uses non-square tiles {tileSize}. Using X dimension {tileSize.X} for overworld grid calculations.");
        }

        return tileSize.X;
    }

    public static bool IsWalkable(TileMapLayer groundLayer, TileMapLayer detailLayer, Vector2I tile)
    {
        return IsLayerWalkable(groundLayer, tile) && IsLayerWalkable(detailLayer, tile);
    }

    public static bool HasTileBlocker(SceneTree tree, Vector2I tile, int tileSize, Node2D? exclude = null)
    {
        foreach (var node in tree.GetNodesInGroup(TileBlockerGroup))
        {
            if (node == exclude || node is not Node2D node2D)
            {
                continue;
            }

            if (WorldToTile(node2D.GlobalPosition, tileSize) == tile)
            {
                return true;
            }
        }

        return false;
    }

    public static IInteractable? FindInteractable(SceneTree tree, Vector2I tile, int tileSize)
    {
        foreach (var node in tree.GetNodesInGroup(InteractableGroup))
        {
            if (node is Node2D node2D
                && node is IInteractable interactable
                && WorldToTile(node2D.GlobalPosition, tileSize) == tile)
            {
                return interactable;
            }
        }

        return null;
    }

    private static bool IsLayerWalkable(TileMapLayer layer, Vector2I tile)
    {
        var tileData = layer.GetCellTileData(tile);
        return tileData == null
            || (tileData.HasCustomData(WalkableCustomDataKey)
                && tileData.GetCustomData(WalkableCustomDataKey).AsBool());
    }
}
