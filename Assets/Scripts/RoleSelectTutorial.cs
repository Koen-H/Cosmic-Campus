using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RoleSelectTutorial : MonoBehaviour
{
    public GameObject tutorialWindow;
    public GameObject screenContainer;
    private List<GameObject> screen;
    private int currentScreenIndex;

    public Button previousButton;
    public Button nextButton;

    private void Start()
    {
        screen = new List<GameObject>();
        

        // Add all child objects of the container to the list
        foreach (Transform child in screenContainer.transform)
        {
            screen.Add(child.gameObject);
        }

        if (screen.Count > 0)
        {
            currentScreenIndex = 0;
            ActivateCurrentObject();
        }

        nextButton.onClick.AddListener(NextObject);
        previousButton.onClick.AddListener(PreviousObject);
    }

    private void NextObject()
    {
        currentScreenIndex++;
        if (currentScreenIndex >= screen.Count)
            currentScreenIndex = 0;
        ActivateCurrentObject();
    }

    private void PreviousObject()
    {
        currentScreenIndex--;
        if (currentScreenIndex < 0)
            currentScreenIndex = screen.Count - 1;
        ActivateCurrentObject();
    }

    private void ActivateCurrentObject()
    {
        for (int i = 0; i < screen.Count; i++)
        {
            if (i == currentScreenIndex)
                screen[i].SetActive(true);
            else
                screen[i].SetActive(false);
        }
    }

    public void CloseTutorial()
    {
        tutorialWindow.SetActive(false); //from X Button
    }

    public void OpenTutorial()
    {
        tutorialWindow.SetActive(true); //from ? Button
    }


}



