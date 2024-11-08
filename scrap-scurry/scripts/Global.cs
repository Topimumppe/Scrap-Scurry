using Godot;
using System;

public partial class Global : Node
{
	private Node inventory_screen = ResourceLoader.Load<PackedScene>("res://scenes/inventory_screen.tscn").Instantiate();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("inventory"))
		{
			ToggleInventory();
		}

	}

	bool is_inventory_open = false;
	public void OpenInventory()
	{
		if (!is_inventory_open)
		{
			GetTree().Root.AddChild(inventory_screen);
			is_inventory_open = true;
		}
	}

	public void CloseInventory()
	{
		if (is_inventory_open)
		{
			GetTree().Root.RemoveChild(inventory_screen);
			is_inventory_open = false;
		}
	}

	public void ToggleInventory()
	{
		if (!is_inventory_open)
		{
			OpenInventory();
		}
		else
		{
			CloseInventory();
		}
	}
}
