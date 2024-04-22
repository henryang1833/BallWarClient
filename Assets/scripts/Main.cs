using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
public class Main : MonoBehaviour
{
    public GameObject foodPrefab;
    public GameObject ballPrefab;
    private Socket socket;
    private string playerName;
    private string playerPass;
    private int playerBallId;
    private string gateIP;
    private int gatePort;
    //消息队列
    private Queue<Dictionary<ProtoField, object>> messages;

    private string messageBuf;
    public enum GAME_STATUS
    {
        CONNECT_TO_GATE, LOG_IN, ENTER, RUNNING, LOG_OUT
    }
    bool isDoing;
    public GAME_STATUS game_status;

    public enum ERROR_CODE
    {
        KICK
    }
    //记录运行过程中的ball
    private Dictionary<int, GameObject> ballRunDict;
    //记录运行过程中的food
    private Dictionary<int, GameObject> foodRunDict;
    private Dictionary<int, int> ballScoreDict;

    private Dictionary<int, int> foodScoreDict;

    // Start is called before the first frame update
    void Start()
    {
        playerName = "101";
        playerPass = "123";
        gateIP = "192.168.3.113";
        gatePort = 8001;
        messageBuf = "";
        ballRunDict = new Dictionary<int, GameObject>();
        foodRunDict = new Dictionary<int, GameObject>();
        ballScoreDict = new Dictionary<int, int>();
        foodScoreDict = new Dictionary<int, int>();
        messages = new Queue<Dictionary<ProtoField, object>>();
        game_status = GAME_STATUS.CONNECT_TO_GATE;
        isDoing = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (game_status)
        {
            case GAME_STATUS.CONNECT_TO_GATE:
                if (!isDoing)
                {
                    ConnetToGate(OnConnectSuccess);
                    isDoing = true;
                }
                break;
            case GAME_STATUS.LOG_IN:
                if (!isDoing)
                {
                    StartCoroutine(Login(onLoginSuccess));
                    isDoing = true;
                }
                break;
            case GAME_STATUS.ENTER:
                if (!isDoing)
                {
                    StartCoroutine(Enter(OnEnterSuccess));
                    isDoing = true;
                }
                break;
            case GAME_STATUS.RUNNING:
                if (isDoing)
                {
                    UpdateGameWorld();
                }
                break;
            case GAME_STATUS.LOG_OUT:
                if (!isDoing)
                {
                    DisConnectToServer();
                    isDoing = true;
                }
                break;
        }
    }

    //连接网关服务器
    private void ConnetToGate(Action<IAsyncResult> callback)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress address = IPAddress.Parse(gateIP);
        EndPoint endPoint = new IPEndPoint(address, gatePort);
        socket.BeginConnect(endPoint, new AsyncCallback(callback), socket);
    }

    //成功连接网关服务器函数
    private void OnConnectSuccess(System.IAsyncResult ar)
    {
        try
        {
            socket.EndConnect(ar);
            Debug.Log("Connectd to gateway!");
            game_status = GAME_STATUS.LOG_IN;
            isDoing = false;
        }
        catch (SocketException e)
        {
            Debug.LogError($"Socket Exception: {e.ToString()}");
            Debug.LogError("网络连接不成功，请检查网络!");
            // todo:弹出提示，网络连接不成功，请检查网络
            OnGameError();
        }
    }
    //运行出错处理
    private void OnGameError()
    {
        throw new NotImplementedException();
    }

    //断开与服务器的连接
    private void DisConnectToServer()
    {
        throw new NotImplementedException();
    }

    //客户端游戏世界各参数更新
    private void UpdateGameWorld()
    {
        //1.处理服务器输入
        ProcessServerInput();
        //2.处理用户输入
        ProcessUserInput();
        //3.插值
        Interpolation();
        throw new NotImplementedException();
    }

    private void Interpolation()
    {
        throw new NotImplementedException();
    }

    private void ProcessUserInput()
    {
        throw new NotImplementedException();
    }

    private void ProcessServerInput()
    {
        Receive();
        while (messages.Count != 0)
        {
            Dictionary<ProtoField, object> message = messages.Dequeue();
            Debug.Assert(message != null && message[ProtoField.PROTO_TYPE] != null);
            switch (message[ProtoField.PROTO_TYPE])
            {
                case GameProto.PROTO_BALL_LIST:
                    DoBallList(message);
                    break;
                case GameProto.PROTO_FOOD_LIST:
                    DoFoodList(message);
                    break;
                case GameProto.PROTO_MOVE:
                    DoMove(message);
                    break;
                case GameProto.PROTO_EAT:
                    DoEat(message);
                    break;
                case GameProto.PROTO_LEAVE:
                    DoLeave(message);
                    break;
                case GameProto.PROTO_KICK:
                    DoKick(message);
                    break;
                default:
                    throw new Exception("got unexcepted proto type");
            }
        }

        throw new NotImplementedException();
    }

    private void DoKick(Dictionary<ProtoField, object> message)
    {
        throw new NotImplementedException();
    }

    private void DoLeave(Dictionary<ProtoField, object> message)
    {
        throw new NotImplementedException();
    }

    private void DoEat(Dictionary<ProtoField, object> message)
    {
        throw new NotImplementedException();
    }

    private void DoBallList(Dictionary<ProtoField, object> message)
    {
        Debug.Assert(message.ContainsKey(ProtoField.BLL_LIST));
        List<Dictionary<ProtoField, object>> balllist = (List<Dictionary<ProtoField, object>>)message[ProtoField.BLL_LIST];
        Debug.Assert(balllist != null && balllist.Count > 0);
        foreach (var ball in balllist)
        {
            int id = (int)ball[ProtoField.BALL_ID];

            GameObject go;
            if (!ballRunDict.ContainsKey(id))
            {
                go = Instantiate(ballPrefab);
                ballRunDict[id] = go;
            }

            go = ballRunDict[id];

            UpdateBall(go, ball);
        }
    }

    private void UpdateBall(GameObject go, Dictionary<ProtoField, object> ball)
    {
        int id = (int)ball[ProtoField.BALL_ID];
        float x = (float)ball[ProtoField.BALL_X];
        float y = (float)ball[ProtoField.BALL_Y];
        float size = (float)ball[ProtoField.BALL_SIZE];
        int score = (int)ball[ProtoField.BALL_SCORE];
        Vector2 pos = new Vector2(x, y);

        go.transform.position = pos;
        go.transform.localScale = Vector2.one * size;
        go.name = $"{ballPrefab.name}_{id}";

        ballScoreDict[id] = score;
    }

    private void DoFoodList(Dictionary<ProtoField, object> message)
    {
        Debug.Assert(message.ContainsKey(ProtoField.FOOD_LIST));
        List<Dictionary<ProtoField, object>> foodlist = (List<Dictionary<ProtoField, object>>)message[ProtoField.FOOD_LIST];
        Debug.Assert(foodlist != null && foodlist.Count > 0);
        foreach (var food in foodlist)
        {
            int id = (int)food[ProtoField.FOOD_ID];

            GameObject go;
            if (!foodRunDict.ContainsKey(id))
            {
                go = Instantiate(foodPrefab);
                foodRunDict[id] = go;
            }

            go = foodRunDict[id];

            UpdateFood(go, food);
        }
    }

    private void UpdateFood(GameObject go, Dictionary<ProtoField, object> food)
    {
        int id = (int)food[ProtoField.FOOD_ID];
        float x = (float)food[ProtoField.FOOD_X];
        float y = (float)food[ProtoField.FOOD_Y];
        float size = (float)food[ProtoField.FOOD_SIZE];
        int score = (int)food[ProtoField.FOOD_SCORE];
        Vector2 pos = new Vector2(x, y);

        go.transform.position = pos;
        go.transform.localScale = Vector2.one * size;
        go.name = $"{foodPrefab.name}_{id}";

        foodScoreDict[id] = score;
    }

    private void DoMove(Dictionary<ProtoField, object> ball)
    {
        Debug.Assert(ball.Count == 8);
        int id = (int)ball[ProtoField.BALL_ID];
        if (id == playerBallId)
        {
            //1.依据权威值更新ball的状态
            GameObject go;
            if (!ballRunDict.ContainsKey(id))
            {
                go = Instantiate(ballPrefab);
                ballRunDict[id] = go;
            }
            go = ballRunDict[id];
            UpdateBall(go, ball);

            //2.处理pending_state
            ServerReconciliation();
        }else
        {
            //3.其他球插值处理
            ReadyInterpolationData();
        }
         
        throw new NotImplementedException();
    }

    private void ReadyInterpolationData()
    {
        throw new NotImplementedException();
    }

    private void ServerReconciliation()
    {
        throw new NotImplementedException();
    }

    //进入游戏场景
    private IEnumerator Enter(Action onEnterSuccess)
    {
        Send(GameProto.EnterProto());
        Receive();
        while (messages.Count == 0)
        {
            yield return null;
        }

        Dictionary<ProtoField, object> message = messages.Dequeue();

        Debug.Assert((string)message[ProtoField.PROTO_TYPE] == GameProto.PROTO_ENTER);

        int requestCode = (int)message[ProtoField.REQUEST_CODE];
        switch (requestCode)
        {
            case GameProto.STATUS_SUCCESS:
                onEnterSuccess();
                break;
            case GameProto.STATUS_FAIL:
                OnGameError();
                break;
            default:
                throw new Exception("requestCode:got unexcepted code value");
        }
    }
    
    //进入游戏场景成功回调函数
    private void OnEnterSuccess()
    {
        game_status = GAME_STATUS.RUNNING;
        isDoing = true;
    }
    
    //登录成功回调函数
    private void onLoginSuccess()
    {
        game_status = GAME_STATUS.ENTER;
        isDoing = false;
    }

    //客户端登录
    private IEnumerator Login(Action onSucceeLogin)
    {
        //1.发送登录协议
        Send(GameProto.LoginProto(playerName, playerPass));
        Receive();

        while (messages.Count == 0)
        {
            yield return null;
        }

        Dictionary<ProtoField, object> message = messages.Dequeue();

        Debug.Assert((string)message[ProtoField.PROTO_TYPE] == GameProto.PROTO_LOGIN);

        int requestCode = (int)message[ProtoField.REQUEST_CODE];
        switch (requestCode)
        {
            case GameProto.STATUS_SUCCESS:
                onSucceeLogin();
                break;
            case GameProto.STATUS_FAIL:
                OnGameError();
                break;
            default:
                throw new Exception("requestCode:got unexcepted code value");
        }
    }

    private void OnKick()
    {
        throw new NotImplementedException();
    }

    void Send(string data)
    {
        try
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), socket);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            OnGameError();
        }
    }

    private void SendCallback(IAsyncResult AR)
    {
        try
        {
            int bytesSent = socket.EndSend(AR);
            Debug.Log($"Sent {bytesSent} bytes to gate.");
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            OnGameError();
        }
    }
    // "hello\r\nworld\r\nhihi\r\ntian\r\nkong\r\n"
    public void Receive()
    {
        try
        {
            // 创建一个缓冲区来接收数据
            byte[] buffer = new byte[1024];
            // 异步开始接收数据
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), buffer);
        }
        catch (Exception e)
        {
            OnGameError();
            Debug.LogError($"Receive failed: {e.Message}");
        }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        try
        {
            // 完成接收数据
            int bytesRead = socket.EndReceive(AR);

            if (bytesRead > 0)
            {
                // 将接收到的字节转换为字符串
                byte[] buffer = (byte[])AR.AsyncState;
                string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                messageBuf += receivedData;

                //提取出message并送入messages队列
                string[] parts = messageBuf.Split(GameProto.separator);

                Debug.Assert(parts != null && parts.Length > 0);

                // 检查原始字符串是否以 \r\n 结尾
                bool endsWithSeparator = messageBuf.EndsWith(GameProto.separator);

                for (int i = 0; i < parts.Length - 1; ++i)
                {
                    Dictionary<ProtoField, object> message = GameProto.ToProtoData(parts[i]);
                    messages.Enqueue(message);
                }

                if (endsWithSeparator)
                {
                    messageBuf = "";
                    Dictionary<ProtoField, object> message = GameProto.ToProtoData(parts[parts.Length - 1]);
                    messages.Enqueue(message);
                }
                else
                {
                    messageBuf = parts[parts.Length - 1];
                }
            }
        }
        catch (Exception e)
        {
            OnGameError();
            Debug.Log($"Receive callback failed: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        if (socket != null)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        throw new NotImplementedException();
    }
}
