using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NamePanelManager : MonoBehaviour
{
    public Text normalName;
    public TextMeshProUGUI tmpName;

    public void OnInit(string nameStr, int maxLength = 8)
    {
        var reg = new Regex(@"(?m)^[àâäôéèëêïïçùûüÿæœÀÂÄÔÉÈËÊÏÎŸÇÙÛÜβÆŒa-zA-Z\p{P}\p{Sm}\p{Nd} \u200B]+$");
        var flag = reg.IsMatch(nameStr);
        // if (maxLength > 0)
        //     nameStr = CutLongName(nameStr, maxLength);
        normalName.text = nameStr;
        tmpName.text = nameStr;
        normalName.gameObject.SetActive(!flag);
        tmpName.gameObject.SetActive(flag);
    }

    private static string CutLongName(string nameStr, int maxLength)
    {
        if (nameStr.Length > maxLength)
        {
            nameStr = nameStr.Substring(0, maxLength) + "...";
        }
        return nameStr;
    }
}
