using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin m_MultiChannelPerlin;

    private static CameraManager _myCamera;
    private static Camera cam;
    private Coroutine shake = null;

    public float movementSpeed = 10f;
    public float rotationSpeed = 3f;
    public float zoomSpeed = 5f;
    public float minZoomDistance = 5f;
    public float maxZoomDistance = 20f;

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
        cam = this.GetComponent<Camera>();
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.B))
        {
            SetFollowTarg(ClientManager.MyClient.playerCharacter.transform);
            SetLookTarg(ClientManager.MyClient.playerCharacter.transform);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            SetFollowTarg(null);
            SetLookTarg(null);
        }
        //Moving camera
        if (Input.GetKey(KeyCode.V))
        {
            float horizontal = 0;
            float vertical = 0;
            float up = 0;
            Vector3 upAxis = transform.up;
            if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1;
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1;
            if (Input.GetKey(KeyCode.UpArrow)) vertical = 1;
            if (Input.GetKey(KeyCode.DownArrow)) vertical = -1;
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.Space)) up = 1;
                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();
                Vector3 right = transform.right;

                Vector3 moveDirection = (forward * vertical + right * horizontal + transform.up * up).normalized;

                virtualCamera.transform.position += moveDirection * movementSpeed * Time.deltaTime;
            }
            else
            {
                if (Input.GetKey(KeyCode.Space)) up = -1;
                Vector3 moveDirection = (transform.up * up).normalized;
                virtualCamera.transform.position += moveDirection * movementSpeed * Time.deltaTime;
                Vector3 eulerRotation = new Vector3(-vertical, horizontal, 0f);
                virtualCamera.transform.Rotate(eulerRotation);
            }
        }
    }


    public Camera GetCamera()
    {
        return cam;
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
