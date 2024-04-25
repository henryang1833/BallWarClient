using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum ProtoField
{
    SESSION_ID, TIME_STAMP, PROTO_TYPE, REQUEST_CODE, REASON,
    BALL_ID, BALL_X, BALL_Y, BALL_SIZE, BALL_SCORE, BLL_LIST, BALL_ADD,
    FOOD_ID, FOOD_X, FOOD_Y, FOOD_SIZE, FOOD_SCORE, FOOD_LIST, FOOD_ADD,
    INPUT_X, INPUT_Y, DEALT_TIME,
}

public class GameProto
{
    public const int STATUS_SUCCESS = 0;
    public const int STATUS_FAIL = 1;
    public const string PROTO_LOGIN = "login";
    public const string PROTO_ENTER = "enter";
    public const string PROTO_MOVE = "move";
    public const string PROTO_KICK = "kick";
    public const string PROTO_LEAVE = "leave";
    public const string PROTO_EAT = "eat";
    public const string PROTO_KILL = "kill";
    public const string PROTO_BALL_LIST = "balllist";
    public const string PROTO_FOOD_LIST = "foodlist";
    public const string PROTO_BALL_ADD = "addball";
    public const string PROTO_FOOD_ADD = "addfood";
    public static string separator = "\r\n";
    public static int sessionId = 0;
    public static int SessionId
    {
        get
        {
            return sessionId++;
        }
    }

    //将协议转为可供使用的数据
    public static Dictionary<ProtoField, object> ToProtoData(string proto)
    {
        if (proto == null)
            throw new Exception("proto == null");

        if (proto.Length == 0)
            throw new Exception("proto.Length == 0");

        Dictionary<ProtoField, object> message = new Dictionary<ProtoField, object>();

        string[] parts = proto.Split(",");

        if (parts == null)
            throw new Exception("parts == null");

        switch (parts[0])
        {
            case PROTO_LOGIN:
                {
                    Debug.Assert(parts.Length == 3);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.REQUEST_CODE] = int.Parse(parts[1]);
                    message[ProtoField.REASON] = parts[2];
                }
                break;
            case PROTO_ENTER:
                {
                    Debug.Assert(parts.Length == 3);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.REQUEST_CODE] = int.Parse(parts[1]);
                    message[ProtoField.REASON] = parts[2];
                }
                break;
            case PROTO_BALL_LIST:
                {
                    Debug.Assert((parts.Length - 1) % 5 == 0);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    List<Dictionary<ProtoField, object>> allballsInfo = new List<Dictionary<ProtoField, object>>();
                    for (int i = 1; i < parts.Length; i += 5)
                    {
                        Dictionary<ProtoField, object> ballInfo = new Dictionary<ProtoField, object>();
                        ballInfo[ProtoField.BALL_ID] = int.Parse(parts[i]);
                        ballInfo[ProtoField.BALL_X] = float.Parse(parts[i + 1]);
                        ballInfo[ProtoField.BALL_Y] = float.Parse(parts[i + 2]);
                        ballInfo[ProtoField.BALL_SIZE] = float.Parse(parts[i + 3]);
                        ballInfo[ProtoField.BALL_SCORE] = int.Parse(parts[i + 4]);
                        allballsInfo.Add(ballInfo);
                    }
                    message[ProtoField.BLL_LIST] = allballsInfo;
                }
                break;
            case PROTO_BALL_ADD:
                {
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.BALL_ID] = int.Parse(parts[1]);
                    message[ProtoField.BALL_X] = float.Parse(parts[2]);
                    message[ProtoField.BALL_Y] = float.Parse(parts[3]);
                    message[ProtoField.BALL_SIZE] = float.Parse(parts[4]);
                    message[ProtoField.BALL_SCORE] = int.Parse(parts[5]);
                }
                break;
            case PROTO_FOOD_ADD:
                {
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.FOOD_ID] = int.Parse(parts[1]);
                    message[ProtoField.FOOD_X] = float.Parse(parts[2]);
                    message[ProtoField.FOOD_Y] = float.Parse(parts[3]);
                    message[ProtoField.FOOD_SIZE] = float.Parse(parts[4]);
                    message[ProtoField.FOOD_SCORE] = int.Parse(parts[5]);
                }
                break;
            case PROTO_FOOD_LIST:
                {
                    Debug.Assert((parts.Length - 1) % 5 == 0);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    List<Dictionary<ProtoField, object>> allfoodsInfo = new List<Dictionary<ProtoField, object>>();
                    for (int i = 1; i < parts.Length; i += 5)
                    {
                        Dictionary<ProtoField, object> foodInfo = new Dictionary<ProtoField, object>();
                        foodInfo[ProtoField.FOOD_ID] = int.Parse(parts[i]);
                        foodInfo[ProtoField.FOOD_X] = float.Parse(parts[i + 1]);
                        foodInfo[ProtoField.FOOD_Y] = float.Parse(parts[i + 2]);
                        foodInfo[ProtoField.FOOD_SIZE] = float.Parse(parts[i + 3]);
                        foodInfo[ProtoField.FOOD_SCORE] = int.Parse(parts[i + 4]);
                        allfoodsInfo.Add(foodInfo);
                    }
                    message[ProtoField.FOOD_LIST] = allfoodsInfo;
                }
                break;
            case PROTO_MOVE:
                {
                    Debug.Assert(parts.Length == 7);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.SESSION_ID] = int.Parse(parts[1]);
                    message[ProtoField.BALL_ID] = int.Parse(parts[2]);
                    message[ProtoField.BALL_X] = float.Parse(parts[3]);
                    message[ProtoField.BALL_Y] = float.Parse(parts[4]);
                    message[ProtoField.BALL_SIZE] = float.Parse(parts[5]);
                    message[ProtoField.BALL_SCORE] = int.Parse(parts[6]);
                }
                break;
            case PROTO_EAT:
                {
                    Debug.Assert(parts.Length == 9);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.SESSION_ID] = int.Parse(parts[1]);
                    message[ProtoField.REQUEST_CODE] = int.Parse(parts[2]);

                    message[ProtoField.BALL_ID] = int.Parse(parts[3]);
                    message[ProtoField.BALL_X] = float.Parse(parts[4]);
                    message[ProtoField.BALL_Y] = float.Parse(parts[5]);
                    message[ProtoField.BALL_SIZE] = float.Parse(parts[6]);
                    message[ProtoField.BALL_SCORE] = int.Parse(parts[7]);

                    message[ProtoField.FOOD_ID] = int.Parse(parts[8]);

                }
                break;
            case PROTO_KILL:
                {
                    Debug.Assert(parts.Length == 13);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.SESSION_ID] = int.Parse(parts[1]);
                    message[ProtoField.REQUEST_CODE] = int.Parse(parts[2]);//0发生kill事件 1：未发生

                    message[ProtoField.BALL_ID] = int.Parse(parts[3]);
                    message[ProtoField.BALL_X] = float.Parse(parts[4]);
                    message[ProtoField.BALL_Y] = float.Parse(parts[5]);
                    message[ProtoField.BALL_SIZE] = float.Parse(parts[6]);
                    message[ProtoField.BALL_SCORE] = int.Parse(parts[7]);

                    //第二个球，被吃的一方
                    message[ProtoField.FOOD_ID] = int.Parse(parts[8]);
                    message[ProtoField.FOOD_X] = float.Parse(parts[9]);
                    message[ProtoField.FOOD_Y] = float.Parse(parts[10]);
                    message[ProtoField.FOOD_SIZE] = float.Parse(parts[11]);
                    message[ProtoField.FOOD_SCORE] = int.Parse(parts[12]);

                }
                break;
            case PROTO_LEAVE:
                {
                    Debug.Assert(parts.Length == 2);
                    message[ProtoField.PROTO_TYPE] = parts[0];
                    message[ProtoField.BALL_ID] = int.Parse(parts[1]);
                }
                break;
            case PROTO_KICK:
                {
                    message[ProtoField.PROTO_TYPE] = parts[0];
                }
                break;
            default:
                throw new Exception("got unexcepted proto type!");
        }
        return message;
    }

    //返回当前时间的时间戳
    public static long Now()
    {
        DateTime now = DateTime.UtcNow;

        // Unix纪元开始时间
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 计算从1970年1月1日至今的时间差（秒数）
        long timestamp = (long)(now - epoch).TotalMilliseconds;
        return timestamp;
    }

    public static string LoginProto(string name, string pass)
    {
        return $"{PROTO_LOGIN},{name},{pass}{separator}";
    }

    public static string EnterProto()
    {
        return $"{PROTO_ENTER}{separator}";
    }

    public static string MoveProto(int playerBallId, int sessionId, float inputX, float inputY, float deltaTime)
    {
        // return $"{PROTO_MOVE},{playerBallId},{sessionId},{inputX},{inputY},{deltaTime}{separator}";
        //agent服务器自动添加playerBallId
        return $"{PROTO_MOVE},{sessionId},{inputX},{inputY},{deltaTime}{separator}";

    }

    public static string EatProto(int playerBallId, int sessionId, int foodId)
    {
        return $"{PROTO_EAT},{sessionId},{foodId}{separator}";
    }

    public static string KillProto(int playerBallId, int sessionId, int secondBallId)
    {
        return $"{PROTO_KILL},{sessionId},{secondBallId}{separator}";
    }
}