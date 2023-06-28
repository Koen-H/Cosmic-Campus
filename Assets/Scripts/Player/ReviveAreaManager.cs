using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ReviveAreaManager : NetworkBehaviour
{
    [Tooltip("How long it takes to revive the player")]
    [SerializeField] float totalReviveTime = 5.0f;
    NetworkVariable<float> reviveTime = new(default,default,NetworkVariableWritePermission.Owner);

    [SerializeField] TextMeshPro displayTimer;
    [SerializeField] HealthBar progressBar;
    private PlayerCharacterController player;

    List<PlayerCharacterController> playersInArea = new();

    private void Awake()
    {
        player = GetComponentInParent<PlayerCharacterController>();
    }
    private void Update()
    {
        if (IsOwner) CheckRevive();
    }

    private void OnEnable()
    {
        playersInArea.Clear();
        reviveTime.OnValueChanged += OnReviveTimeChange;
        progressBar.SetMaxValue(totalReviveTime);
        if (!IsOwner) return;
            reviveTime.Value = totalReviveTime;

    }

    private void OnDisable()
    {
        playersInArea.Clear();
        reviveTime.OnValueChanged -= OnReviveTimeChange;
        CanvasManager.Instance.ToggleRevive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCharacterController otherPlayer = other.GetComponent<PlayerCharacterController>();
            if (otherPlayer.IsOwner) CanvasManager.Instance.ToggleRevive(true);
            playersInArea.Add(otherPlayer);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCharacterController otherPlayer = other.GetComponent<PlayerCharacterController>();
            if (otherPlayer.IsOwner) CanvasManager.Instance.ToggleRevive(false);
            playersInArea.Remove(otherPlayer);
        }
    }


    private void CheckRevive()
    {
        foreach(PlayerCharacterController player in playersInArea)
        {
            if(player.isReviving.Value) reviveTime.Value -= Time.deltaTime;
        }
    }

    void OnReviveTimeChange(float prevValue, float newValue)
    {
        progressBar.UpdateBar(newValue);
        displayTimer.text = ((int)newValue).ToString();
        if (IsOwner && reviveTime.Value < 0) player.Revive();
    }
}
