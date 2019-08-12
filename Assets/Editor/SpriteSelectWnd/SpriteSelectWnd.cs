using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class SpriteSelectWnd : EditorWindow
{
    [MenuItem("Tools/UI/SpriteSelectWnd")]
    public static void OpenChooseWnd()
    {
       var wnd= GetWindow<SpriteSelectWnd>();
//       wnd.minSize = new Vector2(400,200);
    }


    private const string QUIKCK_LIST = "SpriteSelectWndQuickList";
    private const string UIRES_FOLDER_PATHKEY = "UIRES_FOLDER_PATHKEY";
    
    private int QuickListTabWidth = 150;
    private Vector2 scroll_pos = Vector2.zero;
    private string select_ui_name;
    private string cur_select_sprite_name;
    private int SPRITE_LABEL_HEIGHT = 22;
    private int SPRITE_SIZE = 100;
    const int RIGHT_SCROLL_GAP = 10; //滚动条距离窗口右边框的距离 
    private string search_text = "";
    private string str_folderPath = "Assets/LuaFramework/Game/GameAssets/Texture/UIAtlas/Common/Button";
    private const string ATLAS_ROOT = "/LuaFramework/Game/GameAssets/Texture/UIAtlas";

    public static string DefautUIFolder
    {
        get
        {
            
            if (EditorPrefs.HasKey(UIRES_FOLDER_PATHKEY))
            {
                var path=EditorPrefs.GetString(UIRES_FOLDER_PATHKEY);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            return @"E:\SVNAo\美术资源\奥拉星手游\07GUI\000UI资源拆解";
        }
        set
        {
            if (File.Exists(value))
            {
                EditorPrefs.SetString(UIRES_FOLDER_PATHKEY,new FileInfo(value).Directory.FullName);
            }
            else
            {
                EditorPrefs.SetString(UIRES_FOLDER_PATHKEY,value);
            }
            
        }
    } 
    
    private bool autoNativeSize = false;
    public List<string> QuickFolderList=new List<string>();
    private eUIMode curUIMode = eUIMode.选图;
    enum  eUIMode
    {
        选图=0,
        换图=1
    }
    
    private void Awake()
    {
        LoadQuickList();
    }

    private void LoadQuickList()
    {
        QuickFolderList.Clear();
        if (EditorPrefs.HasKey(QUIKCK_LIST))
        {
            var str = EditorPrefs.GetString(QUIKCK_LIST, null);
            var list = str.Split(',');
            foreach (string s in list)
            {
                if (QuickFolderList.Contains(s) == false)
                {
                    QuickFolderList.Add(s);
                }
            }
        }
    }

    void SaveQuickFolderList()
    {
        StringBuilder sb=new StringBuilder();
        for (int i=0;i<QuickFolderList.Count;i++)
        {
            if (i == 0)
            {
                sb.Append(QuickFolderList[i]);
            }
            else
            {
                sb.Append(","+QuickFolderList[i]);
            }
        }
            
        EditorPrefs.SetString(QUIKCK_LIST,sb.ToString());
    }

    void OnGUI()
    {
        DropAreaGUI();
        
        DrawToolMenu();
        DrawQuickListTab();
        try
        {
            DrawSprites(str_folderPath);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message+"Stack "+e.StackTrace);            
        }

    }

    void DrawQuickListTab()
    {
        GUILayout.BeginArea(new Rect(position.width-QuickListTabWidth,100,QuickListTabWidth,position.height-110));
        
        if (GUILayout.Button("设当前为常用"))
        {
            if (QuickFolderList.Contains(str_folderPath) == false)
            {
                QuickFolderList.Add(str_folderPath);
                SaveQuickFolderList();
            }
        }


        GUILayout.Label("常用列表");
        var skin=new GUIStyle(GUI.skin.button);
        skin.alignment = TextAnchor.MiddleRight;
        for (int i = 0; i < QuickFolderList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(QuickFolderList[i], skin,GUILayout.Width(QuickListTabWidth-22)))
            {
                str_folderPath = QuickFolderList[i];
            }

            if (GUILayout.Button("X",GUILayout.Width(20)))
            {
                QuickFolderList.RemoveAt(i);
                SaveQuickFolderList();
                break;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }
    
    private void DrawToolMenu()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("☝", GUILayout.Width(20)))
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(ATLAS_ROOT));
        }

        GUILayout.Label("AutoFit", GUILayout.Width(70));
        autoNativeSize = GUILayout.Toggle(autoNativeSize, "", GUILayout.Width(50));
        string before = search_text;
        string after = EditorGUILayout.TextField("", before, "SearchTextField", GUILayout.Width(200f));
        if (before != after) search_text = after;

        if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
        {
            search_text = "";
            GUIUtility.keyboardControl = 0;
        }

       
        GUILayout.Label("Drag->", GUILayout.Width(70));
        SPRITE_SIZE = (int) GUILayout.HorizontalSlider(SPRITE_SIZE, 50, 200, GUILayout.Width(100));

        DrawSelectFolderMenu();
        if (GUILayout.Button("☀", GUILayout.Width(30)))
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(str_folderPath));
        }

        if (GUILayout.Button("加图", GUILayout.Width(50)))
        {
            TryAddSprite();
        }
        
        if (GUILayout.Button("PackTag", GUILayout.Width(70)))
        {
            SetCurFolderPackingTag();
        }
        
        
        GUILayout.EndHorizontal();
    }

    void SetCurFolderPackingTag()
    {
        var sprList = GetSpiteList(str_folderPath);
        foreach (var sprite in sprList)
        {
            var importer = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
            importer.spritePackingTag = new DirectoryInfo(str_folderPath).Name;
            importer.SaveAndReimport();
        }
    }

    private void TryAddSprite()
    {
        var resPath = EditorUtility.OpenFilePanel("选择要添加的图片", DefautUIFolder, "png");
        if (string.IsNullOrEmpty(resPath) == false)
        {
            var fileInfo = new FileInfo(resPath);
            var path = str_folderPath + "/" + fileInfo.Name;
            File.WriteAllBytes(path, File.ReadAllBytes(resPath));
            AssetDatabase.ImportAsset(path);
            var importer = TextureImporter.GetAtPath(path) as TextureImporter;
            AssetDatabase.Refresh();
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    private void DrawSelectFolderMenu()
    {
        if (GUILayout.Button("✚", GUILayout.Width(30)))
        {
            GenericMenu menu = new GenericMenu();
            var folders = GetAtlasFoldersRecursively();

            if (folders != null && folders.Length > 0)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    var folder = folders[i];
                    var prefix = Application.dataPath + "/" + ATLAS_ROOT + "/";
                    var menuStr = folder.FullName.Substring(prefix.Length, folder.FullName.Length - prefix.Length);
                    menuStr = menuStr.Replace('\\', '/');
                    menu.AddItem(new GUIContent(menuStr), false,
                        delegate(object data) { str_folderPath = folder.FullName; }, null);
                }
            }

            menu.ShowAsContext();
        }
    }

    private static DirectoryInfo[] GetAtlasFoldersRecursively()
    {
        List<DirectoryInfo > infoList=new List<DirectoryInfo>();
        Queue<string> queue=new Queue<string>();
        queue.Enqueue(Application.dataPath + ATLAS_ROOT);


        while (queue.Count>0)
        {
            var path = queue.Dequeue();
            var dirInfo=new DirectoryInfo(path);
            var subDirs = dirInfo.GetDirectories();
            if (subDirs.Length == 0)
            {
                infoList.Add(dirInfo);
            }
            else
            {
                foreach (var subDir in subDirs)
                {
                    queue.Enqueue(subDir.FullName);
                }
            }
        }
        infoList.OrderBy(o => o.Name);
        return infoList.ToArray();
    }

    

    public void DropAreaGUI ()
    {
        Event evt = Event.current;
        GUILayout.Box( "Current DropFolder: "+str_folderPath,GUILayout.Height(30),GUILayout.ExpandWidth(true));
        Rect drop_area = GUILayoutUtility.GetLastRect();
        switch (evt.type) {
            
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains (evt.mousePosition))
                    return;
             
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
         
                if (evt.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag ();
                    evt.Use();
             
                    foreach (Object dragged_object in DragAndDrop.objectReferences)
                    {
                        var path = AssetDatabase.GetAssetPath(dragged_object);
                        str_folderPath = path;
                    }
                }
                break;
        }
    }
    void DrawSprites(string atlas_path)
    {
       
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        

        var sprite_list = GetSpiteList(atlas_path);


        List<Sprite> show_sprite_list = GetListOfSprites(sprite_list, search_text);

        GUILayout.Space(10);

        int scrollWidth = Screen.width - RIGHT_SCROLL_GAP-QuickListTabWidth;
        scrollWidth = Mathf.Max(100, scrollWidth);
        scroll_pos = GUILayout.BeginScrollView(scroll_pos, GUILayout.Width(scrollWidth));

        int col_show_count = (scrollWidth) / (SPRITE_SIZE + 10);
        col_show_count = Mathf.Max(1, col_show_count); //the following for-loop will run forever if we limit this
        int total_count = show_sprite_list.Count;
        int row_index = 0;
        for (int sprite_index = 0; sprite_index < total_count; )
        {
            GUILayout.BeginHorizontal();
            for (int col_index = 0; col_index < col_show_count && sprite_index < total_count; ++col_index)
            {
                DrawSprite(row_index, col_index, show_sprite_list[sprite_index]);
                ++sprite_index;
            }
            GUILayout.EndHorizontal();
            row_index++;
        }

        GUILayout.Space(row_index * (SPRITE_SIZE + SPRITE_LABEL_HEIGHT + 2));

        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUILayout.EndVertical();
    }

    private static List<Sprite> GetSpiteList(string atlas_path)
    {
        var folderInfo = new DirectoryInfo(atlas_path);
        var picPaths = folderInfo.GetFiles("*.png");
        List<Sprite> sprite_list = new List<Sprite>();
        for (int index = 0; index < picPaths.Length; ++index)
        {
            var fileInfo = picPaths[index];
            var strPath = fileInfo.FullName.Substring(Application.dataPath.Length,
                    fileInfo.FullName.Length - Application.dataPath.Length);
            var obj = AssetDatabase.LoadAssetAtPath<Sprite>("Assets" + strPath);
            if (obj)
            {
                sprite_list.Add(obj);
            }
        }

        return sprite_list;
    }

    void DrawSprite(int row_index, int col_index, Sprite sprite)
    {

        Texture2D handle_texture = sprite.texture;
        GUILayout.BeginVertical();

        Rect uv = new Rect(sprite.rect.x / handle_texture.width, sprite.rect.y / handle_texture.height,
                           sprite.rect.width / handle_texture.width, sprite.rect.height / handle_texture.height);
        Rect draw_rect = new Rect(col_index * (SPRITE_SIZE + 10), row_index * (SPRITE_SIZE + SPRITE_LABEL_HEIGHT + 2), SPRITE_SIZE, SPRITE_SIZE);

        GUI.backgroundColor = new Color(.6f, 1.0f, 1.0f, 0.5f);
        if (GUI.Button(draw_rect, ""))
        {
            HandleSpriteClick(sprite);
        }
        GUI.backgroundColor = Color.white;

        if (cur_select_sprite_name == sprite.name)
            DrawRectOutline(draw_rect, Color.green);

        float scale = sprite.rect.width / sprite.rect.height;
        float sprite_width = sprite.rect.width;
        float sprite_height = sprite.rect.height;

        if (sprite_width < sprite_height)
        {
            draw_rect.height = SPRITE_SIZE;
            draw_rect.width = SPRITE_SIZE * scale;
            draw_rect.x += (SPRITE_SIZE - draw_rect.width) / 2;
        }
        else
        {
            draw_rect.width = SPRITE_SIZE;
            draw_rect.height = SPRITE_SIZE / scale;
            draw_rect.y += (SPRITE_SIZE - draw_rect.height) / 2;
        }

        GUI.DrawTextureWithTexCoords(draw_rect, handle_texture, uv);
        var nameRect = new Rect(col_index * (SPRITE_SIZE + 10),
            row_index * (SPRITE_SIZE + SPRITE_LABEL_HEIGHT + 2) + SPRITE_SIZE + 2, SPRITE_SIZE, SPRITE_LABEL_HEIGHT);
        EditorGUI.TextField(nameRect, "",sprite.name ,"ProgressBarBack");
        
        GUILayout.EndVertical();

    }

    private void HandleSpriteClick(Sprite sprite)
    {
        if (Event.current.button == 1)
        {
            var menu=new GenericMenu();
            menu.AddItem(new GUIContent("PingProject"),false,delegate(object data) {
                EditorGUIUtility.PingObject(sprite);
                
              },null);
            
            menu.AddItem(new GUIContent("SpriteEditor"),false,delegate(object data)
            {

                Selection.activeObject = sprite;
                Type type = System.Type.GetType("UnityEditor.SpriteEditorWindow,UnityEditor");
                EditorWindow.GetWindow(type );
                    
            },null);
            
            menu.AddItem(new GUIContent("替换"),false,delegate(object data)
            {
                TryReplaceSprite(sprite);
            },null);
            menu.AddItem(new GUIContent("PingHierarchy"),false,delegate(object data)
            {
                PingInHierarchy(sprite);
            },null);
            
            menu.ShowAsContext();
        }
        else
        {
            if (curUIMode == eUIMode.选图)
            {
                TryChangeSprite(sprite);
            }
            else if (curUIMode == eUIMode.换图)
            {
                TryReplaceSprite(sprite);
            }
        }
        
        
    }

    void PingInHierarchy(Sprite sprite)
    {
        var images = GameObject.FindObjectsOfType<Image>();
        foreach (var img in images)
        {
            if (img.sprite == sprite)
            {
                EditorGUIUtility.PingObject(img.gameObject); 
            }
        }
    }
    

    private static void TryReplaceSprite(Sprite sprite)
    {
        var resPath = EditorUtility.OpenFilePanel("选择要替换的图片", DefautUIFolder, "png");
        if (string.IsNullOrEmpty(resPath) == false)
        {
            DefautUIFolder = resPath;
            var spritePath = AssetDatabase.GetAssetPath(sprite);

            var srcContent = File.ReadAllBytes(resPath);
            File.WriteAllBytes(spritePath,srcContent);
            AssetDatabase.Refresh();
        }
    }

    private void TryChangeSprite(Sprite sprite)
    {
        cur_select_sprite_name = sprite.name;
        foreach (GameObject handle_object in Selection.gameObjects)
        {
            Image handle_image = handle_object.GetComponent<Image>();
            if (handle_image == null)
                continue;

            Undo.RecordObject(handle_image, "ChangeSprite");
            handle_image.sprite = sprite;

            if (autoNativeSize)
            {
                handle_image.SetNativeSize();
            }

            EditorUtility.SetDirty(handle_object);
        }
    }

    static public void DrawRectOutline(Rect rect, Color color)
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

    List<Sprite> GetListOfSprites(List<Sprite> sprite_list, string match)
    {
        if (string.IsNullOrEmpty(match)) return sprite_list;

        List<Sprite> list = new List<Sprite>();

        // First try to find an exact match
        for (int i = 0, imax = sprite_list.Count; i < imax; ++i)
        {
            Sprite s = sprite_list[i];

            if (s != null && !string.IsNullOrEmpty(s.name) && string.Equals(match, s.name, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(s);
                return list;
            }
        }

        // No exact match found? Split up the search into space-separated components.
        string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

        // Try to find all sprites where all keywords are present
        for (int i = 0, imax = sprite_list.Count; i < imax; ++i)
        {
            Sprite s = sprite_list[i];

            if (s != null && !string.IsNullOrEmpty(s.name))
            {
                string tl = s.name.ToLower();
                int matches = 0;

                for (int b = 0; b < keywords.Length; ++b)
                {
                    if (tl.Contains(keywords[b])) ++matches;
                }
                if (matches == keywords.Length) list.Add(s);
            }
        }
        return list;
    }

  

}
