using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ServiceResSelectorUtil
{
    private const string ellipsis = "...";
    public static void TryImportAssets(string nowResRootPath, string[] resRootList, int resSubType, string nowSubDic)
    {
        Debug.Log("TryImportAssets");
        if (DragAndDrop.paths.Length > 0)
        {
            int findIndex = DragAndDrop.paths[0].IndexOf("Assets/");
            bool isInAssets = findIndex == 0;

            foreach (string path in DragAndDrop.paths)
            {
                string fileName = Path.GetFileName(path);
                string destPath = "Assets" + resRootList[resSubType] + nowSubDic + "/" + fileName;
                if (isInAssets)
                {
                    AssetDatabase.MoveAsset(path, destPath);
                    Debug.Log(destPath);
                }
                else
                {
                    File.Copy(path, nowResRootPath + "/" + fileName, true);
                    Debug.Log(nowResRootPath + "/" + fileName);
                    AssetDatabase.Refresh();
                }
                Debug.Log(path);
                UpdateAssetInfo(destPath);
            }
        }
        else
        {
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                if (obj is GameObject)
                {
                    string destPath = "Assets" + resRootList[resSubType] + nowSubDic + "/" + obj.name + ".prefab";
                    PrefabUtility.CreatePrefab(destPath, obj as GameObject);
                    Debug.Log(destPath);
                }
            }
        }
        RefreshAssets();
    }
    public static void CreateDirectory(string path, bool setABName = true)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
            if (setABName)
            {
                string assetPath = path.Replace(Application.dataPath, "Assets");
                AssetImporter ai = AssetImporter.GetAtPath(assetPath);
                string abName = assetPath.Replace("Assets/GameAssets/", "").ToLower();
                ai.assetBundleName = abName;
                AssetDatabase.Refresh();
            }
        }
    }

    public static void UpdateAssetInfo(string assetPath)
    {
        if (assetPath.Contains("Texture/UIAtlas"))
        {
            var ti = TextureImporter.GetAtPath(assetPath) as TextureImporter;
            AssetDatabase.Refresh();
            if (ti)
            {
                ti.textureType = TextureImporterType.Sprite;
                string folderName = assetPath.Replace("/LuaFramework/Game/GameAssets/Texture/UIAtlas/", "");
                string[] arr = folderName.Split('/');
                folderName = "";
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    if(i!=0){
                        folderName +="_" ;
                    }
                    folderName += arr[i];
                }
                ti.spritePackingTag = folderName;
                ti.SaveAndReimport();
            }
        }
        else if (assetPath.Contains("Texture/UITexture"))
        {
            var ti = TextureImporter.GetAtPath(assetPath) as TextureImporter;
            AssetDatabase.Refresh();
            if (ti)
            {
                ti.textureType = TextureImporterType.Default;
                ti.mipmapEnabled = false;
                ti.npotScale = TextureImporterNPOTScale.None;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();

            }
        }
        RefreshAssets();
    }

    public static List<Object> GetSelectObjList(Event currentEvent, List<Object> nowSelectObjList, List<Object> nowResList, Object obj, bool fromDragEvent)
    {
        if ((currentEvent.control || currentEvent.shift) == false || fromDragEvent == false)
        {
            nowSelectObjList.Clear();
        }
        if (currentEvent.shift)
        {
            int objIndex = nowResList.IndexOf(obj);
            int startIndex = nowResList.Count - 1;
            int endIndex = 0;
            foreach (Object o in nowSelectObjList)
            {
                int index = nowResList.IndexOf(o);
                if (index != -1)
                {
                    startIndex = Mathf.Min(startIndex, index);
                    endIndex = Mathf.Max(endIndex, index);
                }
            }
            if (nowSelectObjList.Contains(obj))
            {
                if (objIndex == endIndex)
                {
                    endIndex = endIndex - 1;
                }
                else
                {
                    startIndex = objIndex + 1;
                }
            }
            else
            {
                startIndex = Mathf.Min(startIndex, objIndex);
                endIndex = Mathf.Max(endIndex, objIndex);
            }
            nowSelectObjList = nowResList.GetRange(startIndex, endIndex - startIndex + 1);
        }
        else
        {
            if (nowSelectObjList.Contains(obj) == false)
            {
                nowSelectObjList.Add(obj);
            }
            else if (fromDragEvent == false)
            {
                nowSelectObjList.Remove(obj);
            }
        }

        return nowSelectObjList;
    }

    public static void SelectMoveToNewFolder(List<Object> nowSelectObjList, string nowResRootPath)
    {
        var resPath = EditorUtility.OpenFolderPanel("选择要移动到的目录", nowResRootPath, "");
        if (string.IsNullOrEmpty(resPath) == false)
        {
            resPath = resPath.Replace(Application.dataPath, "Assets");
            MoveToNewFolder(resPath, nowSelectObjList);
        }
    }

    public static void MoveToNewFolder(string resPath, List<Object> nowSelectObjList)
    {
        Debug.Log(resPath);
        int count = nowSelectObjList.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            string oldPath = AssetDatabase.GetAssetPath(nowSelectObjList[i]);
            string newPath = resPath + "/" + Path.GetFileName(oldPath);
            MoveAsset(oldPath, newPath);
        }
        RefreshAssets();
    }

    public static void MoveAsset(string oldPath, string newPath)
    {
        AssetDatabase.MoveAsset(oldPath, newPath);
        UpdateAssetInfo(newPath);
    }

    public static void DeleteObjects(List<Object> nowSelectObjList)
    {
        int count = nowSelectObjList.Count;
        string path = "";
        for (int i = count - 1; i >= 0; i--)
        {
            path = AssetDatabase.GetAssetPath(nowSelectObjList[i]);
            AssetDatabase.DeleteAsset(path);
            Debug.Log("删除:" + path);
        }
        RefreshAssets();
    }

    public static string GetFittableText(string text, Rect area, GUIStyle style)
    {
        // Early exit: If text isn't clipped at all, it doesn't need an ellipsis
        if (style.clipping == TextClipping.Overflow) return text;

        bool willClip = false;
        float textSizeX = 0;
        if (style.wordWrap)
        { // Can be multi-line
            GUIContent textContent = new GUIContent(text);
            float textHeight = style.CalcHeight(textContent, area.width);
            if (textHeight > area.height) willClip = true;
        }
        else
        { // Single line
            GUIContent textContent = new GUIContent(text);
            Vector2 textSize = style.CalcSize(textContent);
            textSizeX = textSize.x;
            if (textSize.x > area.width) willClip = true;
        }

        if (willClip)
        {
            GUIContent ellipsisContent = new GUIContent(ellipsis);
            float ellipsisWidth = style.CalcSize(ellipsisContent).x; // Ellipsis size with this style
            int endIndex = (int)Mathf.Ceil(text.Length * area.width / textSizeX);
            endIndex = Mathf.Max(1, endIndex - 3);
            return text.Substring(0, endIndex) + ellipsis;
        }
        return text;
    }
    static void RefreshAssets()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}