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

        //All buttons past this point will never be sent to the server
        Say,
    }

    public enum CustomMouseButtons : byte
    {
        None = 0,

        Fire,
        AltFire,
    }

    public enum ScreenEffect
    {
        None,
        Death,
        Fall,
        Explosion,
        Drowning,
        Water,
    }

    public enum MineWorldSound
    {
        Dig,
        Build,
        Death,
        ClickHigh,
        ClickLow,
        GroundHit,
        Explosion,
    }

    public enum MineWorldMessage : byte
    {
        BlockBulkTransfer, // x-value, y-value, followed by 64 bytes of blocktype

        BlockSet, // x, y, z, type
        UseTool, // position, heading, blocktype
        HealthUpdate, // ore, cash, weight, max ore, max weight, team ore, red cash, blue cash: ReliableInOrder1
        TriggerExplosion, // position

        PlayerUpdate, // (int id for server), position, heading : UnreliableInOrder1
        PlayerUpdate1, // (int id for server), heading : UnreliableInOrder1
        PlayerJoined, // int id, player name :ReliableInOrder2
        PlayerLeft, // int id              :ReliableInOrder2
        PlayerDead, // (int id for server) :ReliableInOrder2
        PlayerAlive, // (int id for server) :ReliableInOrder2

        ChatMessage, // byte type, string message : ReliableInOrder3
        PlaySound, // byte sound, bool isPositional, ?Vector3 location : ReliableUnordered

        //Update by Oeds
        Hearthbeat,
        PlayerHurt, // allows client to tell server of damage
        PlayerPosition, // server sends client new position
        DayUpdate, // float , Send by the server to notify the player how dark or light the game is
        PlayerRequest, // Client requests something from the server
        PlayerId, // Send the client a number who is given by the server
    }

    public enum PlayerRequests : byte
    {
        Respawn,
    }

    public enum ChatMessageType
    {
        None,
        Server,
        PlayerSay,
    }
}