using UnityEngine;

public static class InteractionDebug
{
    public static void LogDistance(
        string interactionName,
        Transform player,
        Transform target,
        float allowedDistance)
    {
        if (player == null || target == null)
        {
            Debug.LogWarning(
                $"互動距離｜{interactionName}｜玩家或目標尚未指定。"
            );

            return;
        }

        Vector3 difference = player.position - target.position;

        // 包含高度的距離。
        float distance3D = difference.magnitude;

        // 不包含高度的水平距離。
        difference.y = 0f;
        float distanceXZ = difference.magnitude;

        Debug.Log(
            $"互動距離｜{interactionName}｜" +
            $"玩家物件：{player.name}｜" +
            $"目標物件：{target.name}｜" +
            $"玩家座標：{player.position}｜" +
            $"目標座標：{target.position}｜" +
            $"直線距離：{distance3D:F2}｜" +
            $"水平距離：{distanceXZ:F2}｜" +
            $"允許距離：{allowedDistance:F2}",
            target.gameObject
        );
    }
}