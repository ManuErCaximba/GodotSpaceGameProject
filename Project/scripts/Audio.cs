using Godot;
using System.Collections.Generic;

public partial class Audio : Node
{
	// Code adapted from KidsCanCode

	private int _NumPlayers = 12;
	private string _Bus = "master";

	private Queue<AudioStreamPlayer> _Available = new Queue<AudioStreamPlayer>(); // The available players.
	private static Queue<string> _Queue = new Queue<string>();  // The queue of sounds to play.

	public override void _Ready()
    {
		for (int i = 0; i < _NumPlayers; i++)
		{
			AudioStreamPlayer P = new AudioStreamPlayer();
			AddChild(P);
			
			_Available.Enqueue(P);
			
			P.VolumeDb = -10;
			P.Finished += () => OnStreamFinished(P);
			P.Bus = _Bus;
		}
	}

	public override void _Process(double delta)
    {
		if (_Queue.Count != 0 && _Available.Count != 0)
		{
			AudioStreamPlayer Elem = _Available.Dequeue();
			Elem.Stream = (AudioStream) GD.Load(_Queue.Dequeue());
			Elem.Play();
			Elem.PitchScale = (float) GD.RandRange(0.9, 1.1);
		}
	}

	public void OnStreamFinished(AudioStreamPlayer stream)
	{
		_Available.Enqueue(stream);
	}

	public static void Play(string soundPath) // Path (or multiple, separated by commas)
    {
		string[] Sounds = soundPath.Split(",");
		_Queue.Enqueue("res://" + Sounds[GD.Randi() % Sounds.Length].StripEdges());
	}
}
