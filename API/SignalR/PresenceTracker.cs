namespace API.SignalR;

public class PresenceTracker
{
    private readonly Dictionary<string, List<string>> OnlineUsers = new ();

    public Task UserConnected(string username, string connectionId)
    {
        lock(OnlineUsers)
        {
            if (!OnlineUsers.TryGetValue(username, out var connectionIds))
            {
                connectionIds = new List<string>();
                OnlineUsers[username] = connectionIds;
            }

            connectionIds.Add(connectionId);
        }

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string username, string connectionId)
    {
        lock(OnlineUsers)
        {
            if (!OnlineUsers.TryGetValue(username, out var connectionIds)) return Task.CompletedTask;

            connectionIds.Remove(connectionId);
            if (connectionIds.Count == 0)
            {
                OnlineUsers.Remove(username);
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] usernames;
        lock(OnlineUsers)
        {
            usernames = OnlineUsers.Keys.ToArray();
        }

        Array.Sort(usernames);
        return Task.FromResult(usernames);
    }
}
