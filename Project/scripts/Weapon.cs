using Godot;

public partial class Weapon : Resource
{
    [ExportSubgroup("Model")]
    // Model of the weapon
    [Export]
    public PackedScene Model { get; set; }
    // On-screen position
    [Export]
    public Vector3 Position { get; set; }
    [Export]
    // On-screen rotation
    public Vector3 Rotation { get; set; }
    [Export]
    // Muzzle flash
    public SpriteFrames Muzzle { get; set; }
    [Export]
    // On-screen position of muzzle flash
    public Vector3 MuzzlePosition { get; set; }


    [ExportSubgroup("Properties")]
    // Firerate
    [Export(PropertyHint.Range, "0.1,1,")]
    public float Cooldown { get; set; } = 0.1f;
    // Fire distance
    [Export(PropertyHint.Range, "1,20,")]
    public int MaxDistance { get; set; } = 10;
    // Damage per hit
    [Export(PropertyHint.Range, "0,100,")]
    public int Damage { get; set; } = 25;
    // Spread of each shot
    [Export(PropertyHint.Range, "0,5,")]
    public int Spread { get; set; } = 0;
    // Amount of shots
    [Export(PropertyHint.Range, "1,5,")]
    public int ShotCount { get; set; } = 1;
    // Amount of knockback
    [Export(PropertyHint.Range, "0,50,")]
    public int Knockback { get; set; } = 20;


    [ExportSubgroup("Sounds")]
    // Sound path
    [Export]
    public string SoundShoot { get; set; }


    [ExportSubgroup("Crosshair")]
    // Image of crosshair on-screen
    [Export]
    public Texture2D Crosshair { get; set; }
}
