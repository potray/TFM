using UnityEngine;
using System.Collections;
using System;
using Leap;

public class LeapMotionHandControl : HandControlPlatformAPI
{
    private Controller controller;
    private Frame frame;

    public bool DeviceConnected()
    {
        return controller.IsConnected;
    }

    public Vector3 GetIndexTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnity();
    }

    public Vector3 GetMiddleTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_MIDDLE)[0].TipPosition.ToUnity();
    }

    public Vector3 GetPinkyTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_PINKY)[0].TipPosition.ToUnity();
    }

    public Vector3 GetRingTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_RING)[0].TipPosition.ToUnity();
    }

    public Vector3 GetToolTipPosition()
    {
        return frame.Tools.Frontmost.TipPosition.ToUnity();
    }

    public Vector3 GetThumbTipPosition()
    {
        return frame.Hands.Rightmost.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnity();
    }

    public void Init()
    {
        controller = new Controller();
    }

    public void Update()
    {
        frame = controller.Frame();
    }
}
