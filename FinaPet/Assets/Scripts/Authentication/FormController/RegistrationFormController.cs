using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegistrationFormController : MonoBehaviour
{
    private TMP_InputField _usernameInput;
    private TMP_InputField _emailInput;
    private TMP_InputField _confirmEmailInput;
    private TMP_InputField _passwordInput;
    private TMP_InputField _confirmPasswordInput;

    public void Awake()
    {
        Debug.Log("Parent: " + transform.parent?.name);
        _usernameInput = transform.parent.Find("UsernameTextInput/InputField").GetComponent<TMP_InputField>();
        _emailInput = transform.parent.Find("EmailTextInput/InputField").GetComponent<TMP_InputField>();
        _confirmEmailInput = transform.parent.Find("ConfirmEmailTextInput/InputField").GetComponent<TMP_InputField>();
        _passwordInput = transform.parent.Find("PasswordTextInput/InputField").GetComponent<TMP_InputField >();
        _confirmPasswordInput = transform.parent.Find("ConfirmPasswordTextInput/InputField").GetComponent<TMP_InputField>();
    }

    public void CallRegister()
    {
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        // Sanity check on inputfields
        if (verifyInputs())
        {
            form.AddField("username", _usernameInput.text);
            form.AddField("email", _emailInput.text);
            form.AddField("password", _passwordInput.text);
        }
        else
        {
            yield return null;
        }

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        Debug.Log("API Path: " + apiPath);
        WWW www = new WWW(apiPath + "/register.php",form);
        yield return www;

        
        if (www.text == "0")
        {
            Debug.Log("Registration Successful");
            GameObject formRenderController = GameObject.Find("FormRenderController");
            formRenderController.GetComponent<FormRenderScript>().showLoginForm();
            
        }
        else
        {
            Debug.Log(www.text);
            Debug.Log("User registration Failed");
        }
    }

    public bool verifyInputs()
    {
        // Checks if username is valid

        // Checks if email is valid

        // Checks if confirmEmail is same as email

        // Checks if password is valid

        // Checks if confirmPassword is same as password

        return true;

    }
}
