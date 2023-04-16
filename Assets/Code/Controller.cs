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
        public PaperZoomer PaperZoomer;

        // just public for ease of debugging...
        public float RelativeTime = 0;
        public float TimeRate = 0.125f;

        bool Running = false;
        bool Escaping = false;

        // Use this for initialization
        void Start()
        {
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
            }
        }
    }
}