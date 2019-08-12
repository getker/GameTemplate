local transform
local gameObject

LoginPanel = {}

-- this指向table对象
local this = LoginPanel

-- Panel的Awake方法，会在Ctrl里加载
function LoginPanel.Awake(obj)
    gameObject = obj
    transform = obj.transform

    this.InitPanel()
end

-- 初始化面板
function LoginPanel.InitPanel()
    -- btn按钮初始化
    this.showBtnObj = transform:Find('Panel/ShowBtn').gameObject
    
    -- Image初始化
    this.showImage = transform:Find('Panel/ShowImg').gameObject:GetComponent('Image')
end

--单击事件--
function LoginPanel.OnDestroy()
	logWarn("LoginPanel OnDestroy---->>>");
end