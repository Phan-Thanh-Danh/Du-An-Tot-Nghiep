using System.Collections.Concurrent;
using System.Text.Json;

namespace Backend.Services.Comments;

public interface ICommentLikeService
{
    int GetLikesCount(int commentId);
    bool HasUserLiked(int commentId, int userId);
    (int LikesCount, bool IsLiked) ToggleLike(int commentId, int userId);
}

public class CommentLikeService : ICommentLikeService
{
    // commentId -> Set of userIds who liked the comment
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, byte>> _commentLikes = new();
    private readonly string _storageFilePath;
    private readonly object _fileLock = new();

    public CommentLikeService(IWebHostEnvironment? env = null)
    {
        var basePath = env?.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;
        var dir = Path.Combine(basePath, "App_Data");
        if (!Directory.Exists(dir))
        {
            try { Directory.CreateDirectory(dir); } catch { }
        }
        _storageFilePath = Path.Combine(dir, "comment_likes.json");
        LoadFromDisk();
    }

    public int GetLikesCount(int commentId)
    {
        if (_commentLikes.TryGetValue(commentId, out var users))
        {
            return users.Count;
        }
        return 0;
    }

    public bool HasUserLiked(int commentId, int userId)
    {
        if (_commentLikes.TryGetValue(commentId, out var users))
        {
            return users.ContainsKey(userId);
        }
        return false;
    }

    public (int LikesCount, bool IsLiked) ToggleLike(int commentId, int userId)
    {
        var users = _commentLikes.GetOrAdd(commentId, _ => new ConcurrentDictionary<int, byte>());
        bool isLiked;
        if (users.ContainsKey(userId))
        {
            users.TryRemove(userId, out _);
            isLiked = false;
        }
        else
        {
            users.TryAdd(userId, 1);
            isLiked = true;
        }

        SaveToDisk();
        return (users.Count, isLiked);
    }

    private void LoadFromDisk()
    {
        try
        {
            if (File.Exists(_storageFilePath))
            {
                var json = File.ReadAllText(_storageFilePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(json);
                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        if (int.TryParse(kvp.Key, out var cId))
                        {
                            var userDict = new ConcurrentDictionary<int, byte>();
                            foreach (var uId in kvp.Value)
                            {
                                userDict.TryAdd(uId, 1);
                            }
                            _commentLikes[cId] = userDict;
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore corrupted or unreadable initial file
        }
    }

    private void SaveToDisk()
    {
        try
        {
            lock (_fileLock)
            {
                var export = new Dictionary<string, List<int>>();
                foreach (var kvp in _commentLikes)
                {
                    export[kvp.Key.ToString()] = kvp.Value.Keys.ToList();
                }
                var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(_storageFilePath, json);
            }
        }
        catch
        {
            // File write fallback
        }
    }
}
