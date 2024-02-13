using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HelmetMainProcess: ExperimentNetworkClient
{
    private int participantId = 1;
    private bool leftHanded = false;

    private int currentRunConfigIndex; // for example 4, means that previous 4 runs (0,1,2,3) were fulfilled already
    private RunStage currentRunStage = RunStage.Idle; // is this run already in progress or not

    [SerializeField] private ExperimentManager experimentManager;
    
    [Serializable]
    public enum RunStage
    {
        Idle,
        Running
    }

    private void SendSummary()
    {
        var summary = new MessageFromHelmet.Summary();

        summary.id = participantId;
        summary.left = leftHanded;
        summary.distances = experimentManager.GetDistances();

        Send(summary);
    }
    
    protected override void Start()
    {
        base.Start();
        experimentManager.measurement.AddListener(SendSummary);
    }

    protected override void Receive(MessageToHelmet message)
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
                    experimentManager.OnServerSaidPrepare(_runConfigs[currentRunConfigIndex]);
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
                    experimentManager.OnServerSaidFinishTraining();
                }
                break;
            default:
                throw new ArgumentException($"It seems you have implemented a new message from helmet but forget to handle in {nameof(Receive)} method");
        }
    }
}