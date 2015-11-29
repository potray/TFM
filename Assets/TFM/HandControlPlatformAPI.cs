using UnityEngine;
using System.Collections;

interface HandControlPlatformAPI
{
    void init();
    void update();
    bool deviceConnected();
    Vector3 getThumbTipPosition();
    Vector3 getIndexTipPosition();
    Vector3 getMiddleTipPosition();
    Vector3 getRingTipPosition();
    Vector3 getPinkyTipPosition();
}
