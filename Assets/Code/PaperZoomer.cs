using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class PaperZoomer : MonoBehaviour
    {
        [Header("Our Objects")]
        public CanvasGroup[] CGs = new CanvasGroup[2];
        public float[] ZoomOffsets = new float[2];
        public float ZoomLimit;
        public float Flip = -1;

        void Start()
        {
            // we are going to zoom each CG by 2x and then jump back to what should be a perfect loop-point at 1/2 the size...
            //
            //  Time    CG-0            CG-1
            //          scale   alpha   scale   alpha
            //  0       1       0       2       1
            //  0.5     1.5     0.5     3       0.5
            //  1       2       1       4       0
            //
            // (OK, so I am not using the exact time or alpha there, but the 0.5 line just means (part way)
            //
            // so what happens is that the alpha of CGs[0] doesn't actually change, because it is the rear one and its effective alpha
            // is achieved by the change to CGs[1] in front of it...
            //
            // So at t = 0, CG-0 completely hidden behind fully-opaque CG-1 with a scale of 2 -> 2x paper visible
            // Between t = 0 and t = 1, CG-1 fades out and scales to 2x -> a mix of >1 scale and >2 scale paper
            // At t = 1, CG-1 is completely transparent, leaving CG-0 visible with a scale of 2 -> 2x paper visible
            //
            // So the end looks just like the start and we can begin again

            CGs[0].alpha = 1;

            Reset(0);
        }

        private void Reset(float param)
        {
            ZoomLimit = MathF.Log10(2);
            ZoomOffsets[0] = param;
            ZoomOffsets[1] = param - ZoomLimit;
        }

        public void SetZoom(float param)
        {
            float rel_param0 = param - ZoomOffsets[0];
            float rel_param1 = param - ZoomOffsets[1];

            // using x4 instead of x2 allows us a bit of leeway to move the paper around...
            float scale0 = Mathf.Pow(10, rel_param0) * 4 * Flip;
            float scale1 = Mathf.Pow(10, rel_param1) * 4 * -Flip;          // rotating 180 just makes it look different...

            CGs[0].transform.localScale = new Vector3(scale0, scale0, 1);
            CGs[1].transform.localScale = new Vector3(scale1, scale1, 1);
            CGs[1].alpha = 1 - (rel_param1 / ZoomLimit);

            if (rel_param1 >= ZoomLimit)
            {
                Reset(ZoomOffsets[0] + ZoomLimit);
                Flip = -Flip;
            }

        //    if (Done)
        //    {
        //        return true;
        //    }

        //    var ret = false;

        //    if (param < StartInParam)
        //    {
        //        return false;
        //    }

        //    if (!Started)
        //    {
        //        Started = true;

        //        if (LocationTransform != null)
        //        {
        //            Controller.SetTrackingTransform(LocationTransform);
        //        }
        //    }

        //    if (param >= EndLiveParam)
        //    {
        //        ret = true;
        //    }

        //    // this is 0 at the EndInParam and -ve before that...
        //    // meaning we are smaller before the end (scale < 1), and
        //    // bigger after (scale > 1)
        //    float rel_param = param - EndInParam;
        //    var curr_scale = Mathf.Pow(10, rel_param);

        //    // we really shouldn't be required after reaching this size
        //    if (curr_scale > 100)
        //    {
        //        SetDone(true);
        //    }

        //    var hrt = (RectTransform)transform;

        //    hrt.localScale = new Vector3(curr_scale, curr_scale, 1);
        //    // 0 at StartInParam, 1 at EndInParam

        //    float in_frac = (param - StartInParam) / InLength;
        //    CanvasGroup.alpha = Mathf.Clamp01(in_frac);

        //    if (LocationTransform != null)
        //    {
        //        hrt.position = LocationTransform.position;
        //    }

        //    return ret;
        }
    }
}