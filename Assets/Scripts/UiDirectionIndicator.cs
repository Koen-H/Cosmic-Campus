using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UiDirectionIndicator : MonoBehaviour
{
    private Camera cam;

    [SerializeField] RectTransform self; 
    [SerializeField] RectTransform arrow;
    [SerializeField] float margin;
    [SerializeField] float deathDelayPopUpEnd;

    [SerializeField] Image playerIcon;
    [SerializeField] Image arrowIcon;
    private PlayerCharacterController player;


    Vector3 ogSize;
    float spawnAnimationDelay = 0.2f;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) Enable();
        if (Input.GetKeyDown(KeyCode.K)) Disable();
    }

    public void SetData(Sprite PlayerIcon, Sprite ArrowIcon, PlayerCharacterController Player)
    {
        playerIcon.sprite = PlayerIcon;
        arrowIcon.sprite = ArrowIcon;
        player = Player;
        Debug.Log("Why player Null: " + Player);
    }

    private void Start()
    {
        //player = ClientManager.MyClient.playerCharacter.transform;

        cam = Camera.main;
        //canvas = CanvasManager.Instance.GetGameUI().GetComponent<RectTransform>();


        ogSize = self.localScale;
        self.localScale = Vector3.zero;
    }

    private void Enable()
    {
        self.gameObject.SetActive(true);
        StopAllCoroutines(); // to stop Deactivation of self
        StartCoroutine(ScaleOverTime(self, ogSize, spawnAnimationDelay));
    }

    private void Disable()
    {
        StartCoroutine(ScaleOverTime(self, Vector3.zero, spawnAnimationDelay));
        StartCoroutine(DeactivateWithDelay(self.gameObject, spawnAnimationDelay));
    }
    private void FixedUpdate()
    {
        SnapToPlayerPosition(player.transform);
    }


    private void SnapToPlayerPosition(Transform playerPosition)
    {
        if (playerPosition == null) return;

        Vector3 pointOnPlane = cam.transform.position;
        Vector3 point = Vector3.zero;  
        Vector3 normal = Vector3.Cross(cam.transform.right, cam.transform.up);
        Vector3 planeToPoint = point - pointOnPlane;
        float dotProduct = Vector3.Dot(planeToPoint, normal);

        Vector3 screenPosition = cam.WorldToScreenPoint(playerPosition.position);

        if (dotProduct < 0) screenPosition *= -1;
        Vector3 diffCheck = screenPosition; 

        if (screenPosition.x > cam.pixelWidth - margin) screenPosition.x = cam.pixelWidth - margin;
        if (screenPosition.y > cam.pixelHeight - margin) screenPosition.y = cam.pixelHeight - margin;
        if (screenPosition.x < margin) screenPosition.x = margin;
        if (screenPosition.y < margin) screenPosition.y = margin;

        self.anchoredPosition = screenPosition;

        Vector3 dir = Vector3.up;
        if (diffCheck != screenPosition) dir = cam.WorldToScreenPoint(playerPosition.position) - screenPosition;
        arrow.up = -dir;
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
}
