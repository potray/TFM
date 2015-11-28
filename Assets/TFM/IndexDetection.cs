using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Leap;
using System.Collections.Generic;

public class IndexDetection : MonoBehaviour {

    // Enums for checking.
    private enum Objects { Start, End, RealWorldObject };
    public enum GameModes { StraightLine, TouchObject };

    // Leap motion controller.

	Controller controller;

    // Game objects.

    public GameObject sphere;
    public GameObject start;
    public GameObject end;
    public GameObject upperGuide;
    public GameObject lowerGuide;
    public GameObject yesSphere;
    public GameObject noSphere;
    public Text text;

    // Strings.
    
    private string begginingInstructions = "Control the sphere with your index.\nPut the sphere over start.";
    private string begginingTest = "Beggining test in ";
    private string endingTest = "Test finished.";
    private string rebootBegginingTest = "You moved the sphere outside the start position.\nReturn the sphere to the start.";
    private string testInstructions = "Move the sphere to the end position.\nTry not to touch the lines.";    

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


	// Use this for initialization
	void Start () {
        // Set up controller.
        controller = new Controller();

        while (!controller.IsConnected) { }

        Debug.Log("Success!");
        
        // Set up instructions text.
        text.text = begginingInstructions;

        // Set up start and end transform so we avoid checking them every time.
        startPosition = start.transform.position;
        endPosition = end.transform.position;

        time = 0;
    }
	
	// Update is called once per frame
	void Update () {
        // Check controller status.
        if (controller == null)
        {
            return;
        }

        // Get the frame, the right hand and it's index.
        Frame frame = controller.Frame();

        Hand rightHand = frame.Hands.Rightmost;

        Finger index = rightHand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0];
        
        // Set the sphere position to the index position.
        sphere.transform.position = index.TipPosition.ToUnityScaled();
        sphere.transform.position = new Vector3(sphere.transform.position.x * 20, sphere.transform.position.y * 20, sphere.transform.position.z * 20);

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
                indexPositions.Add(index.TipPosition.ToUnity());
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
            case Objects.RealWorldObject:
                distanceToCheck = objectInRealWorldPosition;
                break;
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

        // TODO send info to the server
    }
}
