using System;

namespace MineWorld
{
    /*
     * Pretty much a Struct Class for Lua functions, where input parameters only require the name of the function.
     * Optional parameters include the documentation and additional parameters
     */

    internal class AttrLuaFunc : Attribute
    {
        private readonly String _functionDoc;
        private readonly String _functionName;
        private readonly String[] _functionParameters;

        public AttrLuaFunc(String strFuncName, String strFuncDoc, params String[] strParamDocs)
        {
            _functionName = strFuncName;
            _functionDoc = strFuncDoc;
            _functionParameters = strParamDocs;
        }

        public AttrLuaFunc(String strFuncName, String strFuncDoc)
        {
            _functionName = strFuncName;
            _functionDoc = strFuncDoc;
        }

        public AttrLuaFunc(String strFuncName)
        {
            _functionName = strFuncName;
            _functionDoc = "";
        }

        public String GetFuncName()
        {
            return _functionName;
        }

        public String GetFuncDoc()
        {
            return _functionDoc;
        }

        public String[] GetFuncParams()
        {
            return _functionParameters;
        }
    }
}