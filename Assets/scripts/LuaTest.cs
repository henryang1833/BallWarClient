using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
public class LuaTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LuaEnv luaEnv = new LuaEnv();
        luaEnv.AddLoader(MyCustomerLoader);
        luaEnv.AddLoader(MyABCustomLoader);
        if (luaEnv.DoString("return require 'MyLua'") == null)
            Debug.Log("null");
        else
            Debug.Log("not null");

        // Lua脚本字符串，返回一个值
        object[] result = luaEnv.DoString("return 10 * 5");

        // 检查结果并输出
        if (result.Length > 0)
        {
            Debug.Log(result[0]);  // 输出: 50
        }

        // 执行更复杂的Lua代码并获取多个返回值
        object[] results = luaEnv.DoString("return 10, 'hello'");
        if (results.Length > 0)
        {
            Debug.Log(results[0]);  // 输出: 10
            Debug.Log(results[1]);  // 输出: hello
        }
    }

    byte[] MyCustomerLoader(ref string filepath)
    {
        string path = Application.dataPath + "/Lua/" + filepath + ".lua";
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        return null;
    }

    byte[] MyABCustomLoader(ref string filepath)
    {
        //先加载Lua文件所在的AB包
        AssetBundle abPage = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/lua");
        //从AB包中加载对应的Lua文件
        TextAsset luaText = abPage.LoadAsset<TextAsset>(filepath + ".lua.txt");

        if (luaText != null)
            return luaText.bytes;

        return null;
    }
}
