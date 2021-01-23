using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindingUIPanel : MonoBehaviour
{
    public Text bindQ, bindE, bind1, bind2, bind3;
    public static BindingUIPanel LocalInstance;
    private Dictionary<string, Text> dict = new Dictionary<string, Text>();

    void Start() {
        dict.Add("q", bindQ);
        dict.Add("e", bindE);
        dict.Add("1", bind1);
        dict.Add("2", bind2);
        dict.Add("3", bind3);

        BindingUIPanel.LocalInstance = this;
    }
    public void SetBindText(string key, string name) {
        dict[key].text = "Bound: "+name;
    }

    public void Bind(string key) {
        AuricaCaster.LocalCaster.CacheCurrentSpell(key);
    }
}
