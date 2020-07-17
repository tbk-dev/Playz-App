using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseMSGSet : MonoBehaviour
{
    Firebase.FirebaseApp app;
    public ToggleController BtnReciveAgree, OnSound, OnVibration;

    protected bool isFirebaseInitialized = false;
    public bool IsFirebaseInitialized { get { return isFirebaseInitialized; } }

    public string topic = "TestTopic";

    public Firebase.DependencyStatus dependencyStatus { get; private set; } = Firebase.DependencyStatus.UnavailableOther;

    public void CheckAndFixDependencies()
    {
        DebugLog("CheckAndFixDependencies");
        //Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();
            }
            else
            {
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                DebugLog("error Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string errorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    errorCode = $"Error.{((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString()}: ";
                }
                DebugLog(errorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start()
    {
        CheckAndFixDependencies();
    }

    // Setup message event handlers.
    void InitializeFirebase()
    {
        DebugLog($"Start Firebase Messaging Initialized");
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;

        SubscribeTopic(topic);

        // This will display the prompt to request permission to receive
        // notifications if the prompt has not already been displayed before. (If
        // the user already responded to the prompt, thier decision is cached by
        // the OS and can be changed in the OS settings).
        Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
          task =>
          {
              LogTaskCompletion(task, "RequestPermissionAsync");
              DebugLog("firebase init RequestPermissionAsync");
          }
        );

        DebugLog("Firebase init complite");
        isFirebaseInitialized = true;
    }

    public void SubscribeTopic(string topic)
    {

        Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(
          task =>
          {
              bool isAction = LogTaskCompletion(task, "SubscribeAsync");

              if (isAction)
              {
                  DebugLog("Subscribed to " + topic);
              }
              else
              {
                  DebugLog(" !!! fail Subscribed to " + topic);
              }
          }
        );
    }

    public void Noti()
    {

    }
    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        try
        {
            DebugLog($"Received Title: {e.Message.Notification?.Title}");
            DebugLog($"Received Body: {e.Message.Notification?.Body}");
            DebugLog($"Received ChannelId: {e.Message.Notification.Android?.ChannelId}");

            if (OnVibration)
            {
                DebugLog("  -- onVibration");
                Handheld.Vibrate();
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"OnMessageReceived = {ex.Message}");
        }

    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        DebugLog("Received Registration Token: " + token.Token);
        Debugging.instance.SaveToken($"{token.Token}");
    }


    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // End our messaging session when the program exits.
    public void OnDestroy()
    {
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        try
        {
            Debugging.Instance.DebugLog(s);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

}
