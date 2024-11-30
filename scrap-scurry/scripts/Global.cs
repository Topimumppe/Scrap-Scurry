using Godot;
using System;

public partial class Global : Node
{
	Node inventory = ResourceLoader.Load<PackedScene>("res://scenes/inventory.tscn").Instantiate();

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
			GetTree().Root.AddChild(inventory);
			is_inventory_open = true;
		}
	}

	public void CloseInventory()
	{
		if (is_inventory_open)
		{
			GetTree().Root.RemoveChild(inventory);
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
