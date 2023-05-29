using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ReviveAreaManager : NetworkBehaviour
{
    [Tooltip("How long it takes to revive the player")]
    [SerializeField] float totalReviveTime = 5.0f;
    NetworkVariable<float> reviveTime = new(default);

    [SerializeField] TextMeshPro displayTimer;
    private PlayerCharacterController player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerCharacterController>();
    }

    private void OnEnable()
    {
        reviveTime.OnValueChanged += OnReviveTimeChange;
        reviveTime.Value = totalReviveTime;
    }

    private void OnDisable()
    {
        reviveTime.OnValueChanged -= OnReviveTimeChange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerCharacterController>().SetReviveArea(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerCharacterController>().SetReviveArea(null);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnRevivingServerRpc()
    {
        reviveTime.Value -= Time.deltaTime;

    }

    void OnReviveTimeChange(float prevValue, float newValue)
    {
        displayTimer.text = ((int)newValue).ToString();
        if (IsOwner && reviveTime.Value < 0) player.ReviveServerRpc();
    }
}
