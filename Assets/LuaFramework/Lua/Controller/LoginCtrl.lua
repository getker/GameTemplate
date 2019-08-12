require("Common/define")

LoginCtrl = {}
local this = LoginCtrl
local transform
local gameObject
local lua

-- 构造函数，在CtrlManager.Init()中被调用
function LoginCtrl.New()
    -- 返回一个引用，被CtrlManager.Init()中的某个对象指向
    return this
end

function LoginCtrl.Awake()
    panelMgr:CreatePanel('Login', this.OnCreate)
    this.imgEnable = true
end

function LoginCtrl.OnCreate(obj)
    gameObject = obj
    transform = obj.transform
    -- 添加点击事件
    lua = transform:GetComponent('LuaBehaviour')
    lua:AddClick(LoginPanel.showBtnObj, this.OnClick)
end

function LoginCtrl.OnClick(go)
    this.imgEnable = not this.imgEnable
   
    print('LoginCtrl>>>点击了按钮',this.imgEnable)
    -- LoginPanel.showImage.enabled = this.imgEnable

    UIManager.OpenPanel(CtrlNames.Template, 'Login')
end