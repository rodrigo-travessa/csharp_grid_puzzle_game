using Game.Managers;
using Game.Resources.Buildings;
using Game.UI;
using Godot;

namespace Game;

public partial class Main : Node
{

	private GridManager gridManager;
	private Sprite2D cursor;
	private Node2D YSortRoot;

	private Vector2I? hoveredGridCell;
	private BuildingResource toPlaceBuildingResource;

	private GameUI gameUI;

	public override void _Ready()
	{
		cursor = GetNode<Sprite2D>("Cursor");	
		gridManager = GetNode<GridManager>("GridManager");
		YSortRoot = GetNode<Node2D>("YSortRoot");
		cursor.Visible = false;
		gameUI = GetNode<GameUI>("GameUI");

		gameUI.BuildingResourceSelected += OnBuildingResourceSelected;

		gridManager.ResourceTilesUpdated += OnResourceTilesUpdated;
	}

	public override void _Process(double delta)
	{
		var gridPosition = gridManager.GetMouseGridCellPosition();
		cursor.GlobalPosition = gridPosition * 64;

		if (toPlaceBuildingResource != null && cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			gridManager.ClearHighlightedTiles();
			gridManager.HighlightExpandableBuildableTiles(hoveredGridCell.Value, toPlaceBuildingResource.BuildableRadius);
			gridManager.HighlightResourceTiles(hoveredGridCell.Value, toPlaceBuildingResource.ResourceRadius);
		}
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (evt.IsActionPressed("left_click") && hoveredGridCell.HasValue && gridManager.IsTilePositionBuildable(hoveredGridCell.Value))
		{
			PlaceBuidingAtHoveredPosition();
			cursor.Visible = false;
		}
	}

	private void PlaceBuidingAtHoveredPosition()
	{
		if (!hoveredGridCell.HasValue)
		{
			return;
		}
		var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		YSortRoot.AddChild(building);

		building.GlobalPosition = hoveredGridCell.Value * 64;

		hoveredGridCell = null;
		gridManager.ClearHighlightedTiles();
	}

	private void OnBuildingResourceSelected(BuildingResource resource)
	{
		toPlaceBuildingResource = resource;
		cursor.Visible = !cursor.Visible;
		gridManager.HighlightBuildableTiles();
	}

	private void OnResourceTilesUpdated(int quantity)
	{
		GD.Print(quantity);
	}
}
