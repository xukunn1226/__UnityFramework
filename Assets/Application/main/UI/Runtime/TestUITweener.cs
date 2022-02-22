using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Application.Runtime
{
    /// <summary>
    /// Tweener: 补间动画
    /// Sequence：Tweener链表，按顺序执行一串Tweener
    /// Tween：Tweener + Sequence
    /// </summary>
    public class TestUITweener : MonoBehaviour
    {
        private RectTransform trans;
        public LoopType loopType;
        Vector2 centerPos;
        Image image;

        // Start is called before the first frame update
        void Start()
        {
            trans = GetComponent<RectTransform>();
            centerPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            image = GetComponent<Image>();

            DOTween.Init(true, true);
            DOTween.SetTweensCapacity(100, 100);


            TestUIScale();
            //TestSequence();
            //TestYoyo();
        }

        void TestTweener()
        {
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
            mySequence.PrependInterval(3);
            // Insert a scale tween for the whole duration of the Sequence
            float duration = mySequence.Duration();
            UnityEngine.Debug.Log(duration);
            mySequence.Insert(0, transform.DOScale(new Vector3(3, 3, 3), duration));
            mySequence.onComplete = () => { UnityEngine.Debug.Log("completed."); };
        }

        void TestYoyo()
        {
            var sequence = DOTween.Sequence()
           .Append(transform.DOLocalRotate(new Vector3(0, 0, 360), 1, RotateMode.FastBeyond360).SetRelative())
           .Join(transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 1, 10, 1f));
            sequence.SetLoops(-1, LoopType.Yoyo);
        }

        void TestUIScale()
        {
            Tweener paneltweener = trans.DOScale(new Vector3(2, 2, 1), 0.8f).SetEase(Ease.OutBack);
            paneltweener.SetAutoKill(false);
            paneltweener.Pause();
        }

        private bool isIn;
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if (!isIn)
                {
                    trans.DOPlayForward();
                    isIn = true;
                }
                else
                {
                    trans.DOPlayBackwards();
                    isIn = false;
                }
            }
        }

        void OnGUI()
        {
            if (GUILayout.Button("move to word pos(100,100)"))
            {
                //DoMove的坐标系是左下角为准,移动到100,100位置
                image.rectTransform.DOMove(new Vector2(100, 100), 1f);
            }
            if (GUILayout.Button("move to anchor pos(100,100)"))
            {
                image.rectTransform.DOMove(new Vector3(Screen.width * 0.5f + 100, Screen.height * 0.5f + 100, 0), 1f);
            }
            if (GUILayout.Button("add scale (2,2)"))
            {//每点击一次，在原始缩放基础上放大（2，2）
             //当前sacle(1,1,1)1秒内添加到(3,3,1)
                image.rectTransform.DOBlendableScaleBy(new Vector2(2, 2), 1f);
                //          image.rectTransform.DOScale (new Vector2(2,2),1f);

            }
            if (GUILayout.Button("scale to (2,2,1)"))
            {

                image.rectTransform.DOScale(new Vector3(2, 2, 1), 1f);

            }
            if (GUILayout.Button("rotate 180 degree"))
            {
                //旋转到180度
                image.rectTransform.DORotate(new Vector3(0, 0, 180), 1f);
            }


            if (GUILayout.Button("test tweener event"))
            {
                Tweener tweener = image.rectTransform.DOMove(new Vector3(Screen.width * 0.5f + 300, Screen.height * 0.5f - 100, 0), 1f);
                //tweener.OnPlay(OnPlay);
                //tweener.OnComplete(OnComplete);
                tweener.OnComplete(delegate () 
                {
                    Debug.Log("tween animation 结束");
                });
            }
        }
    }
}