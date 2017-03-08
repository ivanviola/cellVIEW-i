using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UIInteractionBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private MainCameraController cameraController;

    void Start()
    {
        cameraController = FindObjectOfType<MainCameraController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("ENTERED");
        cameraController.ShouldBlockInteraction = true;
        NewSelectionManager.Instance.ClickingDisabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("EXITED");
        cameraController.ShouldBlockInteraction = false;
        NewSelectionManager.Instance.ClickingDisabled = false;
    }

}
