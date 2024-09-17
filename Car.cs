using Godot;
using System;

public partial class Car : VehicleBody3D
{
	private float steering = 0f;
    private float engineForce = 0f;
    private float maxSteeringAngle = 0.6f;
    private float maxEngineForce = 2000f;
	
	[Export]
	VehicleWheel3D wheel1;
	[Export]
	VehicleWheel3D wheel2;
	[Export]
	VehicleWheel3D wheel3;
	[Export]
	VehicleWheel3D wheel4;
	public override void _Process(double delta)
	{
		HandleInput();
		ApplyCarForces();
	}
	private void HandleInput()
    {
        steering = 0f;
        engineForce = 0f;

        if (Input.IsActionPressed("ui_left"))
            steering = maxSteeringAngle;
        if (Input.IsActionPressed("ui_right"))
            steering = -maxSteeringAngle;
        if (Input.IsActionPressed("ui_up"))
            engineForce = -maxEngineForce;
        if (Input.IsActionPressed("ui_down"))
            engineForce = maxEngineForce;
    }
	private void ApplyCarForces()
    {
        wheel1.Steering = steering;
        wheel2.Steering = steering;

        wheel3.EngineForce = engineForce;
        wheel4.EngineForce = engineForce;
    }
}
