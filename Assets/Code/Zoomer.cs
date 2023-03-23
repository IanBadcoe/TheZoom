using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class Zoomer : MonoBehaviour
    {
        public GameObject Centre;
        public CanvasGroup CanvasGroup;
        public bool UseBackgroundColour = false;
        public Color BackgroundColour = Color.red;

        Controller Controller;

        void Start()
        {
            var p = transform.parent;

            while (p != null && Controller == null)
            {
                Controller = p.GetComponent<Controller>();
            }

            Debug.Assert(Controller != null);
        }

        // interp from 0-1 is the "in" process
        //        from 1-2 is the "out" process
        public void SetInterp(float interp)
        {
            if (interp < 1)
            {
                CanvasGroup.alpha = interp;
            }
            else if (interp == 1)
            {
                Controller.PrevBackgroundColour = BackgroundColour;
            }
            else
            {
                CanvasGroup.alpha = 2 - interp;
            }
        }
    }
}