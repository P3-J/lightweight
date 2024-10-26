using Godot;
using System;
using Nakama;
using Godot.Collections;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

public partial class Mainmenu : Node3D
{
    [Export]
	public PackedScene PlayerScene;
    [Signal]
    public delegate void MoveSyncEventHandler(string data);
	public static Client _client;
    public static Mainmenu Client;

	private ISession _session;
	private static ISocket _socket;
    private static IMatch match;

	private globals globals;

    public bool IsHost { get; private set; }
    private static string existingMatchId = null;
    private Dictionary<string, player> playerInstances = new Dictionary<string, player>(); 

    public override void _Ready()
    {

        if (Client != null)
		{
			GD.Print("removing second instance");

			QueueFree();
		}
		else
		{

			Client = this;
		}

		globals = (globals)GetNode("/root/Globals");
        _client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        _client.Timeout = 500;
    }

	public async void AuthenticateUser(string email, string password, string name)
    {
        try
        {
            _session = await _client.AuthenticateEmailAsync(email, password, name);
            GD.Print("User authenticated, session ID: ", _session.UserId);
        }
        catch (Exception e)
        {
            GD.PrintErr("Authentication failed: ", e.Message);
        }
    }

    public static async void SyncData(string data, int opcode)
    {
        await _socket.SendMatchStateAsync(match.Id, opcode, data);
    }

    public ISession GetSession()
    {
        return _session;
    }

    public ISocket GetSocket()
    {
        return _socket;
    }

	private void TransitionToLobby()
    {
        var lobbyScene = GD.Load<PackedScene>("res://scenes/world.tscn");
        var lobby = (World)lobbyScene.Instantiate();

		lobby.SetSocket(_socket); 
        lobby.AddLocalPlayer(_session.UserId, new Vector3(0, 0, 0));
		GetTree().Root.AddChild(lobby);
		QueueFree();

    }

	private void _on_p_1_pressed(){
		AuthenticateUser("user@user.com", "Password123!", "tester");
	}

	private void _on_p_2_pressed(){
		AuthenticateUser("user1@user.com", "Password123!", "tester2");
	}

    private async void _on_join_pressed(){
        _socket = Nakama.Socket.From(_client);
        await _socket.ConnectAsync(_session);

        if (existingMatchId == null)
        {
            match = await _socket.CreateMatchAsync("bdk");
            existingMatchId = match.Id;
            GD.Print("Match created: " + match.Id);
            IsHost = true;
        }
        else
        {
            match = await _socket.JoinMatchAsync(existingMatchId);
            GD.Print("Joined existing match: " + existingMatchId);
            IsHost = false;
        }

        _socket.ReceivedMatchPresence += onMatchPresence;
        _socket.ReceivedMatchState += onMatchState;

        

        if (match.Presences.Count() == 0)
        {
            IsHost = true;
        }

        AddPlayer(_session.UserId, _session.UserId);
    }


    private void SendExistingPlayerStates(string userId)
    {
        foreach (var playerInstance in playerInstances.Values)
        {
            if (playerInstance.UserId != userId)
            {
                PlayerSyncData playerSyncData = new PlayerSyncData
                {
                    Position = playerInstance.GlobalPosition,
                    RotationDegrees = playerInstance.RotationDegrees,
                    id = playerInstance.UserId,
                };

                SyncData(JsonConvert.SerializeObject(playerSyncData), 2);
            }
        }
    }
    private void onMatchState(IMatchState state)
    {
        string data = Encoding.UTF8.GetString(state.State);
        if (state.OpCode == 1) 
        {
            CallDeferred("EmitPlayerSyncDataSignal", data);
        }
        if (state.OpCode == 2) // Handle new player state from existing players
        {
            GD.Print("added new");
            PlayerSyncData playerSyncData = JsonConvert.DeserializeObject<PlayerSyncData>(data);
            AddPlayer(playerSyncData.id, playerSyncData.id);
        }
    }

    public void EmitPlayerSyncDataSignal(string data) => EmitSignal(SignalName.MoveSync, data);


    private void onMatchPresence(IMatchPresenceEvent presenceEvent)
    {
        foreach (var presence in presenceEvent.Joins)
        {
            if (!playerInstances.ContainsKey(presence.UserId))
            {
                AddPlayer(presence.UserId, presence.UserId); // Create player for each joined user
                SendExistingPlayerStates(presence.UserId);
            }
        }

        foreach (var presence in presenceEvent.Leaves)
        {
            if (playerInstances.ContainsKey(presence.UserId))
            {
                playerInstances[presence.UserId].QueueFree(); // Remove player instance if they leave
                playerInstances.Remove(presence.UserId);
            }
        }
    }


    private void AddPlayer(string userId, string playerName)
    {
        var player = PlayerScene.Instantiate<player>(); 
        player.UserId = userId; 
        player.Name = playerName;
        playerInstances[userId] = player; 
        player.IsLocalPlayer = userId == _session.UserId;

        if (player.IsLocalPlayer)
        {
            var camera = player.GetNode<Camera3D>("head/Camera3D");
            camera.Current = true; 
            GD.Print($"{playerName} is the local player.");
        }

        CallDeferred("add_child", player);
        GD.Print($"Player {playerName} added with ID {userId}");

    }

    private async void _on_ping_pressed(){
        var data = Encoding.UTF8.GetBytes("Hello");
        await _socket.SendMatchStateAsync(match.Id, 0, data);
    }
	
}
