﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MineWorld
{
    public enum Buttons
    {
        None=0,

        Fire,
        AltFire,
        
        Forward,
        Backward,
        Left,
        Right,
        Sprint,
        Jump,
        Crouch,

        Ping,
        Deposit,
        Withdraw,
        
        //All buttons past this point will never be sent to the server
        SayAll,
        SayTeam,

        ChangeClass,
        ChangeTeam,

        Tool1,
        Tool2,
        Tool3,
        Tool4,
        Tool5,
        ToolUp,
        ToolDown,
        
        BlockUp,
        BlockDown
    }

    public enum MouseButton
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
        UseTool,                // position, heading, tool, blocktype 
        SelectClass,            // class
        ResourceUpdate,         // ore, cash, weight, max ore, max weight, team ore, red cash, blue cash: ReliableInOrder1
        DepositOre,
        DepositCash,
        WithdrawOre,
        TriggerExplosion,       // position

        PlayerUpdate,           // (uint id for server), position, heading, current tool, animate using (bool): UnreliableInOrder1
        PlayerJoined,           // uint id, player name :ReliableInOrder2
        PlayerLeft,             // uint id              :ReliableInOrder2
        PlayerSetTeam,          // (uint id for server), byte team   :ReliableInOrder2
        PlayerDead,             // (uint id for server) :ReliableInOrder2
        PlayerAlive,            // (uint id for server) :ReliableInOrder2
        PlayerPing,             // uint id

        ChatMessage,            // byte type, string message : ReliableInOrder3
        GameOver,               // byte team
        PlaySound,              // byte sound, bool isPositional, ?Vector3 location : ReliableUnordered
        TriggerConstructionGunAnimation,
        SetBeacon,              // vector3 position, string text ("" means remove)

        //Update by Oeds
        Hearthbeat,
        PlayerCommand,         // uint id, string command  This is sent by client
        PlayerCommandEnable,   // byte Playercommands this is sent by the server
        PlayerUpdate1,         // minus position
        PlayerUpdate2,         // minus heading
        PlayerHurt,             // allows client to tell server of damage
        PlayerPosition,         // server sends client new position
        PlayerRespawn,          // allows the player to respawn
        Killed,                 // Send by the server to notify the player has been killed
        //KillPlayer,            // Send by the server to notify that the player has died
        //RespawnPlayer,         // vector 3 position, Send by the server to notify that the player has respawned
        DayUpdate,              // float , Send by the server to notify the player how dark or light the game is
    }

    public enum PlayerCommands : byte
    {
        None,
        Help,
        Godmode,
        Stopfluids,
        Startfluids,
        Noadmin,
        Nocost,
        Teleportto,
        Kill,
        Setday,
        Setnight,

        Announce,
        Restart,
    }

    public enum ChatMessageType
    {
        None,
        SayAll,
        SayRedTeam,
        SayBlueTeam,
    }

    public enum Mapsize : byte
    {
        Small = 32,
        Normal = 64,
        Large = 128,
        Huge = 192,
    }

    public class ChatMessage
    {
        public string message;
        public ChatMessageType type;
        public float timestamp;
        public int newlines;

        public ChatMessage(string message, ChatMessageType type, float timestamp, int newlines)
        {
            this.message = message;
            this.type = type;
            this.timestamp = timestamp;
            this.newlines = newlines;
        }
    }

    public class Beacon
    {
        public string ID;
        public PlayerTeam Team;
    }
}