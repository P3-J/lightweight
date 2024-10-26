using Godot;
using System;
using Nakama;

public partial class Lobby : Node3D
{
	private ISession _session;

    public override void _Ready()
    {
        if (_session != null)
        {
            LoadWorldScene();
        }
        else
        {
            GD.PrintErr("No session found. Cannot load world.");
        }
    }

    // Method to set the session received from the Main Menu
    public void SetSession(ISession session)
    {
        _session = session;
    }

    private void LoadWorldScene()
    {
        // Load the World scene
        GetTree().ChangeSceneToFile("res://scenes/world.tscn");
    }
}
