using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
public class Main : MonoBehaviour
{
    public GameObject foodPrefab;
    public GameObject ballPrefab;
    public GameObject playerBall;
    private Socket socket;
    private string playerName;
    private string playerPass;
    private int playerBallId;
    private string gateIP;
    private int gatePort;
    private float speed;
    private int serverUpdateRate;
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

    //其他球的位置缓冲区，以便插值
    private Dictionary<int, List<Dictionary<ProtoField, object>>> otherBallPosBuffer;

    private LinkedList<Dictionary<ProtoField, object>> pedingInput;

    // Start is called before the first frame update
    void Start()
    {
        playerName = "101";
        playerPass = "123";
        gateIP = "192.168.3.113";
        gatePort = 8001;
        messageBuf = "";
        playerBallId = int.Parse(playerName);
        ballRunDict = new Dictionary<int, GameObject>();
        foodRunDict = new Dictionary<int, GameObject>();
        ballScoreDict = new Dictionary<int, int>();
        foodScoreDict = new Dictionary<int, int>();
        messages = new Queue<Dictionary<ProtoField, object>>();
        pedingInput = new LinkedList<Dictionary<ProtoField, object>>();
        otherBallPosBuffer = new Dictionary<int, List<Dictionary<ProtoField, object>>>();
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
        if (!ballRunDict.ContainsKey(playerBallId))
            return;
        DoPredictMove();
        DoPredictEat();
        DoPredictKill();
    }

    private void DoPredictKill()
    {
        foreach (var pair in ballRunDict)
        {
            if (pair.Key == playerBallId) continue;
            if (pair.Value.activeSelf && CheckCollide(pair.Value, playerBall))
            {
                int sessionId = GameProto.SessionId;
                string input = GameProto.KillProto(playerBallId, sessionId, pair.Key);
                Send(input);

                Dictionary<ProtoField, object> inputCopy = new Dictionary<ProtoField, object>();
                inputCopy[ProtoField.SESSION_ID] = sessionId;
                inputCopy[ProtoField.PROTO_TYPE] = GameProto.PROTO_KILL;
                inputCopy[ProtoField.BALL_ID] = pair.Key;
                pedingInput.AddLast(inputCopy);

                ApplyInput(inputCopy);
            }
        }
    }

    private void DoPredictEat()
    {
        foreach (var pair in foodRunDict)
        {
            if (pair.Value.activeSelf && CheckCollide(pair.Value, playerBall))
            {
                int sessionId = GameProto.SessionId;
                string input = GameProto.EatProto(playerBallId, sessionId, pair.Key);
                Send(input);

                Dictionary<ProtoField, object> inputCopy = new Dictionary<ProtoField, object>();
                inputCopy[ProtoField.SESSION_ID] = sessionId;
                inputCopy[ProtoField.PROTO_TYPE] = GameProto.PROTO_EAT;
                inputCopy[ProtoField.FOOD_ID] = pair.Key;
                pedingInput.AddLast(inputCopy);

                ApplyInput(inputCopy);
            }
        }
    }

    private void DoPredictMove()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        float deltaTime = Time.deltaTime;
        if (inputX != 0 || inputY != 0)
        {
            int sessionId = GameProto.SessionId;
            string input = GameProto.MoveProto(playerBallId, sessionId, inputX, inputY, deltaTime);
            //1.向服务器发送消息
            Send(input);

            //2.放入pendingInput
            Dictionary<ProtoField, object> inputCopy = new Dictionary<ProtoField, object>();
            inputCopy[ProtoField.SESSION_ID] = sessionId;
            inputCopy[ProtoField.PROTO_TYPE] = GameProto.PROTO_MOVE;
            inputCopy[ProtoField.INPUT_X] = inputX;
            inputCopy[ProtoField.INPUT_Y] = inputY;
            inputCopy[ProtoField.DEALT_TIME] = deltaTime;
            pedingInput.AddLast(inputCopy);

            //3.客户端预测
            ApplyInput(inputCopy);
        }
    }

    private bool CheckCollide(GameObject go1, GameObject go2)
    {
        Transform transform1 = go1.transform;
        Transform transform2 = go2.transform;

        Vector2 pos1 = transform1.position;
        Vector2 pos2 = transform2.position;

        float r1 = transform1.localScale.x / 2;
        float r2 = transform2.localScale.x / 2;

        if (Vector2.Distance(pos1, pos2) <= r1 + r2)
            return true;
        else
            return false;
    }

    private void ApplyInput(Dictionary<ProtoField, object> input)
    {
        switch (input[ProtoField.PROTO_TYPE])
        {
            case GameProto.PROTO_MOVE:
                {
                    float inputX = (float)input[ProtoField.INPUT_X];
                    float inputY = (float)input[ProtoField.INPUT_Y];
                    float deltaTime = (float)input[ProtoField.DEALT_TIME];
                    if (ballRunDict.ContainsKey(playerBallId)) //存活才移动预测
                    {
                        Transform transform = ballRunDict[playerBallId].transform;
                        transform.Translate(new Vector2(inputX, inputY) * speed * deltaTime);
                    }
                }
                break;
            case GameProto.PROTO_EAT:
                {
                    int foodId = (int)input[ProtoField.FOOD_ID];
                    //1.更新分数
                    ballScoreDict[playerBallId] += foodScoreDict[foodId];
                    //2.更新大小
                    playerBall.transform.localScale = Vector2.one * ballScoreDict[playerBallId] * 0.1f;
                    //3.将food隐藏
                    foodRunDict[foodId].SetActive(false);
                }
                break;
            case GameProto.PROTO_KILL:
                {
                    int ballId = (int)input[ProtoField.BALL_ID];
                    if (ballScoreDict[playerBallId] >= ballScoreDict[ballId])
                    {
                        //1.更新分数
                        ballScoreDict[playerBallId] += ballScoreDict[ballId];
                        //2.更新大小
                        playerBall.transform.localScale = Vector2.one * ballScoreDict[playerBallId] * 0.1f;
                        //3.将food隐藏
                        ballRunDict[ballId].SetActive(false);
                    }
                    else
                    {
                        //1.更新分数
                        ballScoreDict[ballId] += ballScoreDict[playerBallId];
                        //2.更新大小
                        ballRunDict[ballId].transform.localScale = Vector2.one * ballScoreDict[ballId] * 0.1f;
                        //3.将food隐藏
                        playerBall.SetActive(false);
                    }
                }
                break;
            default:
                throw new Exception("got unexcepted value!");
        }
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
                case GameProto.PROTO_KILL:
                    DoKill(message);
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
                if (id == playerBallId)
                    go = playerBall;
                else
                    go = Instantiate(ballPrefab);
                ballRunDict[id] = go;
            }

            go = ballRunDict[id];

            UpdateBall(go, ball);
        }
    }

    private void UpdateBall(GameObject go, Dictionary<ProtoField, object> ballStatus)
    {
        int id = (int)ballStatus[ProtoField.BALL_ID];
        float x = (float)ballStatus[ProtoField.BALL_X];
        float y = (float)ballStatus[ProtoField.BALL_Y];
        float size = (float)ballStatus[ProtoField.BALL_SIZE];
        int score = (int)ballStatus[ProtoField.BALL_SCORE];
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

    private void DoMove(Dictionary<ProtoField, object> ballStatus)
    {
        Debug.Assert(ballStatus.Count == 8);
        int id = (int)ballStatus[ProtoField.BALL_ID];
        if (id == playerBallId)
        {
            //1. 依据权威值更新ball的状态
            GameObject go;
            if (!ballRunDict.ContainsKey(id))
            {
                go = Instantiate(ballPrefab);
                ballRunDict[id] = go;
            }
            go = ballRunDict[id];
            UpdateBall(go, ballStatus);

            int lastSessionId = (int)ballStatus[ProtoField.SESSION_ID];
            //2.处理pending_state
            ServerReconciliation(lastSessionId);
        }
        else
        {
            //3. 为其他球准备插值数据
            ReadyInterpolationData(ballStatus);
        }
    }

    private void ServerReconciliation(int lastSessionId)
    {
        for (var node = pedingInput.First; node != null;)
        {
            var nextNode = node.Next;
            int sessionId = (int)node.Value[ProtoField.SESSION_ID];
            //删除所有已确认的服务器输入
            if (sessionId <= lastSessionId)
            {
                pedingInput.Remove(node);
            }
            else
            {
                ApplyInput(node.Value);
            }

            node = nextNode;
        }
    }

    private void ReadyInterpolationData(Dictionary<ProtoField, object> ballStatus)
    {
        int id = (int)ballStatus[ProtoField.BALL_ID];
        long now = GameProto.Now();
        float x = (float)ballStatus[ProtoField.BALL_X];
        float y = (float)ballStatus[ProtoField.BALL_Y];

        if (!otherBallPosBuffer.ContainsKey(id))
        {
            List<Dictionary<ProtoField, object>> posBuffer = new List<Dictionary<ProtoField, object>>();
            otherBallPosBuffer[id] = posBuffer;
        }
        var buffer = otherBallPosBuffer[id];

        Dictionary<ProtoField, object> otherBallPosInfo = new Dictionary<ProtoField, object>();
        otherBallPosInfo[ProtoField.TIME_STAMP] = now;
        otherBallPosInfo[ProtoField.BALL_X] = x;
        otherBallPosInfo[ProtoField.BALL_Y] = y;

        buffer.Add(otherBallPosInfo);
    }

    private void DoEat(Dictionary<ProtoField, object> message)
    {
        int bid = (int)message[ProtoField.BALL_ID];
        int fid = (int)message[ProtoField.FOOD_ID];
        if (bid == playerBallId) //本玩家的吃
        {
            //更新ball
            if (ballRunDict.ContainsKey(bid)) //本文家存活则更新
                UpdateBall(playerBall, message);

            //更新food
            int code = (int)message[ProtoField.REQUEST_CODE];
            if (code == 1) //请求失败 原因：被其他玩家吃了，坐标不对，本玩家已经死亡
            {
                if (foodRunDict.ContainsKey(fid))
                    foodRunDict[fid].SetActive(true);
            }
            else //请求成功
            {
                if (foodRunDict.ContainsKey(fid))
                {
                    Destroy(foodRunDict[fid]);
                    foodRunDict.Remove(fid);
                }
                //玩家死亡，只更新分数
                int score = (int)message[ProtoField.BALL_SCORE];
                ballScoreDict[bid] = score;
            }

            //处理pending
            int lastSessionId = (int)message[ProtoField.SESSION_ID];
            ServerReconciliation(lastSessionId);
        }
        else //其他玩家球的吃 , 一定是请求成功
        {
            Debug.Assert((int)message[ProtoField.REQUEST_CODE] == 0);

            if (ballRunDict.ContainsKey(bid))//其他玩家若存活则延迟更新
                StartCoroutine(LagUpdateBall(bid, serverUpdateRate));
            else    //否则，只更新分数
            {
                int score = (int)message[ProtoField.BALL_SCORE];
                StartCoroutine(LagUpdateScore(bid, score));
            }

            if (foodRunDict.ContainsKey(fid))//延迟删除food
                StartCoroutine(LagRemoveFood(fid, serverUpdateRate));
        }
    }

    private IEnumerator LagUpdateScore(int bid, int score)
    {
        yield return new WaitForSeconds(1.0f / serverUpdateRate);
        ballScoreDict[bid] = score;
    }

    private IEnumerator LagRemoveFood(int fid, int serverUpdateRate)
    {
        yield return new WaitForSeconds(1.0f / serverUpdateRate);
        Destroy(foodRunDict[fid]);
        foodRunDict.Remove(fid);
    }

    private IEnumerator LagUpdateBall(int bid, int serverUpdateRate)
    {
        yield return new WaitForSeconds(1.0f / serverUpdateRate);

        throw new NotImplementedException();
    }

    private void DoKill(Dictionary<ProtoField, object> message)
    {
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
