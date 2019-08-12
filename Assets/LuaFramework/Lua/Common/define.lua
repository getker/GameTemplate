-- 所有面板的名称
CtrlNames = {
	-- Prompt = "PromptCtrl",
	-- Message = "MessageCtrl"
	Template = "TemplateCtrl",	-- 模板
	Login = "LoginCtrl",		-- Login
}

-- 所有控制器的名称
PanelNames = {
	-- "PromptPanel",	
	-- "MessagePanel",
	"TemplatePanel",
	"LoginPanel",
}

--协议类型--
ProtocalType = {
	BINARY = 0,
	PB_LUA = 1,
	PBC = 2,
	SPROTO = 3,
}
--当前使用的协议类型--
TestProtoType = ProtocalType.BINARY;

Util = LuaFramework.Util;
AppConst = LuaFramework.AppConst;
LuaHelper = LuaFramework.LuaHelper;
ByteBuffer = LuaFramework.ByteBuffer;

resMgr = LuaHelper.GetResManager();
panelMgr = LuaHelper.GetPanelManager();
soundMgr = LuaHelper.GetSoundManager();
networkMgr = LuaHelper.GetNetManager();

WWW = UnityEngine.WWW;
GameObject = UnityEngine.GameObject;