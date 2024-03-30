using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class SavedEditor : Editor
{
    //Jsonファイルのパス
    string SavePath() => $"Assets/EditorSaveData/{GetType()}.json";

    //開くときにセーブデータを読み込み
    protected virtual void OnEnable()
    {
        // フォルダがあるか調査，なければ作成
        if (!Directory.Exists("Assets/EditorSaveData"))
        {
            Directory.CreateDirectory("Assets/EditorSaveData");
        }
        // ファイルがあるか調査，なければ作成
        if (!File.Exists(SavePath()))
        {
            File.Create(SavePath());
        }
        else
        {
            using (StreamReader sr = new StreamReader(SavePath()))
            {
                JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), this);
            }
        }
    }

    //閉じるときにデータを保存
    protected virtual void OnDisable()
    {
        //データを保存
        using (StreamWriter sw = new StreamWriter(SavePath(), false))
        {
            string jsonstr = JsonUtility.ToJson(this, false);
            sw.Write(jsonstr);
            sw.Flush();
        }
    }
}