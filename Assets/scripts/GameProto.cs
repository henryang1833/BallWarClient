using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine.Assertions;

public enum ProtoField
{
    SESSION_ID, TIME_STAMP, PROTO_TYPE, REQUEST_CODE, REASON,
    BALL_ID, BALL_X, BALL_Y, BALL_SIZE, BALL_SCORE, BLL_LIST,
    FOOD_ID, FOOD_X, FOOD_Y, FOOD_SIZE, FOOD_SCORE, FOOD_LIST
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
    public const string PROTO_BALL_LIST = "balllist";
    public const string PROTO_FOOD_LIST = "foodlist";
    public static string separator = "\r\n";
    public static string LoginProto(string name, string pass)
    {
        return $"{Now()},{PROTO_LOGIN},{name},{pass}{separator}";
    }

    public static string EnterProto()
    {
        return $"{Now()},{PROTO_ENTER}{separator}";
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

        if (parts.Length < 2)
            throw new Exception("parts.Length<2");

        switch (parts[1])
        {
            case PROTO_LOGIN:
                {
                    message[ProtoField.TIME_STAMP] = long.Parse(parts[0]);
                    message[ProtoField.PROTO_TYPE] = parts[1];
                    message[ProtoField.REQUEST_CODE] = int.Parse(parts[3]);
                    message[ProtoField.REASON] = parts[4];
                }
                break;
            case PROTO_ENTER:
                {
                    message[ProtoField.TIME_STAMP] = long.Parse(parts[0]);
                    message[ProtoField.PROTO_TYPE] = parts[1];
                    message[ProtoField.REQUEST_CODE] = int.Parse(parts[3]);
                    message[ProtoField.REASON] = parts[4];
                }
                break;
            case PROTO_BALL_LIST:
                {
                    message[ProtoField.TIME_STAMP] = long.Parse(parts[0]);
                    message[ProtoField.PROTO_TYPE] = parts[1];
                    List<Dictionary<ProtoField, object>> allballsInfo = new List<Dictionary<ProtoField, object>>();
                    Debug.Assert((parts.Length - 2) % 5 == 0);
                    for (int i = 2; i < parts.Length; i += 5)
                    {
                        Dictionary<ProtoField, object> ballInfo = new Dictionary<ProtoField, object>();
                        ballInfo[ProtoField.BALL_ID] = parts[i];
                        ballInfo[ProtoField.BALL_X] = parts[i+1];
                        ballInfo[ProtoField.BALL_Y] = parts[i+2];
                        ballInfo[ProtoField.BALL_SIZE] = parts[i+3];
                        ballInfo[ProtoField.BALL_SCORE] = parts[i+4];
                        allballsInfo.Add(ballInfo);
                    }
                    message[ProtoField.BLL_LIST] = allballsInfo;

                }
                break;
            case PROTO_FOOD_LIST: break;
            case PROTO_MOVE: break;
            case PROTO_EAT: break;
            case PROTO_LEAVE: break;
            case PROTO_KICK: break;
        }

        throw new NotImplementedException();

        return message;
    }

    //返回当前时间的时间戳
    public static long Now()
    {
        DateTime now = DateTime.UtcNow;

        // Unix纪元开始时间
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 计算从1970年1月1日至今的时间差（毫秒数）
        long timestamp = (long)(now - epoch).TotalSeconds;
        return timestamp;
    }
}