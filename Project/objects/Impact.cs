using Godot;
using System;

public partial class Impact : AnimatedSprite3D
{
	public void _OnAnimationFinished()
	{
		QueueFree();
	}
}
