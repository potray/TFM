using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using SimpleJSON;
using Leap;

public class GUIHandler : MonoBehaviour {

    public Text inputText;
    public Text infoText;
    public Text warningText;
    public Button logInButton;
    public Button confirmButton;
    public Canvas loginCanvas;
    public Canvas selectGameCanvas;
    private int playerId;

    private bool loginCanvasHiddenByController;
    private bool selectGameCanvasHiddenByController;
    private bool deviceWasDisconnectedAtStart;

    private Controller controller;

    public void Start()
    {
        HideButton(confirmButton);
        playerId = -1;

        // Get Leap Motion info.
        controller = new Controller();

        loginCanvasHiddenByController = false;
        selectGameCanvasHiddenByController = false;

        // Show warning if device isn't connected.
        if (!controller.IsConnected)
        {
            warningText.text = "Please connect the Leap Motion device to your computer.";
            loginCanvas.gameObject.SetActive(false);
            deviceWasDisconnectedAtStart = true;
        }
        else {
            deviceWasDisconnectedAtStart = false;
        }
    }

    // Check the Leap Motion Device.
    public void Update()
    {
        if (controller.IsConnected)
        {
            OnLeapMotionConnect();
        }
        else
        {
            OnLeapMotionDisconnect();
        }
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

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Back()
    {
        loginCanvas.gameObject.SetActive(true);
        selectGameCanvas.gameObject.SetActive(false);
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

    // Hide the warning text and show every canvas that was hidden when the device was disconnected.
    public void OnLeapMotionConnect()
    {
        Debug.Log("Connected!");
        warningText.text = "";

        if (loginCanvasHiddenByController || deviceWasDisconnectedAtStart)
        {
            deviceWasDisconnectedAtStart = false;
            loginCanvasHiddenByController = false;
            loginCanvas.gameObject.SetActive(true);
        }

        if (selectGameCanvasHiddenByController)
        {
            selectGameCanvasHiddenByController = false;
            selectGameCanvas.gameObject.SetActive(true);
        }
    }

    
    // Hide every canvas that wasn't hidden and show warning text.
    public void OnLeapMotionDisconnect() {
        Debug.Log("Disconnected!");
        warningText.text = "Leap Motion disconnected, please reconnect it.";
        if (loginCanvas.gameObject.activeSelf)
        {
            loginCanvasHiddenByController = true;
            loginCanvas.gameObject.SetActive(false);
        }

        if (selectGameCanvas.gameObject.activeSelf)
        {
            selectGameCanvasHiddenByController = true;
            selectGameCanvas.gameObject.SetActive(false);
        }
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
                infoText.text = "Logging in as " + firstName + " " + lastName + ". Please confirm or enter a new code.";
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


