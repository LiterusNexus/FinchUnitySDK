using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Finch;

public class Chooser : MonoBehaviour
{
    private enum TrackingType
    {
        Finch,
        Experimental
    }

    public GameObject[] BaseTracking = new GameObject[0];
    public GameObject[] ExperimentalTracking = new GameObject[0];

    public TextMesh Text;

    public static string Path;

    void Start ()
    {
        UpdateState(TrackingType.Finch);
    }

    void Update()
    {
        if (FinchController.GetPressDown(FinchChirality.Any, FinchControllerElement.AppButton))
        {
            UpdateState(TrackingType.Finch);
        }

        if (FinchController.GetPressDown(FinchChirality.Any, FinchControllerElement.ThumbButton))
        {
            UpdateState(TrackingType.Experimental);
        }

        Text.text = Path;
    }

    void UpdateState(TrackingType type)
    {
        FinchController.Left.HapticPulse(120);
        FinchController.Right.HapticPulse(120);

        foreach (var i in BaseTracking)
        {
            i.SetActive(type==TrackingType.Finch);
        }

        foreach (var i in ExperimentalTracking)
        {
            i.SetActive(type==TrackingType.Experimental);
        }
    }
}
