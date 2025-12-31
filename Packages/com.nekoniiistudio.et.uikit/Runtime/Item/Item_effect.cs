using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Item_effect : MonoBehaviour
{
    // Start is called before the first frame updates
    public Appear appear;
    public Behaviour behaviour;
    public Disappear disappear;
    public GameObject[] appearEffects;
    public GameObject[] disappearEffects;
    public Transform targetLocation;

    private Tween tween;

    public GameObject BG0;
    public GameObject Icon0;
    public enum Appear
    {
        ZoomOut,
    }
    public enum Behaviour
    {
        MoveToTarget,

    }
    public enum Disappear
    {
        Pop,
    }
    private void Awake()
    {
        BG0.transform.localScale = Vector3.zero;
    }
    void Start()
    {
        if (!targetLocation) targetLocation = GameObject.Find("Target").transform; // test code, should set target when create the itemeffect
        StartCoroutine(EffectStart());
        switch (behaviour)
        {
            case Behaviour.MoveToTarget:
                StartCoroutine(TweenMoveToTarget());
                break;
            default:
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseOver()
    {

    }
    private void OnDestroy()
    {
        StartCoroutine(EffectDestroy());
    }
    
    public IEnumerator EffectStart()
    {
        if (appearEffects.Length>0)
        {
            int id = 0;
            switch (appear)
            {
                case Appear.ZoomOut:
                    id = 0;
                    break;
                default:
                    break;
            }
            GameObject go = Instantiate(appearEffects[id]);
            //destroy condition
        }

        yield return null;
    }

    public IEnumerator TweenMoveToTarget()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(BG0.transform.DOScale(1.25f, 0.75f));
        mySequence.Append(BG0.transform.DOScale(0.9f, 0.25f));
        mySequence.Append(BG0.transform.DOScale(1f, 0.25f));
        mySequence.Append(transform.DOMove(targetLocation.transform.position, 1.5f));
        mySequence.Append(transform.DOScale(0, 0.25f));
        yield return mySequence.Play().WaitForCompletion();
        Destroy(gameObject);
    }
    public IEnumerator EffectDestroy()
    {
        if (disappearEffects.Length > 0)
        {
            int id = 0;
            switch (disappear)
            {
                case Disappear.Pop:
                    id = 0;
                    break;
                default:
                    break;
            }
            GameObject go = Instantiate(disappearEffects[id]);
            //destroy condition
        }
        yield return null;
    }

}
