namespace MineWorld
{
    public partial class LuaManager
    {
        [AttrLuaFunc("PRINT_MSG")]
        public void printMsg(string msg)
        {
            _server.ConsoleWrite(msg);
        }

        [AttrLuaFunc("PRINT_ERROR")]
        public void printError(string msg)
        {
            _server.ConsoleWriteError(msg);
        }

        [AttrLuaFunc("PRINT_SUCCES")]
        public void printSucces(string msg)
        {
            _server.ConsoleWriteSucces(msg);
        }

        [AttrLuaFunc("REGISTER_EVENT")]
        public void registerEvent(string eventName, string functionName)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                _eventTable[eventName].Add(functionName);
            }
        }

        public ServerPlayer getPlayerById(int id)
        {
            foreach (ServerPlayer dummy in _server.PlayerList.Values)
            {
                if (dummy.ID == id)
                {
                    return dummy;
                }
            }
            return null;
        }

        [AttrLuaFunc("KILL_PLAYER")]
        public void killPlayer(int id)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return;
            }
            else
            {
                _server.KillPlayerSpecific(p);
            }
        }

        [AttrLuaFunc("KICK_PLAYER")]
        public void kickPlayer(int id)
        {
            ServerPlayer p = getPlayerById(id);
            if (p == null)
            {
                return;
            }
            else
            {
                //IServer.KickPlayer(
            }
        }

        [AttrLuaFunc("SET_NAME")]
        public void setName(int id, string name)
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
        public string getName(int id)
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
        public void setGodmode(int id, bool flag)
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