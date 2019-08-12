require "Common/define"
require "Controller/TemplateCtrl"
require "Controller/LoginCtrl"

CtrlManager = {};
local this = CtrlManager;
local ctrlList = {};	--控制器列表--

function CtrlManager.Init()
	log("CtrlManager.Init----->>>");	-- 初始化所有控制器
	ctrlList[CtrlNames.Template] = TemplateCtrl.New();
	ctrlList[CtrlNames.Login] = LoginCtrl.New();
	return this;
end

--添加控制器--
function CtrlManager.AddCtrl(ctrlName, ctrlObj)
	ctrlList[ctrlName] = ctrlObj;
end

--获取控制器--
function CtrlManager.GetCtrl(ctrlName)
	return ctrlList[ctrlName];
end

--移除控制器--
function CtrlManager.RemoveCtrl(ctrlName)
	ctrlList[ctrlName] = nil;
end

--关闭控制器--
function CtrlManager.Close()
	logWarn('CtrlManager.Close---->>>');
end