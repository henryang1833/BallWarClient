local socket = require("socket")
playerid = 101
playerpass = 123
serverIP = "192.168.3.113"
serverPort = 8001
endStr = "\r\n"
balls = {}
foods = {}

-- 创建一个TCP socket
client = socket.tcp()
readbuff = ""
csObj = nil
resp = {}
foodTable = {}
ballTable = {}
game_status = 0
OnFoodEat = nil

-- 连接到服务器
local success, err = client:connect(serverIP, serverPort)

-- 设置非阻塞模式
client:settimeout(0) -- 0秒超时，即非阻塞模式

local str_unpack = function(msgstr)
    local msg = {}
    while true do
        local arg, rest = string.match(msgstr, "(.-),(.*)")
        if arg then
            msgstr = rest
            table.insert(msg, arg)
        else
            table.insert(msg, msgstr)
            break
        end
    end
    return msg[1], msg
end

local str_pack = function(cmd, msg)
    return table.concat(msg, ",") .. "\r\n"
end

function traceback(err)
    print(tostring(err))
    print(debug.traceback())
end

local process_msg = function(msgstr)
    local cmd, msg = str_unpack(msgstr)
    -- print("recv [" .. cmd .. "] {" .. table.concat(msg, ",") .. "}")
    local fun = resp[cmd]
    if not fun then
        print("not fun")
        return
    end

    local ret = table.pack(xpcall(fun, traceback, msg))
    local isok = ret[1]

    if not isok then
        print("is not ok")
        print(ret[2])
        return
    end
end

-- local process_buff = function(readbuff)
--     readbuff = readbuff.."\r\n"
--     -- print("process_buff:"..readbuff)
--     while true do
--         local msgstr, rest = string.match(readbuff, "(.-)\r\n(.*)")
--         if msgstr then
--             print("msgstr:"..msgstr)
--             readbuff = rest
--             process_msg(msgstr)
--         else
--             print("len:"..string.len(readbuff))
--             for i = 1, string.len(readbuff) do
--                 local ascii_val = string.byte(readbuff, i)
--                 print("Character:", readbuff:sub(i, i), "ASCII:", ascii_val)
--             end
--             return readbuff
--         end
--     end
-- end

local disconnect = function()
    print("断线")
    client:close()
end

resp.kick = function()
    disconnect()
end



function recv_loop()
    local recvstr, err = client:receive()
    if recvstr then
        -- print(recvstr)
        -- readbuff = readbuff .. recvstr
        -- print(string.len(readbuff))
        -- readbuff = process_buff(readbuff)
        process_msg(recvstr)
    elseif err == "timeout" then
        print("没有数据接收")
    else
        print("接收失败: ", err)
        disconnect()
    end
end

resp.login = function(msg)
    print("resp.login 被调用")
    print("登录成功"..msg[2])
    game_status = 1
    client:send("enter"..endStr)
end

resp.enter = function(msg)
    print("进入场景成功"..msg[2])
    game_status = 2
end

function move(x,y)
    client:send("shift,"..x..","..y.."\r\n")
end

-- todo 
resp.move = function(msg)
    if not msg then
        print("not msg")
        return
    end
    local idx = 0
    for i = 1, #ballTable, 4 do
        print("resp.move ballTable:"..ballTable[i])
        if tonumber(ballTable[i]) == playerid then
            idx = i
            break
        end
    end
    if idx==0 then
        print("idx==0")
        return
    end
    -- print("原坐标：")
    -- print(table.concat({ballTable[idx],ballTable[idx+1],ballTable[idx+2]},","))
    ballTable[idx+1] = msg[3]
    ballTable[idx+2] = msg[4]
    -- print("现坐标")
    -- print(table.concat({ballTable[idx],ballTable[idx+1],ballTable[idx+2]},","))
end

resp.balllist = function(msg)
    if not msg then
        return
    end
    ballTable = {}
    for i=2,#msg do
        table.insert(ballTable,msg[i])
        -- print("resp.balllist:"..msg[i])
    end
end


resp.foodlist = function(msg)
    if not msg then
        return
    end
    foodTable = {}
    for i=2,#msg do
        table.insert(foodTable,msg[i])
        print("resp.foodlist:"..msg[i])
    end
end

resp.addfood = function(msg)
    if not msg then
        return
    end
    print("resp.addfood:"..table.concat(msg,","))
    for i = 2, #msg, 1 do
        table.insert(foodTable,msg[i])    
    end
end


-- todo debug
resp.eat = function(msg)
    if not msg then
        print("resp.eat:not msg")
        return
    end
    print("resp.eat:"..table.concat(msg,","))
    local playerid = msg[2]
    local foodid = msg[3]
    local ballsize = msg[4]
    local idx = 0
    -- 改变ball的size
    for i = 1, #ballTable, 4 do
        if playerid==ballTable[i] then
            idx = i
            break
        end
    end
    ballTable[idx + 3] = ballsize 
    
    -- 删除被吃的food
    idx = 0
    for i = 1,#foodTable,4 do
        if foodTable[i] == foodid then
            idx = i
            break
        end
    end
    if not (idx>=1 and idx<=#foodTable) then
        prin("resp.eat not (idx>=1 and idx<=#foodTable)")
    end
    for i = 1, 4, 1 do
        table.remove(foodTable,idx)
    end
    if OnFoodEat then
        print("resp.eat Calling OnFoodEat, foodId"..foodid)
        OnFoodEat(tonumber(foodid))
    else
        print("resp.eat OnFoodEat is nil")
    end
end

return success, err
