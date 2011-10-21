using System;
using System.Collections.Generic;
using System.Text;

namespace MineWorld
{
    public partial class LuaManager
    {
        [AttrLuaFunc("PRINT_MSG")]
        public void printMsg(string msg)
        {
            IServer.ConsoleWrite(msg);
        }

        [AttrLuaFunc("PRINT_ERROR")]
        public void printError(string msg)
        {
            IServer.ConsoleWriteError(msg);
        }

        [AttrLuaFunc("PRINT_SUCCES")]
        public void printSucces(string msg)
        {
            IServer.ConsoleWriteSucces(msg);
        }

        [AttrLuaFunc("REGISTER_EVENT")]
        public void registerEvent(string eventName, string functionName)
        {
            if (eventTable.ContainsKey(eventName))
            {
                eventTable[eventName].Add(functionName);
            }
        }

        public ServerPlayer getPlayerById(int id)
        {
            foreach (ServerPlayer dummy in IServer.playerList.Values)
            {
                if (dummy.ID == id)
                {
                    return dummy;
                }
            }
            return null;
        }

        [AttrLuaFunc("KILL_PLAYER")]
        public void setGodmode(int id)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return;
            }
            else
            {
                IServer.KillPlayerSpecific(p);
            }
        }

        [AttrLuaFunc("KICK_PLAYER")]
        public void setGodmode(int id)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return;
            }
            else
            {
                IServer.KickPlayer(
            }
        }

        [AttrLuaFunc("SET_NAME")]
        public void setGodmode(int id, string name)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return;
            }
            else
            {
                p.Name = name;
            }
        }

        [AttrLuaFunc("GET_NAME")]
        public string getGodmode(int id)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return "";
            }
            else
            {
                return p.Name;
            }
        }

        [AttrLuaFunc("SET_GODMODE")]
        public void setGodmode(int id,bool flag)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return;
            }
            else
            {
                p.Godmode = flag;
            }
        }

        [AttrLuaFunc("GET_GODMODE")]
        public bool getGodmode(int id)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return false;
            }
            else
            {
                return p.Godmode;
            }
        }
    }
}
