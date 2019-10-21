using Firebase_Unity.RealtimeDatabase;
using UnityEngine;

namespace Matchmaking_Example.Scripts
{
    public class InitializeDatabase : MonoBehaviour
    {
        public string DatabaseUrl;

        private void Start()
        {
            RealtimeDatabaseController.Instance.PrepareDatabaseUrl(DatabaseUrl, () => Debug.Log("Database is ready"));
        }
    }
}
