using System;
using BettaBeta.Extensions;
using UdonSharp;
using UnityEngine;

namespace BettaBeta.SlideShower
{
    public class PhotoStand : UdonSharpBehaviour
    {
        [SerializeField, Tooltip("横表示か縦表示か")]
        public bool isHorizontal;

        [SerializeField]
        public Renderer renderer;

        [NonSerialized]
        public bool tex1ToTex2 = true;

        [NonSerialized]
        public int counter = 0;

        void Awake()
        {
            renderer = gameObject.GetComponent<Renderer>();
            Debug.Log("set renderer");
        }

        public static PhotoStand[] GetHorizontalPhotoStands(PhotoStand[] photoStands)
        {
            PhotoStand[] horizontalPhotoStands = new PhotoStand[0];
            foreach (var photoStand in photoStands)
            {
                if (photoStand.isHorizontal)
                {
                    horizontalPhotoStands = horizontalPhotoStands.Add(photoStand);
                }
            }
            return horizontalPhotoStands;
        }

        public static PhotoStand[] GetVerticalPhotoStands(PhotoStand[] photoStands)
        {
            PhotoStand[] verticalPhotoStands = new PhotoStand[0];
            foreach (var photoStand in photoStands)
            {
                if (!photoStand.isHorizontal)
                {
                    verticalPhotoStands = verticalPhotoStands.Add(photoStand);
                }
            }
            return verticalPhotoStands;
        }

        public static PhotoStand[] GetPhotoStands(PhotoStand[] photoStands, bool isHorizontal)
        {
            PhotoStand[] rtnPhotoStands = new PhotoStand[0];
            foreach (var photoStand in photoStands)
            {
                if (photoStand.isHorizontal == isHorizontal)
                {
                    rtnPhotoStands = rtnPhotoStands.Add(photoStand);
                }
            }
            return rtnPhotoStands;
        }

        public static PhotoStand GetOldestPhotoStand(PhotoStand[] photoStands)
        {
            PhotoStand oldestPhotoStand = photoStands[0];
            int smallestCount = photoStands[0].counter;
            foreach (var photoStand in photoStands)
            {
                if (smallestCount > photoStand.counter)
                {
                    oldestPhotoStand = photoStand;
                    smallestCount = photoStand.counter;
                }
            }
            // 選ばれた写真立てはカウンタを増加させる
            oldestPhotoStand.counter++;
            return oldestPhotoStand;
        }

        public static PhotoStand GetOldestPhotoStand(PhotoStand[] photoStands, bool isHorizontal)
        {
            PhotoStand[] sameShapePhotoStands = new PhotoStand[0];
            foreach (var photoStand in photoStands)
            {
                if (photoStand.isHorizontal == isHorizontal)
                {
                    sameShapePhotoStands = sameShapePhotoStands.Add(photoStand);
                }
            }
            return GetOldestPhotoStand(sameShapePhotoStands);
        }
    }
}
