using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class Controller : MonoBehaviour
    {
        public GameObject Container;
        public Image Background;
        public Color PrevBackgroundColour;

        // Use this for initialization
        void Start()
        {
            var first = true;

            foreach(Transform t in Container.transform)
            {
                var z = t.GetComponent<Zoomer>();

                if (z != null)
                {
                    if (first)
                    {
                        z.SetInterp(1);
                        PrevBackgroundColour = z.BackgroundColour;
                    }
                    else
                    {
                        z.SetInterp(0);
                    }

                    first = false;
                }
            }
        }
    }
}