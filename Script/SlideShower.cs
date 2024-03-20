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

    [SerializeField]
    public PhotoStand[] photoStands;

    [SerializeField, Tooltip("スライド時間")]
    private float slideDurationSeconds = 10f;

    private int _loadedIndex = -1;
    private int _slidePictureIndex = -1;
    private VRCImageDownloader _imageDownloader;
    private IUdonEventReceiver _udonEventReceiver;
    private Texture2D[] _downloadedTextures;
    private PhotoStand _targetPhotoStand;

    private void Start()
    {
        // ダウンロードしたテクスチャをキャッシュしておく箱.
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
        int counter = (int)(Networking.GetServerTimeInMilliseconds() / 1000f / slideDurationSeconds);
        _loadedIndex = counter % imageUrls.Length;
        _slidePictureIndex = counter % photoStands.Length;

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
            Debug.Log(photoStands.Length.ToString());
            Debug.Log(_slidePictureIndex.ToString());
            _imageDownloader.DownloadImage(imageUrls[_loadedIndex], photoStands[_slidePictureIndex].renderer.material, _udonEventReceiver, rgbInfo);
        }
    }

    private PhotoStand SelectPhotoStand(Texture2D tex, int targetPhotoStandIndex)
    {
        int selectIndex = targetPhotoStandIndex;
        for (int i = 0; i < photoStands.Length; i++)
        {
            if (photoStands[selectIndex].isHorizontal == tex.width > tex.height)
            {
                return photoStands[selectIndex];
            }
            selectIndex = (selectIndex + 1) % photoStands.Length;
        }
        // 見つからなかった場合は仕方ないのでもともと選択されていたものを返す
        return photoStands[targetPhotoStandIndex];
    }

    private void ShowNext(Texture2D tex)
    {
        _targetPhotoStand = SelectPhotoStand(tex, _slidePictureIndex);
        if (!_targetPhotoStand.tex1ToTex2)
        {
            _targetPhotoStand.renderer.sharedMaterial.SetTexture("_Tex1", tex);
        }
        else
        {
            _targetPhotoStand.renderer.sharedMaterial.SetTexture("_Tex2", tex);
        }
        StartChange();
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

            if (_targetPhotoStand.tex1ToTex2)
            {
                _targetPhotoStand.renderer.sharedMaterial.SetFloat("_Lerp", lerp);
            }
            else
            {
                _targetPhotoStand.renderer.sharedMaterial.SetFloat("_Lerp", 1 - lerp);
            }

            if (timeWatch < changeTime)
            {
                timeWatch += Time.deltaTime;
            }
            else
            {
                StopChange();
                _targetPhotoStand.tex1ToTex2 = !_targetPhotoStand.tex1ToTex2;
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