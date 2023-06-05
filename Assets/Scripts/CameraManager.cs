using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;

    private static CameraManager _myCamera;
    public static CameraManager MyCamera
    {
        get
        {
            if (_myCamera == null) Debug.LogError("MyCamera is null");
            return _myCamera;
        }
    }
    private void Awake()
    {
        _myCamera = this;
    }

    public void SetLookTarg(Transform targ)
    {
        virtualCamera.LookAt = targ;
    }

    public void SetFollowTarg(Transform targ)
    {
        virtualCamera.Follow = targ;
    }

    public void TargetPlayer()
    {
        Transform playerTransform =  ClientManager.MyClient.playerCharacter.transform;
        SetFollowTarg(playerTransform);
        SetLookTarg(playerTransform);
    }
}
