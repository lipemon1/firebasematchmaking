namespace Matchmaking_Example.Scripts.Model
{
    [System.Serializable]
    public class Invite
    {
        public string id;
        public string invitorUserId;
        public string invitedUserId;
        public Status status;
    }

    public enum Status
    {
        Unknown = 0,
        Invited = 1,
        WaitingAnswer = 2,
        Denied = 3,
        Accept = 4,
        Timeout = 5
    }
}
