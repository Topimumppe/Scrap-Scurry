using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class Inventory : Control
{
	private GridContainer gridContainer;
	private PackedScene inventoryButton;
	[Export]
	private string itemButtonPath = "res://UI/inventory_Button.tscn";
	[Export]
	private int Capacity { get; set; } = 9;

	public InventoryButton GrabbedObject { get; set; }
	public InventoryButton HoverOverButton { get; set; }
	private Vector2 lastMouseClickedPos;
	private bool overTrash;


	private List<Item> items = new List<Item>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gridContainer = GetNode<GridContainer>("CenterContainer/GridContainer");
		inventoryButton = ResourceLoader.Load<PackedScene>(itemButtonPath);
		PopulateButtons();
	}

	private void PopulateButtons()
	{
		for (int i = 0; i < Capacity; i++)
		{
			InventoryButton currentButton = inventoryButton.Instantiate<InventoryButton>();
			gridContainer.AddChild(currentButton);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<Area2D>("MouseArea2D").Position = GetTree().Root.GetMousePosition();
		if (HoverOverButton != null)
		{
			//GD.Print("Currently on = "+HoverOverButton.GetIndex());
			if (Input.IsActionJustPressed("Throw"))
			{
				GrabbedObject = HoverOverButton;
				lastMouseClickedPos = GetTree().Root.GetMousePosition();
			}

			if (lastMouseClickedPos.DistanceTo(GetTree().Root.GetMousePosition()) > 2)
			{
				if (Input.IsActionPressed("Throw"))
				{
					InventoryButton button = GetNode<Area2D>("MouseArea2D").GetNode<InventoryButton>("InventoryButton");
					button.Show();
					button.UpdateItem(GrabbedObject.InventoryItem, 0);
					
				}
				if (Input.IsActionJustReleased("Throw"))
				{
					if (overTrash)
					{
						DeleteButton(GrabbedObject);
					} else {
					swapButtons(GrabbedObject, HoverOverButton);
					InventoryButton button = GetNode<Area2D>("MouseArea2D").GetNode<InventoryButton>("InventoryButton");
					button.Hide();
					}
				}

			}
		}
		if (Input.IsActionJustReleased("Throw") && overTrash)
		{
			DeleteButton(GrabbedObject);
		}
	}

	public void DeleteButton(InventoryButton inventoryButton)
	{
		items.Remove(inventoryButton.InventoryItem);
		reflowButtons();
		InventoryButton button = GetNode<Area2D>("MouseArea2D").GetNode<InventoryButton>("InventoryButton");
		button.Hide();
	}	

	private void swapButtons(InventoryButton button1, InventoryButton button2)
	{
		int buttonindex = button1.GetIndex();
		int button2index = button2.GetIndex();
		gridContainer.MoveChild(button1, button2index);
		gridContainer.MoveChild(button2, buttonindex);
	}

	public bool Remove(Item item)
	{
		if (canAfford(item))
		{
			Item currentItem = item.Copy();
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].ID == currentItem.ID)
				{
					if (items[i].Quantity - currentItem.Quantity < 0)
					{
						currentItem.Quantity -= items[i].Quantity;
						items[i].Quantity = 0;
						UpdateButton(i);
					}
					else
					{
						items[i].Quantity -= currentItem.Quantity;
						currentItem.Quantity = 0;
						UpdateButton(i);
					}
				}

				if (currentItem.Quantity <= 0)
				{
					break;
				}
			}
			items.RemoveAll(x => x.Quantity <= 0);
			if (currentItem.Quantity > 0)
			{
				Remove(currentItem);
			}
			reflowButtons();
			return true;
		}
		return false;
	}

	private bool canAfford(Item item)
	{
		List<Item> currentItems = items.Where(x => x.ID == item.ID).ToList();
		int i = 0;
		foreach (var item1 in currentItems)
		{
			i += item1.Quantity;
		}
		if (item.Quantity < i)
		{
			return true;
		}
		return false;
	}

	private void reflowButtons()
	{
		for (int i = 0; i < Capacity; i++)
		{
			UpdateButton(i);
		}
	}

	public void Add(Item item)
	{
		Item currentItem = item.Copy(); // kopio
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].ID == currentItem.ID && items[i].Quantity != items[i].StackSize) //jos itemin stackSize ei ole täynnä
			{
				if (items[i].Quantity + currentItem.Quantity > items[i].StackSize) //jos itemin stackSize menee yli
				{
					currentItem.Quantity = items[i].Quantity + currentItem.Quantity - items[i].StackSize; // tallenna kopioon itemin ylijäämä
					items[i].Quantity = currentItem.StackSize;
					UpdateButton(i);
				}
				else //jos itemin stacSize ei mene yli
				{
					items[i].Quantity += currentItem.Quantity; // lisää itemin stakkiin määrä
					currentItem.Quantity = 0;
					UpdateButton(i);
				}
			}
		}
		if (currentItem.Quantity > 0) //itemin ylijäämä
		{
			if (currentItem.Quantity < currentItem.StackSize) //jos itemin ylijäämä on vähemmän kuin stackSize
			{
				items.Add(currentItem); // lisää ylijäämä seuraavaan slottiin
				UpdateButton(items.Count - 1);
			}
			else // jos itemin ylijäämä on enemmän kuin stackSize
			{
				Item tempItem = currentItem.Copy();
				tempItem.Quantity = currentItem.StackSize;
				items.Add(tempItem);
				UpdateButton(items.Count - 1);
				currentItem.Quantity -= currentItem.StackSize;
				Add(currentItem);
			}

		}
	}

	public void UpdateButton(int index)
	{
		if (items.ElementAtOrDefault(index) != null)
		{
			gridContainer.GetChild<InventoryButton>(index).UpdateItem(items[index], index);
		}
		else
		{
			gridContainer.GetChild<InventoryButton>(index).UpdateItem(null, index);
		}
	}

	public void _on_add_button_button_down()
	{
		Add(ResourceLoader.Load<Item>("res://scenes/items/TestItem.tres"));
	}
	public void _on_delete_button_button_down()
	{
		Remove(ResourceLoader.Load<Item>("res://scenes/items/TestItem.tres"));
	}
	public void _on_mouse_area_2d_area_entered(Area2D area)
	{
		//GD.Print("Entered");
		Control button = area.GetParent<Control>();
		if (button is InventoryButton hoverButton)
		{
			HoverOverButton = hoverButton;
		}
	}

	public void _on_mouse_area_2d_area_exited(Area2D area) => HoverOverButton = null;

	public void _on_trash_area_2d_area_entered(Area2D area)	=> overTrash = true;

	public void _on_trash_area_2d_area_exited(Area2D area) => overTrash = false;
}
