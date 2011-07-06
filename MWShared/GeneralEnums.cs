using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MineWorld
{
    public enum CustomKeyBoardButtons : byte
    {
        None = 0,
        
        Forward,
        Backward,
        Left,
        Right,

        Sprint,
        Jump,
        Crouch,
        
        //All buttons past this point will never be sent to the server
        Say,
    }

    public enum CustomMouseButtons : byte
    {
        None = 0,

        Fire,
        AltFire,
    }

    public enum MouseButtons
    {
        LeftButton,
        MiddleButton,
        RightButton,
        WheelUp,
        WheelDown
    }

    public enum ScreenEffect
    {
        None,
        Death,
        Teleport,
        Fall,
        Explosion,
        Drown,
        Water,
    }

    public enum MineWorldSound
    {
        DigDirt,
        DigMetal,
        Ping,
        ConstructionGun,
        Death,
        CashDeposit,
        ClickHigh,
        ClickLow,
        GroundHit,
        Teleporter,
        Jumpblock,
        Explosion,
        RadarLow,
        RadarHigh,
        RadarSwitch,
    }

    public enum MineWorldMessage : byte
    {
        BlockBulkTransfer,      // x-value, y-value, followed by 64 bytes of blocktype ; 
        BlockSet,               // x, y, z, type
        UseTool,                // position, heading, blocktype
        HealthUpdate,         // ore, cash, weight, max ore, max weight, team ore, red cash, blue cash: ReliableInOrder1
        TriggerExplosion,       // position

        PlayerUpdate,           // (int id for server), position, heading : UnreliableInOrder1
        PlayerUpdate1,          // (int id for server), heading : UnreliableInOrder1
        PlayerJoined,           // int id, player name :ReliableInOrder2
        PlayerLeft,             // int id              :ReliableInOrder2
        PlayerDead,             // (int id for server) :ReliableInOrder2
        PlayerAlive,            // (int id for server) :ReliableInOrder2

        ChatMessage,            // byte type, string message : ReliableInOrder3
        PlaySound,              // byte sound, bool isPositional, ?Vector3 location : ReliableUnordered

        //Update by Oeds
        Hearthbeat,
        PlayerCommand,         // int id, string command  This is sent by client
        PlayerHurt,             // allows client to tell server of damage
        PlayerPosition,         // server sends client new position
        PlayerRespawn,          // allows the player to respawn
        Killed,                 // Send by the server to notify the player has been killed
        DayUpdate,              // float , Send by the server to notify the player how dark or light the game is
    }

    public enum ChatMessageType
    {
        None,
        SayServer,
        Say,
    }

    public enum Mapsize : byte
    {
        Small = 32,
        Normal = 64,
        Large = 128,
        Huge = 192,
    }
}