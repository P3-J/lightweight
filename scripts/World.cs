using Godot;
using Nakama;
using System.Collections.Generic;

public partial class World : Node3D
{
    [Export] public PackedScene PlayerScene;

    private ISocket _socket;
    private Dictionary<string, CharacterBody3D> _players = new Dictionary<string, CharacterBody3D>();


    public void AddLocalPlayer(string userId, Vector3 spawnPosition)
    {
        var localPlayer = (CharacterBody3D)PlayerScene.Instantiate();
        localPlayer.Name = userId;
        localPlayer.Position = spawnPosition;
        AddChild(localPlayer);
        _players[userId] = localPlayer; 
    }

    // Update the player position of another player
    public void UpdatePlayerPosition(string userId, Vector3 newPosition)
    {
        if (_players.TryGetValue(userId, out var player))
        {
            player.Position = newPosition;
        }
        else
        {
            var newPlayer = (CharacterBody3D)PlayerScene.Instantiate();
            newPlayer.Name = userId;
            newPlayer.Position = newPosition;
            AddChild(newPlayer);
            _players[userId] = newPlayer;
        }
    }

	public void SetSocket(ISocket socket)
    {
        _socket = socket;
    }

    public void SyncLocalPlayerPosition(string userId, Vector3 position)
    {
        UpdatePlayerPosition(userId, position); 
    }
}
