require("Common/define")

TemplateCtrl = {}
local this = TemplateCtrl
local transform
local gameObject
local lua

-- 构造函数，在CtrlManager.Init()中被调用
function TemplateCtrl.New()
    -- 返回一个引用，被CtrlManager.Init()中的某个对象指向
    return this
end

function TemplateCtrl.Awake()
    panelMgr:CreatePanel('Template', this.OnCreate)
    -- 创建面板，当它创建的时候，会自动为面板添加LuaBehaviour组件
    -- LuaBehaviour又会根据当前面板的名称，调用xxxPanel.lua这个脚本的Awake和Start方法
    -- 具体可以查看LuaBehaviour.cs这个脚本
    -- 为了它能够调用(Template)xxxPanel方法，所以这里的名称一定要与(Template)xxx一致
    -- 同时这个参数也是用于加载ab文件的，具体可以参考PanelManager.cs脚本的CreatePanel()
    -- 它先转为小写，再加上AppConst.ExtName里的设置，就等于ab的名称
    -- 所以，这里创建后的面板名称为：TemplatePanel
    -- 所以，这里创建后的ab名称为template.unity3d
    this.imgEnable = true
end

function TemplateCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    -- 添加点击事件
    lua = transform:GetComponent('LuaBehaviour')
    lua:AddClick(TemplatePanel.btnObj, this.OnClick)
end

function TemplateCtrl.OnClick(go)
    this.imgEnable = not this.imgEnable
   
    print('点击了按钮',this.imgEnable)
    -- TemplatePanel.image.enabled = this.imgEnable
    UIManager.OpenPanel(CtrlNames.Login, 'Template')
end