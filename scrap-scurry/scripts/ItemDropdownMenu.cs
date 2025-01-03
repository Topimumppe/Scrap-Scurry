using Godot;
using System;

public partial class ItemDropdownMenu : Control
{
	private TextureRect icon;
	private Button DeleteButton;
	private PlayerInventory playerInventory;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{ 
		Item item = PlayerInventory.Instance.LastInteractedItem;
		icon = GetNode<PanelContainer>("PanelContainer").GetNode<TextureRect>("TextureRect");
		DeleteButton = GetNode<Button>("ButtonContainer/VBoxContainer/DeleteButton");
		
		icon.Texture = item.Icon;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_delete_button_button_down()
	{
		GD.Print("Delete item");
	}
}
