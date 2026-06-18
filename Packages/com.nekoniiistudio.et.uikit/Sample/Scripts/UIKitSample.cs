using System.Collections;
using System.Collections.Generic;
using ET.UIKit;
using UnityEngine;
namespace ETKitSample
{
    public class UIKitSample : MonoBehaviour
    {
        [SerializeField] private UISScrollView _scrollViewPrefab;
        public void Start()
        {
            Debug.Log("[UIKitSample] Test \nPress Space to instantiate a UIScrollView prefab and test its FocusOnTopContent method.");

        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _scrollViewPrefab.FocusOnTopContent();
            }
        }

    }
}