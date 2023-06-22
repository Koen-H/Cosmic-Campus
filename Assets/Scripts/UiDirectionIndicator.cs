using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UiDirectionIndicator: MonoBehaviour
{
    public Transform player;

    public Camera cam;
    public RectTransform canvas;
    public RectTransform self; 
    [SerializeField] Image fishIcon;
    [SerializeField] RectTransform arrow;

    [SerializeField] float margin;
    [SerializeField] float deathDelayPopUpEnd;

    public bool enableIndicator;
    public bool instaKill;

    bool tracking; 

    Vector3 ogSize;
    float spawnAnimationDelay = 0.2f;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) Enable();
        if (Input.GetKeyDown(KeyCode.K)) Disable();
    }
    public void SetFishImage(Sprite image)
    {
        fishIcon.sprite = image;
    }

    private void Start()
    {
        player = ClientManager.MyClient.playerCharacter.transform;

        cam = Camera.main;
        canvas = CanvasManager.Instance.GetGameUI().GetComponent<RectTransform>();


        ogSize = self.localScale;
        self.localScale = Vector3.zero;
    }

    private void Enable()
    {
        tracking = true;
        self.gameObject.SetActive(true);
        StopAllCoroutines(); // to stop Deactivation of self
        StartCoroutine(ScaleOverTime(self, ogSize, spawnAnimationDelay));
    }

    private void Disable()
    {
        tracking = false;
        StartCoroutine(ScaleOverTime(self, Vector3.zero, spawnAnimationDelay));
        StartCoroutine(DeactivateWithDelay(self.gameObject, spawnAnimationDelay));
    }
    private void FixedUpdate()
    {
            SnapToPlayerPosition(player);
    }


    private void SnapToPlayerPosition(Transform playerPosition)
    {
        if (playerPosition == null) return;

        Vector3 pointOnPlane = cam.transform.position;
        Vector3 point = Vector3.zero;  
        Vector3 normal = Vector3.Cross(cam.transform.right, cam.transform.up);
        Vector3 planeToPoint = point - pointOnPlane;
        float dotProduct = Vector3.Dot(planeToPoint, normal);

        Vector3 screenPosition = cam.WorldToScreenPoint(Vector3.zero);

        if (dotProduct < 0) screenPosition *= -1;

        if (screenPosition.x > cam.pixelWidth - margin) screenPosition.x = cam.pixelWidth - margin;
        if (screenPosition.y > cam.pixelHeight - margin) screenPosition.y = cam.pixelHeight - margin;
        if (screenPosition.x < margin) screenPosition.x = margin;
        if (screenPosition.y < margin) screenPosition.y = margin;

        self.anchoredPosition = screenPosition;
    }
    IEnumerator ScaleOverTime(Transform target, Vector3 endScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float time = 0;

        while (time < duration)
        {
            target.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        target.localScale = endScale;
    }

    IEnumerator DeactivateWithDelay(GameObject Object, float delay)
    {
        yield return new WaitForSeconds(delay);
        //Object.SetActive(false);
    }
    IEnumerator ActivateWithDelay(GameObject Object, float delay, bool animateScale)
    {
        yield return new WaitForSeconds(delay);
        if (Object != self.gameObject) Object.SetActive(true);
        if (animateScale)
        {
            StartCoroutine(ScaleOverTime(Object.transform, Vector3.one, spawnAnimationDelay));
        }
    }

    IEnumerator ActivateWithDelay(GameObject Object, float delay, bool animateScale, float animationSpeedMult)
    {
        yield return new WaitForSeconds(delay);
        if (Object != self.gameObject) Object.SetActive(true);
        if (animateScale)
        {
            StartCoroutine(ScaleOverTime(Object.transform, Vector3.one, spawnAnimationDelay * animationSpeedMult));
        }
    }

    IEnumerator ActivateWithDelay(GameObject Object, float delay, bool animateScale, Action<string> callback)
    {
        yield return new WaitForSeconds(delay);
        if (Object != self.gameObject) Object.SetActive(true);
        if (animateScale)
        {
            StartCoroutine(ScaleOverTime(Object.transform, Vector3.one, spawnAnimationDelay));
        }
        callback?.Invoke("death");
    }



    IEnumerator DisableSelf(float delay)
    {
        yield return new WaitForSeconds(delay);
        enableIndicator = false;
        if (!tracking) Disable();
    }
}
