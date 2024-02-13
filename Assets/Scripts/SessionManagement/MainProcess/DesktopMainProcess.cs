using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesktopMainProcess: ExperimentNetworkServer
{
    // ui stuff
    [SerializeField] private TextMeshProUGUI connectionIndicator;
    
    [SerializeField] private TextMeshProUGUI summaryIndexIndicator;
    
    [SerializeField] private TMP_InputField participantIDTextField;
    [SerializeField] private Button setParticipantIdButton;
    
    [SerializeField] private Button setLeftHanded;
    [SerializeField] private Button setRightHanded;
    
    [SerializeField] private Button prepareButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button validateButton;
    [SerializeField] private Button invalidateButton;
    
    [SerializeField] private Button placeLightAndTrack;
    
    private bool connected = false;
    
    private int summaryIndex = 0;
    private MessageFromHelmet.Summary summary;

    private bool awaitingValidation;
    
    protected override void Start()
    {
        base.Start();
        
        connectionEstablished.AddListener(() => { connected = true; Render(); });
        connectionLost.AddListener(() => { connected = false; Render(); });

        setLeftHanded.onClick.AddListener(() => Send(new MessageToHelmet.SetLeftHanded(true)));
        setRightHanded.onClick.AddListener(() => Send(new MessageToHelmet.SetLeftHanded(false)));
        
        startButton.onClick.AddListener(() => Send(new MessageToHelmet(MessageToHelmet.Code.Start)));
        finishTrainingButton.onClick.AddListener(() => Send(new MessageToHelmet(MessageToHelmet.Code.Stop)));
        
        placeLightAndTrack.onClick.AddListener(() => Send(new MessageToHelmet(MessageToHelmet.Code.SetPath)));
        
        validateButton.onClick.AddListener(() =>
        {
            Send(new MessageToHelmet(MessageToHelmet.Code.ValidateTrial));
            awaitingValidation = false;
            Render();
        });
        invalidateButton.onClick.AddListener(() =>
        {
            Send(new MessageToHelmet(MessageToHelmet.Code.InvalidateTrial));
            awaitingValidation = false;
            Render();
        });

        setParticipantIdButton.onClick.AddListener(() =>
        {
            if (!Int32.TryParse(participantIDTextField.text, out int participantId))
            {
                Debug.LogError("Cannot set not integer participantId");
                return;
            }

            Send(new MessageToHelmet.SetParticipantID(participantId));
        });
        
        Render();
    }

    protected override void Receive(MessageFromHelmet message)
    {
        switch (message.code)
        {
            case MessageFromHelmet.Code.Summary:
                Debug.Log(message);
                summaryIndex++;
                summary = (message as MessageFromHelmet.Summary);
                Render();
                break;
            default:
                throw new ArgumentException($"It seems you have implemented a new message from helmet but forget to handle in {nameof(Receive)} method");
        }
    }

    private void Render()
    {
        connectionIndicator.gameObject.SetActive(true);
    }
}