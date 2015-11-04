using UnityEngine;
using System.Collections;
using Leap;
using System.Collections.Generic;

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

    // Game attributes.
    private int lastHook;
    private List <GameObject> redHooks, blueHooks, redSticks, blueSticks;

    // Use this for initialization
    void Start () {
        // Set up controller.
        controller = new Controller();

        while (!controller.IsConnected) { }

        Debug.Log("Success!");

        // Set up attributes.
        lastHook = -1;

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

        // Start the game
        selectNextHook();
    }
	
	// Update is called once per frame
	void Update () {
        // Get the frame, the right hand and it's index.
        Frame frame = controller.Frame();

        Hand rightHand = frame.Hands.Rightmost;

        Finger index = rightHand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0];        

        // Set the sphere position to the index position.
        sphere.transform.position = new Vector3(- index.TipPosition.x / 10, index.TipPosition.y / 10 + inGamePlatform.transform.position.y + inGameLeapMotion.transform.position.y, index.TipPosition.z / 10 + inGameLeapMotion.transform.position.z);
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

        Debug.Log(i);
        
        // Swap hooks.
        if (lastHook != -1) // Skip the first previous swap
        {
            blueSticks[lastHook].SetActive(false);
            redSticks[lastHook].SetActive(true);
        }

        blueSticks[i].SetActive(true);
        redSticks[i].SetActive(false);

        lastHook = i;
    }


    // This is called by the sphere whenever it touches an object.
    public void HookTouched (GameObject obj)
    {
        /*if (obj.Equals(redLeftHook))
        {
            Debug.Log("leftHook");
        }
        else if (obj.Equals(redRightHook))
        {
            Debug.Log("rightHook");
        }
        else if (obj.Equals(redCenterHook))
        {
            Debug.Log("centerHook");
        }*/

        if (blueHooks.IndexOf(obj) == lastHook)
        {
            selectNextHook();
        }
    }
}
