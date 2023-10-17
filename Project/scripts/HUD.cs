using Godot;
using System;

public partial class HUD : CanvasLayer
{
	private Label _Health;

	public override void _Ready()
	{
		_Health = GetNode<Label>("Health");
	}

	public void _OnHealthUpdate(int health)
	{
        _Health.Text = health + "%";
	}

}
