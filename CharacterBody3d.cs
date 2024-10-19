using Godot;
using System;

public partial class CharacterBody3d : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float IceSpeed = 10.0f;
	public const float JumpVelocity = 4.5f;
	public const float MouseSensitivity = 0.1f; // Adjust sensitivity
	private Node3D head;
	private Camera3D camera;
	private Vector2 mouseDelta; // Tracks mouse movement

	private globals globals;

	private bool onIce = false;
	public override void _Ready()
	{
		head = GetNode<Node3D>("head");
		camera = head.GetNode<Camera3D>("Camera3D");
		globals = (globals)GetNode("/root/Globals");

		globals.Connect("PlayerOnIce", new Callable(this, nameof(OnIce)));

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			mouseDelta = mouseMotion.Relative;
		}
	}


	public void OnIce(Boolean state){
		onIce = state;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero && !onIce)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else if (direction != Vector3.Zero && onIce)
		{
			velocity.X = direction.X * IceSpeed;
			velocity.Z = direction.Z * IceSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
		UpdateMouseLook(delta);
	}

	private void UpdateMouseLook(double delta)
	{
		Vector3 headRotation = head.RotationDegrees;
		headRotation.X -= mouseDelta.Y * MouseSensitivity;
		headRotation.X = Mathf.Clamp(headRotation.X, -90, 90); 
		head.RotationDegrees = headRotation;

		Vector3 bodyRotation = RotationDegrees;
		bodyRotation.Y -= mouseDelta.X * MouseSensitivity;
		RotationDegrees = bodyRotation;

		mouseDelta = Vector2.Zero;
	}
}
