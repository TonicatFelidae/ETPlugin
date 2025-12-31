using ET.SupportKit;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ET.UIKit
{
    public class EULoadingTextUnit : MonoBehaviour
    {
        public string loadingText = "Loading";
        public float span = 1;
        public string dot = ".";
        public int dotAmount = 5;
        private string _curText;
        private int _curDotID = 0;
        private float _curSpan;
        public TextMeshProUGUI tx_loadingText;
        public bool usingCorontireOnEnable = false;
        public void Init(string loadingText, float span, string dot, int dotAmount)
        {
            this.loadingText = loadingText;
            this.span = span;
            this.dot = dot;
            this.dotAmount = dotAmount;
            _curSpan = span;
            _curText = loadingText;
            _curDotID = 0;
        }
        void OnEnable()
        {
            if (usingCorontireOnEnable) StartCoroutine(AnimateLoadingText());
        }

        private IEnumerator AnimateLoadingText()
        {
            while (true)
            {
                if (_curSpan > 0)
                {
                    _curSpan -= Time.deltaTime;
                }
                else
                {
                    _curDotID = ETMath.ClampInLoop(_curDotID + 1, 0, dotAmount + 1);
                    _curText = "Loading";
                    if (_curDotID != 0)
                    {
                        for (int i = 1; i <= _curDotID; i++)
                        {
                            _curText += dot;
                        }
                    }
                    tx_loadingText.text = _curText;
                    _curSpan = span;
                }
                yield return null; // This makes the coroutine wait until the next frame.
            }
        }
        public string GetText()
        {
            if (_curSpan > 0)
            {
                _curSpan -= Time.deltaTime;
            }
            else
            {
                _curDotID = ETMath.ClampInLoop(_curDotID + 1, 0, dotAmount + 1);
                _curText = loadingText;
                Debug.Log(_curDotID);
                if (_curDotID != 0)
                {
                    for (int i = 1; i <= _curDotID; i++)
                    {
                        _curText += dot;
                    }
                }
                _curSpan = span;
            }
            return _curText;
        }
    }
}
