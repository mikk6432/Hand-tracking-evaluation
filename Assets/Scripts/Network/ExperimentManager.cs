using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

// ReSharper disable once CheckNamespace
public partial class ExperimentManager : MonoBehaviour
{
    // not used for trainings. To finish training, method (OnServerSaidFinishTraining should be called)
    public readonly UnityEvent trialsFinished = new();
    public readonly UnityEvent requestTrialValidation = new();
    public readonly UnityEvent<string> unexpectedErrorOccured = new();
    public int participantID;
    public bool leftHanded; // indicates, that dominant hand is left
    public bool standing; // indicates that the participant is stationary

    private GameObject sphere;
    private GameObject fingerTipMock;
    public float speed = 5f;
    private float originalX;

    private bool idle = false;

    #region MonoBehaviour methods

    public void Start()
    {
        SetupExperiment();
        ActualizeHands();
    }

    public void SetupExperiment()
    {
        // Setup sphere stuff here
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(0, 1.5f, 0);
        float sphereSize = 0.8f;
        sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
        fingerTipMock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fingerTipMock.transform.position = new Vector3(0, 1.5f, 0);
        fingerTipMock.transform.localScale = new Vector3(0.5f * sphereSize, 0.5f * sphereSize, 0.5f * sphereSize);
        OnServerSetPath();
    }

    #endregion

    #region Track & Light stuff
    [Space]
    [SerializeField] private GameObject track;
    [SerializeField] private GameObject sceneLight; // remark: we interpret it as track in standing context. Hand Ref and path ref depend on it, actually


    [SerializeField] private GameObject walkingDirection; // walking context (relative to track)

    public void PlaceTrackForwardFromHeadset()
    {
        var (position, rotation) = HeadsetOXZProjection();
        float halfTrackLength = 2.75f;
        position += rotation * new Vector3(0, 0, halfTrackLength + .3f); // half track length and small offset more 
        track.transform.SetPositionAndRotation(position, rotation);
    }

    public void PlaceSphereAboveTrack()
    {
        var (position, rotation) = HeadsetOXZProjection();
        position += rotation * new Vector3(0, 0, .5f); // half track length and small offset more 
        track.transform.SetPositionAndRotation(position, rotation);
    }

    public void UpdateSphere()
    {

        float cmToEachSide = 0.1f;
        float newX = originalX + Mathf.Sin(Time.time * speed) * cmToEachSide;

        // Update the sphere's position
        sphere.transform.position = new Vector3(newX, sphere.transform.position.y, sphere.transform.position.z);

    }

    public void PlaceLightWhereHeadset()
    {
        var (position, rotation) = HeadsetOXZProjection();
        sceneLight.transform.SetPositionAndRotation(position, rotation);
    }

    public void PlaceLightWhereTrack()
    {
        var trackTransform = track.transform;
        sceneLight.transform.SetPositionAndRotation(trackTransform.position, trackTransform.rotation);
    }
    #endregion

    #region Head stuff
    [Space]
    [SerializeField] private GameObject headset;
    [SerializeField] private GameObject neckBase;

    private (Vector3 position, Quaternion rotation) HeadsetOXZProjection()
    {
        var headsetTransform = headset.transform;
        var headsetPosition = headsetTransform.position;
        var position = new Vector3(headsetPosition.x, 0, headsetPosition.z);

        var headsetForward = headsetTransform.forward;
        var rotation = Quaternion.LookRotation(new Vector3(headsetForward.x, 0, headsetForward.z));
        return (position, rotation);
    }
    #endregion

    #region Hands stuff
    [Space]
    // oculus hands here. Note, that we keep inactive gameObjects which we don't use 
    [SerializeField] private GameObject leftIndexTip;
    [SerializeField] private GameObject rightIndexTip;
    private GameObject dominantHandIndexTip; // holds selector

    private void ActualizeHands()
    {
        leftIndexTip.SetActive(leftHanded);
        rightIndexTip.SetActive(!leftHanded);
    }
    #endregion

    [SerializeField] private GameObject RefFrame;

    #region Logging stuff

    private int _measurementId = 0;
    public List<float> distances = new List<float>();
    public void Log()
    {
        distances.Add(GetDistance(sphere, fingerTipMock));
        _measurementId++;
    }

    public List<float> GetDistances()
    {
        return distances;
    }

    public bool GetIdle()
    {
        return idle;
    }

    public bool GetLeftHanded()
    {
        return leftHanded;
    }

    public bool GetStanding()
    {
        return standing;
    }

    public float GetDistance(GameObject obj1, GameObject obj2)
    {
        return Vector3.Distance(obj1.transform.position, obj2.transform.position);
    }
    #endregion

    #region Event Redirecting Methods
    public void OnServerSaidStart()
    {
        idle = false;
    }
    public void OnServerSaidStop()
    {
        idle = true;
    }
    public void OnServerSetPath()
    {
        PlaceTrackForwardFromHeadset();
        PlaceSphereAboveTrack();
        PlaceLightWhereTrack();
    }
    public void OnServerSetLeft()
    {
        leftHanded = true;
    }

    #endregion
}
