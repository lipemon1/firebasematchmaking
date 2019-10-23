using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Firebase.Unity.Editor;
using Newtonsoft.Json;
using UnityEngine;

namespace Firebase_Unity.RealtimeDatabase
{
    public class RealtimeDatabaseController : SingletonMonoBehaviour<RealtimeDatabaseController>
    {
        private static bool _isConfigured;
        private static DatabaseReference _currentDatabase;
        private static FirebaseDatabase _currentDatabaseDefaultInstance;
        
        //Delegates
        private Action<string> _onDataChanged; 
        
        public void PrepareDatabaseUrl(string urlDatabase, Action onFinishPrepare)
        {
            if (FirebaseUnityController.IsConfigured() == false)
            {
                FirebaseUnityController.PrepareController(() => PrepareDatabaseUrl(urlDatabase, onFinishPrepare));
                return;
            }
                
            FirebaseUnityController.CurrentApp().SetEditorDatabaseUrl(urlDatabase);
            
            // Get the root reference location of the database.
            _currentDatabaseDefaultInstance = FirebaseDatabase.DefaultInstance;
            _currentDatabase = FirebaseDatabase.DefaultInstance.RootReference;

            _isConfigured = true;
            
            _currentDatabaseDefaultInstance.SetPersistenceEnabled(true);
            
            onFinishPrepare?.Invoke();
        }

        #region Download Data

        public static void GetObject<T>(string objectName, string path, Action<T, string> onLoadComplete)
        {
            var pahtValue = objectName + "/" + path;
            GetObjectByPath(pahtValue, onLoadComplete);
        }

        private static void GetObjectByPath<T>(string path, Action<T, string> onDataLoaded)
        {
            _currentDatabaseDefaultInstance
                .GetReference(path)
                .GetValueAsync().ContinueWith(task => {
                    if (task.IsFaulted) {
                        Debug.LogError(task.Exception?.Message);
                    }
                    else if (task.IsCompleted)
                    {
                        var snapshot = task.Result;
                        onDataLoaded?.Invoke(JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue()), snapshot.Key);
                    }
                });
        }

        #endregion

        #region Upload Data

        /// <summary>
        /// This guy will run into you PATH property and at the end off the path will create a new object with a new
        /// KEY generated automatically. After that will upload you data inside this new key
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dataToUpload"></param>
        /// <param name="onLoadComplete"></param>
        /// <typeparam name="T"></typeparam>
        public static void WriteObjectByPathCreatingKey<T>(string path, T dataToUpload, Action<T, string> onLoadComplete)
        {
            var keyToUse = _currentDatabase.Root.Child(path).Push().Key;
            _currentDatabase.Child(path).Child(keyToUse)
                .SetRawJsonValueAsync(JsonConvert.SerializeObject(dataToUpload)).ContinueWith(task => {
                    if (task.IsFaulted) {
                        Debug.LogError(task.Exception?.Message);
                    }
                    else if (task.IsCompleted)
                    {
                        onLoadComplete?.Invoke(dataToUpload, keyToUse);
                    }
                });
        }
        
        /// <summary>
        /// This guy will push a reference key (Dictionary<string, object> to a exist object)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="referenceData"></param>
        /// <param name="data"></param>
        /// <param name="onLoadComplete"></param>
        /// <typeparam name="T"></typeparam>
        public static void WriteObjectReferenceToPath<T>(string path, string key, IDictionary<string, object> referenceData, T data, Action<T, string> onLoadComplete)
        {
            _currentDatabase.Child(path).UpdateChildrenAsync(referenceData).ContinueWith(task => {
                    if (task.IsFaulted) {
                        Debug.LogError(task.Exception?.Message);
                    }
                    else if (task.IsCompleted)
                    {
                        onLoadComplete?.Invoke(data, key);
                    }
            });
        }

        public static void WriteValueOnPath(IDictionary<string, object> updates, Action callback)
        {
            _currentDatabase.UpdateChildrenAsync(updates).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception?.Message);
                }
                else if (task.IsCompleted)
                {
                    callback?.Invoke();
                }
            });
        }

        #endregion

        #region Listeners

        private void OnDataChanged<T>(object sender, ValueChangedEventArgs args, Action<T, string> action) {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
            }
            else
            {
                if (args.Snapshot.Value == null)
                {
                    Debug.LogError("There is no data with key: " + args.Snapshot.Key);
                }
                else
                {
                    var snapshot = args.Snapshot;
                    var dataDeserialized = JsonConvert.DeserializeObject<T>(snapshot.GetRawJsonValue());
                    action?.Invoke(dataDeserialized, snapshot.Key);   
                }
            }
        }
        
        private void OnListDataChanged<T>(object sender, ValueChangedEventArgs args, Action<List<KeyValuePair<string, T>>> listAction)
        {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
            }
            else
            {
                if (args.Snapshot.Value == null)
                {
                    Debug.LogError("There is no data with key: " + args.Snapshot.Key);
                }
                else
                {
                    var snapshot = args.Snapshot;
                    var dataDeserialized = JsonConvert.DeserializeObject<Dictionary<string, T>>(snapshot.GetRawJsonValue());
                    listAction?.Invoke(dataDeserialized?.ToList());   
                }
            }
        }

        #region CoreListeners

        public void AddListener<T>(string referenceToListen, Action<T, string> callback)
        {
            void Listener(object sender, ValueChangedEventArgs args)
            {
                OnDataChanged(sender, args, callback);
            }

            AddListener(referenceToListen, Listener);
        }

        private static void AddListener(string referenceToListen, EventHandler<ValueChangedEventArgs> newListener)
        {
            if (_isConfigured == false)
            {
                Debug.LogError("You are trying to add listener but your database is not ready yet");
                return;
            }
            
            _currentDatabaseDefaultInstance.GetReference(referenceToListen).ValueChanged += newListener;
        }

        public void AddListListener<T>(string referenceToListen, Action<List<KeyValuePair<string, T>>> callback)
        {
            void Listener(object sender, ValueChangedEventArgs args)
            {
                OnListDataChanged<T>(sender, args, callback);
            }

            AddListener(referenceToListen, Listener);
        }

        #endregion

        #endregion
    }
}
