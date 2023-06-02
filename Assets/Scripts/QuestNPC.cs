using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    public OnMapNPC self; 


    public virtual OnMapNPC Interact(List<OnMapNPC> students)
    {
        if(self is StudentNPC)
        {
            Debug.Log("Hey i am a student, please take me to my teacher UwU");
            this.gameObject.SetActive(false);
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
        }
        return null;
    }    
}
