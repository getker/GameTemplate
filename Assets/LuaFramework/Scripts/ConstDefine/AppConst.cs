using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework {
    public class AppConst {
        // 打安卓包：{DebugMode = false, ExampleMode = false, UpdateMode = true, LuaByteMode = false, LuaBundleMode = true}
        // 打PC包： {DebugMode = true, ExampleMode = false, UpdateMode = true, LuaByteMode = false, LuaBundleMode = true}
        // 本地开发： {DebugMode = false, ExampleMode = false, UpdateMode = false, LuaByteMode = false, LuaBundleMode = false}
        public const bool DebugMode = false;                       //调试模式-用于内部测试
        /// <summary>
        /// 如果想删掉框架自带的例子，那这个例子模式必须要
        /// 关闭，否则会出现一些错误。
        /// </summary>
        public const bool ExampleMode = false;                       //例子模式 

        /// <summary>
        /// 如果开启更新模式，前提必须启动框架自带服务器端。
        /// 否则就需要自己将StreamingAssets里面的所有内容
        /// 复制到自己的Webserver上面，并修改下面的WebUrl。
        /// </summary>
        public const bool UpdateMode = false;                       //更新模式-默认关闭 
        public const bool LuaByteMode = false;                       //Lua字节码模式-默认关闭 
        public const bool LuaBundleMode = false;                    //Lua代码AssetBundle模式
        //- 默认true,设置为false，直接读取lua脚本代码。改C#代码记得Clear wrap files后重新生成(DebugMode可能也要设置为true)

        public const int TimerInterval = 1;
        public const int GameFrameRate = 30;                        //游戏帧频

        public const string AppName = "LuaFramework";               //应用程序名称
        public const string LuaTempDir = "Lua/";                    //临时目录
        public const string AppPrefix = AppName + "_";              //应用程序前缀
        public const string ExtName = ".unity3d";                   //素材扩展名
        public const string AssetDir = "StreamingAssets";           //素材目录 
        // public const string WebUrl = "http://localhost:6688/";      //测试更新地址
        public const string WebUrl = "http://123.207.29.127:62228/StreamingAssets/";      //测试更新地址 

        public static string UserId = string.Empty;                 //用户ID
        public static int SocketPort = 0;                           //Socket服务器端口
        public static string SocketAddress = string.Empty;          //Socket服务器地址

        public static string FrameworkRoot {
            get {
                return Application.dataPath + "/" + AppName;
            }
        }
    }
}