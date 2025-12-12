using UnityEngine;
using UnityEngine.UI;

public class PokeTypeButton : MonoBehaviour
{
    public string pokemonType;
    public Image icon;
    public Button button;

    public void Initialize(string type, Sprite sprite, System.Action<string> callback)
    {
        pokemonType = type;
        icon.sprite = sprite;
        button.onClick.AddListener(() => callback(type));
    }
}
