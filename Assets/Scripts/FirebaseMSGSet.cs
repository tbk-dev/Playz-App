using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Messaging;

public class FirebaseMSGSet : MonoBehaviour
{
    Firebase.FirebaseApp app;

    protected bool isFirebaseInitialized = false;
    public bool IsFirebaseInitialized { get { return isFirebaseInitialized; } }

    public string defaultTopic = "playznoti";
    public string userToken;

    public Firebase.DependencyStatus dependencyStatus { get; private set; } = Firebase.DependencyStatus.UnavailableOther;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start()
    {
        CheckAndFixDependencies();
    }

    public void CheckAndFixDependencies()
    {
        Debugging.instance.Loglate("Start Firebase Messaging CheckAndFixDependencies");

        //Firebase.FirebeApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            //Debugging.instance.Loglate($"Current dependency status: {dependencyStatus.ToString()});
            
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                ToggleTokenOnInit();

                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();
            }
            else
            {
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Debugging.instance.Loglate("error Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Setup message event handlers.
    void InitializeFirebase()
    {
        try
        {
            Debugging.instance.Loglate($"Start Firebase Messaging Initialized");


            SubscribeTopic(defaultTopic);

            // This will display the prompt to request permission to receive
            // notifications if the prompt has not already been displayed before. (If
            // the user already responded to the prompt, thier decision is cached by
            // the OS and can be changed in the OS settings).
            Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
              task =>
              {
                  LogTaskCompletion(task, "RequestPermissionAsync");
              }
            );

            Debugging.instance.Loglate("Firebase init complite");
            isFirebaseInitialized = true;
        }
        catch (Exception ex)
        {
            Debugging.instance.Loglate($"Firebase init complite ex : {ex.Message}");

        }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            Debugging.instance.Loglate(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            Debugging.instance.Loglate(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string errorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    errorCode = $"Error.{((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString()}: ";
                }
                Debugging.instance.Loglate(errorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            Debugging.instance.Loglate(operation + " completed");
            complete = true;
        }
        return complete;
    }


    public void SubscribeTopic(string topic)
    {
        bool isAction = false;
        Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(
          task =>
          {
              isAction = LogTaskCompletion(task, "SubscribeAsync");

          }
        ).Wait();


        if (isAction)
        {
            Debugging.instance.Loglate("Subscribed to " + topic);
        }
        else
        {
            Debugging.instance.Loglate(" !!! fail Subscribed to " + topic);
        }
    }

    public void UnSubscribeTopic(string topic)
    {
        Debug.Log($"Start  Firebase Messaging UnSubscribeTopic");
        bool isAction = false;
        
        Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync(topic).ContinueWithOnMainThread(
          task =>
          {
              isAction = LogTaskCompletion(task, "UnsubscribeAsync");

          }
        );

        if (isAction)
        {
            Debugging.instance.Loglate("Unsubscribed from " + topic);
        }
        else
        {
            Debugging.instance.Loglate("fail Unsubscribed from " + topic);
        }
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        Debugging.instance.Loglate("OnMessageReceived_Debugging new message");
        var notification = e.Message.Notification;
        if (notification != null)
        {
            Debugging.instance.Loglate($"Received Title: {notification.Title}");
            Debugging.instance.Loglate($"Received Body: {notification.Body}");

            if (notification.Android != null)
            {
                Debugging.instance.Loglate($"Received ChannelId: {notification.Android?.ChannelId}");
            }

        }
        else
        {
            Debugging.instance.Loglate($"notification is null reciveMessage : {e.Message}");
        }

        try
        {
            if (e.Message.From.Length > 0)
                Debugging.instance.Loglate("from: " + e.Message.From);


            if (e.Message.Link != null)
            {
                Debugging.instance.Loglate("link: " + e.Message.Link.ToString());
            }

            if (e.Message.Data.Count > 0)
            {
                Debugging.instance.Loglate("data:");
                foreach (KeyValuePair<string, string> iter in e.Message.Data)
                {
                    Debugging.instance.Loglate("  " + iter.Key + ": " + iter.Value);
                }
            }

        }
        catch (Exception ex)
        {
            Debugging.instance.Loglate($"OnMessageReceived = {ex.Message}");
        }

    }


    public void ToggleTokenOnInit()
    {
        Debug.Log($"run ToggleTokenOnInit...");
        bool newValue = true;//!FirebaseMessaging.TokenRegistrationOnInitEnabled;
        FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
        Debugging.instance.Loglate("Set TokenRegistrationOnInitEnabled to " + newValue);

        if(newValue)
        {
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;

            SubscribeTopic(defaultTopic);
        }
        else
        {
            Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
            Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;

            UnSubscribeTopic(defaultTopic);
        }

    }

    //public void GetToken()
    //{
    //    Firebase.Auth.FirebaseUser user = auth.CurrentUser;
    //    user.TokenAsync(true).ContinueWith(task => {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogError("TokenAsync was canceled.");
    //            return;
    //        }

    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError("TokenAsync encountered an error: " + task.Exception);
    //            return;
    //        }

    //        string idToken = task.Result;

    //        // Send token to your backend via HTTPS
    //        // ...
    //    });
    //}


    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        userToken = token.Token;
        Debugging.instance.SaveToken($"{token.Token}");
    }

    Firebase.DependencyStatus lastStatus;
    public void LogDependencyStatus()
    {
        Debugging.instance.DebugLog("dependency status: " + dependencyStatus.ToString());
        //while (true)
        {
            if (lastStatus != dependencyStatus)
            {
                //DebugLog("One or more Firebase dependencies are not present.");
                //DebugLog("Current dependency status: " + firebaseMSGSet.dependencyStatus.ToString());

                lastStatus = dependencyStatus;
                Debugging.instance.DebugLog("change dependency status: " + dependencyStatus.ToString());
            }
            //yield return new WaitForSeconds(1);
        }
    }


}
