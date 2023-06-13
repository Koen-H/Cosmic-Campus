using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class QuestNPC : NetworkBehaviour
{
    public OnMapNPC self;

    public event System.Action<Transform> OnTargetChange;
    private Transform currentTarget;

    [SerializeField] public  GameObject door;
    [SerializeField] public Animator doorAnimation;

    [HideInInspector] public Vector3 doorPosition;
    [HideInInspector] public Vector3 doorNormal;
    [HideInInspector] public int doorId; 


    private void Start()
    {
        if (door != null)
        {
/*            GameObject newDoor = Instantiate(door, this.transform.parent);
            newDoor.transform.position = doorPosition;
            newDoor.transform.rotation = Quaternion.LookRotation(-doorNormal, Vector3.up);
            doorAnimation = newDoor.GetComponent<Animator>();*/
        }
    }


    public Transform CurrentTarget
    {
        get { return currentTarget; }
        set
        {
            //if (currentTarget == value) return;
            currentTarget = value;
            OnTargetChange?.Invoke(currentTarget);
        }
    }

    public QuestNPCState enemyState = QuestNPCState.WAITING;

    private void OnTriggerEnter(Collider other)
    {
        if (self is TeacherNPC)
        {
            QuestStudentNPC student = other.GetComponent<QuestStudentNPC>();
            if (student && student.self is StudentNPC)
            {
                student.CurrentTarget = null;
                Destroy(student.GetComponent<CapsuleCollider>());
                self.requiredStudents--;
            }
            if (self.requiredStudents == 0) OpenDoorClientRpc();
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public virtual void InteractServerRpc(ServerRpcParams serverRpcParams = default)//List<OnMapNPC> students, 
    {
        if (self is StudentNPC && !CurrentTarget)
        {
            Debug.Log("Hey i am a student, please take me to my teacher UwU");
            //this.gameObject.SetActive(false);
            CurrentTarget = LobbyManager.Instance.GetClient(serverRpcParams.Receive.SenderClientId).playerCharacter.transform;
            enemyState = QuestNPCState.FOLLOWING;
            //Destroy(this.GetComponent<CapsuleCollider>());
            //return self;
        }
        /*        else if (self is TeacherNPC)
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
                }*/
        //return null;
    }    
    [ClientRpc]
    public void OpenDoorClientRpc()
    {
        if (IsServer)
        {
            doorAnimation.SetTrigger("Animate");
        }
        else
        {
            RoomGenerator.Instance.OpenDoorClientRpc(doorId);
        }
    }
}

public enum QuestNPCState { WAITING, FOLLOWING, BORED}
