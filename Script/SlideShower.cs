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

    [SerializeField, Tooltip("表示するレンダーテクスチャ(写真を表示するマテリアルを一番上にしてね！)")]
    private new Renderer[] renderers;

    [SerializeField, Tooltip("スライド時間")]
    private float slideDurationSeconds = 10f;

    private int _loadedIndex = -1;
    private int _slidePictureIndex = -1;
    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver _udonEventReceiver;
    private string[] _captions = new string[0];
    private Texture2D[] _downloadedTextures;
    private bool[] _tex1ToTex2s;

    private void Start()
    {
        // ダウンロードしたテクスチャをキャッシュしておく箱.
        _downloadedTextures = new Texture2D[imageUrls.Length];

        _tex1ToTex2s = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            _tex1ToTex2s[i] = true;
        }

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
        int counter = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / slideDurationSeconds);
        _loadedIndex = counter % imageUrls.Length;
        _slidePictureIndex = counter % renderers.Length;

        var nextTexture = _downloadedTextures[_loadedIndex];

        if (nextTexture != null)
        {
            // Image already downloaded! No need to download it again.
            ShowNext(nextTexture);
        }
        else
        {
            var rgbInfo = new TextureInfo();
            rgbInfo.GenerateMipMaps = true;
            _imageDownloader.DownloadImage(imageUrls[_loadedIndex], renderers[_slidePictureIndex].material, _udonEventReceiver, rgbInfo);
        }
    }

    bool changing = false;
    void StartChange()
    {
        changing = true;
    }
    void StopChange()
    {
        changing = false;
    }

    private void ShowNext(Texture2D tex)
    {
        if (!_tex1ToTex2s[_slidePictureIndex])
        {
            renderers[_slidePictureIndex].sharedMaterial.SetTexture("_Tex1", tex);
        }
        else
        {
            renderers[_slidePictureIndex].sharedMaterial.SetTexture("_Tex2", tex);
        }
        StartChange();
    }

    [SerializeField] float changeTime = 5f;
    float timeWatch = 0f;
    void Update()
    {
        if (changing)
        {
            if (timeWatch > changeTime)
            {
                timeWatch = changeTime;
            }

            float lerp = 1 - (changeTime - timeWatch) / changeTime;

            if (_tex1ToTex2s[_slidePictureIndex])
            {
                renderers[_slidePictureIndex].sharedMaterial.SetFloat("_Lerp", lerp);
            }
            else
            {
                renderers[_slidePictureIndex].sharedMaterial.SetFloat("_Lerp", 1 - lerp);
            }

            if (timeWatch < changeTime)
            {
                timeWatch += Time.deltaTime;
            }
            else
            {
                StopChange();
                _tex1ToTex2s[_slidePictureIndex] = !_tex1ToTex2s[_slidePictureIndex];
                timeWatch = 0f;
            }
        }
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        Debug.Log($"Image loaded: {result.SizeInMemoryBytes} bytes.");

        ShowNext(result.Result);
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