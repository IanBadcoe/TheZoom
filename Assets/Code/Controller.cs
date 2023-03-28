using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace Code
{
    public class Controller : MonoBehaviour
    {
        public GameObject Container;
        public Image Background;
        public Color PrevBackgroundColour;
        public float StartDelay;

        float RelativeTime;

        bool Running = false;
        bool Escaping = false;

        // Use this for initialization
        void Start()
        {
            // "param" is going to be a function of time, maybe just linear time
            // but for clarity keep the name separate
            float cumulative_start_param = -1;

            List<Transform> transforms = new List<Transform>();

            foreach (Transform t in Container.transform)
            {
                var z = t.GetComponent<Zoomer>();

                if (z != null)
                {
                    if (cumulative_start_param == -1)
                    {
                        // make the start delay happen fully before the initial fade in
                        // later fades overlap the preceding period
                        cumulative_start_param = StartDelay + z.InLength;
                    }

                    z.SetParamOffset(cumulative_start_param);

                    // add out total live time to the running total
                    cumulative_start_param += z.LiveLength;

                    transforms.Insert(0, t);
                }
            }

            foreach(Transform t in transforms)
            {
                t.SetParent(transform);
                t.SetParent(Container.transform);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Running = !Running;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Escaping)
                {
                    Application.Quit();
                }
                else
                {
                    Escaping = true;
                }
            }
            else if (Input.anyKeyDown)
            {
                Escaping = false;
            }

            if (!Running)
            {
                return;
            }

            RelativeTime += Time.deltaTime;

            foreach (Transform t in Container.transform)
            {
                var z = t.GetComponent<Zoomer>();

                if (z != null)
                {
                    // param == time at the moment...
                    z.SetInterp(RelativeTime);
                }
            }
        }

        //private void OnGUI()
        //{
        //    var style = new GUIStyle();
        //    style.fontSize = 32;
        //    Handles.Label(new Vector3(Screen.width - 400, Screen.height - 200, 0), $"{LastTime}", style);
        //}
    }
}