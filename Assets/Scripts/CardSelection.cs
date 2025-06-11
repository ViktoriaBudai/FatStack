using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardSelection : MonoBehaviour
{
    private bool isSelected = false;
    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;
    private InputAction clickAction;

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Create a new InputAction for mouse clicks
        clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        clickAction.performed += ctx => OnClick();
        clickAction.Enable();
    }

    void OnMouseOver()
    {
        Debug.Log("Mouse is over: " + gameObject.name);
    }

    private void OnClick()
    {
        Debug.Log("Card Clicked: " + gameObject.name);

        isSelected = !isSelected;
        transform.position = isSelected ? originalPosition + new Vector3(0, 0.3f, 0) : originalPosition;
        spriteRenderer.color = isSelected ? Color.yellow : Color.white;
    }

    private void OnDestroy()
    {
        clickAction.Disable();
    }
}
