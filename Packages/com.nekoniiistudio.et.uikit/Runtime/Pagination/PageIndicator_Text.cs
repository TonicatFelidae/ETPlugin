using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ET.UIKit.PaginationSystem
{
    public class PageIndicator_Text : PageIndicator
    {
        [SerializeField] TextMeshProUGUI tx_pageIndicator;
        public override void Show(int curMaxItemCount, int curPage, int itemPerPage, int curPageFromNumber, int curPageToNumber)
        {
            base.Show(curMaxItemCount, curPage, itemPerPage, curPageFromNumber, curPageToNumber);
            tx_pageIndicator.text = $"{curPageFromNumber+1}-{curPageToNumber+1}|{curMaxItemCount}";
        }
    }
}