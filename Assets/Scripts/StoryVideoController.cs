using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StoryVideoController : MonoBehaviour
{
    [Header("影片")]

    [Tooltip("此影片專用的播放器。請放在不會隨分頁停用的物件上。")]
    public VideoPlayer videoPlayer;

    [Tooltip("此影片的進度條。首次完整觀看之前不可拖曳。")]
    public Slider videoProgressSlider;

    [Tooltip("顯示片段的已播放時間與總長度。")]
    public TMP_Text videoTimeText;


    [Header("播放範圍")]

    [Tooltip("片段開始秒數。例如 00:41 填 41；從頭播放填 0。")]
    [Min(0f)]
    public float startTime = 0f;

    [Tooltip("片段結束秒數。例如 04:19 填 259；填 0 代表播放到影片結尾。")]
    [Min(0f)]
    public float endTime = 0f;


    [Header("觀看紀錄")]

    [Tooltip("是否已完整觀看此片段。只保留在本次遊戲。")]
    [SerializeField]
    private bool hasCompletedFirstViewing;


    [Header("腳本")]

    [Tooltip("用來讀取測試模式。測試模式可略過首次觀看限制。")]
    public GameManager gameManager;


    // 測試模式只暫時略過限制，不會直接寫入完成紀錄。
    public bool HasCompletedFirstViewing =>
        hasCompletedFirstViewing ||
        (gameManager != null && gameManager.IsTestMode);

    // 保存的是原始影片中的秒數。
    private double savedVideoTime;

    private bool isVideoPageVisible;
    private bool wantsToPlay;
    private bool isSeeking;
    private bool endedAtSegmentEnd;


    private void Awake()
    {
        savedVideoTime = startTime;

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.playbackSpeed = 1f;

            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.seekCompleted += OnSeekCompleted;
            videoPlayer.loopPointReached += OnVideoEnded;
            videoPlayer.errorReceived += OnVideoError;
        }

        if (videoProgressSlider != null)
        {
            videoProgressSlider.minValue = 0f;
            videoProgressSlider.maxValue = 1f;
            videoProgressSlider.wholeNumbers = false;

            videoProgressSlider.onValueChanged.AddListener(
                OnProgressChanged
            );
        }
    }


    private void OnEnable()
    {
        RefreshVideoUI();
    }


    private void OnDisable()
    {
        SetPageVisible(false);
    }


    private void Update()
    {
        if (videoPlayer != null &&
            videoPlayer.isPrepared &&
            !isSeeking &&
            !endedAtSegmentEnd)
        {
            // 不提供加速播放。
            videoPlayer.playbackSpeed = 1f;

            if (videoPlayer.isPlaying && IsVideoPageVisible())
            {
                savedVideoTime = System.Math.Max(
                    GetStartTime(),
                    videoPlayer.time
                );

                double end = GetEndTime();

                if (end > GetStartTime() &&
                    videoPlayer.time >= end)
                {
                    CompleteSegment();
                }
            }
        }

        RefreshVideoUI();
    }


    public void PlayVideo()
    {
        if (!IsVideoPageVisible() ||
            videoPlayer == null ||
            wantsToPlay)
        {
            return;
        }

        if (videoPlayer.clip == null)
        {
            Debug.LogWarning("尚未指定影片 Video Clip。", this);
            return;
        }

        if (GetDuration() <= 0d)
        {
            Debug.LogWarning(
                "影片播放範圍無效，請確認開始與結束秒數。",
                this
            );

            return;
        }

        videoPlayer.playbackSpeed = 1f;

        if (endedAtSegmentEnd)
        {
            savedVideoTime = GetStartTime();
            endedAtSegmentEnd = false;
        }

        wantsToPlay = true;

        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
            return;
        }

        // 定位尚未完成時，交由定位完成事件接續播放。
        if (!isSeeking)
        {
            SeekToSavedTime();
        }
    }


    private void OnVideoPrepared(VideoPlayer source)
    {
        if (!wantsToPlay || !IsVideoPageVisible())
        {
            return;
        }

        SeekToSavedTime();
    }


    private void SeekToSavedTime()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared)
        {
            return;
        }

        if (!videoPlayer.canSetTime)
        {
            wantsToPlay = false;

            Debug.LogError(
                "此播放器無法調整時間，不能播放指定片段。",
                this
            );

            return;
        }

        videoPlayer.Pause();

        double latest = System.Math.Max(
            GetStartTime(),
            GetEndTime() - 0.1d
        );

        savedVideoTime = System.Math.Min(
            System.Math.Max(GetStartTime(), savedVideoTime),
            latest
        );

        // 已在指定位置附近時，直接接續播放。
        if (System.Math.Abs(
                videoPlayer.time - savedVideoTime) < 0.01d)
        {
            isSeeking = false;
            ResumeIfRequested();
            return;
        }

        isSeeking = true;
        videoPlayer.time = savedVideoTime;
    }


    private void OnSeekCompleted(VideoPlayer source)
    {
        isSeeking = false;
        ResumeIfRequested();
    }


    private void ResumeIfRequested()
    {
        if (wantsToPlay &&
            IsVideoPageVisible() &&
            videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }


    public void PauseVideo()
    {
        wantsToPlay = false;

        if (videoPlayer == null || !videoPlayer.isPrepared)
        {
            return;
        }

        if (!isSeeking && !endedAtSegmentEnd)
        {
            savedVideoTime = System.Math.Max(
                GetStartTime(),
                videoPlayer.time
            );
        }

        videoPlayer.Pause();
    }


    private void CompleteSegment()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }

        savedVideoTime = GetEndTime();
        endedAtSegmentEnd = true;
        wantsToPlay = false;

        hasCompletedFirstViewing = true;

        RefreshVideoUI();
    }


    private void OnVideoEnded(VideoPlayer source)
    {
        // 整部影片自然結束時也視為完成。
        // 提前結束的指定片段，則由 Update 判斷。
        if (wantsToPlay && IsVideoPageVisible() && !isSeeking)
        {
            CompleteSegment();
        }
    }


    private void OnVideoError(VideoPlayer source, string message)
    {
        wantsToPlay = false;
        isSeeking = false;

        Debug.LogError($"影片播放失敗：{message}", this);
    }


    private void OnProgressChanged(float value)
    {
        if (!HasCompletedFirstViewing ||
            !IsVideoPageVisible() ||
            videoPlayer == null ||
            !videoPlayer.isPrepared ||
            !videoPlayer.canSetTime ||
            isSeeking)
        {
            return;
        }

        double duration = GetDuration();

        if (duration <= 0d)
        {
            return;
        }

        // 記住拖曳前是否正在播放。
        wantsToPlay = videoPlayer.isPlaying;

        savedVideoTime =
            GetStartTime() +
            Mathf.Clamp01(value) * duration;

        endedAtSegmentEnd = false;
        SeekToSavedTime();

        RefreshVideoUI();
    }


    private void RefreshVideoUI()
    {
        double duration = GetDuration();

        double elapsed = System.Math.Min(
            duration,
            System.Math.Max(
                0d,
                savedVideoTime - GetStartTime()
            )
        );

        if (videoProgressSlider != null)
        {
            videoProgressSlider.interactable =
                HasCompletedFirstViewing &&
                IsVideoPageVisible() &&
                videoPlayer != null &&
                videoPlayer.isPrepared &&
                videoPlayer.canSetTime &&
                !isSeeking &&
                duration > 0d;

            videoProgressSlider.SetValueWithoutNotify(
                duration > 0d
                    ? (float)(elapsed / duration)
                    : 0f
            );
        }

        if (videoTimeText != null)
        {
            videoTimeText.text =
                $"{FormatTime(elapsed)} / {FormatTime(duration)}";
        }
    }


    private double GetClipLength()
    {
        if (videoPlayer == null || videoPlayer.clip == null)
        {
            return 0d;
        }

        return videoPlayer.clip.length;
    }


    private double GetStartTime()
    {
        return System.Math.Max(0d, startTime);
    }


    private double GetEndTime()
    {
        double length = GetClipLength();

        return endTime > 0f
            ? System.Math.Min(endTime, length)
            : length;
    }


    private double GetDuration()
    {
        return System.Math.Max(
            0d,
            GetEndTime() - GetStartTime()
        );
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


    public void SetPageVisible(bool visible)
    {
        isVideoPageVisible = visible;

        if (!visible)
        {
            PauseVideo();
        }

        RefreshVideoUI();
    }


    public void ResetAfterComputerClosed()
    {
        wantsToPlay = false;
        isSeeking = false;
        isVideoPageVisible = false;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        savedVideoTime = GetStartTime();
        endedAtSegmentEnd = false;

        // 保留完整觀看紀錄。
        RefreshVideoUI();
    }


    public void CompleteViewingForQuickStart()
    {
        hasCompletedFirstViewing = true;
        ResetAfterComputerClosed();
    }


    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.seekCompleted -= OnSeekCompleted;
            videoPlayer.loopPointReached -= OnVideoEnded;
            videoPlayer.errorReceived -= OnVideoError;
        }

        if (videoProgressSlider != null)
        {
            videoProgressSlider.onValueChanged.RemoveListener(
                OnProgressChanged
            );
        }
    }
}