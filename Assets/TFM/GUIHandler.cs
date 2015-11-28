using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using SimpleJSON;

public class GUIHandler : MonoBehaviour {

    public Text inputText;
    public Text infoText;
    public Button logInButton;
    public Button confirmButton;
    public Canvas loginCanvas;
    public Canvas selectGameCanvas;
    private int playerId;

    public void Start()
    {
        HideButton(confirmButton);
        playerId = -1;
    }

    public void LogIn()
    {
        // Get the text in the input field, check if the code is valid.
        try
        {
            playerId = int.Parse(inputText.text);
            infoText.text = "";

        }
        catch 
        {
            infoText.text = "Please enter a valid code.";
        }

        if (playerId != -1)
        {
            string url = "";
            if (Settings.debug)
            {
                url += "http://127.0.0.1:8000";
            }
            else
            {
                url += "http://tfmheroku.herokuapp.com";
            }
            url += "/patients/validateCode?code=" + playerId;
            Debug.Log(url);

            var www = new WWW(url);

            // Wait for request to complete
            StartCoroutine(WaitForRequest(www));
         
        }
    }

    public void OnTextInputClick() {
        infoText.text = "Enter the code your doctor provided above.";
        HideButton(confirmButton);
        ShowButton(logInButton);
    }

    // Save playerprefs and show select game buttons.
    public void Confirm()
    {
        PlayerPrefs.SetInt("PlayerID", playerId);
        loginCanvas.gameObject.SetActive(false);
        selectGameCanvas.gameObject.SetActive(true);
    }

    private void HideButton (Button button)
    {
        button.gameObject.SetActive(false);
    }

    private void ShowButton (Button button)
    {
        button.gameObject.SetActive(true);
    }

    public void LaunchGame (String game)
    {
        Application.LoadLevel(game);
    }

    private IEnumerator WaitForRequest (WWW www)
    {
        yield return www;

        if (www.error == null)
        {
            Debug.Log(www.text);
            var json = JSON.Parse(www.text);
            var firstName = json["first_name"].Value;
            var lastName = json["last_name"].Value;
            var ok = json["ok"].AsBool;
            Debug.Log(firstName + " " + lastName + " " + ok);

            if (ok)
            {
                infoText.text = "Loggin in as " + firstName + " " + lastName + ". Please confirm or enter a new code.";
                HideButton(logInButton);
                ShowButton(confirmButton);
            }
            else
            {
                infoText.text = "The code you entered doesn't exists, please try again";
            }

        }
        else
        {
            Debug.Log("Error! " + www.error);
        }
    }
}
