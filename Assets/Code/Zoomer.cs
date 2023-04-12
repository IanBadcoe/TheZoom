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
        public GameObject ChildLocation;
        public GameObject Text;
        public CanvasGroup CanvasGroup;
        public Canvas ImageCanvas;
        public Canvas TextCanvas;

        float InLength;          // time over which we fade in, at the end of this our scale is 1
                                 // and the next zoom can start fading in usingour original size/position as a guide
        float LiveLength;        // total time we live, including fade-in 

        // run params
        // ParamOffset is the zoom where we finish fading-in...
        // our zoom is 1 at this point
        float ParamOffset;

        RectTransform LocationTransform;
        Controller Controller;

        // convenience
        float StartInParam => ParamOffset;
        float EndInParam => ParamOffset + InLength;
        float EndLiveParam => ParamOffset + LiveLength;

        bool Started = false;
        public bool Done = false;

        void Start()
        {
            var p = transform.parent;

            while (p != null && Controller == null)
            {
                Controller = p.GetComponent<Controller>();

                p = p.parent;
            }

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
        //                         InLength          InLength of successor 
        //                        |                 |
        //                   +----------------+------------------+
        //                                __..|             __.. |
        //                            __..    |         __..     |
        //                        __..        |     __..         |
        // 0 (e.g. invisible) __..            | __..             | (completely hidden and can be discarded)
        //                                    |                  |
        //                                    |                  |
        //                                    |                  |
        //                          ZoomOffset                    ZoomOffset of successor
        //
        public float SetZoomOffset(float param, RectTransform start_location, Rect whole_screen, int index)
        {
            ImageCanvas.sortingOrder = index;
            TextCanvas.sortingOrder = index + 100;

            ParamOffset = param;
            // position in this updates as the previous zoomer scales, so we can just take it live
            LocationTransform = start_location;

            var text_rt = (RectTransform)Text.transform;
            var h_rt = (RectTransform)transform;

            var start_scale = 0.5f;

            if (start_location != null)
            {
                // scale so initially our text fits inside the given child-location
                start_scale = MathF.Min(
                    start_location.rect.width / text_rt.rect.width,
                    start_location.rect.height / text_rt.rect.height);
            }

            // end when the current text fills the screen on one axis or the other
            var end_scale = Mathf.Max(
                whole_screen.width / text_rt.rect.width,
                whole_screen.height / text_rt.rect.height);

            // During our "In" phase, we zoom from scale to 1
            // zoom has to be exponential with param
            // (reason: suppose something occupies 1/2 the screen and zooms to fill the screen,
            //          suppose also that it contains a nested zoomer which occupies 1/2 the sceen
            //          when the outer is fully zoomed, the nexted zoomer will take the same time until it
            //          occupies the whole screen, and during that time the outer zoomer will need to double
            //          in size again to stay in register with it, therefore:
            //          time    zoom-outer  zoom-inner (relative zooms)
            //          0       1           0.5
            //          1       2           1
            //          2       4           2
            //
            // this means that param and zoom are related by:
            // zoom = 10.0 ^ param (it doesn't matter what number we use for 10...)
            //
            // and the time required to make a zoom change of dZ is:
            //
            // dParam = log(dZ)

            // it takes this long to get from the required start size to unity
            InLength = Mathf.Log10(1 / start_scale);
            // it takes this long to get from the required start size to fill the screen
            LiveLength = Mathf.Log10(end_scale / start_scale);

            return InLength;
        }

        public void SetZoom(float param)
        {
            if (Done)
            {
                return;
            }

            var ret = false;

            if (param < StartInParam)
            {
                return;
            }

            if (!Started)
            {
                Started = true;

                if (LocationTransform != null)
                {
                    Controller.SetTrackingTransform(LocationTransform);
                }
            }

            // this is 0 at the EndInParam and -ve before that...
            // meaning we are smaller before the end (scale < 1), and
            // bigger after (scale > 1)
            float rel_param = param - EndInParam;
            var curr_scale = Mathf.Pow(10, rel_param);

            // we really shouldn't be required after reaching this size
            if (curr_scale > 100)
            {
                Done = true;
            }

            var hrt = (RectTransform)transform;

            hrt.localScale = new Vector3(curr_scale, curr_scale, 1);
            // 0 at StartInParam, 1 at EndInParam

            float in_frac = (param - StartInParam) / InLength;
            CanvasGroup.alpha = Mathf.Clamp01(in_frac);

            if (LocationTransform != null)
            {
                hrt.position = LocationTransform.position;
            }
        }
    }
}