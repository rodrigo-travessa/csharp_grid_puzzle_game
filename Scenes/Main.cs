using Game.Managers;
using Game.Resources.Buildings;
using Godot;

namespace Game;

public partial class Main : Node
{

	private GridManager gridManager;
	private Sprite2D cursor;
	private BuildingResource towerScene;
	private BuildingResource villageScene;
	private Button placeTowerButton;
	private Button placeVillageButton;
	private Node2D YSortRoot;

	private Vector2I? hoveredGridCell;
	private BuildingResource toPlaceBuildingResource;

	public override void _Ready()
	{
		cursor = GetNode<Sprite2D>("Cursor");
		towerScene = GD.Load<BuildingResource>("res://Assets/Resources/Buildings/Tower.tres");
		villageScene = GD.Load<BuildingResource>("res://Assets/Resources/Buildings/Village.tres");
		placeTowerButton = GetNode<Button>("PlaceTowerButton");
		placeVillageButton = GetNode<Button>("PlaceVillageButton");
		gridManager = GetNode<GridManager>("GridManager");
		YSortRoot = GetNode<Node2D>("YSortRoot");
		cursor.Visible = false;

		placeTowerButton.Pressed += OnTowerButtonPressed;
		placeVillageButton.Pressed += OnVillageButtonPressed;
		//Alternate Way of connecting signals
		// placeBuildingButton.Connect(Button.SignalName.Pressed, Callable.From(OnButtonPressed));

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

	private void OnVillageButtonPressed()
	{
		toPlaceBuildingResource = villageScene;
		cursor.Visible = !cursor.Visible;
		gridManager.HighlightBuildableTiles();
	}

	private void OnTowerButtonPressed()
	{
		toPlaceBuildingResource = towerScene;
		cursor.Visible = !cursor.Visible;
		gridManager.HighlightBuildableTiles();
	}

	private void OnResourceTilesUpdated(int quantity)
	{
		GD.Print(quantity);
	}
}
