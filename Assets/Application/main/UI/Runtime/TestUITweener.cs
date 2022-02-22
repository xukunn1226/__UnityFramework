using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Application.Runtime
{
    /// <summary>
    /// Tweener: 补间动画
    /// Sequence：Tweener链表，按顺序执行一串Tweener
    /// Tween：Tweener + Sequence
    /// </summary>
    public class TestUITweener : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            DOTween.Init(true, true);
            DOTween.SetTweensCapacity(100, 100);

            // method 1.
            // DOTween.To()

            // method 2.
            transform.DOLocalMove(new Vector3(0, 0, 0), 5);

            // method 3.
            // DOTween.ToAlpha();
        }

        void TestSequence()
        {
            // Grab a free Sequence to use
            Sequence mySequence = DOTween.Sequence();
            // Add a movement tween at the beginning
            mySequence.Append(transform.DOMoveX(1, 1));
            // Add a rotation tween as soon as the previous one is finished
            mySequence.Append(transform.DORotate(new Vector3(0, 180, 0), 1));
            // Delay the whole Sequence by 1 second
            mySequence.PrependInterval(1);
            // Insert a scale tween for the whole duration of the Sequence
            mySequence.Insert(0, transform.DOScale(new Vector3(3, 3, 3), mySequence.Duration()));
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}