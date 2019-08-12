using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

public class ServiceResSelector : EditorWindow
{
    [MenuItem("Tools/UI/ServiceResSelector")]
    static void OpenWindow()
    {
        var wnd = GetWindow<ServiceResSelector>();
    }
    #region 常量
    private const string SERVICERESSELECTOR_LATELY = "SERVICERESSELECTOR_LATELY";


    private const string ATLAS_ROOT = "/LuaFramework/Game/GameAssets/Texture/UIAtlas/";
    private const string ATLAS_DYNAMIC_ROOT = "/LuaFramework/Game/GameAssets/Texture/UIAtlas/Dynamic/";
    private const string PREFABS_ROOT = "/LuaFramework/Game/GameAssets/Prefabs/UI/";
    private const string Texture_ROOT = "/LuaFramework/Game/GameAssets/Texture/UITexture/";

    private const int RES_ITEM_LABEL_HEIGHT = 22;
    private const int MIN_RES_ITEM_SIZE = 20;
    private const int MAX_RES_ITEM_SIZE = 80;

    private const int RES_LIST_HEIGHT_OFFY = 25;
    private const int RES_LIST_START_Y = 45;

    private const int serviceListTabWidth = 150;
    #endregion

    #region 变量
    public string[] subDicPopOptions = new string[] { "当前" };
    private int subDicIndex = 0;
    private string newServiceName = "";


    private int resItemSize = 100;

    private string[] resRubTypeList;
    private string[] resSubTypeList;
    private string[] resRootList;

    private List<string> serviceList = new List<string>();
    private List<string> latelyServiceList = new List<string>();


    private Vector2 serviceListScrollPos = Vector2.zero;
    private int listType = 0;
    private int resSubType = 0;
    private Vector2 resListScrollPos = Vector2.zero;
    private string selectService = "";
    private Object nowSelectObj = null;
    private List<Object> nowSelectObjList = new List<Object>();

    private string nowResRootPath = "";
    private List<Object> nowResList = new List<Object>();

    private GUIStyle resNameGUIStyle = new GUIStyle();

    private GUIStyle selectServiceNameGUIStyle = new GUIStyle();

    private bool isInEditorName = false;
    private Timer editorNameTimer = null;

    private bool isDirty = false;


    private string nowFocusedControlName = "";

    private Color SelectMaskColor = new Color(62 / 255f, 95 / 255f, 200 / 255f, 0.5f);

    private Rect resListRect = new Rect();
    #endregion
    private void Awake()
    {
        resRubTypeList = new string[] {
        Application.dataPath + ATLAS_ROOT,
        Application.dataPath + ATLAS_DYNAMIC_ROOT,
        Application.dataPath + Texture_ROOT,
        Application.dataPath + PREFABS_ROOT,
        };

        resRootList = new string[] {
        ATLAS_ROOT,
        ATLAS_DYNAMIC_ROOT,
        Texture_ROOT,
        PREFABS_ROOT,
        };

        resSubTypeList = new string[]{
        "Atlas","AtlasDynamic","UITexture","Prefabs"
        };
        resNameGUIStyle.alignment = TextAnchor.MiddleLeft;
        resNameGUIStyle.normal.textColor = Color.white;

        selectServiceNameGUIStyle.alignment = TextAnchor.LowerLeft;
        selectServiceNameGUIStyle.normal.textColor = Color.white;

        FindServiceList();
    }
    void OnGUI()
    {
        nowFocusedControlName = GUI.GetNameOfFocusedControl();
        resNameGUIStyle.clipping = TextClipping.Clip;
        //resNameGUIStyle.wordWrap = false; 

        resListRect.x = 0;//serviceListTabWidth;
        resListRect.y = 0;//  RES_LIST_START_Y;
        resListRect.width = Screen.width - serviceListTabWidth;
        resListRect.height = position.height - RES_LIST_HEIGHT_OFFY - RES_LIST_START_Y;

        GUILayout.BeginHorizontal();
        DrawServiceList();
        DrawSubTypes();
        DrawResList();
        //CheckEvent();
        GUILayout.EndHorizontal();
        if (isDirty)
        {
            isDirty = false;
            if (nowSelectObj != null)
            {
                string controlName = GetControlName(nowSelectObj);
                Debug.Log(controlName);
                //GUI.FocusControl(controlName);
                EditorGUI.FocusTextInControl(controlName);
            }
        }
    }

    #region 界面绘制
    void Update()
    {
        if (isDirty)
        {
            //OnGUI();
            Repaint();
            Debug.Log("Repaint");
        }
        //Debug.Log(Input.GetKeyDown(KeyCode.LeftControl));
        //Debug.Log(Input.GetKey(KeyCode.A));
    }

    void OnInspectorUpdate()
    {
        //开启窗口的重绘，不然窗口信息不会刷新
        if (isDirty)
        {
            Debug.Log("Repaint");
            Repaint();
        }
    }


    void DrawServiceList()
    {

        GUILayout.BeginVertical();
        //GUILayout.BeginArea(new Rect(position.width - serviceListTabWidth, 100, serviceListTabWidth, position.height - 110));
        GUILayout.BeginHorizontal();
        GUILayout.Label("列表:", GUILayout.Width(50));
        if (GUILayout.Button("刷新", GUILayout.Width(50)))
        {
            FindServiceList();
        }
        GUILayout.EndHorizontal();
        listType = GUILayout.SelectionGrid(listType, new[] { "最近", "全部" }, 3, GUILayout.Width(150));

        var useList = latelyServiceList;
        //GUILayout.Space(10);
        bool isAllList = listType == 1;
        if (isAllList)
        {
            useList = serviceList;
        }
        serviceListScrollPos = GUILayout.BeginScrollView(serviceListScrollPos, GUILayout.Width(serviceListTabWidth));
        var skin = new GUIStyle(GUI.skin.button);
        //skin.alignment = TextAnchor.MiddleRight;
        for (int i = 0; i < useList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(useList[i], skin, GUILayout.Width(serviceListTabWidth - 25)))
            {
                selectService = useList[i];
                UpdateLatelyServiceList(selectService, isAllList);
                RestSubDic();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        //GUILayout.EndArea();
        GUILayout.EndVertical();

        Handles.BeginGUI();
        Handles.color = Color.black;
        Handles.DrawLine(new Vector3(serviceListTabWidth, 0), new Vector3(serviceListTabWidth, position.height));
        Handles.EndGUI();
    }


    void DrawSubTypes()
    {
        int startX = serviceListTabWidth;
        GUILayout.BeginArea(new Rect(startX, 0, position.width - startX, position.height));
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("当前service:", GUILayout.Width(80));
        selectServiceNameGUIStyle.alignment = TextAnchor.LowerLeft;
        selectServiceNameGUIStyle.normal.textColor = Color.green;
        GUILayout.Label(selectService, selectServiceNameGUIStyle, GUILayout.Width(100));
        GUILayout.Label("子目录:", GUILayout.Width(50));
        subDicIndex = EditorGUILayout.Popup(subDicIndex, subDicPopOptions, GUILayout.Width(100));

        newServiceName = EditorGUILayout.TextField(newServiceName, GUILayout.Width(200));
        if (GUILayout.Button("创建", GUILayout.Width(50)))
        {
            CreateOrJumpToService(newServiceName);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("资源类型:", GUILayout.Width(50));
        resSubType = GUILayout.SelectionGrid(resSubType, resSubTypeList, resSubTypeList.Length, GUILayout.Width(100 * resSubTypeList.Length));

        //GUILayout.Box(" ", GUILayout.Height(50), GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        //GUILayout.Box(" ", GUILayout.Height(position.height - RES_LIST_HEIGHT_OFFY), GUILayout.ExpandWidth(true));

        GUILayout.EndVertical();
        GUILayout.EndArea();

        Handles.BeginGUI();
        Handles.color = Color.black;
        Handles.DrawLine(new Vector3(serviceListTabWidth, RES_LIST_START_Y), new Vector3(position.width, RES_LIST_START_Y));
        Handles.EndGUI();
    }

    void DrawResList()
    {
        if (string.IsNullOrEmpty(selectService))
        {
            return;
        }
        int startX = serviceListTabWidth + 2;
        int scrollWidth = Screen.width - startX - 2;
        nowResRootPath = resRubTypeList[resSubType] + nowSubDic;

        Handles.BeginGUI();
        Handles.color = Color.black;
        float tempY = position.height - RES_LIST_HEIGHT_OFFY;
        Handles.DrawLine(new Vector3(startX, tempY), new Vector3(position.width, tempY));
        Handles.EndGUI();

        Rect rectArea = new Rect(serviceListTabWidth, RES_LIST_START_Y, position.width - serviceListTabWidth, position.height - RES_LIST_HEIGHT_OFFY - RES_LIST_START_Y);
        GUILayout.BeginArea(rectArea);
        if (Directory.Exists(nowResRootPath) == false)
        {
            GUILayout.Label("不存在此目录:" + nowResRootPath);
            if (GUILayout.Button("创建(带ABName)", GUILayout.Width(serviceListTabWidth - 22)))
            {
                if (UnityEditor.EditorUtility.DisplayDialog("创建目录", "是否创建目录并设置ABName：\n" + nowResRootPath + "", "确认", "取消"))
                {
                    ServiceResSelectorUtil.CreateDirectory(nowResRootPath);
                }
            }
            if (GUILayout.Button("创建(不带ABName)", GUILayout.Width(serviceListTabWidth - 22)))
            {
                if (UnityEditor.EditorUtility.DisplayDialog("创建目录", "是否创建目录：\n" + nowResRootPath + "", "确认", "取消"))
                {
                    ServiceResSelectorUtil.CreateDirectory(nowResRootPath, false);
                }
            }
        }
        else
        {

            nowResList = GetResList(nowResRootPath);
            scrollWidth = Mathf.Max(100, scrollWidth);

            resListScrollPos = GUILayout.BeginScrollView(resListScrollPos, GUILayout.Width(scrollWidth), GUILayout.Height(position.height - RES_LIST_HEIGHT_OFFY - RES_LIST_START_Y));
            int col_show_count = (scrollWidth) / (resItemSize + 10);
            int offY = RES_ITEM_LABEL_HEIGHT + 2;
            bool isMinMode = resItemSize == MIN_RES_ITEM_SIZE;
            if (isMinMode)
            {
                col_show_count = 1;
                offY = 0;
                resNameGUIStyle.alignment = TextAnchor.MiddleLeft;
            }
            else
            {
                resNameGUIStyle.alignment = TextAnchor.MiddleCenter;
            }
            col_show_count = Mathf.Max(1, col_show_count); //the following for-loop will run forever if we limit this
            int total_count = nowResList.Count;
            int row_index = 0;
            Dictionary<int, Rect> rectList = new Dictionary<int, Rect>();
            for (int res_index = 0; res_index < total_count;)
            {
                GUILayout.BeginHorizontal();
                for (int col_index = 0; col_index < col_show_count && res_index < total_count; ++col_index)
                {
                    Object res = nowResList[res_index];
                    Rect draw_rect = DrawResItem(row_index, col_index, res, offY, isMinMode, scrollWidth);
                    rectList[res_index] = draw_rect;
                    ++res_index;
                }
                GUILayout.EndHorizontal();
                row_index++;
            }
            GUILayout.Space(row_index * (resItemSize + offY));
            if (total_count == 0)
            {
                GUILayout.Label("当前目录为空，请拖资源进来");
            }
            GUILayout.EndScrollView();
            CheckEvent(rectList, nowResList, rectArea);
        }
        GUILayout.EndArea();
        Rect sliderRect = new Rect(position.width - 100, position.height - 20, 150, 20);
        resItemSize = (int)EditorGUI.Slider(sliderRect, resItemSize, MIN_RES_ITEM_SIZE, MAX_RES_ITEM_SIZE);
        //resItemSize = (int)GUILayout.HorizontalSlider(resItemSize, MIN_RES_ITEM_SIZE, MAX_RES_ITEM_SIZE, GUILayout.Width(100));
        if (nowSelectObj != null)
        {
            Rect selNameRect = new Rect(startX, position.height - 20, position.width - 250, 20);
            string path = AssetDatabase.GetAssetPath(nowSelectObj);
            EditorGUI.LabelField(selNameRect, "", Path.GetFileName(path));
        }
    }

    Rect DrawResItem(int row_index, int col_index, Object res, int offY, bool isMinMode, int scrollWidth)
    {
        bool isNowSelectObj = isSelectObj(res);
        string resName = res.name;
        string controlName = GetControlName(res);
        GUILayout.BeginVertical();
        Rect totalRect = new Rect(0, 0, 1, 1);
        Rect draw_rect = new Rect(col_index * (resItemSize + 10), row_index * (resItemSize + offY), resItemSize, resItemSize);
        totalRect.x = draw_rect.x;
        totalRect.y = draw_rect.y;
        totalRect.width = resItemSize;
        totalRect.height = resItemSize + RES_ITEM_LABEL_HEIGHT;


        var nameRect = new Rect(col_index * (resItemSize + 10), row_index * (resItemSize + offY) + resItemSize + 2, resItemSize, RES_ITEM_LABEL_HEIGHT);
        if (isMinMode)
        {
            nameRect.x = resItemSize + 5;
            nameRect.y = draw_rect.y;
            nameRect.width = scrollWidth;
            totalRect.width = scrollWidth;
            totalRect.height = resItemSize;
        }

        if (isNowSelectObj && isMinMode && nowFocusedControlName != controlName)
        {
            EditorGUI.DrawRect(totalRect, SelectMaskColor);
        }
        if (res is Sprite)
        {
            DrawSprit(draw_rect, res as Sprite);
        }
        else if (res is Texture2D)
        {
            DrawTexture(draw_rect, res as Texture2D);
        }
        else
        {
            GUI.Box(draw_rect, EditorGUIUtility.IconContent("PrefabNormal Icon"));
            //EditorGUI.DrawRect(draw_rect, Color.gray);

        }
        if (isNowSelectObj && isMinMode == false)
        {
            DrawRectOutline(draw_rect, Color.green);
        }

        EditorGUI.BeginChangeCheck();


        GUI.backgroundColor = new Color(.6f, 1.0f, 1.0f, 0f);
        if (isNowSelectObj == false || isInEditorName == false)
        {
            EditorGUI.LabelField(nameRect, "", ServiceResSelectorUtil.GetFittableText(resName, nameRect, resNameGUIStyle), resNameGUIStyle);
            /*            Rect btnRect = new Rect(col_index * (resItemSize + 10), row_index * (resItemSize + offY), position.width, resItemSize);
                        if (GUI.Button(btnRect, ""))
                        {
                            OnResItemClick(res, true);
                        }*/
        }
        else
        {
            GUI.SetNextControlName(controlName);
            resName = EditorGUI.DelayedTextField(nameRect, "", resName, resNameGUIStyle);

            if (EditorGUI.EndChangeCheck() && string.IsNullOrEmpty(resName) == false)
            {
                isInEditorName = false;
                Debug.Log("change：" + resName);
                string path = AssetDatabase.GetAssetPath(res);
                AssetDatabase.RenameAsset(path, resName);
                RefreshAssets();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
        return totalRect;

    }

    void DrawSprit(Rect draw_rect, Sprite sprite)
    {
        Texture2D handle_texture = sprite.texture;
        float scale = sprite.rect.width / sprite.rect.height;
        float sprite_width = sprite.rect.width;
        float sprite_height = sprite.rect.height;

        Rect uv = new Rect(sprite.rect.x / handle_texture.width, sprite.rect.y / handle_texture.height,
                   sprite.rect.width / handle_texture.width, sprite.rect.height / handle_texture.height);

        if (sprite_width < sprite_height)
        {
            draw_rect.height = resItemSize;
            draw_rect.width = resItemSize * scale;
            draw_rect.x += (resItemSize - draw_rect.width) / 2;
        }
        else
        {
            draw_rect.width = resItemSize;
            draw_rect.height = resItemSize / scale;
            draw_rect.y += (resItemSize - draw_rect.height) / 2;
        }

        GUI.DrawTextureWithTexCoords(draw_rect, handle_texture, uv);
        ;
    }

    void DrawTexture(Rect draw_rect, Texture2D tex)
    {
        Rect uv = new Rect(0, 0, tex.width, tex.height);
        uv.x /= tex.width;
        uv.y /= tex.height;
        uv.width /= tex.width;
        uv.height /= tex.height;
        GUI.DrawTextureWithTexCoords(draw_rect, tex, uv);
    }

    void DrawRectOutline(Rect rect, Color color)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = EditorGUIUtility.whiteTexture;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
            GUI.color = Color.white;
        }
    }
    #endregion

    #region 事件相关
    void CheckEvent(Dictionary<int, Rect> rectList, List<Object> resList, Rect rectArea)
    {
        //void CheckEvent(Rect draw_rect, Object obj,int index) {
        Event evt = Event.current;
        Object obj = null;
        switch (evt.type)
        {
            case EventType.ContextClick://这个不触发 不知道为啥
                evt.Use();
                break;
            case EventType.MouseDown:
                DragAndDrop.PrepareStartDrag();
                //obj = GetNowPosItem(rectList, resList, evt);
                //UpdateDragObjectReferences(obj);
                evt.Use();
                break;
            case EventType.MouseUp:
                Debug.Log("MouseUp");
                obj = GetNowPosItem(rectList, resList, evt);
                UpdateDragObjectReferences(obj);
                if (obj != null)
                {
                    string path = AssetDatabase.GetAssetPath(nowSelectObj);
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path); ;
                }
                evt.Use();
                break;
            case EventType.MouseDrag:
                // If drag was started here:
                Debug.Log("MouseDrag");
                obj = GetNowPosItem(rectList, resList, evt);
                UpdateDragObjectReferences(obj, true);
                if (obj != null && DragAndDrop.GetGenericData("test") != null)
                {
                    DisposeEditorNameTimer();
                    DragAndDrop.StartDrag("test");
                    //Debug.Log("StartDrag:");
                    evt.Use();
                }
                break;
            case EventType.DragUpdated:
                //Debug.Log("DragUpdated");
                var objectReferences = DragAndDrop.objectReferences;
                var paths = DragAndDrop.paths;
                if ((objectReferences.Length > 0 && objectReferences[0] != nowSelectObj) || (paths.Length > 0 && paths[0].IndexOf("Assets/") != 0))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                evt.Use();
                break;
            case EventType.Repaint:
                break;
            case EventType.DragPerform:
                DragAndDrop.AcceptDrag();
                ServiceResSelectorUtil.TryImportAssets(nowResRootPath, resRootList, resSubType, nowSubDic);
                evt.Use();
                break;
            case EventType.DragExited:
                //Debug.Log("DragExited");
                DragAndDrop.PrepareStartDrag();
                evt.Use();
                break;
        }
    }

    Object GetNowPosItem(Dictionary<int, Rect> rectList, List<Object> resList, Event evt)
    {
        Rect r;
        Object obj = null;
        if (resListRect.Contains(evt.mousePosition) == false)
        {
            return null;
        }
        Vector2 pos = evt.mousePosition + resListScrollPos;
        foreach (int i in rectList.Keys)
        {
            if (rectList[i].Contains(pos))
            {
                r = rectList[i];
                obj = resList[i];
                break;
            }
        }
        return obj;
    }

    void UpdateDragObjectReferences(Object obj, bool fromDragEvent = false)
    {
        if (obj != null)
        {
            //Debug.Log("SetGenericData:" + obj.name + "#");
            if (Event.current.button == 1)
            {
                OnContextClick();
            }
            else
            {
                OnResItemClick(obj, true, fromDragEvent);
                DragAndDrop.SetGenericData("test", obj);
                Object[] arr = new Object[] { obj };
                DragAndDrop.objectReferences = nowSelectObjList.ToArray();
            }
        }
    }

    void OnResItemClick(Object obj, bool needPing, bool fromDragEvent)
    {
        if (obj != null && nowSelectObj == obj && (Event.current.control || Event.current.shift) == false)
        {
            DelayShowTextField();
        }
        else
        {
            isInEditorName = false;
        }
        nowSelectObj = null;
        nowSelectObjList = ServiceResSelectorUtil.GetSelectObjList(Event.current, nowSelectObjList, nowResList, obj, fromDragEvent);

        if (nowSelectObjList.Count > 0)
        {
            nowSelectObj = nowSelectObjList[nowSelectObjList.Count - 1];
        }

        GUI.FocusControl(null);
        if (needPing && nowSelectObj)
        {
            EditorGUIUtility.PingObject(obj);
        }
    }
    //右键菜单栏
    private void OnContextClick()
    {
        var menu = new GenericMenu();
        string path = AssetDatabase.GetAssetPath(nowSelectObj);
/*        menu.AddItem(new GUIContent("Inspector"), false, delegate (object data)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            Selection.activeObject = obj;
        }, null);*/
        menu.AddItem(new GUIContent("打开文件夹"), false, delegate (object data)
        {
            EditorUtility.RevealInFinder(path);
        }, null);
        if (nowSelectObj is Sprite)
        {
            menu.AddItem(new GUIContent("SpriteEditor"), false, delegate (object data)
            {

                Selection.activeObject = nowSelectObj;
                System.Type type = System.Type.GetType("UnityEditor.SpriteEditorWindow,UnityEditor");
                EditorWindow.GetWindow(type);

            }, null);
        }
        menu.AddItem(new GUIContent("移动到Atlas"), false, delegate (object data)
        {
            ServiceResSelectorUtil.MoveToNewFolder("Assets" + ATLAS_ROOT + nowSubDic, nowSelectObjList);
        }, null);

        menu.AddItem(new GUIContent("移动到AtlasDynamic"), false, delegate (object data)
        {
            ServiceResSelectorUtil.MoveToNewFolder("Assets" + ATLAS_DYNAMIC_ROOT + nowSubDic, nowSelectObjList);
        }, null);

        menu.AddItem(new GUIContent("移动到Texture"), false, delegate (object data)
        {
            ServiceResSelectorUtil.MoveToNewFolder("Assets" + Texture_ROOT + nowSubDic, nowSelectObjList);
        }, null);

        menu.AddItem(new GUIContent("移动到其他文件夹"), false, delegate (object data)
        {
            ServiceResSelectorUtil.SelectMoveToNewFolder(nowSelectObjList, nowResRootPath);
        }, null);

        menu.AddItem(new GUIContent("------------------------------------"), false, delegate (object data)
        {

        }, null);
        menu.AddItem(new GUIContent("删除"), false, delegate (object data)
        {
            nowSelectObj = null;
            ServiceResSelectorUtil.DeleteObjects(nowSelectObjList);
        }, null);

        menu.ShowAsContext();
    }

    #endregion

    #region 其他
    bool isSelectObj(Object obj)
    {
        return nowSelectObjList.Contains(obj);
    }
    void DelayShowTextField()
    {
        DisposeEditorNameTimer();
        if (isInEditorName == false)
        {
            editorNameTimer = new Timer();
            editorNameTimer.Interval = 500;
            editorNameTimer.Elapsed += new ElapsedEventHandler((obj, eventArg) =>
            {
                ShowTextField();
            });
            editorNameTimer.Enabled = true;
        }
    }

    void ShowTextField()
    {
        Debug.Log("ShowTextField");
        isInEditorName = true;
        DisposeEditorNameTimer();
        isDirty = true;
        //DrawResList();
    }

    void DisposeEditorNameTimer()
    {
        if (editorNameTimer != null)
        {
            editorNameTimer.Dispose();
        }
    }
    #endregion
    #region 数据获取更新
    string nowSubDic
    {
        get
        {
            if (subDicIndex == 0)
            {
                return selectService + "";
            }
            else
            {
                return selectService + "/" + subDicPopOptions[subDicIndex];
            }
        }
    }
    string GetControlName(Object res)
    {
        return "ResNameText:" + res.name;
    }

    void FindServiceList()
    {
        serviceList.Clear();
        foreach (string path in resRubTypeList)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            DirectoryInfo[] dics = root.GetDirectories();
            foreach (DirectoryInfo dic in dics)
            {
                if (dic.Name != "Dynamic")
                {
                    serviceList.Add(dic.Name);
                }
            }
        }

        //string path = Application.dataPath + "/" + ATLAS_ROOT;

        string str = EditorPrefs.GetString(SERVICERESSELECTOR_LATELY);
        string[] arr = str.Split(',');
        latelyServiceList = arr.ToList<string>();
        for (int i = latelyServiceList.Count - 1; i >= 0; i--)
        {
            if (serviceList.Contains(latelyServiceList[i]) == false)
            {
                latelyServiceList.RemoveAt(i);
            }
        }
        if (latelyServiceList.Contains("Common") == false)
        {
            latelyServiceList.Insert(0, "Common");
        }

    }

    void UpdateLatelyServiceList(string name, bool updateUse)
    {
        string str = "";
        List<string> list = latelyServiceList;
        if (updateUse == false)
        {
            list = latelyServiceList.GetRange(0, latelyServiceList.Count);
        }

        list.Remove(name);
        list.Insert(0, name);
        int count = Mathf.Min(10, list.Count);
        for (int i = 0; i < count; i++)
        {
            str += list[i] + ",";
        }
        EditorPrefs.SetString(SERVICERESSELECTOR_LATELY, str);
    }


    void RestSubDic(string subName = "")
    {
        subDicIndex = 0;

        List<string> list = GetAllSubResList();

        if (string.IsNullOrEmpty(subName) == false)
        {
            if (list.Contains(subName) == false)
            {
                list.Add(subName);
            }
            subDicIndex = list.IndexOf(subName);
        }
        subDicPopOptions = list.ToArray<string>();
    }
    List<string> GetAllSubResList()
    {
        List<string> list = new List<string>();
        list.Add("当前");
        foreach (string str in resRubTypeList)
        {
            GetSubResList(list, str + selectService);
        }
        return list;
    }
    void GetSubResList(List<string> list, string path)
    {
        if (Directory.Exists(path))
        {
            DirectoryInfo root = new DirectoryInfo(path);
            DirectoryInfo[] dics = root.GetDirectories();
            foreach (DirectoryInfo dic in dics)
            {
                list.Add(dic.Name);
            }
        }
    }

    List<Object> GetResList(string resPath)
    {
        var folderInfo = new DirectoryInfo(resPath);
        var picPaths = folderInfo.GetFiles("*.png");
        var picPaths2 = folderInfo.GetFiles("*.jpg");
        var prefabPaths = folderInfo.GetFiles("*.prefab");
        List<FileInfo> allPath = new List<FileInfo>();
        allPath = picPaths.Concat(prefabPaths).ToList();
        allPath = allPath.Concat(picPaths2).ToList();
        List<Object> resList = new List<Object>();
        for (int index = 0; index < allPath.Count; ++index)
        {
            var fileInfo = allPath[index];
            var strPath = fileInfo.FullName.Substring(Application.dataPath.Length,
            fileInfo.FullName.Length - Application.dataPath.Length);
            Object obj = AssetDatabase.LoadAssetAtPath<Sprite>("Assets" + strPath);
            //var obj = AssetDatabase.LoadMainAssetAtPath("Assets" + strPath);
            if (obj == null)
            {
                obj = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets" + strPath);
            }
            if (obj == null)
            {
                obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets" + strPath);
            }
            if (obj)
            {
                resList.Add(obj);
            }
        }

        return resList;
    }
    void CreateOrJumpToService(string newServiceName)
    {
        if (string.IsNullOrEmpty(newServiceName) == false)
        {
            string[] arr = newServiceName.Split('/');
            if (arr[0] != ".")
            {
                selectService = arr[0];
            }
            if (serviceList.Contains(selectService) == false)
            {
                serviceList.Insert(0, selectService);
            }
            if (arr.Length > 1)
            {
                RestSubDic(arr[1]);
            }
            else
            {
                RestSubDic();
            }
        }
    }
    #endregion
    void RefreshAssets()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}