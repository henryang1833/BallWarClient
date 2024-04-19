using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
using System;
using XLuaTest;
using XLua.LuaDLL;
using Unity.Mathematics;
using System.Security.Cryptography;
[LuaCallCSharp]
public class ClientSocket : MonoBehaviour
{

    public GameObject foodPrefab; // 将预制体拖拽到这个公共变量上
    public GameObject ballPrefab; // 将预制体拖拽到这个公共变量上
    public GameObject playerBall;
    public enum GAME_STATUS
    {
        Login = 0, Enter = 1, Running = 2, LogOut = 3
    };
    public GAME_STATUS game_status;
    LuaEnv luaEnv = null;
    Dictionary<int, GameObject> foodItem;
    Dictionary<int, GameObject> ballItem;
    public int[] foodInfo;
    public int foodInfoLen;
    LuaFunction luaMove;
    void Start()
    {
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(MyCustomerLoader);
        luaEnv.AddLoader(MyABCustomLoader);
        object[] res = luaEnv.DoString("return require 'clientsocket'");
        luaEnv.Global.Set("OnFoodEat", (System.Action<int>)OnFoodEat);
        luaEnv.Global.Set("csObj", this.gameObject);
        foodItem = new Dictionary<int, GameObject>();
        ballItem = new Dictionary<int, GameObject>();
        playerBall = GameObject.Find("PlayerBall");
        if (res != null)
            Debug.Log(res.Length);
        else
            Debug.Log("res is null");

        if (Convert.ToInt32(res[0]) == 1)
        {
            //登录
            game_status = GAME_STATUS.Login;
            luaEnv.DoString("client:send('login,'..playerid..','..playerpass..endStr)");
        }
        // 获取Lua函数
        luaMove = luaEnv.Global.Get<LuaFunction>("move");

    }


    // 供 Lua 调用的方法，用于创建预制体并设置位置
    public GameObject CreatePrefab(GameObject prefab, int id, float x, float y, float scale)
    {
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            instance.transform.localScale = Vector3.one * scale;
            instance.name = string.Format("{0}_{1}",prefab.name,id);
            return instance;
        }
        return null;
    }

    void Update()
    {
        luaEnv.DoString("recv_loop()");
        List<object> foodInfo = luaEnv.Global.Get<List<object>>("foodTable");
        List<object> ballInfo = luaEnv.Global.Get<List<object>>("ballTable");


        int foodId = 0;
        int ballId = 0;
        float x = 0, y = 0, scale = 0.5f;
        Debug.Log("foodInfo == null:" + (foodInfo == null));
        Debug.Log("foodInfo.Count:" + foodInfo.Count);
        Debug.Log("ballInfo == null:" + (ballInfo == null));
        Debug.Log("ballInfo.Count:" + ballInfo.Count);

        for (int i = 0; i < foodInfo.Count; ++i)
        {
            if (i % 4 == 0)
            {
                foodId = Convert.ToInt32(foodInfo[i]);

            }
            else if (i % 4 == 1)
            {
                x = Convert.ToSingle(foodInfo[i]);
            }
            else if (i % 4 == 2)
            {
                y = Convert.ToSingle(foodInfo[i]);
            }
            else
            {
                scale = Convert.ToSingle(foodInfo[i]);
                if (!foodItem.ContainsKey(foodId))
                {
                    GameObject go = CreatePrefab(foodPrefab, foodId, x, y, scale);
                    foodItem.Add(foodId, go);
                }
                else
                {
                    foodItem[foodId].transform.position = new Vector2(x, y);
                    foodItem[foodId].transform.localScale = Vector3.one * scale;
                }
            }
        }

        for (int i = 0; i < ballInfo.Count; ++i)
        {
            if (i % 4 == 0)
            {
                ballId = Convert.ToInt32(ballInfo[i]);
            }
            else if (i % 4 == 1)
            {
                x = Convert.ToSingle(ballInfo[i]);
            }
            else if (i % 4 == 2)
            {
                y = Convert.ToSingle(ballInfo[i]);
            }
            else
            {
                scale = Convert.ToSingle(ballInfo[i]);
                if (!ballItem.ContainsKey(ballId))
                {
                    GameObject go = null;
                    int playerid = luaEnv.Global.Get<int>("playerid");
                    if (ballId == playerid)
                    {
                        go = playerBall;
                        go.transform.position = new Vector2(x, y);
                        go.transform.localScale = Vector3.one * scale;
                    }
                    else
                        go = CreatePrefab(ballPrefab, ballId, x, y, scale);

                    ballItem.Add(ballId, go);
                }
                else
                {
                    //改变玩家球坐标，若是本玩家，若在一定阈值以本地坐标为主，
                    //超过一定阈值以服务器坐标为主，其他玩家以服务器坐标为主
                    ballItem[ballId].transform.position = new Vector2(x, y);
                    ballItem[ballId].transform.localScale = Vector3.one * scale;
                }
            }
        }
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        if (horizontalInput != 0 || verticalInput != 0)
        {
            Debug.Log("horizontalInput:" + horizontalInput + ",verticalInput:" + verticalInput);
            // 检查函数是否成功获取
            if (luaMove != null)
                luaMove.Call(Math.Sign(horizontalInput), Math.Sign(verticalInput)); // 调用函数
            else
                Debug.LogError("luaMove is null");
        }
    }
    void VerifyLoginSucc()
    {
        object[] res = luaEnv.DoString("client:receive()");

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
    [LuaCallCSharp]
    public void OnFoodEat(int foodId)
    {
        Debug.Assert(foodItem.ContainsKey(foodId));
        Debug.Log("resp.eat:Destory " + foodItem[foodId].name);
        Destroy(foodItem[foodId]); 
        foodItem.Remove(foodId);
    }
}
