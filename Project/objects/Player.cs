using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	public int MovementSpeed = 5;
	[Export]
    public int JumpStrength = 7;
    [Export]
    public Weapon[] Weapons = new Weapon[2];
	[Export]
    public int MouseSensitivity = 10;
	[Export]
    public TextureRect Crosshair;

    private Weapon _Weapon;
    private int _WeaponIndex = 0;

    private float _WeaponKnockback = 0.0f;
    private float _CameraKnockback = 0.0f;

	private bool _MouseCaptured = true;

    private Vector3 _MovementVelocity;

    private Vector3 _LocalInput;
    private Vector2 _InputMouse;

    private float _CurrentYaw = 0.0f;
    private float _CurrentPitch = 90.0f;

    private int _Health = 100;
    private float _Gravity = 0.0f;

    private bool _PreviouslyFloored = false;

    private bool _JumpSingle = true;
    private bool _JumpDouble = true;

	private Vector3 _ContainerOriginalPos;

	private Tween _Tween;

	[Signal]
	public delegate void HealthUpdatedEventHandler(int health);

	private Camera3D _Camera;
    private RayCast3D _RayCast;
    private AnimatedSprite3D _Muzzle;
    private Node3D _Container;
    private AudioStreamPlayer _SoundFootsteps;
    private Timer _BlasterCooldown;

	public override void _Ready()
    {
		_Camera = GetNode<Camera3D>("Head/Camera");
		_RayCast = GetNode<RayCast3D>("Head/Camera/RayCast");
		_Container = GetNode<Node3D>("Head/Camera/Container");
		_SoundFootsteps = GetNode<AudioStreamPlayer>("SoundFootsteps");
		_BlasterCooldown = GetNode<Timer>("Cooldown");

        _Muzzle = new AnimatedSprite3D();
        _Container.AddChild(_Muzzle);
        _ContainerOriginalPos = _Container.Position;

        Input.MouseMode = Input.MouseModeEnum.Captured;
        _InitiateChangeWeapon(_WeaponIndex);
    }

    public override void _PhysicsProcess(double delta)
    {
        	
        // Handle functions
        
        _HandleControls(delta);
        _HandleGravity(delta);
        
        // Movement

        Vector3 _AppliedVelocity;
        
        _MovementVelocity = Transform.Basis * _MovementVelocity; // Move forward
        
        _AppliedVelocity = Velocity.Lerp(_MovementVelocity, (float) (delta * 10));
        _AppliedVelocity.Y = -_Gravity;
        
        Velocity = _AppliedVelocity;
        MoveAndSlide();
        
        // Rotation

        _CurrentPitch = (float) Math.Clamp(_CurrentPitch + _CameraKnockback - _InputMouse.Y, 0.0f, 180.0f);
        _Camera.Quaternion = new Quaternion(_Camera.Basis.X, (_CurrentPitch - 90.0f) * (float) Math.PI / 180);
        _CameraKnockback = Mathf.Lerp(_CameraKnockback, 0.0f, (float) delta * 5);

        _CurrentYaw = MathUtils.NormalizeAngle(_CurrentYaw + _InputMouse.X);
        Quaternion = new Quaternion(-Basis.Y, _CurrentYaw * (float) Math.PI / 180);

        _InputMouse = new Vector2(0.0f, 0.0f);
        
        // Movement sound
        
        _SoundFootsteps.StreamPaused = true;
        
        if (IsOnFloor())
        {
            if (Math.Abs(Velocity.X) > 1 || Math.Abs(Velocity.Z) > 1)
                _SoundFootsteps.StreamPaused = false;
        }
        
        // Landing after jump or falling
        
        float _PosY = (float) Mathf.Lerp(_Camera.Position.Y, 0.0, delta * 5);
        //_Camera.Position = new Vector3(_Camera.Position.X, _PosY, _Camera.Position.Z);
        
        if (IsOnFloor() && _Gravity > 1 && !_PreviouslyFloored) // Landed
        {
            Audio.Play("sounds/land.ogg");
            //_Camera.Position = new Vector3(_Camera.Position.X, -0.1f, _Camera.Position.Z);
        }
        
        _PreviouslyFloored = IsOnFloor();
        
        // Falling/respawning
        
        if (Position.Y < -10)
            GetTree().ReloadCurrentScene();
    }

	public void Damage(int amount)
	{
		_Health -= amount;
		EmitSignal(SignalName.HealthUpdated, _Health);

		if (_Health < 0)
			GetTree().ReloadCurrentScene();
	}

    // Mouse movement

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion @mouseEvent && _MouseCaptured)
        {
            _InputMouse = @mouseEvent.Relative / MouseSensitivity;
        }
    }

    private void _HandleControls(double delta)
    {
        // Mouse capture
        
        if (Input.IsActionJustPressed("mouse_capture"))
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _MouseCaptured = true;
        }
        
        if (Input.IsActionJustPressed("mouse_capture_exit"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _MouseCaptured = false;
            
            _InputMouse = Vector2.Zero;
        }
        
        // Movement
        
        _LocalInput.X = Input.GetAxis("move_left", "move_right");
        _LocalInput.Z = Input.GetAxis("move_forward", "move_back");
        
        _MovementVelocity = _LocalInput.Normalized() * MovementSpeed;
        
        // Rotation
        
        Vector3 _RotationInput = Vector3.Zero;
        
        _RotationInput.Y = Input.GetAxis("camera_left", "camera_right");
        _RotationInput.X = Input.GetAxis("camera_up", "camera_down") / 2;
        
        // Shooting
        
        _ActionShoot(delta);
        
        // Jumping
        
        if (Input.IsActionJustPressed("jump"))
        {    
            if (_JumpSingle || _JumpDouble)
            {
                Audio.Play("sounds/jump_a.ogg, sounds/jump_b.ogg, sounds/jump_c.ogg");
            }
            
            if (_JumpDouble)
            {
                _Gravity = -JumpStrength;
                _JumpDouble = false;
            }

            if (_JumpSingle)
            {
                _ActionJump();
            }
        }

        // Weapon switching
        
        _ActionWeaponToggle();
    }

    // Jumping

    private void _ActionJump()
    {
        _Gravity = -JumpStrength;
        
        _JumpSingle = false;
        _JumpDouble = true;
    }

    // Toggle between available weapons (listed in 'weapons')

    private void _ActionWeaponToggle()
    {
        if (Input.IsActionJustPressed("weapon_toggle"))
        {
            _WeaponIndex = MathUtils.Wrap(_WeaponIndex + 1, 0, Weapons.Length - 1);
            _InitiateChangeWeapon(_WeaponIndex);
            
            Audio.Play("sounds/weapon_change.ogg");
        }
    }

    private void _ActionShoot(double delta)
    {
        if (Input.IsActionPressed("shoot") && _BlasterCooldown.IsStopped())
        {
            //if (!_BlasterCooldown.IsStopped()) { return; } // Cooldown for shooting
            
            Audio.Play(_Weapon.SoundShoot);
            
            //_Container.Position = new Vector3((float) (_Container.Position.X + 0.25), _Container.Position.Y, _Container.Position.Z);  // Knockback of _Weapon visual
            _CameraKnockback += _Weapon.Knockback * 0.1f;
            _WeaponKnockback = _Weapon.Knockback * 0.1f;
            // Knockback of _Camera
            _MovementVelocity += new Vector3(0, 0, _Weapon.Knockback); // Knockback
            
            // Set muzzle flash position, play animation
            
            _Muzzle.SpriteFrames = _Weapon.Muzzle;
            _Muzzle.Play("default");
            
            _Muzzle.RotationDegrees = new Vector3(_Muzzle.RotationDegrees.X, _Muzzle.RotationDegrees.Y, GD.RandRange(-45, 45));
            _Muzzle.Scale = Vector3.One * 2 * (float) GD.RandRange(0.40, 0.75);
            _Muzzle.Position = _Weapon.MuzzlePosition + new Vector3((float) GD.RandRange(-0.1, 0.1), (float) GD.RandRange(-0.1, 0.1), 0.0f);
            
            _BlasterCooldown.Start(_Weapon.Cooldown);
            
            // Shoot the _Weapon, amount based on shot count
            
            for (int i = 0; i <= _Weapon.ShotCount; i++)
            {
				int _PosXY = GD.RandRange(-_Weapon.Spread, _Weapon.Spread);
                _RayCast.TargetPosition = new Vector3(_PosXY, _PosXY, _RayCast.TargetPosition.Z);
                
                _RayCast.ForceRaycastUpdate();
                
                // Don't create impact when _RayCast didn't hit
                if (_RayCast.IsColliding())
                {
                
                    GodotObject _Collider = _RayCast.GetCollider();
                    
                    // Hitting an enemy
                    
                    if (_Collider is Enemy)
                    {
                        Enemy _Enemy = (Enemy) _Collider;
                        _Enemy.Damage(_Weapon.Damage);
                    }
                    
                    // Creating an impact animation
                    
                    PackedScene _Impact = GD.Load<PackedScene>("res://objects/Impact.tscn");
                    Impact _ImpactInstance = (Impact) _Impact.Instantiate();
                    
                    _ImpactInstance.Play("shot");
                    
                    GetTree().Root.AddChild(_ImpactInstance);
                    
                    _ImpactInstance.SetIndexed("position", _RayCast.GetCollisionPoint() + (_RayCast.GetCollisionNormal() / 10));
                    //_ImpactInstance.Position = _RayCast.GetCollisionPoint();
                    _ImpactInstance.LookAt(_Camera.GlobalTransform.Origin, Vector3.Up, true);
                }
            }
        }
        _WeaponKnockback = Mathf.Lerp(_WeaponKnockback, 0.0f, (float) delta * 5);
        _Container.Position = new Vector3(Mathf.Lerp(_ContainerOriginalPos.X + (_WeaponKnockback * 0.05f), _ContainerOriginalPos.X, (float) delta), _ContainerOriginalPos.Y, _ContainerOriginalPos.Z);
    }

    private void _HandleGravity(double delta)
    {
        _Gravity += 20.0f * (float) delta;
	
        if (_Gravity > 0 && IsOnFloor())
        {
            _JumpSingle = true;
            _Gravity = 0;
        }
    }

    private void _InitiateChangeWeapon(int index)
    {
        _WeaponIndex = index;

        _Tween = GetTree().CreateTween();
        _Tween.SetEase(Tween.EaseType.InOut);
        //_Tween.TweenProperty(_Container, "position", new Vector3(1.2f, -2.1f, -0.75f), 1f);
		_Tween.TweenCallback(Callable.From(_ChangeWeapon));
    }

	private void _ChangeWeapon()
    {
		_Weapon = Weapons[_WeaponIndex];

        // Step 1. Remove previous _Weapon model(s) from _Container
        
        foreach (Node n in _Container.GetChildren())
        {
            if (n is not AnimatedSprite3D)
                _Container.RemoveChild(n);
        }
        
        // Step 2. Place new _Weapon model in _Container
        
        Node3D _WeaponModel = (Node3D) _Weapon.Model.Instantiate();
        _Container.AddChild(_WeaponModel);
        
        //_WeaponModel.SetIndexed("position", _Weapon.Position);
        //_WeaponModel.SetIndexed("rotation", _Weapon.Rotation);
        
        // Step 3. Set model to only render on layer 2 (the _WeaponCamera)
        
        // foreach (MeshInstance3D _Child in _WeaponModel.FindChildren("*", "MeshInstance3D"))
        // {
        //     _Child.Layers = 2;
        // }
            
        // Set _Weapon data
        
        _RayCast.TargetPosition = new Vector3(0, 0, -1) * _Weapon.MaxDistance;
        Crosshair.Texture = _Weapon.Crosshair;
    }
}
