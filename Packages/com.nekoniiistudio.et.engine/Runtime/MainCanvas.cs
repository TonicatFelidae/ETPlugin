using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Zenject.SpaceFighter;

namespace ET.UIKit
{ /// <summary>
  /// ET type zero class >>>> MainCanvas <<<
  /// 
  /// Components: 
  /// - Loading screen system
  /// </summary>
    public class MainCanvas : MonoBehaviour
    {
        public Transform bottomContent;
        public Transform content;
        public Transform topContent;
        public Transform tooltipContent;
        public Transform popupContent;
        private Animator animator;

        [SerializeField] GameObject[] _avartarObjects;
        [SerializeField] GameObject _loadingObject;
        [SerializeField] GameObject _loadingBlackout;
        //
        public GameObject avatar;
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        private void Start()
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
        #region Laoding Screen
        public void ShowLoadingScreenAni(bool show, bool isInstant = false)
        {
            if (show)
            {
                if (isInstant)
                {
                    _loadingObject.gameObject.SetActive(true);
                    _loadingBlackout.gameObject.SetActive(true);
                    _loadingBlackout.GetComponent<Image>().color = Color.black;  
                    animator.SetTrigger("ShowLoadingInstant");
                }
                else
                {
                    animator.SetTrigger("ShowLoading");
                }
            }
            else
            {
                animator.SetTrigger("HideLoading");

            }
        }
        public void ShowLoadingScreenAniAutoClose(float closeAfterSec, bool isInstant = false)
        {
            StopAllCoroutines();
            StartCoroutine(IEShowLoadingScreenAniAutoClose(closeAfterSec,isInstant));
        }
        IEnumerator IEShowLoadingScreenAniAutoClose(float closeAfterSec, bool isInstant = false)
        {
            ShowLoadingScreenAni(true, isInstant);
            yield return new WaitForSeconds(closeAfterSec);
            ShowLoadingScreenAni(false, isInstant);

        }
        #endregion
        public void FinishToilet()
        {
            animator.SetTrigger("FinishToilet");

        }
        public void FlashToBlack()
        {
            animator.SetTrigger("FlashToBlack");
        }
        public void ShowAvatar(bool enable)
        {
            avatar.SetActive(enable);
        }
        public void ShowAvatar(string speakerName, bool enable = true)
        {
            avatar.SetActive(enable);
            foreach (var item in _avartarObjects)
            {
                item.SetActive(item.name == speakerName);
            }
        }
        #region Debug
        public void D_ShowLoading() => animator.SetTrigger("ShowLoading");
        public void D_HideLoading() => animator.SetTrigger("HideLoading");
        public void D_ShowLoadingInstant() => animator.SetTrigger("ShowLoadingInstant");
        #endregion
    }
}

