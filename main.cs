using Godot;
using System;

public class DrivableCar : Node3D
{
    private Node3D car;
    private VehicleBody3D vehicleBody;
    private VehicleWheel3D[] wheels = new VehicleWheel3D[4];
    private HeightMapShape3D terrainShape;
    private RigidBody3D terrainBody;
    private float steering = 0f;
    private float engineForce = 0f;
    private float maxSteeringAngle = 0.3f;
    private float maxEngineForce = 2000f;

    public override void _Ready()
    {
        CreateTerrain();
        CreateCar();
    }

    private void CreateTerrain()
    {
        terrainBody = new RigidBody3D();
        AddChild(terrainBody);

        MeshInstance3D terrainMesh = new MeshInstance3D();
        terrainShape = new HeightMapShape3D();
        
  
        PlaneMesh mesh = new PlaneMesh();
        mesh.Size = new Vector2(1000, 1000);
        terrainMesh.Mesh = mesh;
        terrainBody.AddChild(terrainMesh);

        ShaderMaterial terrainMaterial = new ShaderMaterial();
        Shader shader = new Shader();
        shader.Code = @"
            shader_type spatial;
            uniform float height = 20.0;
            void fragment() {
                vec3 pos = VERTEX;
                float h = sin(pos.x * 0.05) * cos(pos.z * 0.05) * height;
                VERTEX.y += h;
                NORMAL = normalize(cross(dFdx(VERTEX), dFdy(VERTEX)));
                COLOR = vec4(vec3(0.3, 0.7, 0.3), 1.0); // Simple green color
            }";
        terrainMaterial.Shader = shader;
        terrainMesh.MaterialOverride = terrainMaterial;

        // Assign the terrain shape to the RigidBody
        CollisionShape3D terrainCollision = new CollisionShape3D();
        terrainCollision.Shape = terrainShape;
        terrainBody.AddChild(terrainCollision);
    }
    private void CreateCar()
    {
        car = new Node3D();
        vehicleBody = new VehicleBody3D();
        car.AddChild(vehicleBody);

        MeshInstance3D carMesh = new MeshInstance3D();
        BoxMesh carBodyMesh = new BoxMesh();
        carBodyMesh.Size = new Vector3(3, 1, 5); // Car body dims
        carMesh.Mesh = carBodyMesh;
        vehicleBody.AddChild(carMesh);

        // col shape
        CollisionShape3D carCollisionShape = new CollisionShape3D();
        BoxShape3D boxShape = new BoxShape3D();
        boxShape.Size = new Vector3(3, 1, 5);
        carCollisionShape.Shape = boxShape;
        vehicleBody.AddChild(carCollisionShape);

        for (int i = 0; i < 4; i++)
        {
            wheels[i] = new VehicleWheel3D();
            vehicleBody.AddChild(wheels[i]);

            wheels[i].SuspensionRestLength = 0.4f;
            wheels[i].DampingCompression = 0.2f;
            wheels[i].DampingRelaxation = 0.4f;
            wheels[i].SuspensionStiffness = 20.0f;
            wheels[i].MaxSuspensionTravelCm = 500f;

            MeshInstance3D wheelMesh = new MeshInstance3D();
            CylinderMesh wheelCylinder = new CylinderMesh();
            wheelCylinder.Height = 0.5f;
            wheelCylinder.Radius = 0.6f;
            wheelMesh.Mesh = wheelCylinder;

            CollisionShape3D wheelCollision = new CollisionShape3D();
            CylinderShape3D cylinderShape = new CylinderShape3D();
            cylinderShape.Radius = 0.6f;
            cylinderShape.Height = 0.5f;
            wheelCollision.Shape = cylinderShape;

            wheels[i].AddChild(wheelMesh);
            wheels[i].AddChild(wheelCollision);
        }

        // Position the wheels correctly (example for a basic car)
        wheels[0].Translation = new Vector3(-1.5f, -0.5f, 2.0f); // Front-left
        wheels[1].Translation = new Vector3(1.5f, -0.5f, 2.0f);  // Front-right
        wheels[2].Translation = new Vector3(-1.5f, -0.5f, -2.0f); // Back-left
        wheels[3].Translation = new Vector3(1.5f, -0.5f, -2.0f);  // Back-right

        // Add the car node to the main scene
        AddChild(car);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleInput();
        ApplyCarForces();
    }

    // Handle the user input for car driving
    private void HandleInput()
    {
        steering = 0f;
        engineForce = 0f;

        if (Input.IsActionPressed("ui_left"))
            steering = -maxSteeringAngle;

        if (Input.IsActionPressed("ui_right"))
            steering = maxSteeringAngle;

        if (Input.IsActionPressed("ui_up"))
            engineForce = maxEngineForce;

        if (Input.IsActionPressed("ui_down"))
            engineForce = -maxEngineForce;
    }

    // Apply forces to move and steer the car
    private void ApplyCarForces()
    {
        wheels[0].Steering = steering;
        wheels[1].Steering = steering;

        wheels[2].EngineForce = engineForce;
        wheels[3].EngineForce = engineForce;
    }
}
