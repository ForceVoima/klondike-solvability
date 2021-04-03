using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class Settings : MonoBehaviour
    {
        public float cardWidth = 6.23f;
        public float cardHeight = 8.7f;
        public float cardThickness = 0.1f;

        public float cardAngle = 2f;

        [SerializeField] private float closedCardMinSpacing = 15f;

        public float ClosedCardMinSpacing
        {
            get
            {
                return cardHeight * closedCardMinSpacing / 100f;
            }
        }

        public Quaternion faceUp = Quaternion.LookRotation(
                forward: Vector3.forward,
                upwards: Vector3.up);

        public Quaternion faceDown = Quaternion.LookRotation(
                forward: Vector3.forward,
                upwards: Vector3.down);
    }   
}
