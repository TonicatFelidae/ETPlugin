using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI
{
    public class UIToggle : MonoBehaviour
    {
        public delegate void ChangeHandler(bool isOn);
        private List<ChangeHandler> _delegates = new List<ChangeHandler>();

        private ChangeHandler _realOnChanged;

        public Image _optImage = null;
        public UnityEngine.UI.Toggle _toggle = null;
        public Image _icon;
        public Sprite _spriteOn;
        public Sprite _spriteOff;
        private bool _isOn;

        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                _isOn = value;
                _toggle.isOn = _isOn;
                onChange(_isOn);
            }
        }

        public Sprite SpriteBackground
        {
            set
            {
                _optImage.sprite = value;
            }
        }

        public event ChangeHandler OnChange
        {
            add
            {
                _realOnChanged += value;
                _delegates.Add(value);
            }

            remove
            {
                _realOnChanged -= value;
                _delegates.Remove(value);
            }
        }

        public Color Color
        {
            set
            {
                if (_optImage != null)
                {
                    _optImage.color = value;
                }
            }
        }

        public void Awake()
        {
            if (_toggle == null)
                throw new System.NullReferenceException();

            _toggle.onValueChanged.AddListener(onChange);
            _isOn = _toggle.isOn;
            setSprite();
        }

        public void RemoveAllListeners()
        {
            for (int i = 0, c = _delegates.Count; i < c; ++i)
            {
                var d = _delegates[i];
                _realOnChanged -= d;
            }
            _delegates.Clear();
        }

        public void SetInitValue(bool isOn)
        {
            _isOn = isOn;
            setValue(_isOn);
            setSprite();
            if (_realOnChanged != null) _realOnChanged(_isOn);
        }

        private void setValue(bool isOn)
        {
            _toggle.isOn = isOn;
        }

        private void onChange(bool isOn)
        {
            _isOn = isOn;
            setSprite();
            if (_realOnChanged != null) _realOnChanged(_isOn);
        }

        private void setSprite()
        {
            if (_isOn)
            {
                _icon.sprite = _spriteOn;
            }
            else
            {
                _icon.sprite = _spriteOff;
            }
        }
    }
}
