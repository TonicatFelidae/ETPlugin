using ET.SupportKit.Collection;
using System;
using System.Collections.Generic;
using UnityEngine;
using ET.FileSystem;
using UnityEngine.Events;
using System.Collections;
using System.Linq;
using ET.FileSystem.FileReader;

namespace ET.Module.Dialogue
{
    ///<summary>
    /// <1> 2 type: 
    /// - Conversation is two people talking with each other
    /// - Monologue is one person selftalk
    /// Both use Conversation.struct
    /// <2> each Dia file and convert to one separate Conversation item in data
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        #region Data
        public Dictionary<string, Conversation> data;

        [Header("DATA EXTRACT")]
        public bool readAllDataOnAwake;
        public string loadingProtocolID = "DialogueManagerProtocol";
        public string dataFileID = "Dia";
        public int readFromIndex = 0; // current min check data
        public int readToIndex = 100; // current max check data, smaller is better

        [Header("CLASS REFERENCES")]
        [SerializeField] private FileReaderManager _fileReaderManager;

        #endregion
        //Current

        [NonSerialized] public bool isPlaying = false;
        private string _curIndex; // cur index of conversation
        private string CurIndex // cur index of conversation
        {
            get => _curIndex;
            set
            {
                if (_curIndex != value)
                {
                    isPlaying = true;
                    _curIndex = value;
                    onConversationChange?.Invoke(data[CurIndex]);
                    data[CurIndex].onStart?.Invoke();
                    _curDialogue = 0;
                }
                else
                {
                    isPlaying = true;
                    if (_curDialogue >= data[CurIndex].dialogues.Count)
                    {
                        ConversationFinished();
                    }
                    else
                    {
                        onConversationPlay?.Invoke(data[CurIndex].dialogues[_curDialogue]);
                        _curDialogue += 1;
                    }
                }

            }
        }
        private int _curDialogue; // cur index of dialogue
        [Header("EVENTS")]
        public UnityEvent<Conversation> onConversationChange = new();
        public UnityEvent<Dialogue> onConversationPlay = new();
        public UnityEvent<Conversation> onConversationFinish = new();

        private void Awake()
        {
            _fileReaderManager.AddDataLoadingProtocol(
                new DataLoadingProtocol(
                    loadingProtocolID,
                    dataFileID,
                    DataPathLocation.StreamingAssetsData,
                    FolderPath.DialogueData,
                    FileReaderID.Csv_TextLines_Comma,
                    DataExtractorFunc_Dialogue.KeyConversation
                    )
                );
            if (readAllDataOnAwake) // awake and auto get all conveersation data
            {
                ReadAllData();
            }
        }
        public void ReadAllData()
        {
            SetData(_fileReaderManager.GetData(loadingProtocolID, readFromIndex, readToIndex));
        }


        #region Play Entry
        public void SetRandomIndexAndPlay(string index, UnityAction onStart = null, UnityAction onFinish = null)
        {
            CurIndex = GetRandomIDFromAlternativeGroup(index);
        }
        public void SetIndexAndPlay(string index, UnityAction onStart = null, UnityAction onFinish = null)
        {
            CurIndex = index;
        }
        #endregion
        /// <summary>
        /// This play use to repeat and continue conversation
        /// </summary>
        public void Play()
        {
            CurIndex = _curIndex;
        }
        private void ConversationFinished()
        {
            onConversationFinish?.Invoke(data[CurIndex]); // DialogUI finish event
            //onConversationFinish?.Invoke(data[CurIndex]); // DialogUI finish event event
            //reserve previousConversation
            Conversation previousConversation = data[CurIndex];
            //reset to default, also to easy to detect error
            _curDialogue = 0;
            _curIndex = null;
            isPlaying = false;
            //invoke previousConversation
            previousConversation.onFinish?.Invoke(); // own dialougue finish, may invoke a new dialogue
        }
        private string GetRandomIDFromAlternativeGroup(string inputIndex)
        {
            List<string> listKeys = data.Where(kv => kv.Key.Contains(inputIndex)).Select(kv => kv.Key).ToList();
            return listKeys.RandomElement();
        }
        #region Data
        public void SetData(List<Dictionary<string, object>> rawListDataFromFileManager)
        {
            if (data == null) data = new();
            foreach (var item in rawListDataFromFileManager)
            {
                data = data.Merge(item.ToDictionary_Dynamic(DataExtractorConverterFunc_Dialogue.objConversation));
            }
            D.Sys.File($"Dialogue Data: {data.Count}");
        }
        public Conversation GetConversation(string index)
        {
            return data[index];
        }
        #endregion
    }
}

