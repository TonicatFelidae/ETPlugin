using ET.Module.VersionControlSystem;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ET.Module.TsakaFieldSystem
{
    public class TsakaField
    {
        private ElementalField _field;
        internal ElementalField Field => _field;
        private IEnumerator enumerator;
        private MonoBehaviour _body;
        private Coroutine _coroutine;
        private string _baseWind = "0.1";
        private string _curWind => Application.version;
        private string _curWater => $"TaniaFeldiae{Key}";
        private string Key
        {
            get
            {
                string ret = "1999";
                ret = GameObject.FindAnyObjectByType<ETMagicLightPoint>().lightPoint;
                return ret;
            }
        }
        /// <summary>
        /// cast magic barrier to protect me game :>
        /// It can work separatly with ETVersionControl
        /// it default reconize version is 0.1
        /// </summary>
        public void Cast(GameObject wizardBody, UnityAction onConnectionFailed, UnityAction onPassBarrier, UnityAction onNotPassBarrier, int manaUse)
        {
            _body = wizardBody.AddComponent<ETMagicFirePoint>();
            if (_coroutine != null)
            {
                _body.StopCoroutine(_coroutine);
                _coroutine = null;
            }
            _coroutine = _body.StartCoroutine(GetVersionInfo(onConnectionFailed,onPassBarrier, onNotPassBarrier));
        }
        private string GenLink()
        {
            string me = "tonicatfelidae";
            string very = "github";
            string cute = "io";
            string dotx = ".";
            return "https://" + me + dotx + very + dotx + cute + "/barrier_" + Application.productName + ".json";
        }
        internal IEnumerator GetVersionInfo(UnityAction onConnectionFailed,UnityAction onPassBarrier, UnityAction onNotPassBarrier)
        {
            string xx = _baseWind + "/steam/" + _baseWind;
            UnityWebRequest water = UnityWebRequest.Get(GenLink());
            yield return water.SendWebRequest();
        
            if (water.result != UnityWebRequest.Result.Success)
            {
                onConnectionFailed?.Invoke();
            }
            else
            {
                string fire = water.downloadHandler.text;
                _field = JsonUtility.FromJson<ElementalField>(fire);
                Process(_field,onPassBarrier, onNotPassBarrier);
            }
        }
        private void Process(ElementalField elementalField, UnityAction onPassBarrier, UnityAction onNotPassBarrier)
        {
            string xx = _baseWind + _baseWind;
            switch (elementalField.fire)
            {
                case "T32D092":
                    if (_curWater == elementalField.water) onPassBarrier.Invoke();
                    else onNotPassBarrier.Invoke();
                    break;
                case "T73D923":
                    if (_curWind == elementalField.wind) onPassBarrier.Invoke();
                    else onNotPassBarrier.Invoke();
                    break;
                case "T34D953":
                    if (_curWind == elementalField.wind && _curWater == elementalField.water) onPassBarrier.Invoke();
                    else onNotPassBarrier.Invoke();
                    break;
                case "T29D345":
                    if (IsVersionOneLargerOrEqual(_curWind, elementalField.wind)) onPassBarrier.Invoke();
                    else onNotPassBarrier.Invoke();
                    break;
                case "T29D035":
                    if (IsVersionOneLargerOrEqual(_curWind, elementalField.wind) && _curWater == elementalField.water) onPassBarrier.Invoke();
                    else onNotPassBarrier.Invoke();
                    break;
                default:
                    break;
            }
        }

        private bool IsVersionOneLargerOrEqual(string version1, string version2)
        {
            // Split version strings into components
            int[] v1 = version1.Split('.').Select(int.Parse).ToArray();
            int[] v2 = version2.Split('.').Select(int.Parse).ToArray();

            // Compare each component
            for (int i = 0; i < Math.Min(v1.Length, v2.Length); i++)
            {
                if (v1[i] < v2[i])
                    return false; // version1 is smaller
                else if (v1[i] > v2[i])
                    return true; // version1 is larger
            }

            // If all components are equal so far, the longer version is considered larger
            return true;
        }
        [Serializable]
        public class ElementalField
        {
            public string fire;
            public string wind;
            public string water;
        }
    }
    public class NextVersionNoffication
    {
        public string fire = "fire";
        public string wind = "wind";
        public string water = "water";
    }
}