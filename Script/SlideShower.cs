using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SlideShower : UdonSharpBehaviour
{
    [SerializeField, Tooltip("ロードするURL")]
    public VRCUrl[] imageUrls;

    [SerializeField, Tooltip("表示するレンダーテクスチャ")]
    private new Renderer renderer;

    [SerializeField, Tooltip("スライド時間")]
    private float slideDurationSeconds = 10f;

    private int _loadedIndex = -1;
    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver _udonEventReceiver;
    private string[] _captions = new string[0];
    private Texture2D[] _downloadedTextures;

    private void Start()
    {
        // ダウンロードしたテクスチャをキャッシュしておく.
        _downloadedTextures = new Texture2D[imageUrls.Length];

        // ガベージコレクションが行われないようにVRCImageDownloaderを保存しておく
        _imageDownloader = new VRCImageDownloader();

        // イメージをロードするためにIUdonEventReceiverをキャストして取得
        _udonEventReceiver = (IUdonEventReceiver)this;

        // イメージを再帰的にダウンロード
        LoadNextRecursive();
    }

    public void LoadNextRecursive()
    {
        LoadNext();
        SendCustomEventDelayedSeconds(nameof(LoadNextRecursive), slideDurationSeconds);
    }

    private void LoadNext()
    {
        // All clients share the same server time. That's used to sync the currently displayed image.
        _loadedIndex = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / slideDurationSeconds) % imageUrls.Length;

        var nextTexture = _downloadedTextures[_loadedIndex];

        if (nextTexture != null)
        {
            // Image already downloaded! No need to download it again.
            renderer.sharedMaterial.mainTexture = nextTexture;
        }
        else
        {
            var rgbInfo = new TextureInfo();
            rgbInfo.GenerateMipMaps = true;
            _imageDownloader.DownloadImage(imageUrls[_loadedIndex], renderer.material, _udonEventReceiver, rgbInfo);
        }
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");

        _downloadedTextures[_loadedIndex] = result.Result;
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log($"Image not loaded: {result.Error.ToString()}: {result.ErrorMessage}.");
    }

    private void OnDestroy()
    {
        _imageDownloader.Dispose();
    }
}