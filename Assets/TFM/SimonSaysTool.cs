using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;

public class SimonSaysTool : MonoBehaviour {


    // Leap motion controller.
    HandControlPlatformAPI controller;

    // Gameobjects from scene.

    public GameObject sphere;
    public GameObject inGameLeapMotion;
    public GameObject inGamePlatform;
    public GameObject redLeftHook;
    public GameObject redRightHook;
    public GameObject redCenterHook;
    public GameObject blueLeftHook;
    public GameObject blueRightHook;
    public GameObject blueCenterHook;

    public GameObject redLeftStickWithHook;
    public GameObject redRightStickWithHook;
    public GameObject redCenterStickWithHook;
    public GameObject blueLeftStickWithHook;
    public GameObject blueRightStickWithHook;
    public GameObject blueCenterStickWithHook;

    public Text text;
    
    // Game attributes.
    private int lastHook, totalHooks;
    public int maxHooks;
    private List <GameObject> redHooks, blueHooks, redSticks, blueSticks;

    // Strings
    private string instructions = "Touch the blue hook, don't touch the red ones!";
    private string connectDevice = "Please connect the device or press alt+F4 to exit.";
    private string restarted = "The test had to be restarted. Please begin.";
    private string gameEnding = "Game finished! Sending data to the server.";
    private string gameEnded = "Data sent, returning to previous screen...";
    private string url = "http://tfmheroku.herokuapp.com/sendTestResult";
    private string debugurl = "http://127.0.0.1:8000/sendTestResult";

    // Positions of the fingers
    private List<Vector3> toolPositions;
    private List<double> times;
    private List<double> touchTimes;

    private double time;
    private bool deviceWasDisconnected = false;
    private bool instructionsShown = false;

    // Use this for initialization
    void Start () {
        // Set up controller.
        controller = new LeapMotionHandControl();
        controller.Init();

        // Show a warning message if the device isn't connected. Show instructions when the device is connected.
        if (!controller.DeviceConnected())
        {
            text.text = connectDevice;
        }

        Init();

        if (Settings.debug)
            url = debugurl;
    }
    
    // This method resets the game.
    private void Init() {
        if (deviceWasDisconnected)
        {
            Debug.Log("Device was disconnected");
            if (!instructionsShown)
            {
                instructionsShown = true;
                text.text = instructions;
            }
            else
                text.text = restarted;
            deviceWasDisconnected = false;
        }
        lastHook = -1;
        totalHooks = -1;

        redHooks = new List<GameObject>();
        redHooks.Add(redLeftHook);
        redHooks.Add(redRightHook);
        redHooks.Add(redCenterHook);

        blueHooks = new List<GameObject>();
        blueHooks.Add(blueLeftHook);
        blueHooks.Add(blueRightHook);
        blueHooks.Add(blueCenterHook);

        redSticks = new List<GameObject>();
        redSticks.Add(redLeftStickWithHook);
        redSticks.Add(redRightStickWithHook);
        redSticks.Add(redCenterStickWithHook);

        blueSticks = new List<GameObject>();
        blueSticks.Add(blueLeftStickWithHook);
        blueSticks.Add(blueRightStickWithHook);
        blueSticks.Add(blueCenterStickWithHook);

        toolPositions = new List<Vector3>();
        times = new List<double>();
        touchTimes = new List<double>();
        time = 0;

        // Hide all the blue sticks, show all the red sticks, in case the game was restarted.
        foreach (GameObject stick in blueSticks)
        {
            stick.SetActive(false);
        }
        foreach (GameObject stick in redSticks)
        {
            stick.SetActive(true);
        }
        // Start the game
        selectNextHook();
    }

	
	// Update is called once per frame
	void Update () {
        // If the controller was disconnected, reset the game.
        if (!controller.DeviceConnected())
        {        
            deviceWasDisconnected = true;
            text.text = connectDevice;
        }
        else
        {
            if (deviceWasDisconnected)
            {
                Init();
            }
            // If we are here, the instructions were shown.
            instructionsShown = true;
            // Tell the controller to update.
            controller.Update();
            // Set the sphere position to the index position.
            sphere.transform.position = new Vector3(- controller.GetToolTipPosition().x / 10,
                controller.GetToolTipPosition().y / 10 + inGameLeapMotion.transform.position.y/2,
               - controller.GetToolTipPosition().z / 10 + inGameLeapMotion.transform.position.z);

            // Check if the tool is in Leap's field of vision.
            if (controller.GetToolTipPosition().x != 0 && controller.GetToolTipPosition().y != 0 && controller.GetToolTipPosition().z != 0 && totalHooks != maxHooks)
            {
                // We have an index, so we add the positions of all fingers and the type to the lists.
                time += Time.deltaTime;
                toolPositions.Add(controller.GetToolTipPosition());
                times.Add(time);
            }
        }        
    }

    void selectNextHook ()
    {
        // Generate a valid random number.
        System.Random rnd = new System.Random();
        int i = lastHook;
        while (i == lastHook)
        {
            i = rnd.Next(0, 3);
        }

        totalHooks++;
        
        // Swap hooks.
        if (lastHook != -1) // Skip the first previous swap
        {
            blueSticks[lastHook].SetActive(false);
            redSticks[lastHook].SetActive(true);
        }

        if (totalHooks == maxHooks)
        {
            EndGame();
        }
        else
        {
            blueSticks[i].SetActive(true);
            redSticks[i].SetActive(false);

            lastHook = i;
        }
    }

    private void EndGame()
    {
        text.text = gameEnding;
        // Open string
        string positionsString = "{";

        // Add tool key and open x.
        positionsString += "\"tool\":{\"x\":{";

        // Add x positions
        for (int j = 0; j < times.Count; j++)
        {
            positionsString += j.ToString() + ":" + toolPositions[j].x.ToString() + ", ";
        }

        // Close x, open y
        positionsString += "}, \"y\":{";

        // Add y positions
        for (int j = 0; j < times.Count; j++)
        {
            positionsString += j.ToString() + ":" + toolPositions[j].y.ToString() + ", ";
        }

        // Close y, open z
        positionsString += "}, \"z\":{";

        // Addd z positions
        for (int j = 0; j < times.Count; j++)
        {
            positionsString += j.ToString() + ":" + toolPositions[j].z.ToString() + ", ";
        }

        // Close z and finger
        positionsString += "}}";

        // Add touch times
        positionsString += "\"touchTimes\":{";

        for (int i = 0; i < touchTimes.Count; i++)
        {
            positionsString += i.ToString() + ":" + touchTimes[i].ToString() + ", ";
        }

        // Add time
        positionsString += "}\"times\":{";

        // Add times
        for (int i = 0; i < times.Count; i++)
        {
            positionsString += i.ToString() + ":" + times[i].ToString() + ", ";
        }

        // Close time and string
        positionsString += "}}";        

        var json = JSON.Parse(positionsString.ToString());

        // Send data to server.
        WWWForm form = new WWWForm();
        form.AddField("patient_id", PlayerPrefs.GetInt("PlayerID"));
        form.AddField("test_type", "ST");
        form.AddField("result", json.ToString());

        WWW www = new WWW(url, form);

        StartCoroutine(WaitForRequest(www));

    }

    private IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        text.text = gameEnded;

        if (www.error == null)
        {
            Debug.Log(www.text);
        }
        else
        {
            Debug.Log("Error! " + www.error);
        }

        StartCoroutine(GoToStartScreen());
    }

    private IEnumerator GoToStartScreen()
    {
        yield return new WaitForSeconds(3);

        // Tell the main level to load de "select test" screen.
        PlayerPrefs.SetInt("LoadLevel", 1);
        Application.LoadLevel("main");
    }

    // This is called by the sphere whenever it touches an object.
    public void HookTouched (GameObject obj)
    {
        if (blueHooks.IndexOf(obj) == lastHook)
        {
            touchTimes.Add(time);
            selectNextHook();
        }
    }
}
