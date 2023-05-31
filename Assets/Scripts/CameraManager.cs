using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;


    public void SetLookTarg(Transform targ)
    {
        virtualCamera.LookAt = targ;
    }

    public void SetFollowTarg(Transform targ)
    {
        virtualCamera.Follow = targ;
    }
}
