using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StoryPageController : MonoBehaviour
{
    [Header("故事頁面")]

    [Tooltip("依閱讀順序放入頁面。第 0 個是影片頁，第 1 個是文字頁；之後可繼續增加。")]
    public GameObject[] pages;

    [Tooltip("影片所在的頁面索引，從 0 開始。影片放第一頁時填 0。")]
    [Min(0)]
    public int videoPageIndex = 0;

    [Tooltip("目前頁面索引，從 0 開始。由程式更新，請勿在播放中手動修改。")]
    [SerializeField]
    private int currentPageIndex = 0;


    [Header("分頁按鈕與頁碼")]

    [Tooltip("所有頁面的上一頁按鈕。程式會一起更新可互動狀態。")]
    public Button[] previousPageButtons;

    [Tooltip("所有頁面的下一頁按鈕。程式會一起更新可互動狀態。")]
    public Button[] nextPageButtons;

    [Tooltip("所有頁面的頁碼文字，例如 1 / 2。程式會一起更新。")]
    public TMP_Text[] pageNumberTexts;


    [Header("影片")]

    [Tooltip("掛在 StoryContent 上的 VideoPlayer，使用本機影片檔。")]
    public VideoPlayer videoPlayer;


    private void Awake()
    {
        if (videoPlayer != null)
        {
            // 必須按播放才開始，不自動循環。
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
        }
    }


    private void OnEnable()
    {
        // 每次進入故事，都從第一頁開始。
        ShowPage(0);
    }


    private void OnDisable()
    {
        // 離開故事或關閉電腦時，停止影片與聲音。
        StopVideo();
    }


    // ==========================================
    // 上一頁
    // ==========================================

    public void PreviousPage()
    {
        if (currentPageIndex <= 0)
        {
            return;
        }

        ShowPage(currentPageIndex - 1);
    }


    // ==========================================
    // 下一頁
    // ==========================================

    public void NextPage()
    {
        if (pages == null ||
            currentPageIndex >= pages.Length - 1)
        {
            return;
        }

        ShowPage(currentPageIndex + 1);
    }


    // ==========================================
    // 顯示指定頁面
    // ==========================================

    private void ShowPage(int pageIndex)
    {
        // 切頁時停止影片，返回影片頁也不自動播放。
        StopVideo();

        if (pages == null || pages.Length == 0)
        {
            RefreshNavigation();
            return;
        }

        currentPageIndex = Mathf.Clamp(
            pageIndex,
            0,
            pages.Length - 1
        );

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        RefreshNavigation();
    }


    // ==========================================
    // 更新按鈕與頁碼
    // ==========================================

    private void RefreshNavigation()
    {
        int pageCount = pages != null ? pages.Length : 0;

        bool canGoPrevious =
            pageCount > 0 && currentPageIndex > 0;

        bool canGoNext =
            pageCount > 0 && currentPageIndex < pageCount - 1;

        if (previousPageButtons != null)
        {
            foreach (Button button in previousPageButtons)
            {
                if (button != null)
                {
                    button.interactable = canGoPrevious;
                }
            }
        }

        if (nextPageButtons != null)
        {
            foreach (Button button in nextPageButtons)
            {
                if (button != null)
                {
                    button.interactable = canGoNext;
                }
            }
        }

        string pageLabel = pageCount > 0
            ? $"{currentPageIndex + 1} / {pageCount}"
            : "0 / 0";

        if (pageNumberTexts != null)
        {
            foreach (TMP_Text pageText in pageNumberTexts)
            {
                if (pageText != null)
                {
                    pageText.text = pageLabel;
                }
            }
        }
    }


    // ==========================================
    // 播放／繼續影片
    // ==========================================

    public void PlayVideo()
    {
        if (!isActiveAndEnabled ||
            pages == null ||
            videoPageIndex >= pages.Length ||
            videoPageIndex < 0 ||
            currentPageIndex != videoPageIndex ||
            pages[videoPageIndex] == null ||
            !pages[videoPageIndex].activeInHierarchy ||
            videoPlayer == null)
        {
            return;
        }

        if (videoPlayer.clip == null)
        {
            Debug.LogWarning(
                "故事頁尚未指定影片 Video Clip。",
                this
            );

            return;
        }

        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }


    // ==========================================
    // 暫停影片
    // ==========================================

    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }


    // ==========================================
    // 停止影片並重設播放位置
    // ==========================================

    private void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
}