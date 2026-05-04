using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostGameNotification : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
public class PushNotificationData
{
    public const int D7InactivityHours = 168;
    public const int D14InactivityHours = 336;
    public const int D30InactivityHours = 720;

    public string title;
    public string message;
    public int timeInHour;

    public PushNotificationData(string title, string message, int timeInHour)
    {
        this.title = title;
        this.message = message;
        this.timeInHour = timeInHour;
    }

    public static PushNotificationData GetDefaultPushNotificationData()
        => new PushNotificationData("We miss you!", "Come back and play the game!", D7InactivityHours);
}