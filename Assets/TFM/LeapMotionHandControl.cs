using UnityEngine;
using System.Collections;
using System;
using Leap;

public class LeapMotionHandControl : HandControlPlatformAPI
{
    private Controller controller;
    private Frame frame;

    public bool deviceConnected()
    {
        return controller.IsConnected;
    }

    public Vector3 getIndexTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnity();
    }

    public Vector3 getMiddleTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_MIDDLE)[0].TipPosition.ToUnity();
    }

    public Vector3 getPinkyTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_PINKY)[0].TipPosition.ToUnity();
    }

    public Vector3 getRingTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_RING)[0].TipPosition.ToUnity();
    }

    public Vector3 getThumbTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnity();
    }

    public void init()
    {
        controller = new Controller();
    }

    public void update()
    {
        frame = controller.Frame();
    }
}
