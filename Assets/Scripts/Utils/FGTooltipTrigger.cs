using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FGTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)] public string Description;
    
    FGTooltipController controller;
    
    void Start() => controller = FindFirstObjectByType<FGTooltipController>(FindObjectsInactive.Include);

    public void OnPointerEnter(PointerEventData eventData) => controller.Show(eventData.position, Description);
    public void OnPointerExit(PointerEventData eventData) => controller.Hide();
}