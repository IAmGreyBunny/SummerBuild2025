using UnityEngine;

public class PlayerAuthSession
{
    
    public static string Username { get; private set; } = "";
    public static int PlayerId { get; private set; } = -1;
    public static bool IsLoggedIn { get; private set; } = false;

    public static void StartSession(string username, int userId)
    {
        Username = username;
        PlayerId = userId;
        IsLoggedIn = true;
    }

    public static void EndSession()
    {
        Username = "";
        PlayerId = -1;
        IsLoggedIn = false;
    }
}
