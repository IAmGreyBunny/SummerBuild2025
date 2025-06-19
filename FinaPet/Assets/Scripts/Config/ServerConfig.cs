using UnityEngine;
using System.IO;
using Unity.VisualScripting;

[System.Serializable]
public class ServerConfig
{
    public string serverIP;
    public int serverPort;
    public string apiBasePath;

    // Static method to load config from file
    public static ServerConfig LoadFromFile(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename).Replace("\\", "/");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ServerConfig>(json);
        }
        Debug.LogError("Config file not found at " + path);
        return null;
    }

    public string GetApiPath()
    {
        return ("http://" + serverIP + ":" + serverPort.ToString() + "/" + apiBasePath).TrimEnd('/');
    }
}