using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Core {
    public class ButtonTriggers : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        [SerializeField] public UnityEvent onPointerDown;
        [SerializeField] public UnityEvent onPointerUp;

        public void OnPointerDown(PointerEventData eventData) {
            onPointerDown.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData) {
            onPointerUp.Invoke();
        }
    }
}
