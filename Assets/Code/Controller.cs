using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;
using DG.Tweening;

namespace Code
{
    public class Controller : MonoBehaviour
    {
        public GameObject Container;
        public Image Background;
        public Color PrevBackgroundColour;

        // just public for ease of debugging...
        public float RelativeTime = 0;
        public float TimeRate = 0.125f;

        bool Running = false;
        bool Escaping = false;

        public Transform TrackingTransform1;
        public float TrackFrac1 = 0;
        public float TrackRate = 0.02f;

        // Use this for initialization
        void Start()
        {
            // zoom is going to be a function of time, maybe just linear time
            // but for clarity keep the name separate
            float cumulative_start_zoom = 0;

            Zoomer prev_layer = null;

            int index = 0;

            foreach (Transform t in Container.transform)
            {
                var z = t.GetComponent<Zoomer>();

                if (z != null)
                {
                    // add our total live time to the running total
                    cumulative_start_zoom += z.SetZoomOffset(cumulative_start_zoom,
                        (RectTransform)prev_layer?.ChildLocation.transform,
                        ((RectTransform)Container.transform).rect,
                        index);

                    index += 1;
                    prev_layer = z;

                    if (TrackingTransform1 == null)
                    {
                        TrackingTransform1 = z.ChildLocation.transform;
                    }
                }
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

            if (Running)
            {
                RelativeTime += Time.deltaTime * TimeRate;

                Zoomer prev_layer = null;

                foreach (Transform t in Container.transform)
                {
                    var z = t.GetComponent<Zoomer>();

                    if (z != null && t.gameObject.activeSelf)
                    {
                        // zoom == time at the moment...
                        z.SetZoom(RelativeTime);

                        prev_layer = z;

                        if (z.Done)
                        {
                            Destroy(z.gameObject);
                        }
                    }
                }

                // when the tracking position is not (0, 0), we need to scale it
                // with everything else
                float scale_change = Mathf.Pow(10, Time.deltaTime * TimeRate);
                Container.transform.position = new Vector3(Container.transform.position.x * scale_change,
                    Container.transform.position.y * scale_change,
                    Container.transform.position.z);

                // We want to get LocationTransform to 0, 0 so we shift the whole container, negatively, by
                // a proportion of where it currently is...

                if (TrackingTransform1 != null)
                {
                    Container.transform.position = new Vector3(
                        -TrackingTransform1.position.x,
                        -TrackingTransform1.position.y,
                        Container.transform.position.z);
                    TrackingTransform1 = null;
                    //TrackFrac1 += TrackRate;
                    //if (TrackFrac1 > 1)
                    //{
                    //    TrackFrac1 = 1;
                    //}

                    //float interp1 = DOVirtual.EasedValue(0, 1, TrackFrac1, Ease.InCubic);
                    //Container.transform.position -= TrackingTransform1.position * interp1;

                    //if (interp1 > 0.999f)
                    //{
                    //    TrackingTransform1 = null;
                    //}
                }
            }
        }

        internal void SetTrackingTransform(RectTransform t)
        {
            //TrackingTransform1 = t;
            //TrackFrac1 = 0;
        }

        //private void OnGUI()
        //{
        //    var style = new GUIStyle();
        //    style.fontSize = 32;
        //    Handles.Label(new Vector3(Screen.width - 400, Screen.height - 200, 0), $"{LastTime}", style);
        //}
    }
}