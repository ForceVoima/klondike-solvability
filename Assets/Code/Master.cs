using UnityEngine;

namespace Klondike
{
    public class Master : MonoBehaviour
    {
        [SerializeField] protected Stock _stock;
        [SerializeField] protected WastePile _waste;
        [SerializeField] protected FoundationPile[] _foundations;
        [SerializeField] protected ClosedPile[] _closedPiles;
        [SerializeField] protected BuildPile[] _buildPiles;
    }
}
