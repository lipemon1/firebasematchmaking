using System.Collections.Generic;
using System.Linq;
using Firebase_Unity.RealtimeDatabase;
using Matchmaking_Example.Scripts.Model;
using UnityEngine;

namespace Matchmaking_Example.Scripts
{
    public class ConnectionMaker : MonoBehaviour
    {
        [Header("Information to Create Match")]
        public string CurrentUserId;
        public string UserIdToInvite;

        [Header("Information To Accept Match")]
        public int MatchIndexToAnswer;
        public Status NewStatus;
        
        [Header("Invites Lists")]
        public List<Invite> CreatedInvites = new List<Invite>();
        public List<Invite> ReceivedInvite = new List<Invite>();

        [ContextMenu("Start to Listen Matches")]
        public void ListenToMatches()
        {
            RealtimeDatabaseController.Instance.AddListListener<Invite>($"matches/{CurrentUserId}", OnMatchesChanged);
        }

        private void OnMatchesChanged(List<KeyValuePair<string, Invite>> keyValuePairs)
        {
            var listReceived = new List<Invite>();
            
            foreach (var keyValuePair in keyValuePairs)
            {
                var newInvite = keyValuePair.Value;
                newInvite.id = keyValuePair.Key;
                listReceived.Add(newInvite);
            }

            var possibleNewInvites = listReceived?.Where(i => i.invitedUserId == CurrentUserId)?.ToList();

            if (possibleNewInvites?.Count > 0)
                ReceivedInvite = possibleNewInvites;
        }

        [ContextMenu("Accept Invite")]
        public void AnswerMatch()
        {
            var newInvite = ReceivedInvite[MatchIndexToAnswer];
            newInvite.status = NewStatus;
            
            var newDictionary = new Dictionary<string, object>();
            var path = $"match/{CurrentUserId}/{newInvite.id}/status";
            newDictionary.Add(path, newInvite.status);
            
            RealtimeDatabaseController.WriteValueOnPath(newDictionary, () => Debug.Log("Updated"));
        }

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
            
            RealtimeDatabaseController.WriteObjectByPathCreatingKey($"matches/{newInvite.invitedUserId}", newInvite, OnMatchCreated);
        }

        private void OnMatchCreated(Invite invite, string inviteId)
        {
            invite.id = inviteId;
            
            CreatedInvites.Add(invite);
            
            RealtimeDatabaseController.Instance.AddListener<Invite>($"matches/{inviteId}", OnMatcheChanged);
        }

        private void OnMatcheChanged(Invite inviteChanged, string inviteId)
        {
            inviteChanged.id = inviteId;
            
            Debug.Log("Matche " + inviteChanged?.id + " changed");
            
            var possibleInvite = CreatedInvites?.FirstOrDefault(i => i?.id == inviteId);

            if (possibleInvite != null)
            {
                CreatedInvites.Remove(possibleInvite);
                CreatedInvites.Add(inviteChanged);
            }
        }
    }
}
