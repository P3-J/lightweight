using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class globals : Node
{
    // Define signals
    [Signal]
    public delegate void PlayerOnIceEventHandler();

    [Signal]
    public delegate void TouchedDoubleJumpEventHandler();


    public string email;

}
