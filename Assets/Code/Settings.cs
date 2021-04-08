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
        [SerializeField] private float buildCardMaxSpacing = 30f;
        public float BuildCardMaxPacing
        {
            get
            {
                return cardHeight * buildCardMaxSpacing / 100f;
            }
        }

        public Material normal;
        public Material suitBlock;
        public Material solverBlock;
        public Material lowPriority;

        public static Settings _instance;
        public static Settings Instance
        {
            get { return _instance; }
        }

        public Quaternion faceUp = Quaternion.LookRotation(
                forward: Vector3.forward,
                upwards: Vector3.up);

        public Quaternion faceDown = Quaternion.LookRotation(
                forward: Vector3.forward,
                upwards: Vector3.down);

        public void Init()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate settings!");
                return;
            }
        }

        public Material GetHighlight(Effect code)
        {
            switch (code)
            {
                case Effect.Normal:
                    return normal;
                case Effect.SuitBlock:
                    return suitBlock;
                case Effect.SolverBlock:
                    return solverBlock;
                case Effect.LowPriority:
                    return lowPriority;
            }

            return normal;
        }
    }   
}
