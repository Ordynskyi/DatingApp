namespace API.SignalR;

public class PresenceTracker
{
    private readonly Dictionary<string, List<string>> onlineUsers = new ();

    public Task<bool> UserConnected(string username, string connectionId)
    {
        var isOnline = false;
        lock(onlineUsers)
        {
            if (!onlineUsers.TryGetValue(username, out var connectionIds))
            {
                connectionIds = new List<string>();
                onlineUsers[username] = connectionIds; 
                isOnline = true;
            }

            connectionIds.Add(connectionId);
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        var isOffline = false;
        lock(onlineUsers)
        {
            if (!onlineUsers.TryGetValue(username, out var connectionIds)) 
            {
                return Task.FromResult(isOffline);
            }

            connectionIds.Remove(connectionId);
            if (connectionIds.Count == 0)
            {
                onlineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] usernames;
        lock(onlineUsers)
        {
            usernames = onlineUsers.Keys.ToArray();
        }

        Array.Sort(usernames);
        return Task.FromResult(usernames);
    }

    public Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string>? connectionIds;

        lock (onlineUsers)
        {
            connectionIds = onlineUsers.GetValueOrDefault(username) ?? new List<string>();
        }

        return Task.FromResult(connectionIds);
    }
}
