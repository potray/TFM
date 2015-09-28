using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Leap;

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

    private string begginingInstructions = "Controla la pelota con tu índice.\nPon la pelota sobre la salida.";
    private string begginingTest = "Comenzando prueba en ";
    private string endingTest = "Prueba finalizada";
    private string rebootBegginingTest = "Has movido la pelota fuera de la posición de salida.\nVuelve a ponerla en la misma posición.";
    private string testInstructions = "Mueve la pelota hasta la posición de fin\n sin que se salga de las guías.";
    private string scoreStart = "Su puntuación ha sido de ";
    private string scoreFinal = " puntos.";
    private string timeStart = "Ha tardado ";
    private string timeFinal = " segundos en realizar la prueba";

    // Adjustable attributes.

    public double distanceDelta = 0.08;
    private double xDelta = 1;

    // Game play attributes.

    public GameModes gameMode;

    private bool checkingForStart = true;
    private bool checkingForEnd = false;
    private bool countDownStarted = false;

    private int secondsToStart = 3;
    private float startTimer = 3;
    private float time = 0;
    private float previousX;

    private Vector3 objectInRealWorldPosition = new Vector3(-0.1f, 2.5f, 2.0f);
    private Vector3 fingerPosition;


    // Attributes that are loaded at start to improve performance.

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float lineCenterY;
    private float totalDistanceToCenter = 0;


	// Use this for initialization
	void Start () {
        // Set up controller.
        controller = new Controller();

        while (!controller.IsConnected) { }

        Debug.Log("Success!");

        switch (gameMode)
        {
            case GameModes.StraightLine:
                // Set up instructions text.
                text.text = begginingInstructions;

                // Set up start and end transform so we avoid checking them every time.
                startPosition = start.transform.position;
                endPosition = end.transform.position;

                // Set up the middle of the guides.
                lineCenterY = upperGuide.transform.position.y - upperGuide.transform.position.y;

                // The first X is the start one
                previousX = startPosition.x;

                break;
            case GameModes.TouchObject:
                
                break;
        }

        
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

        switch (gameMode)
        {
            case GameModes.StraightLine:
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

                        // If the ball traveled enough distance in the X axis, check Y distance.
                        float sphereX = sphere.transform.position.x;

                        if (sphereX > (previousX + xDelta))
                        {
                            previousX = sphereX;

                            // Add Y distance to the center if the distance is big enough.

                            float sphereY = sphere.transform.position.y;
                            float sphereDistanceToCenter = Mathf.Abs(sphereY - lineCenterY);

                            if (sphereDistanceToCenter > distanceDelta)
                            {
                                totalDistanceToCenter += sphereDistanceToCenter;
                            }
                        }
                    }
                }
                break;

            case GameModes.TouchObject:
                // Get the index position un "real Unity coordinates"
                fingerPosition = index.TipPosition.ToUnityScaled();
                fingerPosition = new Vector3(fingerPosition.x * 20, fingerPosition.y * 20, fingerPosition.z * 20);

                if (Input.GetButtonDown("Jump"))
                {
                    Debug.Log(fingerPosition);
                }

                    
                // Check if the user is touching the real world object.
                if (checkDistance(Objects.RealWorldObject))
                {
                    yesSphere.SetActive(true);
                    noSphere.SetActive(false);
                }
                else
                {
                    yesSphere.SetActive(false);
                    noSphere.SetActive(true);
                }
                break;
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

        float xDistance = 0, yDistance = 0, zDistance = 0;

        switch (gameMode)
        {
            case GameModes.StraightLine:
                xDistance = sphere.transform.position.x - distanceToCheck.x;
                yDistance = sphere.transform.position.y - distanceToCheck.y;

                if (Mathf.Abs(xDistance) > distanceDelta || Mathf.Abs(yDistance) > distanceDelta)
                {
                    isInRange = false;
                } 

                break;
            case GameModes.TouchObject:
                xDistance = fingerPosition.x - distanceToCheck.x;
                yDistance = fingerPosition.y - distanceToCheck.y;
                zDistance = fingerPosition.z - distanceToCheck.z;

                if (Mathf.Abs(xDistance) > distanceDelta || Mathf.Abs(yDistance) > distanceDelta || Mathf.Abs(zDistance) > distanceDelta)
                {
                    isInRange = false;
                }
                break;
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

        text.text = endingTest + "\n" + 
            scoreStart + totalDistanceToCenter + scoreFinal + "\n" +
            timeStart + time + timeFinal;
    }
}
