using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LuaInterface;

namespace MineWorld
{
    public partial class LuaManager
    {
        private readonly MineWorldServer _server;
        private readonly Lua _luaVm;

        private readonly Dictionary<string, List<string>> _eventTable = new Dictionary<string, List<string>>();
        private string _folderPath;

        private DateTime _lastUpdate;

        public LuaManager(MineWorldServer s)
        {
            _server = s;
            _luaVm = new Lua();

            _eventTable["ON_UPDATE"] = new List<string>();
            _eventTable["ON_PLAYERDIED"] = new List<string>();

            RegisterLuaFunctions(this);
            //RegisterLuaFunctions(LuaF);
        }

        public void RegisterLuaFunctions(Object o)
        {
            Type pTrgType = o.GetType();
            foreach (MethodInfo mInfo in pTrgType.GetMethods())
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    if (attr.GetType() == typeof (AttrLuaFunc))
                    {
                        AttrLuaFunc pAttr = (AttrLuaFunc) attr;
                        _luaVm.RegisterFunction(
                            pAttr.GetFuncName(),
                            o,
                            mInfo);
                    }
                }
            }
        }

        public void LoadScriptFiles(string path)
        {
            _folderPath = path;
            List<String> addonsToLoad = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(File.OpenRead("scripts\\Addons.cfg"));
                String line = sr.ReadLine();
                while (line != null)
                {
                    addonsToLoad.Add(line);
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch
            {
                foreach (FileInfo fileName in (new DirectoryInfo(_folderPath)).GetFiles())
                {
                    if (fileName.Name.Contains(".lua"))
                    {
                        addonsToLoad.Add(fileName.Name);
                    }
                }
            }
            foreach (FileInfo f in (new DirectoryInfo(_folderPath)).GetFiles())
            {
                if (f.Name.Contains(".lua") && addonsToLoad.Contains(f.Name))
                {
                    _luaVm.DoFile(f.FullName);
                    //LOADED !!!
                }
            }
        }

        public void RaiseEvent(string eventName, string arguments)
        {
            if (_eventTable.ContainsKey(eventName))
            {
                foreach (string s in _eventTable[eventName])
                {
                    _luaVm.DoString(s + "(" + arguments + ")");
                }
            }
        }

        public void Update()
        {
            int elapsedTime = (int) Math.Round((DateTime.Now - _lastUpdate).TotalMilliseconds);
            foreach (string s in _eventTable["ON_UPDATE"])
            {
                try
                {
                    _luaVm.DoString(s + "(" + elapsedTime + ");");
                }
                catch (Exception)
                {
                }
            }
            _lastUpdate = DateTime.Now;
        }
    }
}