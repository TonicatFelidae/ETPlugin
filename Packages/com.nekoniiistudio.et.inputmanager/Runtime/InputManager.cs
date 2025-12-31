using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using ET.Module.ETInput;

namespace ET.Module
{
    public class InputManager : MonoBehaviour
    {
        void Expample()
        {
            if (Input.GetKey(this.GetKey(InputCode.Left)))
            {
                //do something
            }
        }
        /*
         * Current event:
         * Key down
         * Current thread:
         * - ob to input
         * - init to input and change to input
         * Plan: 
         * Divide keycode event onkey, onkeydown,onkeyup 
         *
         */
        public List<IInputItem> keyCodeItems;
        public List<ButtonItem> buttonItems;
        public List<EventItem> eventItems;

        private List<IInputItem> onClickItem;

        private List<InputKey> inputKeys;

        private InputSetting _data;
        public void Init(InputSetting inputSettingData)
        {
            _data = inputSettingData;   
        }
        private void Awake()
        {
            onClickItem = new List<IInputItem>();
            if (keyCodeItems == null) keyCodeItems = new List<IInputItem>();
            TranslateKeycode();
        }
        private void TranslateKeycode()
        {
            for (int i = 0; i < keyCodeItems.Count; i++)
            {
                keyCodeItems[i].SetKeyCode((KeyCode)Enum.Parse(typeof(KeyCode), keyCodeItems[i].KeyCodeText));
            }
        }
        private void Update()
        {
            //obsolete
            if (Input.anyKeyDown)
            {
                foreach (IInputItem item in keyCodeItems)
                {
                    if (Input.GetKeyDown(item.KeyCodex))
                        switch (item)
                        {
                            case ButtonItem b:
                                if (!b.button.interactable || !b.button.gameObject.activeSelf) return;
                                b.button.onClick.Invoke();
                                break;
                            case EventItem e:
                                e.unityAction.Invoke();
                                break;
                            default:
                                break;
                        }
                }

            }
        }
        public void AnimationClickButton() // use late
        {
            //
            //var originScale = _targetButton.transform.localScale;
            //var sequence = DOTween.Sequence();
            //sequence.Append(_targetButton.transform.DOScale(originScale * 0.9f, 0.1f));
            //sequence.Append(_targetButton.transform.DOScale(originScale, 0.1f));
            //sequence.OnComplete(() =>
            //{
            //    _animation = false;
            //    _targetButton.onClick.Invoke();
            //});
        }
        public void Assign(Button button, string keyCodeText, TextMeshProUGUI keyCodeUIText = null)
        {
            ButtonItem buttonItem = new ButtonItem(button, keyCodeText);
            buttonItem.SetKeyCode((KeyCode)Enum.Parse(typeof(KeyCode), buttonItem.keyCodeText));
            if (!keyCodeItems.Contains(buttonItem)) keyCodeItems.Add(buttonItem);
        }
        public void Assign(Button button, KeyCode keyCode, TextMeshProUGUI keyCodeUIText = null)
        {
            ButtonItem buttonItem = new ButtonItem(button, keyCode.ToString());
            buttonItem.SetKeyCode(keyCode);
            if (!keyCodeItems.Contains(buttonItem)) keyCodeItems.Add(buttonItem);
        }
        public void Assign(IInputItem keyCodeItem)
        {
            IInputItem item = keyCodeItem;
            item.SetKeyCode((KeyCode)Enum.Parse(typeof(KeyCode), item.KeyCodeText));
            if (!keyCodeItems.Contains(item)) keyCodeItems.Add(item);
        }
        public void Assign(EventItem_Action unityAction, string keyCodeText)
        {
            EventItem eventItem = new EventItem(unityAction, keyCodeText);
            eventItem.SetKeyCode((KeyCode)Enum.Parse(typeof(KeyCode), eventItem.keyCodeText));
            if (!keyCodeItems.Contains(eventItem)) keyCodeItems.Add(eventItem);
        }
        public void Assign(EventItem_Action unityAction, KeyCode keyCode)
        {
            EventItem eventItem = new EventItem(unityAction, keyCode.ToString());
            eventItem.SetKeyCode(keyCode);
            if (!keyCodeItems.Contains(eventItem)) keyCodeItems.Add(eventItem);
        }

        // get
        public KeyCode GetKey(InputCode inputCode)
        {
            return _data.inputDynamicKeyCodes.Find(x => x.inputCode == inputCode).keyCode1;
        }
        //Invoke
        public bool InvokeEvent_TwoKeyOpposite(ref byte inputState, InputCode inputCode1, InputCode inputCode2, UnityAction action1, UnityAction action2) //update
        {
            if (Input.GetKey(GetKey(inputCode1)) && Input.GetKey(GetKey(inputCode2)))
            {
                if (Input.GetKeyDown(GetKey(inputCode1)))
                {
                    inputState = 1;
                }
                else if (Input.GetKeyDown(GetKey(inputCode2)))
                {
                    inputState = 2;
                }
            }
            else
            {
                inputState = 0;
                if (Input.GetKey(GetKey(inputCode1)))
                {
                    inputState = 1;
                }
                if (Input.GetKey(GetKey(inputCode2)))
                {
                    inputState = 2;
                }
            }
            if (inputState == 1)
            {
                action1.Invoke();
                return true;
            }
            else if (inputState == 2)
            {
                action2.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }
        //Manager
        public bool CheckKeyCodeExits(ref InputCode ret, KeyCode newKeyCode)
        {
            DynamicKeyCode DynamicKeyCode1 = _data.inputDynamicKeyCodes.Find(x => x.keyCode1 == newKeyCode);
            if (DynamicKeyCode1 != null)
            {
                ret = DynamicKeyCode1.inputCode;
                return true;
            }
            else return false;
        }
        public void ChangeInputCode(InputCode curInputCode, KeyCode newKeyCode)
        {
            DynamicKeyCode DynamicKeyCode1 = _data.inputDynamicKeyCodes.Find(x => x.inputCode == curInputCode);
            KeyCode curKeyCode = DynamicKeyCode1.keyCode1;
            DynamicKeyCode1.keyCode1 = newKeyCode;
            DynamicKeyCode DynamicKeyCode2 = _data.inputDynamicKeyCodes.Find(x => x.keyCode1 == curKeyCode);
            DynamicKeyCode2.keyCode1 = curKeyCode;
        }
    }
    public interface IInputItem
    {
        public string KeyCodeText { get; set; }
        public KeyCode KeyCodex { get; }
        public string PlayerPrefKey { get; }
        public void SetKeyCode(KeyCode key);
    }
    [Serializable]
    public struct InputKey
    {
        public InputCode inputCode;
        public EventItem inputItem;
    }
    [Serializable]
    public class DynamicKeyCode
    {
        public string indexText;
        public InputCode inputCode;
        public KeyCode keyCode1;
        public KeyCode keyCode2; // currently only use keyCode1
        [NonSerialized] public string keyCodePlayerPref;
    }
    [Serializable]
    public struct ButtonItem : IInputItem
    {
        public Button button;
        public string keyCodeText;
        [NonSerialized] public KeyCode keyCode;
        [NonSerialized] public Animator clickAnimator;
        public string playerPrefKey;
        public ButtonItem(Button button, string keyCodeText) : this()
        {
            this.button = button;
            this.keyCodeText = keyCodeText;
        }

        public string KeyCodeText { get => keyCodeText; set => keyCodeText = value; }
        public KeyCode KeyCodex => keyCode;
        public string PlayerPrefKey { get => playerPrefKey; }

        public void SetKeyCode(KeyCode key)
        {
            keyCode = key;
        }
        public void PlayAnimator()
        {
            if (clickAnimator)
            {
                clickAnimator.Play("onClickDown");
            }
        }
    }
    [Serializable]
    public class EventItem_Action : UnityEvent { }
    [Serializable]
    public struct EventItem : IInputItem
    {
        public EventItem_Action unityAction;
        public string keyCodeText;
        [NonSerialized]
        public KeyCode keyCode;
        public string playerPrefKey;

        public EventItem(EventItem_Action unityAction, string keyCodeText) : this()
        {
            this.unityAction = unityAction;
            this.keyCodeText = keyCodeText;
        }
        public string KeyCodeText { get => keyCodeText; set => keyCodeText = value; }
        public KeyCode KeyCodex { get => keyCode; }
        public string PlayerPrefKey { get => playerPrefKey; }
        public void SetKeyCode(KeyCode key)
        {
            keyCode = key;
        }
    }
}

