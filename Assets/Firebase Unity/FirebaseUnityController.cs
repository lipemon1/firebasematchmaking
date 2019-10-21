using Firebase;
using UnityEngine;

namespace Firebase_Unity
{
    public class FirebaseUnityController : SingletonMonoBehaviour<FirebaseUnityController>
    {
        private static bool _isConfigured;
        private static FirebaseApp _currentApp;

        public static void PrepareController(System.Action onPrepareCallback)
        {
            // Initialize Firebase
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    // Crashlytics will use the DefaultInstance, as well;
                    // this ensures that Crashlytics is initialized.
                    _currentApp = FirebaseApp.DefaultInstance;
                    _isConfigured = true;
                    
                    onPrepareCallback?.Invoke();
                }
                else
                {
                    Debug.LogError(System.String.Format(
                        "Could not resolve all Firebase dependencies: {0}",dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });
        }

        public static FirebaseApp CurrentApp()
        {
            if (_isConfigured)
                return _currentApp;
            else
            {
                Debug.LogError("The app is not configured so far");
                return null;
            }
        }

        public static bool IsConfigured()
        {
            return _isConfigured;
        }
    } 
}
