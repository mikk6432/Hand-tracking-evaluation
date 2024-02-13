using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MessageFromHelmet
{
    [Serializable]
    public enum Code
    {
        Summary,
    }

    public MessageFromHelmet()
    {
    }


    [Serializable]
    public class Summary : MessageFromHelmet
    {
        public int id;
        public bool left; // whether user is left handed

        public List<float> distances;

        public override string ToString()
        {
            return base.ToString() + $", participantId={id}," + (left ? "left" : "right") + $"Handed" + " Distances: [" +  String.Join(", ", distances.ToArray());
        }
    }
}

[Serializable]
class MessageToHelmet
{
    [Serializable]
    public enum Code
    {
        SetParticipantID,
        SetLeftHanded,

        SetPath,

        Start,
        Stop
    }

    public readonly Code code;

    public MessageToHelmet(Code _code)
    {
        code = _code;
    }

    public override string ToString()
    {
        return $"ToHelmet: code={Enum.GetName(typeof(Code), code)}";
    }

    [Serializable]
    public class SetParticipantID : MessageToHelmet
    {
        public readonly int participantID;

        public SetParticipantID(int _participantID) : base(Code.SetParticipantID)
        {
            participantID = _participantID;
        }

        public override string ToString()
        {
            return base.ToString() + $", participantID={participantID}";
        }
    }

    [Serializable]
    public class SetLeftHanded : MessageToHelmet
    {
        public readonly bool leftHanded;

        public SetLeftHanded(bool _leftHanded) : base(Code.SetLeftHanded)
        {
            leftHanded = _leftHanded;
        }

        public override string ToString()
        {
            return base.ToString() + $", leftHanded={leftHanded}";
        }
    }
}


public class HelmetMainProcess : MonoBehaviour
{
    private bool leftHanded = false;
    private int participantId = 0;
    private int currentRunConfigIndex; // for example 4, means that previous 4 runs (0,1,2,3) were fulfilled already
    private RunStage currentRunStage = RunStage.Idle; // is this run already in progress or not

    [SerializeField] private ExperimentManager experimentManager;

    [Serializable]
    public enum RunStage
    {
        Idle,
        Running
    }

    private void Send(MessageFromHelmet.Summary summary){
        // temp mock send method
    }

    protected void Start()
    {
        experimentManager.Start();
    }

    void Receive(MessageToHelmet message)
    {
        Debug.Log(message.ToString());
        switch (message.code)
        {
            case MessageToHelmet.Code.SetLeftHanded:
                if (currentRunStage == RunStage.Idle)
                {
                    leftHanded = (message as MessageToHelmet.SetLeftHanded).leftHanded;
                }
                break;
            case MessageToHelmet.Code.SetParticipantID:
                if (currentRunStage == RunStage.Idle)
                {
                    participantId = (message as MessageToHelmet.SetParticipantID).participantID;
                }
                break;
            case MessageToHelmet.Code.SetPath:
                if (currentRunStage == RunStage.Idle)
                {
                    experimentManager.OnServerSaidStart();
                }
                break;
            case MessageToHelmet.Code.Start:
                if (currentRunStage == RunStage.Idle)
                {
                    currentRunStage = RunStage.Running;
                    experimentManager.OnServerSaidStart();
                }
                break;
            case MessageToHelmet.Code.Stop:
                if (currentRunStage == RunStage.Running)
                {
                    currentRunStage = RunStage.Idle;
                    experimentManager.OnServerSaidStop();
                }
                break;
            default:
                throw new ArgumentException($"It seems you have implemented a new message from helmet but forget to handle in {nameof(Receive)} method");
        }
    }
}