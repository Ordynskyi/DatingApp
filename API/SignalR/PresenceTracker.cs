namespace API.SignalR;

public class PresenceTracker
{
    private readonly Dictionary<string, List<string>> onlineUsers = new ();

    public Task UserConnected(string username, string connectionId)
    {
        lock(onlineUsers)
        {
            if (!onlineUsers.TryGetValue(username, out var connectionIds))
            {
                connectionIds = new List<string>();
                onlineUsers[username] = connectionIds;
            }

            connectionIds.Add(connectionId);
        }

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string username, string connectionId)
    {
        lock(onlineUsers)
        {
            if (!onlineUsers.TryGetValue(username, out var connectionIds)) return Task.CompletedTask;

            connectionIds.Remove(connectionId);
            if (connectionIds.Count == 0)
            {
                onlineUsers.Remove(username);
            }
        }

        return Task.CompletedTask;
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
