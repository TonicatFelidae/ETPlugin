using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ET.Module.Dialogue;

public class DialogueManagerDemo : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public void Start()
    {

        dialogueManager.onConversationChange.AddListener(OnChange);
        dialogueManager.onConversationPlay.AddListener(OnPlay);
        dialogueManager.onConversationFinish.AddListener(OnFinish);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) {
            dialogueManager.SetIndexAndPlay("0");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            dialogueManager.SetIndexAndPlay("3");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            dialogueManager.SetIndexAndPlay("4");
        }
    }
    public void OnChange(Conversation data)
    {
        Debug.Log("Change to "+ data.ID);
    }
    public void OnPlay(Dialogue data)
    {
        Debug.Log($"{data.ID} action {data.actionCode} : {data.content} ");

    }
    public void OnFinish(Conversation data)
    {
        Debug.Log("End "+ data.ID);
    }
}
