using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Leap;
using System.Collections.Generic;
using SimpleJSON;

public class StraightLine : MonoBehaviour {

    // Enums for checking.
    private enum Objects { Start, End };
    
    HandControlPlatformAPI controller;

    // Game objects.

    public GameObject sphere;
    public GameObject start;
    public GameObject end;
    public GameObject upperGuide;
    public GameObject lowerGuide;
    public Text text;

    // Strings.
    
    private string begginingInstructions = "Control the sphere with your index.\nPut the sphere over start.";
    private string begginingTest = "Beggining test in ";
    private string endingTest = "Test finished, sending data...";
    private string rebootBegginingTest = "You moved the sphere outside the start position.\nReturn the sphere to the start.";
    private string testInstructions = "Move the sphere to the end position.\nTry not to touch the lines.";
    private string connectDevice = "Please connect the device or press alt+F4 to exit.";
    private string gameEnded = "Data sent, returning to previous screen...";
    private string previousText;

    // Game play attributes.
    private bool checkingForStart = true;
    private bool checkingForEnd = false;
    private bool countDownStarted = false;

    public float distanceDelta;

    private int secondsToStart = 3;
    private float startTimer = 3;
    private float previousX;

    private Vector3 objectInRealWorldPosition = new Vector3(-0.1f, 2.5f, 2.0f);
    private Vector3 fingerPosition;

    // Data to send to the server.
    private List<Vector3> indexPositions;
    private List<double> times;
    private double time;


    // Attributes that are loaded at start to improve performance.
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float totalDistanceToCenter = 0;

    // URLs
    private string url = "http://tfmheroku.herokuapp.com/sendTestResult";
    private string debugurl = "http://127.0.0.1:8000/sendTestResult";


    void Start () {
        // Set up controller.
        controller = new LeapMotionHandControl();
        controller.Init();      
                
        // Set up instructions text.
        text.text = begginingInstructions;
        previousText = begginingInstructions;

        // Set up start and end transform so we avoid checking them every time.
        startPosition = start.transform.position;
        endPosition = end.transform.position;

        time = 0;
        indexPositions = new List<Vector3>();
        times = new List<double>();

        if (Settings.debug)
            url = debugurl;
    }
	
	// Update is called once per frame
	void Update () {
        if (!controller.DeviceConnected())
        {
            //Debug.Log("disconnected");
            if (text.text != connectDevice)
            {
                //Debug.Log("Switching text");
                previousText = text.text;
            }
            text.text = connectDevice;
        } 
        else
        {
            if (text.text == connectDevice)
                text.text = previousText;
            // Tell the controller to update.
            controller.Update();

            // Set the sphere position to the index position.
            sphere.transform.position = new Vector3(controller.GetIndexTipPosition().x / 50,
                controller.GetIndexTipPosition().y / 50, 
               1);

            // Check for test to start.
            if (checkingForStart)
            {
                if (checkDistance(Objects.Start))
                {
                    // Continue or start countdown.
                    countDownStarted = true;

                    // Set the text.
                    startTimer -= Time.deltaTime;

                    if (startTimer < 0)
                    {
                        startTest();
                    }
                    else
                    {
                        if (startTimer < 1)
                        {
                            secondsToStart = 1;
                        }
                        else if (startTimer < 2)
                        {
                            secondsToStart = 2;
                        }

                        text.text = begginingTest + " " + secondsToStart.ToString();
                    }
                }
                else if (countDownStarted)
                {
                    // Restart the countdown.
                    startTimer = 3;
                    secondsToStart = 3;
                    text.text = rebootBegginingTest;
                    countDownStarted = false;
                }
            }

            // Check for test to end.
            if (checkingForEnd)
            {
                if (checkDistance(Objects.End))
                {
                    endTest();
                }
                else
                {
                    // Add time.
                    time += Time.deltaTime;
                    times.Add(time);

                    // Add index position.
                    indexPositions.Add(controller.GetIndexTipPosition());
                }
            }
        }
    }

    private bool checkDistance (Objects obj)
    {
        bool isInRange = true;

        Vector3 distanceToCheck;

        switch (obj)
        {
            case Objects.Start:
                distanceToCheck = startPosition;
                break;
            case Objects.End:
                distanceToCheck = endPosition;
                break;
            // This default block is just for the compiler. It shouldn't execute ever.
            default:
                distanceToCheck = startPosition;
                break;
        }

        // Check x and y distance to the object.

        float xDistance = 0, yDistance = 0;
        
        xDistance = sphere.transform.position.x - distanceToCheck.x;
        yDistance = sphere.transform.position.y - distanceToCheck.y;

        if (Mathf.Abs(xDistance) > distanceDelta || Mathf.Abs(yDistance) > distanceDelta)
        {
            isInRange = false;
        } 

        return isInRange;
    }

    private void startTest()
    {

        checkingForStart = false;
        checkingForEnd = true;

        text.text = testInstructions;
    }

    private void endTest()
    {
        checkingForEnd = false;

        text.text = endingTest;
        
        // Open string
        string handPositionsString = "{";

        // Add finger key and open x.
        handPositionsString += "\"index\":{\"x\":{";

        // Add x positions
        for (int j = 0; j < times.Count; j++)
        {
            handPositionsString += j.ToString() + ":" + indexPositions[j].x.ToString() + ", ";
        }

        // Close x, open y
        handPositionsString += "}, \"y\":{";

        // Add y positions
        for (int j = 0; j < times.Count; j++)
        {
            handPositionsString += j.ToString() + ":" + indexPositions[j].y.ToString() + ", ";
        }

        // Close y, open z
        handPositionsString += "}, \"z\":{";

        // Addd z positions
        for (int j = 0; j < times.Count; j++)
        {
            handPositionsString += j.ToString() + ":" + indexPositions[j].z.ToString() + ", ";
        }

        // Close z and finger
        handPositionsString += "}}";

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
        form.AddField("test_type", "SL");
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
}
