using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNotificationChannels : MonoBehaviour {

    const string pluginName = "com.digistorm.notification_channel_controller.NotificationChannelCreator";
    const string unityPlayerName = "com.unity3d.player.UnityPlayer";

    private void Start()
    {
        //Check to make sure we are running on
        if (Application.platform == RuntimePlatform.Android)
        {
            //Find the main Unity player class
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass(unityPlayerName);
            if (unityPlayerClass == null)
            {
                Debug.LogError("Could not find Unity player class named: " + unityPlayerName);
                return;
            }

            //Get the current context of the main Unity player class
            AndroidJavaObject unityPlayerContext = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            if (unityPlayerContext == null)
            {
                Debug.LogError("Could not get the current context of the Unity player class: " + unityPlayerName);
                return;
            }

            //Find the Android notification class
            AndroidJavaClass androidNotificationClass = new AndroidJavaClass(pluginName);
            if (androidNotificationClass == null)
            {
                Debug.LogError("Could not find plugin class named: " + pluginName);
                return;
            }

            //Get the singleton instance of the Android notification class
            AndroidJavaObject pluginObject = androidNotificationClass.CallStatic<AndroidJavaObject>("getInstance");
            if (pluginObject == null)
            {
                Debug.LogError("Could not get instance of the plugin: " + pluginName);
                return;
            }

            /* Call the createNotificationChannels() function on the singleton instance of the
             * Android notification class and pass in the application's current context.
             * This will create the notification channels for our application.
             */
            pluginObject.Call("createNotificationChannels", unityPlayerContext);
        }
    }
}
