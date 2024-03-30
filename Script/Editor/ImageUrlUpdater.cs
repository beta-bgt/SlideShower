using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomEditor(typeof(SlideShower))]
public class ImageUrlUpdater : SavedEditor
{
    [SerializeField] string deployUri;
    [SerializeField] string pictDirectory;


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Label("写真が置いてあるディレクトリ");
        pictDirectory = GUILayout.TextField(pictDirectory);
        GUILayout.Label("写真をデプロイしているURL");
        deployUri = GUILayout.TextField(deployUri);

        if (GUILayout.Button("ImageUrlUpdate"))
        {
            // 写真の名前を取得する
            List<string> pictNames = GetFiles(pictDirectory, "png").ToList();

            // デプロイ先のURLと写真の名前を結合する
            List<string> imageUrlStrings = pictNames.Select(uri => deployUri + uri).ToList();

            // ImageUrlのセッティング
            SlideShower slideShower = (SlideShower)target;
            slideShower.imageUrls = imageUrlStrings.Select(str => new VRCUrl(str)).ToArray();
        }
        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 指定したディレクトリ内で指定した拡張子を持つアセットを取得する
    /// </summary>
    /// <param name="path">検索するディレクトリ</param>
    /// <param name="extensions">検索する拡張子</param>
    /// <returns></returns>
    public static string[] GetFiles(string path, params string[] extensions)
    {
        return Directory
            .GetFiles(path, "*.*")
            .Where(c => extensions.Any(extension => c.EndsWith(extension)))
            .Select(c => Path.GetFileName(c))
            .ToArray()
        ;
    }
}
