using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class AnalyticsReporter : MonoBehaviour
{
    public static void ReportLevelCompleted()
    {
        Analytics.CustomEvent("level_completed", new Dictionary<string, object>
        {
            { "scene_index", SceneManager.GetActiveScene().buildIndex },
            { "scene_name", SceneManager.GetActiveScene().name },
            { "time_elapsed", Time.timeSinceLevelLoad }
        });
    }

    public static void ReportPlayerDied(Vector3 position)
    {
        Analytics.CustomEvent("player_died", new Dictionary<string, object>
        {
            { "scene_index", SceneManager.GetActiveScene().buildIndex },
            { "scene_name", SceneManager.GetActiveScene().name },
            { "time_elapsed", Time.timeSinceLevelLoad },
            { "death_position", position}
        });
    }

    public static void ReportPlayerReset(Vector3 position)
    {
        Analytics.CustomEvent("player_died", new Dictionary<string, object>
        {
            { "scene_index", SceneManager.GetActiveScene().buildIndex },
            { "scene_name", SceneManager.GetActiveScene().name },
            { "time_elapsed", Time.timeSinceLevelLoad },
            { "reset_position", position}
        });
    }

    public static void ReportPickup(Vector3 position, string pickupName)
    {
        Analytics.CustomEvent("pickup_collected", new Dictionary<string, object>
        {
            { "pickup_name", pickupName },
            { "scene_index", SceneManager.GetActiveScene().buildIndex },
            { "scene_name", SceneManager.GetActiveScene().name },
            { "time_elapsed", Time.timeSinceLevelLoad },
            { "reset_position", position}
        });
    }
}
