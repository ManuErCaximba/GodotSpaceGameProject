using Godot;
using System;

public partial class Enemy : Area3D
{
	[Export]
	public Player Player;

	private RayCast3D _RayCast;
	private AnimatedSprite3D _MuzzleA;
	private AnimatedSprite3D _MuzzleB;

	private int _Health = 100;
	private float _Time = 0.0f;
	private Vector3 _TargetPosition;
	private bool _Destroyed = false;

	public override void _Ready()
	{
		_RayCast = GetNode<RayCast3D>("RayCast");
		_MuzzleA = GetNode<AnimatedSprite3D>("MuzzleA");
		_MuzzleB = GetNode<AnimatedSprite3D>("MuzzleB");

		_TargetPosition = Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Player != null)
		{
			LookAt(Player.Position + new Vector3(0, 0.5f, 0), Vector3.Up, true); // Look at player
		}
		_TargetPosition.Y += Mathf.Cos(_Time * 5) * (float) delta; // Sine movement (up and down)

		_Time += (float) delta;

		Position = _TargetPosition;
	}

	// Take damage from player
	public void Damage(int amount)
	{
		Audio.Play("sounds/enemy_hurt.ogg");

		_Health -= amount;

		if (_Health <= 0 && !_Destroyed)
			_Destroy();
	}

	// Destroy the enemy when out of health
	private void _Destroy()
	{
		Audio.Play("sounds/enemy_destroy.ogg");

		_Destroyed = true;
		QueueFree();
	}

	// Shoot when timer hits 0
	private void _OnTimerTimeout()
	{
		_RayCast.ForceRaycastUpdate();

		if (_RayCast.IsColliding())
		{
			GodotObject Collider = _RayCast.GetCollider();
			if (Collider is Player _Player)
			{
				_MuzzleA.Frame = 0;
				_MuzzleA.Play("default");
				_MuzzleA.RotationDegrees = new Vector3(_MuzzleA.RotationDegrees.X, _MuzzleA.RotationDegrees.Y, GD.RandRange(-45, 45));

				_MuzzleB.Frame = 0;
				_MuzzleB.Play("default");
				_MuzzleB.RotationDegrees = new Vector3(_MuzzleB.RotationDegrees.X, _MuzzleB.RotationDegrees.Y, GD.RandRange(-45, 45));

				Audio.Play("sounds/enemy_attack.ogg");

				_Player.Damage(5);
			} 
		}
	}
}
