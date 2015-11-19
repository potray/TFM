using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using Leap;
using System.Collections.Generic;
using SimpleJSON;

public class Game : MonoBehaviour {


    // Leap motion controller.

    private Controller controller;

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
    private int lastHook, totalHooks, maxHooks;
    private List <GameObject> redHooks, blueHooks, redSticks, blueSticks;

    // Strings
    private string gameEnding = "Juego terminado";

    // Positions of the fingers
    private List<double> indexPositions;
    private List<double> times;

    private double time;

    // Use this for initialization
    void Start () {
        // Set up controller.
        controller = new Controller();

        while (!controller.IsConnected) { }

        Debug.Log("Success!");

        // Set up attributes.
        lastHook = -1;
        maxHooks = 1;
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

        indexPositions = new List<double>();
        times = new List<double>();
        time = 0;

        // Start the game
        selectNextHook();
    }
	
	// Update is called once per frame
	void Update () {
        // Get the frame, the right hand and it's index.
        Frame frame = controller.Frame();

        Hand rightHand = frame.Hands.Rightmost;

        Pointable index = rightHand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0];
        
        //Pointable index = frame.Tools.Rightmost;

        // Set the sphere position to the index position.
        sphere.transform.position = new Vector3(- index.TipPosition.x / 10, index.TipPosition.y / 10 + inGamePlatform.transform.position.y + inGameLeapMotion.transform.position.y, index.TipPosition.z / 10 + inGameLeapMotion.transform.position.z);

        // Check if the hand is in Leap's field of vision.
        if (index.TipPosition.x != 0 && index.TipPosition.y != 0 && index.TipPosition.z != 0 && totalHooks != maxHooks)
        {
            time += Time.deltaTime;
            indexPositions.Add(index.TipPosition.y);
            times.Add(time);
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
        Debug.Log(totalHooks);
        
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

        using (StreamWriter sr = new StreamWriter("file.txt"))
        {
            sr.WriteLine(times.Count);

            foreach(var item in indexPositions)
            {
                sr.WriteLine(item);
            }

            sr.WriteLine("Times");
            foreach (var item in times)
            {
                sr.WriteLine(item);
            }

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
