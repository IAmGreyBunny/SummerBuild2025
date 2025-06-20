using System;
using System.Collections;
using TMPro;
using UnityEngine;
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
        WWW www = new WWW(apiPath + "/login.php",form);
        yield return www;

        
        if (www.text == "0")
        {
            Debug.Log("Login Successful");
            SceneManager.LoadScene("MainMenu");
            
        }
        else
        {
            Debug.Log(www.text);
            Debug.Log("User Login Failed");
        }
    }

    public bool verifyInputs()
    {
        // Checks if username is valid

        // Checks if password is valid

        return true;

    }
}
