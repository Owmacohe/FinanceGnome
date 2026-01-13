using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FGBalanceSheetCellController : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Image background;

    public void Initialize(string text, Color backgroundColour)
    {
        this.text.text = text;
        background.color = backgroundColour;
    }
}