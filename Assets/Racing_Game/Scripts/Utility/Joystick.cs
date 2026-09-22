//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

// Source : https://youtu.be/9gOF_plwd1g?feature=shared

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ALIyerEdon
{   
    public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        public Image background;
        public Image handle;
        Vector2 position;

        public void OnDrag(PointerEventData pointerEventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle
                (background.rectTransform,
                    pointerEventData.position,
                    pointerEventData.pressEventCamera,
                    out position))
            {
                // Get the touch position
                position.x = position.x / (background.rectTransform.sizeDelta.x);
                position.y = position.y / (background.rectTransform.sizeDelta.y);

                if (position.magnitude > 1.0f)
                {
                    position = position.normalized;
                }

                // Move the stick
                handle.rectTransform.anchoredPosition =
                    new Vector2(position.x * (background.rectTransform.sizeDelta.x / 4),
                        position.y * (background.rectTransform.sizeDelta.y / 4));

            }
        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            OnDrag(pointerEventData);
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            position = Vector2.zero;
            handle.rectTransform.anchoredPosition = Vector2.zero;
        }

        public float GetHorizontal(float minValue)
        {
            if (position.x > minValue || position.x < -minValue)
                return position.x;
            else
                return 0;
        }
    }
}