using Godot;
using System;
using Nakama;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

public partial class player : CharacterBody3D
{
    public const float Speed = 10.0f;
    public const float IceSpeed = 15.0f;
    public const float JumpVelocity = 6.5f;
    public const float CardJumpVelocity = 8.5f;
    public const float MouseSensitivity = 0.1f; 
    public const float AirControlFactor = 0.2f; 
    public const float LerpSpeed = 0.1f; 
    public const float AirSpeed = 50f;

    private Node3D head;
    private Camera3D camera;
    private ColorRect card;
    private Vector2 mouseDelta;
    private globals globals;
    private bool onIce = false;
    private bool hasDoubleJump = false;
    private bool hasDoneRegularJump = false;
    private Vector3 targetVelocity; 
    public string UserId { get; set; }

    public bool IsLocalPlayer = false;
    private const float positionSyncThreshold = 0.1f; // Threshold for position synchronization

    public override void _Ready()
    {
        head = GetNode<Node3D>("head");
        camera = GetNode<Camera3D>("head/Camera3D");
        globals = (globals)GetNode("/root/Globals");
        card = GetNode<ColorRect>("Control/card");

        globals.Connect("PlayerOnIce", new Callable(this, nameof(OnIce)));
        globals.Connect("TouchedDoubleJump", new Callable(this, nameof(EnableDoubleJump)));

        if (!IsLocalPlayer)
        {
            camera.Current = false; 
        }
        else
        {
            camera.Current = true; 
        }

        Mainmenu.Client.MoveSync += onPlayerDataSync;
    }

    private void onPlayerDataSync(string data)
    {
        var playerData = JsonConvert.DeserializeObject<PlayerSyncData>(data);

        if (playerData.id == UserId)
        {
            SetPositionAndRotation(playerData.Position, playerData.RotationDegrees);
        }
    }

    private void SetPositionAndRotation(Vector3 position, Vector3 rotationDegrees)
    {
        GlobalPosition = position;
        RotationDegrees = rotationDegrees;
    }


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            mouseDelta = mouseMotion.Relative;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsLocalPlayer) return;

        Vector3 velocity = Velocity;

        // Apply gravity
        if (!IsOnFloor())
        {
            velocity.Y -= 12f * (float)delta;
        }
        else
        {
            hasDoneRegularJump = false;
        }

        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = Vector3.Zero;

        Vector3 cameraForward = camera.GlobalTransform.Basis.Z;
        Vector3 cameraRight = camera.GlobalTransform.Basis.X;

        cameraForward.Y = 0;
        cameraForward = cameraForward.Normalized();
        cameraRight = cameraRight.Normalized();

        if (IsOnFloor() || onIce)
        {
            direction = (cameraForward * inputDir.Y + cameraRight * inputDir.X).Normalized();
        }
        else
        {
            direction = (cameraForward * inputDir.Y + cameraRight * inputDir.X).Normalized();
            direction *= AirControlFactor;
        }

        velocity = HandleJump(velocity);

        // Calculate target velocity
        if (direction != Vector3.Zero)
        {
            float targetSpeed = onIce ? IceSpeed : (IsOnFloor() ? Speed : AirSpeed);
            targetVelocity = new Vector3(direction.X * targetSpeed, velocity.Y, direction.Z * targetSpeed);
        }
        else
        {
            targetVelocity = new Vector3(Mathf.MoveToward(velocity.X, 0, 2f), velocity.Y, Mathf.MoveToward(velocity.Z, 0, 2f));
        }

        velocity.X = Mathf.Lerp(velocity.X, targetVelocity.X, LerpSpeed);
        velocity.Z = Mathf.Lerp(velocity.Z, targetVelocity.Z, LerpSpeed);

        Velocity = velocity;
        MoveAndSlide();

        // Sync position if significant movement occurs
  
        SyncPosition(Position);


        UpdateMouseLook(delta);
    }

    public void SyncPosition(Vector3 newPosition)
    {
        
        PlayerSyncData playerSyncData = new PlayerSyncData(){
            Position = GlobalPosition,
            RotationDegrees = RotationDegrees,
            id = UserId,
        };

        Mainmenu.SyncData(JsonConvert.SerializeObject(playerSyncData), 1);
    }
    
    public Vector3 HandleJump(Vector3 velocity)
    {
        if (Input.IsActionJustPressed("jump") && !hasDoneRegularJump)
        {
            hasDoneRegularJump = true;
            velocity.Y = JumpVelocity;
        }

        if (Input.IsActionJustPressed("rmb") && hasDoubleJump)
        {
            hasDoubleJump = false;
            card.Visible = false;
            velocity.Y = CardJumpVelocity;
        }

        return velocity;
    }

    public void OnIce(bool state)
    {
        onIce = state;
    }

    public void EnableDoubleJump(Area3D blocky)
    {
        if (!hasDoubleJump)
        {
            card.Visible = true;
            hasDoubleJump = true;
            blocky.QueueFree();
        }
    }

    private void UpdateMouseLook(double delta)
    {
        Vector3 headRotation = head.RotationDegrees;
        headRotation.X -= mouseDelta.Y * MouseSensitivity;
        headRotation.X = Mathf.Clamp(headRotation.X, -89, 89);
        head.RotationDegrees = headRotation;

        Vector3 bodyRotation = RotationDegrees;
        bodyRotation.Y -= mouseDelta.X * MouseSensitivity;
        RotationDegrees = bodyRotation;

        mouseDelta = Vector2.Zero;
    }
}
