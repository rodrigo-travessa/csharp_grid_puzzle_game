using Game.Autoloads;
using Game.Components;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Managers;

public partial class GridManager : Node
{
    [Signal]
    public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);
    [Export]
    private TileMapLayer highlightTileMapLayer;
    [Export]
    private TileMapLayer baseTerrainTileMapLayer;

    private List<TileMapLayer> allTileMapLayers = new();

    private HashSet<Vector2I> validBuildableTiles = new();

    private HashSet<Vector2I> collectedResourceTiles = new();

    private const string IS_BUILDABLE = "is_buildable";
    private const string IS_WOOD = "is_wood";

    public override void _Ready()
    {
        GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
        allTileMapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
    }

    public void HighlightBuildableTiles()
    {
        foreach (var tile in validBuildableTiles)
        {
            highlightTileMapLayer.SetCell(tile, 0, new Vector2I(0, 0));
        }
    }

    public void HighlightExpandableBuildableTiles(Vector2I tilePos, int radius)
    {
        HighlightBuildableTiles();
        var expansionTiles = GetValidTilesInRadius(tilePos, radius);
        var ValidExpansionTiles = expansionTiles.Except(validBuildableTiles);
        foreach (Vector2I tile in ValidExpansionTiles)
        {
            highlightTileMapLayer.SetCell(tile, 0, new Vector2I(1, 0));
        }
    }

  	public void HighlightResourceTiles(Vector2I rootCell, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(rootCell, radius);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in resourceTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

    public bool TileHasCustomData(Vector2I tilePosition, string dataName)
    {
        foreach (var layer in allTileMapLayers)
        {
            var customData = layer.GetCellTileData(tilePosition);
            if (customData == null) continue;
            return (bool)customData.GetCustomData(dataName);
        }
        return false;
    }

    public bool IsTilePositionBuildable(Vector2I tilePosition)
    {
        return validBuildableTiles.Contains(tilePosition);
    }
    public void ClearHighlightedTiles()
    {
        highlightTileMapLayer.Clear();
    }

    public Vector2I GetMouseGridCellPosition()
    {
        var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();
        var gridPos = mousePosition / 64;
        gridPos = gridPos.Floor();
        return new Vector2I((int)gridPos.X, (int)gridPos.Y);
    }
    private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPos();
        var resourceTiles = GetResourceTilesInRadius(rootCell, buildingComponent.BuildingResource.ResourceRadius);

        var oldResourceTilesCount = collectedResourceTiles.Count;
        collectedResourceTiles.UnionWith(resourceTiles);
        if (oldResourceTilesCount != collectedResourceTiles.Count)
        {
            EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);            
        }

    }

    private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
    {
        var rootCell = buildingComponent.GetGridCellPos();
        var validTiles = GetValidTilesInRadius(rootCell, buildingComponent.BuildingResource.BuildableRadius);
        validBuildableTiles.UnionWith(validTiles);
        validBuildableTiles.Remove(buildingComponent.GetGridCellPos());

        var listOfBuildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
        foreach (var building in listOfBuildingComponents)
        {
            validBuildableTiles.Remove(building.GetGridCellPos());
        }
    }
    private void OnBuildingPlaced(BuildingComponent buildingComponent)
    {
        UpdateValidBuildableTiles(buildingComponent);
        UpdateCollectedResourceTiles(buildingComponent);
    }

   	private List<Vector2I> GetTilesInRadius(Vector2I rootCell, int radius, Func<Vector2I, bool> filterFn)
	{
		var result = new List<Vector2I>();
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);
				if (!filterFn(tilePosition)) continue;
				result.Add(tilePosition);
			}
		}
		return result;
	}
    
   	private List<Vector2I> GetValidTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell, radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_BUILDABLE);
		});
	}

	private List<Vector2I> GetResourceTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell, radius, (tilePosition) =>
		{
			return TileHasCustomData(tilePosition, IS_WOOD);
		});
	}

    private List<TileMapLayer> GetAllTileMapLayers(TileMapLayer baseLayer)
    {
        var result = new List<TileMapLayer>();
        var children = baseLayer.GetChildren();
        children.Reverse();
        foreach (var child in children)
        {
            if (child is TileMapLayer childLayer)
            {
                result.AddRange(GetAllTileMapLayers(childLayer));
            }
        }
        result.Add(baseLayer);
        return result;
    }
}
