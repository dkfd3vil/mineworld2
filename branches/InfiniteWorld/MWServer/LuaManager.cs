using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using LuaInterface;

namespace MineWorld
{
    public partial class LuaManager
    {
        Lua LuaVM;
        private MineWorldServer IServer;
        //LuaFunctions LuaF;

        Dictionary<string, List<string>> eventTable = new Dictionary<string, List<string>>();

        DateTime ProgramStartTime = DateTime.Now;
        DateTime lastUpdate;

        string FolderPath;

        public LuaManager(MineWorldServer s)
        {
            IServer = s;
            LuaVM = new Lua();
            //LuaF = new LuaFunctions();

            try
            {
                eventTable["ON_UPDATE"] = new List<string>();
                eventTable["ON_PLAYERDIED"] = new List<string>();

                RegisterLuaFunctions(this);
                //RegisterLuaFunctions(LuaF);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void RegisterLuaFunctions(Object o)
        {
            try
            {
                Type pTrgType = o.GetType();
                foreach (MethodInfo mInfo in pTrgType.GetMethods())
                {
                    foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                    {
                        if (attr.GetType() == typeof(AttrLuaFunc))
                        {
                            AttrLuaFunc pAttr = (AttrLuaFunc)attr;
                            LuaVM.RegisterFunction(
                                pAttr.getFuncName(),
                                o,
                                mInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void LoadScriptFiles(string path)
        {
            FolderPath = path;
            try
            {
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
                    foreach (FileInfo fileName in (new DirectoryInfo(FolderPath)).GetFiles())
                    {
                        if (fileName.Name.Contains(".lua"))
                        {
                            addonsToLoad.Add(fileName.Name);
                        }
                    }
                }
                foreach (FileInfo f in (new DirectoryInfo(FolderPath)).GetFiles())
                {
                    if (f.Name.Contains(".lua") && addonsToLoad.Contains(f.Name))
                    {
                        LuaVM.DoFile(f.FullName);
                        //LOADED !!!
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void RaiseEvent(string eventName, string arguments)
        {
            if (eventTable.ContainsKey(eventName))
            {
                foreach (string s in eventTable[eventName])
                {
                    LuaVM.DoString(s + "(" + arguments + ")");
                }
            }
        }

        public void Update()
        {
            try
            {
                if (lastUpdate == null) lastUpdate = DateTime.Now;
                int elapsedTime = (int)Math.Round((DateTime.Now - lastUpdate).TotalMilliseconds);
                foreach (string s in eventTable["ON_UPDATE"])
                {
                    try
                    {
                        LuaVM.DoString(s + "(" + elapsedTime + ");");
                    }
                    catch
                    { }
                }
                lastUpdate = DateTime.Now;
            }
            catch (Exception e)
            {
                throw e;
            }
        } 
    }
}
