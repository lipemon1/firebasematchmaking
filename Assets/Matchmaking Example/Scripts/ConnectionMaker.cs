using System.Collections.Generic;
using Firebase_Unity.RealtimeDatabase;
using Matchmaking_Example.Scripts.Model;
using UnityEngine;

namespace Matchmaking_Example.Scripts
{
    public class ConnectionMaker : MonoBehaviour
    {
        public string CurrentUserId;
        public string UserIdToInvite;
        public float InviteTimeout;

        public List<Invite> CreatedInvites = new List<Invite>();

        [ContextMenu("Start to Listen Matches")]
//        public void ListenToMatches()
//        {
//            RealtimeDatabaseController.Instance.AddListener("matches", OnMatchesChanged);
//        }
//
//        private void OnMatchesChanged(List<Invite, string> matches)
//        {
//            
//        }
        

        [ContextMenu("Create Invite")]
        public void CreateInvite()
        {
            var newInvite = new Invite()
            {
                id = null,
                status = Status.Invited,
                invitorUserId = CurrentUserId,
                invitedUserId = UserIdToInvite
            };
            
            RealtimeDatabaseController.WriteObjectByPathCreatingKey("matches", newInvite, OnMatchCreated);
        }

        private void OnMatchCreated(Invite invite, string inviteId)
        {
            invite.id = inviteId;
            
            CreatedInvites.Add(invite);
        }
    }
}
