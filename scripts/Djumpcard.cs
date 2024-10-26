using Godot;
using System;

public partial class Djumpcard : Area3D
{
	private globals globals;
	public override void _Ready()
	{
		globals = GetNode<globals>("/root/Globals");
		Connect("body_entered", new Callable(this, nameof(TouchedJump)));
	}

	private void TouchedJump(Node3D body){
		globals.EmitSignal("TouchedDoubleJump", this);
	}




}
