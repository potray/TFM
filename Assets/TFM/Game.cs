using UnityEngine;
using System.Collections;
using Leap;

public class Game : MonoBehaviour {


    // Leap motion controller.

    private Controller controller;

    // Gameobjects from scene.

    public GameObject sphere;

    // Use this for initialization
    void Start () {
        // Set up controller.
        controller = new Controller();

        while (!controller.IsConnected) { }

        Debug.Log("Success!");
    }
	
	// Update is called once per frame
	void Update () {
        // Get the frame, the right hand and it's index.
        Frame frame = controller.Frame();

        Hand rightHand = frame.Hands.Rightmost;

        Finger index = rightHand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0];

        // Set the sphere position to the index position.
        sphere.transform.position = new Vector3(- index.TipPosition.x / 10, index.TipPosition.y / 10, index.TipPosition.z / 10);

    }


}
