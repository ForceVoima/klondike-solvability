using UnityEngine;

namespace Klondike
{
    public class InputForwarder : MonoBehaviour
    {
        [SerializeField] private PlayerPile pile;
        private void OnMouseEnter()
        {
            pile.OnMouseEnter();
        }

        private void OnMouseExit()
        {
            pile.OnMouseExit();
        }
    }
}
