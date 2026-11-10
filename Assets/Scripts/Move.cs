using UnityEngine; // 引入 Unity 基本功能。
using UnityEngine.AI; // 引入 NavMesh 導航功能。

public class Move : MonoBehaviour
{
    // ==========================================
    // NavMesh
    // ==========================================

    [Header("NavMesh")]

    public NavMeshAgent agent;
    // 控制玩家在 NavMesh 上移動。


    [Header("NavMesh 邊界設定")]

    public float edgeStopPadding = 0.05f;
    // 遇到 NavMesh 邊界時，
    // 除了 Agent 自己的 Radius 之外，
    // 再額外往 NavMesh 內側退多少距離。


    public float destinationUpdateThreshold = 0.15f;
    // 新目的地與上一個實際目的地至少相差多少，
    // 才重新呼叫 SetDestination。
    //
    // 這是這次新增的重點：
    // 避免長按時一直對同一個邊界點重新尋路。


    // ==========================================
    // 角色動畫
    // ==========================================

    [Header("角色動畫")]

    public Animator animator;
    // 控制角色 Idle / Walk 動畫。


    public float walkSpeedThreshold = 0.05f;
    // 實際速度低於這個值時，
    // 視為已經停止。


    // ==========================================
    // 移動設定
    // ==========================================

    [Header("移動設定")]

    public bool canMove = true;
    // 玩家目前是否允許移動。


    public float arrivalTolerance = 0.05f;
    // 判斷是否到達目的地時，
    // 額外允許的距離誤差。


    // ==========================================
    // 目前路徑資料
    // ==========================================

    private NavMeshPath calculatedPath;
    // 暫存路徑計算結果。


    private Vector3 actualDestination;
    // 玩家目前真正要走到的 NavMesh 位置。


    private bool hasDestination = false;
    // 目前是否有正在前往的目的地。


    // ==========================================
    // 上一次真正目的地
    // ==========================================

    private Vector3 lastActualDestination;
    // 上一次真正交給 Agent 的目的地。


    private bool hasLastActualDestination = false;
    // 是否曾經設定過真正目的地。


    // ==========================================
    // 初始化
    // ==========================================

    private void Awake()
    {
        // 如果 Inspector 沒有指定 Agent。
        if (agent == null)
        {
            // 自動取得 Player 上的 NavMeshAgent。
            agent = GetComponent<NavMeshAgent>();
        }


        // 如果 Inspector 沒有指定 Animator。
        if (animator == null)
        {
            // 從 Player 或子物件尋找 Animator。
            animator = GetComponentInChildren<Animator>();
        }


        // 建立可重複使用的 NavMeshPath。
        calculatedPath = new NavMeshPath();
    }


    // ==========================================
    // 每幀更新玩家移動狀態
    // ==========================================

    private void Update()
    {
        // ======================================
        // 安全檢查
        // ======================================

        if (agent == null)
        {
            SetWalking(false);
            return;
        }


        if (!agent.enabled)
        {
            SetWalking(false);
            return;
        }


        if (!agent.isOnNavMesh)
        {
            // Agent 不在 NavMesh 上時，
            // 不能讀取 remainingDistance。
            SetWalking(false);
            return;
        }


        if (!canMove)
        {
            SetWalking(false);
            return;
        }


        // ======================================
        // 目前沒有目的地
        // ======================================

        if (!hasDestination)
        {
            SetWalking(false);
            return;
        }


        // ======================================
        // Agent 還在計算路徑
        // ======================================

        if (agent.pathPending)
        {
            // 此時先依實際速度決定動畫。
            SetWalking(
                IsAgentActuallyMoving()
            );

            return;
        }


        // ======================================
        // 路徑突然消失
        // ======================================

        if (!agent.hasPath)
        {
            // 不再認為目前有目的地。
            hasDestination = false;

            // 回到 Idle。
            SetWalking(false);

            return;
        }


        // ======================================
        // 判斷是否抵達目的地
        // ======================================

        float remainingDistance =
            agent.remainingDistance;


        bool hasArrived =
            !float.IsInfinity(remainingDistance) &&
            remainingDistance <=
            agent.stoppingDistance +
            arrivalTolerance;


        // ======================================
        // 已到目的地
        // ======================================

        if (hasArrived)
        {
            // 清除現在的導航路徑。
            agent.ResetPath();


            // 現在沒有正在前往的目的地。
            hasDestination = false;


            // 動畫切回 Idle。
            SetWalking(false);

            return;
        }


        // ======================================
        // 正常移動
        // ======================================

        SetWalking(
            IsAgentActuallyMoving()
        );
    }


    // ==========================================
    // 設定玩家目的地
    // ==========================================

    // ==========================================
    // 設定玩家目的地
    // ==========================================

    public void SetTargetPosition(
        Vector3 requestedPosition)
    {
        // ======================================
        // 基本安全檢查
        // ======================================

        // 如果目前不允許玩家移動。
        if (!canMove)
        {
            return;
        }


        // 如果沒有 NavMeshAgent。
        if (agent == null)
        {
            return;
        }


        // 如果 Agent 沒有啟用。
        if (!agent.enabled)
        {
            return;
        }


        // 如果 Agent 目前不在 NavMesh 上。
        if (!agent.isOnNavMesh)
        {
            return;
        }


        // ======================================
        // 重新計算路徑
        // ======================================

        // 清除上一次計算結果。
        calculatedPath.ClearCorners();


        // 計算玩家目前位置到點擊位置的路徑。
        bool foundPath =
            agent.CalculatePath(
                requestedPosition,
                calculatedPath
            );

        // 顯示這次實際使用的路徑狀態。
        Debug.Log(
            $"尋路結果：{foundPath}｜狀態：{calculatedPath.status}｜" +
            $"玩家：{agent.transform.position}｜點擊：{requestedPosition}"
        );

        // 在 Scene 視窗畫出計算後的路徑，持續顯示 5 秒。
        Vector3[] debugCorners = calculatedPath.corners;

        for (int i = 1; i < debugCorners.Length; i++)
        {
            Debug.DrawLine(
                debugCorners[i - 1] + Vector3.up * 0.2f,
                debugCorners[i] + Vector3.up * 0.2f,
                Color.red,
                5f
            );
        }

        // 如果完全找不到任何路徑。
        // 原始點擊位置無法尋路時，
        // 沿著點擊位置往玩家方向，尋找可抵達的位置。
        if (!foundPath ||
            calculatedPath.status == NavMeshPathStatus.PathInvalid)
        {
            TryMoveCloserAlongLine(requestedPosition);
            return;
        }


        // ======================================
        // 完整路徑
        // ======================================

        if (
            calculatedPath.status ==
            NavMeshPathStatus.PathComplete
        )
        {
            // 代表這個位置真的可以抵達。
            //
            // 直接讓 NavMeshAgent 自己尋路。
            // 有牆壁、轉角時也會自己繞過去。
            TrySetActualDestination(
                requestedPosition
            );


            return;
        }


        // ======================================
        // 部分路徑
        // ======================================

        if (
            calculatedPath.status ==
            NavMeshPathStatus.PathPartial
        )
        {
            // 取得這條 Partial Path 的所有轉折點。
            Vector3[] corners =
                calculatedPath.corners;


            // 完全沒有 corner 就不移動。
            if (corners.Length == 0)
            {
                return;
            }


            // ==================================
            // 找到 NavMesh 邊界位置
            // ==================================

            // Partial Path 最後一個 corner
            // 就是目前能走到的最遠位置。
            Vector3 edgePosition =
                corners[corners.Length - 1];


            // ==================================
            // 計算進入邊界的方向
            // ==================================

            Vector3 pathDirection;


            // 如果至少有兩個 corner。
            if (corners.Length >= 2)
            {
                // 使用最後一段路徑的方向。
                pathDirection =
                    edgePosition -
                    corners[corners.Length - 2];
            }
            else
            {
                // 如果只有一個 corner，
                // 就使用玩家目前位置到邊界的方向。
                pathDirection =
                    edgePosition -
                    transform.position;
            }


            // 不考慮高度。
            pathDirection.y = 0f;


            // 如果方向有效。
            if (pathDirection.sqrMagnitude > 0.001f)
            {
                // 正規化方向。
                pathDirection.Normalize();
            }


            // ==================================
            // 往 NavMesh 內側退一小段
            // ==================================

            // Agent 的 Radius 代表角色在導航時
            // 本身需要占用的水平空間。
            //
            // 如果直接停在 polygon 最外緣，
            // 模型或 Agent 很容易看起來跨出 Bake。
            float safeDistance =
                agent.radius +
                edgeStopPadding;


            // 從真正的 NavMesh 邊界，
            // 沿著路徑反方向退回去。
            Vector3 safeEdgePosition =
                edgePosition -
                pathDirection *
                safeDistance;

            Debug.Log(
                $"有執行內縮｜邊界：{edgePosition}｜" +
                $"內縮目的地：{safeEdgePosition}｜內縮距離：{safeDistance}"
            );

            // 將安全位置設成真正目的地。
            TrySetActualDestination(
                safeEdgePosition
            );


            return;
        }


        // ======================================
        // 無效路徑
        // ======================================

        // PathInvalid 時完全不移動。
        return;
    }

    // ==========================================
    // 系統指定移動：只接受完整路徑
    // ==========================================

    public bool TrySetSystemTargetPosition(Vector3 destination)
    {
        if (!canMove ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return false;
        }

        // 不使用定位空物件的高度。
        destination.y = agent.transform.position.y;

        calculatedPath.ClearCorners();

        if (!agent.CalculatePath(destination, calculatedPath) ||
            calculatedPath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        Vector3[] corners = calculatedPath.corners;

        if (corners.Length == 0)
        {
            return false;
        }

        // 避免目標離 NavMesh 太遠，
        // Unity 對應到附近其他位置卻被當成成功。
        Vector3 pathEnd = corners[corners.Length - 1];

        if (GetPlanarSqrDistance(pathEnd, destination) > 0.15f * 0.15f)
        {
            return false;
        }

        agent.isStopped = false;

        // 直接使用剛剛驗證過的路徑。
        if (!agent.SetPath(calculatedPath))
        {
            return false;
        }

        actualDestination = pathEnd;
        lastActualDestination = pathEnd;
        hasLastActualDestination = true;
        hasDestination = true;

        return true;
    }

    // ==========================================
    // 嘗試移動到完整可達路徑
    // ==========================================

    private bool TryMoveToCompletePath(
        Vector3 destination)
    {
        // 清除之前的路徑資料。
        calculatedPath.ClearCorners();


        // 計算前往這個位置的路徑。
        bool foundPath =
            agent.CalculatePath(
                destination,
                calculatedPath
            );


        // 如果連路徑都沒有找到。
        if (!foundPath)
        {
            return false;
        }


        // 只有完整路徑才接受。
        if (
            calculatedPath.status !=
            NavMeshPathStatus.PathComplete
        )
        {
            return false;
        }


        // 設定真正目的地。
        TrySetActualDestination(
            destination
        );


        return true;
    }


    // ==========================================
    // 真正設定 NavMeshAgent 的目的地
    // ==========================================

    private void TrySetActualDestination(
        Vector3 newDestination)
    {
        // ======================================
        // 判斷是否和上一個目的地太接近
        // ======================================

        if (hasLastActualDestination)
        {
            // 使用 XZ 平面距離。
            //
            // 不比較 Y，
            // 避免 NavMesh 高度細微差異
            // 被誤判成新的目的地。
            float destinationDifference =
                GetPlanarSqrDistance(
                    newDestination,
                    lastActualDestination
                );


            float updateThresholdSqr =
                destinationUpdateThreshold *
                destinationUpdateThreshold;


            // 如果新舊目的地幾乎完全相同。
            if (
                destinationDifference <=
                updateThresholdSqr
            )
            {
                // ==================================
                // 情況 A：
                // Agent 本來就正在前往這裡
                // ==================================

                if (hasDestination)
                {
                    // 不重新 SetDestination。
                    return;
                }


                // ==================================
                // 情況 B：
                // Agent 已經停在這個位置附近
                // ==================================

                float distanceFromPlayer =
                    GetPlanarSqrDistance(
                        transform.position,
                        newDestination
                    );


                float stopDistance =
                    agent.stoppingDistance +
                    arrivalTolerance +
                    destinationUpdateThreshold;


                // 如果玩家已經非常靠近這個點。
                if (
                    distanceFromPlayer <=
                    stopDistance *
                    stopDistance
                )
                {
                    // 不重新啟動 Agent。
                    return;
                }
            }
        }


        // ======================================
        // 這是一個真正不同的新目的地
        // ======================================

        actualDestination =
            newDestination;


        // 保存為上一次實際目的地。
        lastActualDestination =
            newDestination;


        // 標記我們已經有歷史目的地。
        hasLastActualDestination =
            true;


        // 標記目前正在前往目的地。
        hasDestination =
            true;


        // 確保 Agent 可以移動。
        agent.isStopped =
            false;


        // 正式設定目的地。
        agent.SetDestination(
            actualDestination
        );
    }


    // ==========================================
    // 判斷 Agent 是否真的還在移動
    // ==========================================

    private bool IsAgentActuallyMoving()
    {
        // 將速度門檻轉成平方，
        // 避免每幀進行平方根計算。
        float thresholdSqr =
            walkSpeedThreshold *
            walkSpeedThreshold;


        // 只有真正速度大於門檻，
        // 才算正在走。
        return
            agent.velocity.sqrMagnitude >
            thresholdSqr;
    }


    // ==========================================
    // 計算 XZ 平面的距離平方
    // ==========================================

    private float GetPlanarSqrDistance(
        Vector3 pointA,
        Vector3 pointB)
    {
        // 計算兩點差距。
        Vector3 difference =
            pointA -
            pointB;


        // 不計算高度差。
        difference.y = 0f;


        // 回傳平方距離。
        return difference.sqrMagnitude;
    }


    // ==========================================
    // 開啟／關閉玩家移動
    // ==========================================

    public void SetMovementEnabled(
        bool enabled)
    {
        // 更新玩家移動權限。
        canMove =
            enabled;


        // Agent 不存在。
        if (agent == null)
        {
            return;
        }


        // Agent 沒啟用。
        if (!agent.enabled)
        {
            return;
        }


        // Agent 不在 NavMesh。
        if (!agent.isOnNavMesh)
        {
            SetWalking(false);
            return;
        }


        // 禁止玩家移動。
        if (!enabled)
        {
            StopMoving();
        }
        else
        {
            // 重新允許 Agent。
            agent.isStopped =
                false;
        }
    }


    // ==========================================
    // 立即停止玩家
    // ==========================================

    public void StopMoving()
    {
        // 如果 Agent 可以正常操作。
        if (
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh
        )
        {
            // 清除導航路徑。
            agent.ResetPath();


            // 停止 Agent。
            agent.isStopped =
                true;
        }


        // 清除目前目的地。
        hasDestination =
            false;


        // 清除上一次目的地紀錄。
        //
        // 下次重新開始移動時，
        // 即使點相同位置也允許重新導航。
        hasLastActualDestination =
            false;


        // 回到 Idle。
        SetWalking(false);
    }


    // ==========================================
    // 更新 Walk / Idle
    // ==========================================

    private void SetWalking(
        bool isWalking)
    {
        // Animator 不存在。
        if (animator == null)
        {
            return;
        }


        // 更新 Animator 中的 walk Bool。
        animator.SetBool(
            "walk",
            isWalking
        );
    }

    // ==========================================
    // 沿玩家與點擊位置的直線，尋找可達目的地
    // ==========================================

    private bool TryMoveCloserAlongLine(Vector3 requestedPosition)
    {
        Vector3 startPosition = agent.nextPosition;

        float planarDistance = Mathf.Sqrt(
            GetPlanarSqrDistance(startPosition, requestedPosition)
        );

        if (planarDistance <= 0.01f)
        {
            return false;
        }

        // 一般每隔約 0.1 單位檢查一次。
        // 遠距離時最多檢查 256 段，限制單次搜尋成本。
        const float searchStep = 0.1f;
        const int maxSegments = 256;

        int segments = Mathf.Clamp(
            Mathf.CeilToInt(planarDistance / searchStep),
            1,
            maxSegments
        );

        // 只搜尋候選點附近的小範圍 NavMesh。
        const float sampleRadius = 0.3f;

        // 使用與玩家相同的 Agent 類型及可行走區域。
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        // 從點擊位置開始，逐步退向玩家。
        for (int i = segments; i >= 1; i--)
        {
            float t = (float)i / segments;

            Vector3 candidatePosition = Vector3.Lerp(
                startPosition,
                requestedPosition,
                t
            );

            // 先將候選位置對應到附近的 NavMesh。
            if (!NavMesh.SamplePosition(
                    candidatePosition,
                    out NavMeshHit hit,
                    sampleRadius,
                    filter))
            {
                continue;
            }

            // 限制水平偏移，避免搜尋點明顯偏離原本直線。
            if (GetPlanarSqrDistance(
                    candidatePosition,
                    hit.position) > searchStep * searchStep)
            {
                continue;
            }

            // 避免選到玩家腳下，造成幾乎沒有移動。
            float minimumMoveDistance =
                agent.stoppingDistance + arrivalTolerance;

            if (GetPlanarSqrDistance(
                    startPosition,
                    hit.position) <=
                minimumMoveDistance * minimumMoveDistance)
            {
                continue;
            }

            // 找到 NavMesh 不代表玩家一定走得到。
            // 必須再確認完整路徑。
            if (!TryMoveToCompletePath(hit.position))
            {
                continue;
            }

            Debug.DrawLine(
                startPosition + Vector3.up * 0.2f,
                hit.position + Vector3.up * 0.2f,
                Color.green,
                2f
            );

            Debug.Log(
                $"改走直線上的可達位置｜" +
                $"原始點擊：{requestedPosition}｜" +
                $"替代目的地：{hit.position}"
            );

            return true;
        }

        // 沒有找到可向前移動的替代位置。
        return false;
    }
}