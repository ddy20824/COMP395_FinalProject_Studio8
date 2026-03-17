using UnityEngine;

public struct CameraFocusData
{
    public Transform targetTransform;
    public bool isFocusing;
}


[CreateAssetMenu(menuName = "Events/CameraFocus Event Channel")]
public class CameraFocusEventChannel : BaseEventChannel<CameraFocusData> { }