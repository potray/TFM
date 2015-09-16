using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Leap;

public class IndexDetection : MonoBehaviour {

    // Enums for checking.
    private enum Objects {Start, End};

    // Leap motion controller.

	Controller controller;

    // Game objects.

    public GameObject sphere;
    public GameObject start;
    public GameObject end;
    public GameObject upperGuide;
    public GameObject lowerGuide;
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

    private double distanceDelta = 0.08;
    private double xDelta = 1;

    // Game play attributes.

    private bool checkingForStart = true;
    private bool checkingForEnd = false;
    private bool countDownStarted = false;

    private int secondsToStart = 3;
    private float startTimer = 3;
    private float time = 0;
    private float previousX;
    
    
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

        // Set up instructions text.
        text.text = begginingInstructions;

        // Set up start and end transform so we avoid checking them every time.
        startPosition = start.transform.position;
        endPosition = end.transform.position;

        // Set up the middle of the guides.
        lineCenterY = upperGuide.transform.position.y - upperGuide.transform.position.y;

        // The first X is the start one
        previousX = startPosition.x;
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
        sphere.transform.position = new Vector3 (sphere.transform.position.x*20, sphere.transform.position.y*20, sphere.transform.position.z*20);

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
            } else if (countDownStarted)
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
        float xDistance = sphere.transform.position.x - distanceToCheck.x;
        float yDistance = sphere.transform.position.y - distanceToCheck.y;
        
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

        text.text = endingTest + "\n" + 
            scoreStart + totalDistanceToCenter + scoreFinal + "\n" +
            timeStart + time + timeFinal;
    }
}
