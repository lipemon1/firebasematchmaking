using System;
using Firebase_Unity.RealtimeDatabase;
using Matchmaking_Example.Scripts.Model;
using UnityEngine;

namespace Matchmaking_Example.Scripts.Behaviours
{
    public class CreateUser : MonoBehaviour
    {
        public User UserToCreate;

        public User LastUserCreated;

        [ContextMenu("Create new user")]
        public void CreateNewUser()
        {
            UserToCreate.id = null;
            RealtimeDatabaseController.WriteObjectByPathCreatingKey("users", UserToCreate, OnUserCreated);   
        }

        private void OnUserCreated(User user, string userId)
        {
            user.id = userId;

            var msg = "User Created \n";
            msg += "Id: " + user.id + "\n";
            msg += "Name: " + user.name + "\n";
            msg += "Email: " + user.email + "\n";
            
            Debug.Log(msg);

            LastUserCreated = user;
        }
    }
}
