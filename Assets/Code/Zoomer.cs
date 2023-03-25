using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class Zoomer : MonoBehaviour
    {
        [Header("Our Objects")]
        public GameObject Centre;
        public CanvasGroup CanvasGroup;

        [Header("Interp Config")]
        public float InLength;
        public float LiveLength;
        public float OutLength;

        [Header("Appearance Params")]
        //public bool UseBackgroundColour = false;
        //public Color BackgroundColour = Color.red;
        public float RotationRate = 1;
        public float ScaleRate = 1;

        public enum Phase
        {
            Unknown,

            Before,
            In,
            Live,
            Out,
            After
        }

        public Phase CurrentPhase { get; private set; } = Phase.Unknown;

        // run params
        // ParamOffset is the param where we finish fading-in...
        float ParamOffset;
        Controller Controller;

        // convenience
        bool Running => CurrentPhase != Phase.Before && CurrentPhase != Phase.After;
        float StartInParam => ParamOffset - InLength;
        float EndInParam => ParamOffset;
        float StartOutParam => ParamOffset + LiveLength;
        float EndOutParam => StartOutParam + OutLength;

        Tweener Tween;

        float LastParam = -1;
        float InitRotation;

        void Start()
        {
            var p = transform.parent;

            while (p != null && Controller == null)
            {
                Controller = p.GetComponent<Controller>();
                p = p.parent;
            }

            Debug.Assert(Controller != null);

            InitRotation = transform.rotation.eulerAngles.y;

            // we may have things set wrongly in the overall appearance, to allow easy editing
            // e.g. not _invisible_ but this sets us to the correct sart-of-run state
            Initialise();
        }

        private void Initialise()
        {
            CanvasGroup.alpha = 0;
        }

        // anatomy of a ZoomSegment:
        //
        //                       InLength    +---LiveLength----+   OutLength
        //                               \  /|                 |\ /
        //                                 / |                 | \
        //                                /  |                 |  \
        // 0 (e.g. invisible) <----------+   |                 |   +---------->
        //                                   |
        //                                   |
        //                                   |
        //                        ParamOffset 
        //
        // Calling SetOffset adjusts EndInParam (e.g. the start of the full-displayed section)
        // to new_end_in_param and also adjusts the other three params by the same relative change
        public void SetParamOffset(float offset)
        {
            ParamOffset = offset;
        }

        // interp from 0-1 is the "in" process
        //        from 1-2 is the "out" process
        public void SetInterp(float param)
        {
            if (param < StartInParam)
            {
                if (Running)
                {
                    Reset();
                }

                CurrentPhase = Phase.Before;

                return;
            }
            else if (param > EndOutParam)
            {
                if (Running)
                {
                    Reset();
                }

                CurrentPhase = Phase.After;

                return;
            }

            if (param < EndInParam)
            {
                if (CurrentPhase != Phase.In)
                {
                    Reset();

                    Tween = InitIn(param);
                }
            }
            else if (param < StartOutParam)
            {
                if (CurrentPhase != Phase.Live)
                {
                    Reset();

                    Tween = InitLive(param);
                }
            }
            else if (param < EndOutParam)
            {
                if (CurrentPhase != Phase.Out)
                {
                    Reset();

                    Tween = InitOut(param);
                }
            }

            float delta_param = param - LastParam;
            LastParam = param;

            Tween.ManualUpdate(delta_param, delta_param);
        }

        private Tweener InitIn(float param)
        {
            CurrentPhase = Phase.In;
            LastParam = StartInParam;
            return DOTween.To(Interpolate, 0, InLength, InLength)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Manual);
        }

        private Tweener InitLive(float param)
        {
            CurrentPhase = Phase.Live;
            LastParam = EndInParam;
            return DOTween.To(Interpolate, 0, LiveLength, LiveLength)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Manual);
        }

        private Tweener InitOut(float param)
        {
            CurrentPhase = Phase.Out;
            LastParam = StartOutParam;
            return DOTween.To(Interpolate, 0, OutLength, OutLength)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Manual);
        }

        private void Interpolate(float interp)
        {
            float alpha = 1;
            float scale = 1;
            float rotation = InitRotation;

            switch (CurrentPhase)
            {
                case Phase.In:
                    alpha = interp / InLength;
                    rotation = InitRotation + interp;
                    scale = interp / InLength;
                    break;

                case Phase.Live:
                    rotation = InitRotation + InLength + interp;
                    scale = 1 + interp;
                    break;

                case Phase.Out:
                    alpha = 1 - interp / OutLength;
                    rotation = InitRotation + InLength + LiveLength + interp;
                    scale = 1 + LiveLength + interp / OutLength;
                    break;
            }

            CanvasGroup.alpha = alpha;
            transform.rotation = Quaternion.Euler(0, 0, rotation * RotationRate);
            scale *= ScaleRate;
            transform.localScale = new Vector3(scale, scale, scale);
        }

        private void Reset()
        {
            if (Tween != null)
            {
                Tween.Complete();
            }

            Tween = null;

            CurrentPhase = Phase.Unknown;
        }
    }
}