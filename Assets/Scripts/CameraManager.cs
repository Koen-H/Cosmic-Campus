using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    CinemachineBasicMultiChannelPerlin m_MultiChannelPerlin;

    private static CameraManager _myCamera;
    private static Camera camera;
    Coroutine shake = null;

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
        m_MultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        camera = this.GetComponent<Camera>();
    }

    public Camera GetCamera()
    {
        return camera;
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
        Transform playerTransform =  ClientManager.MyClient.playerCharacter.centerPoint.transform;
        SetFollowTarg(playerTransform);
        SetLookTarg(playerTransform);
    }


    public void ShakeCamera(float intensity, float time)
    {
        m_MultiChannelPerlin.m_AmplitudeGain = intensity;
        //m_MultiChannelPerlin.m_FrequencyGain = intensity;
        if (shake != null) StopCoroutine(shake);
        shake = StartCoroutine(Shaking(intensity, time));
    }

    IEnumerator Shaking(float intensity, float time)
    {
        float decreasePerSecond = intensity / time;

        while (m_MultiChannelPerlin.m_AmplitudeGain > 0)
        {
            m_MultiChannelPerlin.m_AmplitudeGain -= decreasePerSecond * Time.deltaTime;
            yield return null;
        }

        m_MultiChannelPerlin.m_AmplitudeGain = 0;
        //m_MultiChannelPerlin.m_FrequencyGain = 0;
    }

}
