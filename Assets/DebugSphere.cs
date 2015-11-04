using UnityEngine;
using System.Collections;

public class DebugSphere : MonoBehaviour {

    public Game game;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter (Collider col)
    {
        game.HookTouched(col.gameObject);
    }
}
