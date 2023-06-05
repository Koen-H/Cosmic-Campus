using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    public OnMapNPC self;

    public event System.Action<Transform> OnTargetChange;
    private Transform currentTarget;
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
            students.Clear();
            return self; 
        }
        return null;
    }    
}

public enum QuestNPCState { WAITING, FOLLOWING, BORED}
