using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using ET.SupportKit.Collection;

namespace ET.Module.Dialogue
{
    public struct Conversation
    {
        public string ID; // ID of this conversation: example cv_talk1,cv_talk2,cv_miaAndCia
        public List<Dialogue> dialogues; // speaker and their content, may have multi line
        public UnityAction onStart;
        public UnityAction onFinish;
        public SpeakerType speakerType;

        public Conversation(string _ID, List<Dialogue> _dialogues)
        {
            ID = _ID;
            dialogues = _dialogues;
            onStart = null;
            onFinish = null;
            speakerType = SpeakerType.Default;
        }
        public Conversation SetSpeaker(SpeakerType speakerType)
        {
            this.speakerType = speakerType;
            return this;
        }
        public Conversation SetOnStart(UnityAction onStart)
        {
            this.onStart = onStart;
            return this;
        }
        public Conversation SetOnFinish(UnityAction onFinish)
        {
            this.onFinish = onFinish;
            return this;
        }
    }
    public struct Dialogue
    {
        public string ID; // Speaker
        public string actionCode; // [action]
        public string content; // Content use "//" to interrupt conversations

        public Dialogue(string iD, string actionCode, string content)
        {
            ID = iD;
            this.actionCode = actionCode;
            this.content = content;
        }
        // public UnityAction onStart; // not use, dialogues event need to set by developer not supporter
        // public UnityAction onFinish; // not use,  dialogues event need to set by developer not supporter


    }
    public enum SpeakerType
    {
        Default,
        NaraBox,
        SingleConverstion,
    }
}

