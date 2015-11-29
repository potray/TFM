using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;

public class Game : MonoBehaviour {


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
    private string gameEnded = "Data sent, returning to previous screen.";
    private string url = "http://tfmheroku.herokuapp.com/sendTestResult";
    private string debugurl = "http://127.0.0.1:8000/sendTestResult";

    // Positions of the fingers
    private List<List<Vector3>> fingerPositions;
    private List<double> times;

    private double time;
    private bool deviceWasDisconnected = false;
    private bool instructionsShown = false;

    // Use this for initialization
    void Start () {
        // Set up controller.
        controller = new LeapMotionHandControl();
        controller.init();

        // Show a warning message if the device isn't connected. Show instructions when the device is connected.
        if (!controller.deviceConnected())
        {
            while (!controller.deviceConnected())
            {
                text.text = connectDevice;
            }
            text.text = instructions;
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

        fingerPositions = new List<List<Vector3>>();
        for (int i = 0; i < 5; i++)
        {
            fingerPositions.Add(new List<Vector3>());
        }
        times = new List<double>();
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
        if (!controller.deviceConnected())
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
            controller.update();
            // Set the sphere position to the index position.
            sphere.transform.position = new Vector3(- controller.getIndexTipPosition().x / 10,
                controller.getIndexTipPosition().y / 10 + inGamePlatform.transform.position.y + inGameLeapMotion.transform.position.y,
               - controller.getIndexTipPosition().z / 10 + inGameLeapMotion.transform.position.z);

            // Check if the hand is in Leap's field of vision.
            if (controller.getIndexTipPosition().x != 0 && controller.getIndexTipPosition().y != 0 && controller.getIndexTipPosition().z != 0 && totalHooks != maxHooks)
            {
                // We have an index, so we add the positions of all fingers and the type to the lists.
                time += Time.deltaTime;
                fingerPositions[0].Add(controller.getThumbTipPosition());
                fingerPositions[1].Add(controller.getIndexTipPosition());
                fingerPositions[2].Add(controller.getMiddleTipPosition());
                fingerPositions[3].Add(controller.getRingTipPosition());
                fingerPositions[4].Add(controller.getPinkyTipPosition());
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
        string[] keys = new string[] {"thumb", "index", "middle", "ring", "pinky" };
        // Open string
        string handPositionsString = "{";

        for (int i = 0; i < 5; i++)
        {
            // Add finger key and open x.
            handPositionsString += "\"" + keys[i] + "\":{\"x\":{";

            // Add x positions
            for (int j = 0; j < times.Count; j++)
            {
                handPositionsString += j.ToString() + ":" + fingerPositions[i][j].x.ToString() + ", ";
            }

            // Close x, open y
            handPositionsString += "}, \"y\":{";

            // Add y positions
            for (int j = 0; j < times.Count; j++)
            {
                handPositionsString += j.ToString() + ":" + fingerPositions[i][j].y.ToString() + ", ";
            }

            // Close y, open z
            handPositionsString += "}, \"z\":{";

            // Addd z positions
            for (int j = 0; j < times.Count; j++)
            {
                handPositionsString += j.ToString() + ":" + fingerPositions[i][j].z.ToString() + ", ";
            }

            // Close z and finger
            handPositionsString += "}}";
        }

        // Add time
        handPositionsString += "\"times\":{";

        // Add times
        for (int i = 0; i < times.Count; i++)
        {
            handPositionsString += i.ToString() + ":" + times[i].ToString() + ", ";
        }

        // Close time and string
        handPositionsString += "}}";        

        var json = JSON.Parse(handPositionsString.ToString());

        // Send data to server.
        WWWForm form = new WWWForm();
        form.AddField("patient_id", PlayerPrefs.GetInt("PlayerID"));
        form.AddField("test_type", "SS");
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
    }

    // This is called by the sphere whenever it touches an object.
    public void HookTouched (GameObject obj)
    {
        if (blueHooks.IndexOf(obj) == lastHook)
        {
            selectNextHook();
        }
    }
}
