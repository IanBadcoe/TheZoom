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
        public PaperZoomer PaperZoomer;

        // just public for ease of debugging...
        public float RelativeTime = 0;
        public float TimeRate = 0.125f;

        bool Running = false;
        bool Escaping = false;

        public Transform TrackingTransform1;
        public Transform TrackingTransform2;
        public float TrackFrac1 = 0;
        public float TrackFrac2 = 0;
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

                if (z != null && z.gameObject.activeInHierarchy)
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
                        TrackingTransform1 = z.transform;
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

                PaperZoomer.SetZoom(RelativeTime);

                foreach (Transform t in Container.transform)
                {
                    var z = t.GetComponent<Zoomer>();

                    if (z != null && t.gameObject.activeInHierarchy)
                    {
                        // zoom == time at the moment...
                        z.SetZoom(RelativeTime);
                    }
                }

                if (TrackingTransform1 != null)
                {
                    TrackFrac1 += TrackRate;
                    if (TrackFrac1 > 1)
                    {
                        TrackFrac1 = 1;
                    }

                    float interp1 = DG.Tweening.DOVirtual.EasedValue(0, 1, TrackFrac1, Ease.InCubic);

                    // We want to get LocationTransform to 0, 0 so we shift the whole container, negatively, by
                    // a proportion of where it currently is...

                    Container.transform.position -= TrackingTransform1.position * interp1;
                }

                if (TrackingTransform2 != null) {
                    float interp2 = DOVirtual.EasedValue(0, 1, TrackFrac2, Ease.InCubic);

                    Container.transform.position -= TrackingTransform2.position * interp2;

                    TrackFrac2 -= TrackRate;
                    if (TrackFrac2 < 0)
                    {
                        TrackFrac2 = 0;
                        TrackingTransform2 = null;
                    }
                }

                // move the paper in unity with the zoomers/text
                PaperZoomer.transform.position = Container.transform.position;
            }
        }

        internal void SetTrackingTransform(RectTransform t)
        {
            TrackingTransform2 = TrackingTransform1;
            TrackFrac2 = TrackFrac1;

            TrackingTransform1 = t;
            TrackFrac1 = 0;
        }

        //private void OnGUI()
        //{
        //    var style = new GUIStyle();
        //    style.fontSize = 32;
        //    Handles.Label(new Vector3(Screen.width - 400, Screen.height - 200, 0), $"{LastTime}", style);
        //}
    }
}