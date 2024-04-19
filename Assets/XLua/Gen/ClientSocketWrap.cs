#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class ClientSocketWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(ClientSocket);
			Utils.BeginObjectRegister(type, L, translator, 0, 2, 6, 6);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CreatePrefab", _m_CreatePrefab);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnFoodEat", _m_OnFoodEat);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "foodPrefab", _g_get_foodPrefab);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ballPrefab", _g_get_ballPrefab);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "playerBall", _g_get_playerBall);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "game_status", _g_get_game_status);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "foodInfo", _g_get_foodInfo);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "foodInfoLen", _g_get_foodInfoLen);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "foodPrefab", _s_set_foodPrefab);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ballPrefab", _s_set_ballPrefab);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "playerBall", _s_set_playerBall);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "game_status", _s_set_game_status);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "foodInfo", _s_set_foodInfo);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "foodInfoLen", _s_set_foodInfoLen);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new ClientSocket();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to ClientSocket constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreatePrefab(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.GameObject _prefab = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
                    int _id = LuaAPI.xlua_tointeger(L, 3);
                    float _x = (float)LuaAPI.lua_tonumber(L, 4);
                    float _y = (float)LuaAPI.lua_tonumber(L, 5);
                    float _scale = (float)LuaAPI.lua_tonumber(L, 6);
                    
                        var gen_ret = gen_to_be_invoked.CreatePrefab( _prefab, _id, _x, _y, _scale );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnFoodEat(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _foodId = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.OnFoodEat( _foodId );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_foodPrefab(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.foodPrefab);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ballPrefab(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ballPrefab);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_playerBall(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.playerBall);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_game_status(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.game_status);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_foodInfo(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.foodInfo);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_foodInfoLen(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.foodInfoLen);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_foodPrefab(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.foodPrefab = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ballPrefab(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ballPrefab = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_playerBall(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.playerBall = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_game_status(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                ClientSocket.GAME_STATUS gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.game_status = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_foodInfo(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.foodInfo = (int[])translator.GetObject(L, 2, typeof(int[]));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_foodInfoLen(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ClientSocket gen_to_be_invoked = (ClientSocket)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.foodInfoLen = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
