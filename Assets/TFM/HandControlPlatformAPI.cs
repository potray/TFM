using UnityEngine;
using System.Collections;

interface HandControlPlatformAPI
{
    void Init();
    void Update();
    bool DeviceConnected();
    Vector3 GetThumbTipPosition();
    Vector3 GetIndexTipPosition();
    Vector3 GetMiddleTipPosition();
    Vector3 GetRingTipPosition();
    Vector3 GetPinkyTipPosition();
}
