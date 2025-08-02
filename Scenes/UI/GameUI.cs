using Godot;
using System;

namespace Game.UI;

public partial class GameUI : MarginContainer
{
    private Button placeTowerButton;
    private Button placeVillageButton;

    [Signal]
    public delegate void PlaceTowerButtonPressedEventHandler();
    [Signal]
    public delegate void PlaceVillageButtonPressedEventHandler();


    public override void _Ready()
    {

        placeTowerButton = GetNode<Button>("%PlaceTowerButton");
        placeVillageButton = GetNode<Button>("%PlaceVillageButton");

        placeTowerButton.Pressed += OnTowerButtonPressed;
        placeVillageButton.Pressed += OnVillageButtonPressed;
    }
    
    private void OnVillageButtonPressed()
	{
        EmitSignal(SignalName.PlaceVillageButtonPressed);
	}

    private void OnTowerButtonPressed()
    {
        EmitSignal(SignalName.PlaceTowerButtonPressed);
    }

}
