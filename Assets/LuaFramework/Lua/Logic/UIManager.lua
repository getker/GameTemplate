-- require "Common/define"
-- require "Controller/TemplateCtrl"
-- require "Controller/LoginCtrl"

UIManager = {};
local this = UIManager;

function UIManager.Init()
    log("UIManager.Init----->>>");	-- 初始化
    
	return this;
end

--打开面板--
-- ctrlName:define.lua的CtrlNames
-- ctrl:返回控制器
function UIManager.OpenPanel(ctrlName, closeName)
	local ctrl = CtrlManager.GetCtrl(ctrlName);
    if ctrl ~= nil then
        ctrl:Awake();
    end
    if closeName then
        this.ClosePanel(closeName)
    end
    return ctrl
end

--关闭面板--
-- name：名称例如Login
function UIManager.ClosePanel(name)
	panelMgr:ClosePanel(name)
end

function UIManager.Close()
	logWarn('UIManager.Close---->>>');
end