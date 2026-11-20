using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StoryVideoController : MonoBehaviour
{
    [Header("故事頁面")]


    [Header("影片")]

    [Tooltip("放在 ComputerPanel 下獨立物件上的 VideoPlayer，不要隨故事頁停用。")]
    public VideoPlayer videoPlayer;

    [Tooltip("影片進度條。首次看完之前不可拖曳。")]
    public Slider videoProgressSlider;

    [Tooltip("顯示目前時間與影片總長度的 TMP 文字。")]
    public TMP_Text videoTimeText;

    [Tooltip("是否已真正完整觀看一次。只保留在本次遊戲，不會自動存檔。")]
    [SerializeField]
    private bool hasCompletedFirstViewing = false;

    // 對外統一查詢：已看完影片，或目前是測試模式。
    public bool HasCompletedFirstViewing
    {
        get
        {
            return hasCompletedFirstViewing ||
                   (gameManager != null && gameManager.IsTestMode);
        }
    }


    // 記住目前影片位置，與首次觀看紀錄分開保存。
    private double savedVideoTime;

    // 影片準備完成後，是否需要開始播放。
    private bool playWhenPrepared;

    // 重播時，判斷目前是否停在影片結尾。
    private bool endedAtLastFrame;

    // 由電腦分頁控制器通知目前是否顯示影片頁。
    private bool isVideoPageVisible;

    [Header("腳本")]
    [Tooltip("整體流程控制器，用來判斷是否處於測試模式。")]public GameManager gameManager;

    private void Awake()
    {
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.playbackSpeed = 1f;

            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoEnded;
        }

        if (videoProgressSlider != null)
        {
            videoProgressSlider.minValue = 0f;
            videoProgressSlider.maxValue = 1f;
            videoProgressSlider.wholeNumbers = false;

            // 使用程式綁定，Inspector 不必再接事件。
            videoProgressSlider.onValueChanged.AddListener(
                OnProgressChanged
            );
        }
    }


    private void OnEnable()
    {
        // 分頁由 ComputerInteractable 統一管理。
        // 啟用時不自動播放。
        RefreshVideoUI();
    }


    private void OnDisable()
    {
        isVideoPageVisible = false;
        PauseVideo();
    }


    private void Update()
    {
        if (videoPlayer != null && videoPlayer.isPrepared)
        {
            // 首次觀看固定正常速度。
            if (!HasCompletedFirstViewing)
            {
                videoPlayer.playbackSpeed = 1f;
            }

            if (!endedAtLastFrame)
            {
                savedVideoTime =
                    System.Math.Max(0d, videoPlayer.time);
            }
        }

        RefreshVideoUI();
    }

    public void PlayVideo()
    {
        if (!IsVideoPageVisible() || videoPlayer == null)
            return;

        if (videoPlayer.clip == null)
        {
            Debug.LogWarning("尚未指定影片 Video Clip。", this);
            return;
        }

        if (videoPlayer.isPlaying)
            return;

        videoPlayer.playbackSpeed = 1f;

        if (endedAtLastFrame)
        {
            // 看完後再按播放，從頭重播。
            videoPlayer.Stop();
            savedVideoTime = 0d;
            endedAtLastFrame = false;
        }

        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
        }
        else
        {
            // 等待影片準備完成才播放。
            playWhenPrepared = true;
            videoPlayer.Prepare();
        }
    }


    private void OnVideoPrepared(VideoPlayer source)
    {
        // 準備期間若已切頁或暫停，就不開始播放。
        if (!playWhenPrepared || !IsVideoPageVisible())
            return;

        playWhenPrepared = false;
        source.Play();
    }


    public void PauseVideo()
    {
        playWhenPrepared = false;

        if (videoPlayer == null || !videoPlayer.isPrepared)
            return;

        if (!endedAtLastFrame)
        {
            savedVideoTime =
                System.Math.Max(0d, videoPlayer.time);
        }

        videoPlayer.Pause();
    }


    private void OnVideoEnded(VideoPlayer source)
    {
        // 由自然播放到結尾的事件解鎖首次觀看。
        hasCompletedFirstViewing = true;
        endedAtLastFrame = true;
        playWhenPrepared = false;
        savedVideoTime = GetDuration();

        RefreshVideoUI();
    }


    private void OnProgressChanged(float value)
    {
        if (!HasCompletedFirstViewing ||
            !IsVideoPageVisible() ||
            videoPlayer == null ||
            !videoPlayer.isPrepared ||
            !videoPlayer.canSetTime)
        {
            return;
        }

        double duration = GetDuration();

        if (duration <= 0d)
            return;

        // 留在最後一格之前，避免部分裝置無法定位至精確結尾。
        double latestTime =
            System.Math.Max(0d, duration - 0.1d);

        savedVideoTime =
            System.Math.Min(value * duration, latestTime);

        endedAtLastFrame = false;
        videoPlayer.time = savedVideoTime;
    }


    private void RefreshVideoUI()
    {
        double duration = GetDuration();

        if (videoProgressSlider != null)
        {
            videoProgressSlider.interactable =
                HasCompletedFirstViewing &&
                IsVideoPageVisible() &&
                videoPlayer != null &&
                videoPlayer.isPrepared &&
                videoPlayer.canSetTime &&
                duration > 0d;

            // 更新顯示時不觸發跳轉事件。
            videoProgressSlider.SetValueWithoutNotify(
                duration > 0d
                    ? Mathf.Clamp01((float)(savedVideoTime / duration))
                    : 0f
            );
        }

        if (videoTimeText != null)
        {
            videoTimeText.text =
                $"{FormatTime(savedVideoTime)} / " +
                $"{FormatTime(duration)}";
        }
    }


    private double GetDuration()
    {
        if (videoPlayer == null)
            return 0d;

        double duration = videoPlayer.clip != null
            ? videoPlayer.clip.length
            : videoPlayer.length;

        return double.IsNaN(duration) ||
               double.IsInfinity(duration) ||
               duration < 0d
            ? 0d
            : duration;
    }


    private string FormatTime(double seconds)
    {
        int total = (int)System.Math.Max(0d, seconds);

        return $"{total / 60:00}:{total % 60:00}";
    }


    private bool IsVideoPageVisible()
    {
        return isActiveAndEnabled && isVideoPageVisible;
    }


    // 只有真正關閉電腦時呼叫。
    public void ResetAfterComputerClosed()
    {
        playWhenPrepared = false;

        if (videoPlayer != null)
            videoPlayer.Stop();

        savedVideoTime = 0d;
        endedAtLastFrame = false;

        // 完整觀看紀錄保留，不必每次重新解鎖。
        RefreshVideoUI();
    }


    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoEnded;
        }

        if (videoProgressSlider != null)
        {
            videoProgressSlider.onValueChanged.RemoveListener(
                OnProgressChanged
            );
        }
    }

    // ==========================================
    // 快速開始：補齊影片觀看狀態
    // ==========================================

    public void CompleteViewingForQuickStart()
    {
        // 快速開始代表前面的教材已完成。
        hasCompletedFirstViewing = true;

        // 保留重新觀看的功能，播放位置從頭開始。
        ResetAfterComputerClosed();

        // 解鎖分頁與更新進度條狀態。
        RefreshVideoUI();
    }

    // ==========================================
    // 接收目前影片頁的顯示狀態
    // ==========================================

    public void SetPageVisible(bool visible)
    {
        isVideoPageVisible = visible;

        if (!visible)
        {
            PauseVideo();
        }

        RefreshVideoUI();
    }
}