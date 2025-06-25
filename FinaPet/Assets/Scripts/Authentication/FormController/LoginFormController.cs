using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginFormController : MonoBehaviour
{
    private TMP_InputField _usernameInput;
    private TMP_InputField _passwordInput;

    public void Awake()
    {
        Debug.Log("Parent: " + transform.parent?.name);
        _usernameInput = transform.parent.Find("UsernameTextInput/InputField").GetComponent<TMP_InputField>();
        _passwordInput = transform.parent.Find("PasswordTextInput/InputField").GetComponent<TMP_InputField >();
    }

    public void CallLogin()
    {
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        // Sanity check on inputfields
        if (verifyInputs())
        {
            form.AddField("username", _usernameInput.text);
            form.AddField("password", _passwordInput.text);
        }
        else
        {
            yield return null;
        }

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        Debug.Log("API Path: " + apiPath);
        UnityWebRequest request = UnityWebRequest.Post(apiPath + "/login.php", form);
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request error: " + request.error);
        }
        else
        {
            _LoginResponse loginResponse = JsonUtility.FromJson<_LoginResponse>(request.downloadHandler.text);

            if (loginResponse.status_code == 0)
            {
                Debug.Log("Login Successful");
                SceneManager.LoadScene("MainMenu");
                PlayerAuthSession.StartSession(loginResponse.player_data.username, loginResponse.player_data.player_id);
                PlayerDataManager.FetchPlayerData(loginResponse.player_data.player_id);
            }
            else
            {
                Debug.Log("Login Failed with message: " + loginResponse.error_message);
            }
        }
    }

    public bool verifyInputs()
    {
        // Checks if username is valid

        // Checks if password is valid

        return true;

    }

    [Serializable]
    private class _LoginResponse
    {
        public int status_code;
        public string error_message;
        public _PlayerData player_data;
    }

    [Serializable]
    private class _PlayerData
    {
        public int player_id;
        public string username;
    }
}
