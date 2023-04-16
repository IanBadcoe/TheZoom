using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class PaperZoomer : MonoBehaviour
    {
        public GameObject PaperTemplate;
        public Sprite[] Sprites;

        GameObject[] Layers;

        private int NumFrames => Sprites.Length;

        void Start()
        {
            Layers = new GameObject[NumFrames];

            int idx = 0;

            foreach(var sprite in Sprites)
            {
                GameObject go = Instantiate(PaperTemplate, transform);

                Image img = go.GetComponent<Image>();
                img.sprite = sprite;

                // -ve values seem to be ignored here
                go.GetComponent<Canvas>().sortingOrder = /*NumFrames -*/ idx;

                Layers[idx] = go;

                idx++;
            }

            SetZoom(0);
        }

        // zoom = 0 means CGs[0] is at scale 2 (about to start fading out)
        //      and CGs[1] is at scale 1 (about to start becoming visible because the other is fading out)
        // zoom = 1 means CGs[0] is at scale 4 (alpha = 0.5),
        //      CGs[1] is at scale 2 (about to start fading out)
        //      and CGs[2] is at scale 1 (about to start getting revealed)
        // zoom = 2 means CGs[0] is at scale 8 (alpha = 0, just become completely invisible)
        //      CGs[1] is atg scale 4 (alpha = 0.5)

        // param    scale-0 alpha-0 scale-1 alpha-1 scale-2 alpha-2 scale-3 alpha-3  ...     scale-N alpha-N
        // 0        2       1       1       1       -       -       -       -        ...     4       0
        // 0.5      2.5     0.5     1.5     1       -       -       -       -        ...     4       0
        // 1        4       0.5     2       1       1       1       -       -        ...     -       -
        // 2        -       -       4       0.5     2       1       1       -        ...     -       -
        // 3        0       0       0       0       4       0       2       0.5      ...     -       -
        //
        // N        2       1       1       1       -       -       -       -        ...     4       0

        public void SetZoom(float param)
        {
            int idx = 0;

            foreach (var layer in Layers)
            {
                float rel_param = param - idx;

                // so at param = 0, rel_param is 0 for CGs[0], -1 for CGs[1]

                // now, at param = 0, for CGs[NumFrames - 1], rel_param is 2

                // when we hit rel_param = 0, we want to be at scale = 2, so that as we fade out the layer behind us
                // is already at scale = 1 and doesn't have any gaps around the edge...
                var scale = Mathf.Pow(2, rel_param) /** 2*/;

                layer.transform.localScale = new Vector3(scale, scale, 1);

                var alpha = Mathf.Clamp01(rel_param + 1/*/ 2*/);

                layer.GetComponent<CanvasGroup>().alpha = alpha;

                idx++;
            }
        }
    }
}