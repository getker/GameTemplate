local transform
local gameObject

TemplatePanel = {}

-- this指向table对象
local this = TemplatePanel

-- Panel的Awake方法，会在Ctrl里加载
function TemplatePanel.Awake(obj)
    gameObject = obj
    transform = obj.transform

    this.InitPanel()
end

-- 初始化面板
function TemplatePanel.InitPanel()
    -- btn按钮初始化
    -- this.btnClose = transform:FindChild("Button").gameObject;
    -- this.btnOpen = transform:Find("Open").gameObject;
	-- this.gridParent = transform:Find('ScrollView/Grid');
    
    this.btnObj = transform:Find('Panel/LoginBtn').gameObject
    
    -- Image初始化
    this.image = transform:Find('Panel/BG').gameObject:GetComponent('Image')
end

--[[
制作一个面板：
    1.Game/Prefabs下建对应目录，并放上制作好的预设
    2.packager.cs的HandleGameBundle()添加打包信息
    3.新建xxxCtrl和xxxPanel
    4.define.lua中添加CtrlNames和PanelNames
    5.CtrlManager.lua中require和Init()中初始化
    6.可选 - 在Game.OnInitOK()中执行
    local ctrl = CtrlManager.GetCtrl(CtrlNames.Login);
    if ctrl ~= nil then
        ctrl:Awake();
    end
]]--