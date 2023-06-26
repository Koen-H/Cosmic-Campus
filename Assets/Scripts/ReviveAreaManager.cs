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
    private PlayerCharacterController player;

    List<PlayerCharacterController> playersInArea = new();

    private void Awake()
    {
        player = GetComponentInParent<PlayerCharacterController>();
    }

    private void OnEnable()
    {
        playersInArea.Clear();
        reviveTime.OnValueChanged += OnReviveTimeChange;
        if (!IsOwner) return;
            reviveTime.Value = totalReviveTime;
    }

    private void OnDisable()
    {
        playersInArea.Clear();
        reviveTime.OnValueChanged -= OnReviveTimeChange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInArea.Add(other.GetComponent<PlayerCharacterController>());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInArea.Remove(other.GetComponent<PlayerCharacterController>());
        }
    }

    private void Update()
    {
        if (IsOwner) CheckRevive();
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
        displayTimer.text = ((int)newValue).ToString();
        if (IsOwner && reviveTime.Value < 0) player.Revive();
    }
}
