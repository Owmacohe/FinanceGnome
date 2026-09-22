using TMPro;
using UnityEngine;

public class FGTooltipController : MonoBehaviour
{
    [SerializeField] TMP_Text description;
    
    public void Show(Vector3 position, string description)
    {
        if (string.IsNullOrEmpty(description)) return;
        
        gameObject.SetActive(true);
        
        transform.position = position;
        this.description.text = description;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}