using Godot;
using System;


public partial class iceground : Area3D
{
	private globals globals;
    public override void _Ready()
    {
        globals = (globals)GetNode("/root/Globals");
		Connect("body_entered", new Callable(this, nameof(_on_body_entered)));
		Connect("body_exited", new Callable(this, nameof(_on_body_exited)));
    }

	private void _on_body_entered(Node3D body){
		if (body.IsInGroup("player")){
			globals.EmitSignal("PlayerOnIce", true);
		}
	}

	private void _on_body_exited(Node3D body){
		if (body.IsInGroup("player")){
			globals.EmitSignal("PlayerOnIce", false);
		}
	}

}
