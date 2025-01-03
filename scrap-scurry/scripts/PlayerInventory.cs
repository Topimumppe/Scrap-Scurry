using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

public partial class PlayerInventory : Control
{
	public static PlayerInventory Instance { get; private set; }
	Node DropDownMenu;  // = ResourceLoader.Load<PackedScene>("res://scenes/item_dropdown_menu.tscn").Instantiate();
	private GridContainer gridContainer;
	private Panel panel1;
	private Panel panel2;
	private Panel panel3;
	private Panel panel4;
	private Panel panel5;
	private Panel panel6;
	private Panel panel7;
	private Panel panel8;
	private Panel panel9;
	private TextureRect icon;
	private Label quantityLabel;
	private List<Panel> panels = new List<Panel>();
	private List<Item> items = new List<Item>(8);
	private Item inventoryItem;
	private Panel HoverOverPanel { get; set; }
	private Item GrabbedItem { get; set; }
	public Item LastInteractedItem { get; set; }
	private bool itemIsGrabbed;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		gridContainer = GetNode<GridContainer>("GridContainer");
		populatePanels();
	}

	public void populatePanels()
	{
		panel1 = gridContainer.GetChild<Panel>(0);
		panel2 = gridContainer.GetChild<Panel>(1);
		panel3 = gridContainer.GetChild<Panel>(2);
		panel4 = gridContainer.GetChild<Panel>(3);
		panel5 = gridContainer.GetChild<Panel>(4);
		panel6 = gridContainer.GetChild<Panel>(5);
		panel7 = gridContainer.GetChild<Panel>(6);
		panel8 = gridContainer.GetChild<Panel>(7);
		panel9 = gridContainer.GetChild<Panel>(8);

		panels.Add(panel1);
		panels.Add(panel2);
		panels.Add(panel3);
		panels.Add(panel4);
		panels.Add(panel5);
		panels.Add(panel6);
		panels.Add(panel7);
		panels.Add(panel8);
		panels.Add(panel9);

		for (int i = 0; i < panels.Count; i++)
		{
			items.Add(null);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<Area2D>("MouseArea2D").Position = GetTree().Root.GetMousePosition();
		if (HoverOverPanel != null)
		{
			if (Input.IsActionPressed("Throw") && items.ElementAtOrDefault(HoverOverPanel.GetIndex()) != null && itemIsGrabbed == false)
			{
				GrabbedItem = items.ElementAtOrDefault(HoverOverPanel.GetIndex());
				itemIsGrabbed = true;
				Panel panel = GetNode<Area2D>("MouseArea2D").GetNode<Panel>("ExamplePanel");
				Label panelLabel = panel.GetNode<Label>("Label");
				TextureRect panelIcon = panel.GetNode<TextureRect>("TextureRect");

				panelLabel.Text = GrabbedItem.Quantity.ToString();
				panelIcon.Texture = GrabbedItem.Icon;
				panel.Show();
			}
			if (Input.IsActionJustReleased("Throw") && itemIsGrabbed == true)
			{
				itemIsGrabbed = false;
				Panel panel = GetNode<Area2D>("MouseArea2D").GetNode<Panel>("ExamplePanel");
				Label panelLabel = panel.GetNode<Label>("Label");
				TextureRect panelIcon = panel.GetNode<TextureRect>("TextureRect");

				panelIcon.SetTexture(null);
				panelLabel.Text = string.Empty;
				panel.Hide();

				if (HoverOverPanel != null)
				{
					swapPanels(HoverOverPanel, GrabbedItem);
					GrabbedItem = null;
				}
			}
			if (Input.IsActionJustPressed("RightClick") && items.ElementAtOrDefault(HoverOverPanel.GetIndex()) != null)
			{
				LastInteractedItem = items.ElementAtOrDefault(HoverOverPanel.GetIndex());
				GD.Print("Last interacted item: " + LastInteractedItem.Name);
				ToggleDropdownMenu();
			}
		}
	}

	public void swapPanels(Panel panel, Item item)
	{
		int hoverIndex = panel.GetIndex();
		int itemIndex = items.IndexOf(item);
		MoveItemIndex(itemIndex, hoverIndex);
		UpdateItem(item, hoverIndex);
		GD.Print("Moved " + itemIndex + " to " + hoverIndex + ".");
	}

	public void MoveItemIndex(int fromIndex, int toIndex)
	{
		if (fromIndex < 0 || fromIndex >= 9 || toIndex < 0 || toIndex >= 9)
		{
			throw new IndexOutOfRangeException();
		}
		Item item = items[fromIndex];
		if (item == null)
		{
			return;
		}
		Item item2 = items[toIndex] ?? null;
		if (item2 == null)// jos slotti on tyhjä
		{
			items[fromIndex] = null;
		}
		else
		{
			items[fromIndex] = item2;
		}
		items[toIndex] = item;
		reflowPanels();
	}

	static int GetExcess(int stackSize, int ItemQuantity)
	{
		int excess = stackSize - ItemQuantity;
		return Math.Abs(excess);
	}
	public int GetNextEmptySlot()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == null)
			{
				return i;
			}
		}
		return -1;
	}
	public void Add(Item item)
	{
		if (CheckForFullInventory())
		{
			GD.Print("Inventory is full");
			return;
		}

		Item currentItem = item.Copy();
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == null)
			{
				continue;
			}
			// if item is already in inventory
			// and there is space
			if (items[i].ID == currentItem.ID && items[i].Quantity < items[i].StackSize)
			{
				if (items[i].Quantity + currentItem.Quantity > items[i].StackSize)
				{
					int currentItemsSum = items[i].Quantity + currentItem.Quantity;
					currentItem.Quantity = GetExcess(items[i].StackSize, currentItemsSum);
					items[i].Quantity = items[i].StackSize;
					break;
				}
				else
				{
					items[i].Quantity += currentItem.Quantity;
					reflowPanels();
					return;
				}
			}
		}

		if (currentItem.Quantity > currentItem.StackSize)
		{
			Item copyItem = currentItem.Copy();
			copyItem.Quantity = GetExcess(currentItem.StackSize, currentItem.Quantity);
			currentItem.Quantity = currentItem.StackSize;
			GD.Print("Adding excess of excess " + copyItem.Quantity);
			Add(copyItem);
		}
		AddItemToList(currentItem);
		reflowPanels();
	}

	public void AddToInventory(Item item)
	{
		Item currentItem = item.Copy();
		if (item == null)
		{
			GD.Print("Item is null");
			return;
		}
		if (CheckForEmptyInventory() == true)
		{
			if (currentItem.Quantity > item.StackSize)
			{
				Item copyItem = item.Copy();
				copyItem.Quantity = GetExcess(item.StackSize, currentItem.Quantity);
				currentItem.Quantity = item.StackSize;
				AddItemToList(currentItem);
				Add(copyItem);
				reflowPanels();
			}
			else
			{
				AddItemToList(currentItem);
				reflowPanels();
			}

		}
		else
		{
			Add(currentItem);
		}
	}

	public void AddItemToList(Item item)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == null)
			{
				items[i] = item;
				UpdateItem(item, i);
				break;
			}
		}

	}

	public bool CheckForEmptyInventory()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] != null)
			{
				return false;
			}
		}
		return true;
	}
	public bool CheckForFullInventory()
	{
		bool isFull = false;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == null)
			{
				isFull = false;
				break;
			}
			else
			{
				if (items[i].Quantity < items[i].StackSize)
				{
					isFull = false;
					break;
				}
				isFull = true;
			}
		}
		return isFull;
	}
	public void Remove(Item item)
	{
		if (canAfford(item))
		{
			Item currentItem = item.Copy();
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] == null)
				{
					continue;
				}
				if (items[i].ID == currentItem.ID)
				{
					if (items[i].Quantity - currentItem.Quantity < 0)
					{
						currentItem.Quantity -= items[i].Quantity;
						GD.Print(currentItem.Quantity);
						items[i].Quantity = 0;
					}
					else
					{
						items[i].Quantity -= currentItem.Quantity;
						currentItem.Quantity = 0;
					}
				}

				if (currentItem.Quantity <= 0)
				{
					break;
				}
			}
			List<Item> currentItems = items.Where(x => x != null && x.Quantity <= 0).ToList();
			foreach (Item i in currentItems)
			{
				items[items.IndexOf(i)] = null;
			}
			if (currentItem.Quantity > 0)
			{
				Remove(currentItem);
			}
			reflowPanels();
		}
	}

	public void reflowPanels()
	{
		for (int i = 0; i < items.Count; i++)
		{
			UpdateItem(items.ElementAtOrDefault(i) ?? null, i);
		}
	}
	public void UpdateItem(Item item, int index)
	{
		if (item == null)
		{
			icon = panels[index].GetNode<TextureRect>("TextureRect");
			quantityLabel = panels[index].GetNode<Label>("Label");
			icon.SetTexture(null);
			quantityLabel.Text = string.Empty;
		}
		else
		{
			icon = panels[index].GetNode<TextureRect>("TextureRect");
			quantityLabel = panels[index].GetNode<Label>("Label");
			icon.Texture = item.Icon;
			quantityLabel.Text = item.Quantity.ToString();
		}

	}

	public bool canAfford(Item item)
	{
		List<Item> currentItems = items.Where(x => x != null && x.ID == item.ID).ToList();
		int i = 0;
		foreach (var item1 in currentItems)
		{
			i += item1.Quantity;
		}
		if (item.Quantity <= i)
		{
			return true;
		}
		GD.Print("Cannot afford");
		return false;
	}
	public void _on_add_button_button_down()
	{
		AddToInventory(ResourceLoader.Load<Item>("res://scenes/items/Stick_Item.tres"));
	}

	public void _on_delete_button_button_down()
	{
		Remove(ResourceLoader.Load<Item>("res://scenes/items/Stick_Item.tres"));
	}

	public void _on_add_button_2_button_down()
	{
		AddToInventory(ResourceLoader.Load<Item>("res://scenes/items/Stone_Item.tres"));
	}

	public void _on_delete_button_2_button_down()
	{
		Remove(ResourceLoader.Load<Item>("res://scenes/items/Stone_Item.tres"));
	}
	public void _on_mouse_area_2d_area_entered(Area2D area)
	{
		Control slot = area.GetParent<Control>();
		if (slot is Panel hoverPanel)
		{
			HoverOverPanel = hoverPanel;
		}
	}

	public void _on_mouse_area_2d_area_exited(Area2D area) => HoverOverPanel = null;

	public void ToggleDropdownMenu()
	{
		if (!is_menu_open)
		{
			OpenMenu();
		}
		else
		{
			CloseMenu();
		}
	}
	bool is_menu_open = false;

	public void OpenMenu()
	{
		if (!is_menu_open)
		{
			DropDownMenu = ResourceLoader.Load<PackedScene>("res://scenes/item_dropdown_menu.tscn").Instantiate();
			GetTree().Root.AddChild(DropDownMenu);
			is_menu_open = true;
		}
	}

	public void CloseMenu()
	{
		if (is_menu_open)
		{
			LastInteractedItem = null;
			//GetTree().Root.RemoveChild(DropDownMenu);
			DropDownMenu.QueueFree();
			is_menu_open = false;
		}
	}
}
