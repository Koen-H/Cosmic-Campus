using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    public OnMapNPC self;

    public event System.Action<Transform> OnTargetChange;
    private Transform currentTarget;

    [SerializeField] private GameObject door;
    [SerializeField] private Animator doorAnimation; 

    [HideInInspector] public Vector3 doorPosition; 
    [HideInInspector] public Vector3 doorNormal; 


    private void Start()
    {
        if (door != null)
        {
            GameObject newDoor = Instantiate(door, this.transform.parent);
            newDoor.transform.position = doorPosition;
            newDoor.transform.rotation = Quaternion.LookRotation(-doorNormal, Vector3.up);
            doorAnimation = newDoor.GetComponent<Animator>();
        }
    }


    public Transform CurrentTarget
    {
        get { return currentTarget; }
        set
        {
            if (currentTarget == value) return;
            currentTarget = value;
            OnTargetChange?.Invoke(currentTarget);
        }
    }

    public QuestNPCState enemyState = QuestNPCState.WAITING;

    public virtual OnMapNPC Interact(List<OnMapNPC> students, Transform player)
    {
        if(self is StudentNPC)
        {
            Debug.Log("Hey i am a student, please take me to my teacher UwU");
            //this.gameObject.SetActive(false);
            CurrentTarget = player;
            enemyState = QuestNPCState.FOLLOWING;
            Destroy(this.GetComponent<CapsuleCollider>());
            return self;
        }
        else if (self is TeacherNPC)
        {
            foreach (var requiredStudent in self.dependency)
            {
                if (!students.Contains(requiredStudent))
                {
                    Debug.Log("You need to collect more students!");
                    return null;
                }
            }
            Debug.Log("Thank you for bringing my students, You may continue");
            OpenDoor();
            students.Clear();
            
            return self; 
        }
        return null;
    }    

    public void OpenDoor()
    {

        doorAnimation.SetTrigger("Animate");
    }
}

public enum QuestNPCState { WAITING, FOLLOWING, BORED}
