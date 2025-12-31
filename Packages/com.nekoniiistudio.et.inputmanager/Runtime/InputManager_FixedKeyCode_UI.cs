using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ET.Module
{
    [RequireComponent(typeof(Button))]
    public class InputManager_FixedKeyCode_UI : MonoBehaviour
    {
        public string keyCodeText;
        [Header("Optional")]
        public TextMeshProUGUI keyCodeUIText;
        public Animator clickAnimator;
        private ButtonItem _buttonItem;
        private InputManager _manager;
        private void Awake()
        {
            _buttonItem.button = GetComponent<Button>();
            _buttonItem.keyCodeText = keyCodeText;
            if (clickAnimator) _buttonItem.clickAnimator = clickAnimator;
        }
        public void Init(InputManager inputManager)
        {
            _manager = inputManager;
        }
        private void Start()
        {
            setUIText();
            _manager.Assign(_buttonItem);
        }
        private void setUIText()
        {
            if(keyCodeUIText) keyCodeUIText.text = keyCodeText;
        }
    }
}