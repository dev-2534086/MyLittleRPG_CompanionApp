using UnityEngine;

public class TypeFilterBar : MonoBehaviour
{
    public PokeTypeButton typeButtonPrefab;
    public Transform parent;
    public Sprite[] typeIcons;
    public PokedexPaginationController pokedex;

    private readonly string[] types = new string[]
    {
        "fighting", "psychic", "poison", "dragon", "ghost",
        "dark", "ground", "fire", "fairy", "water", "flying",
        "normal", "rock", "electric", "bug", "grass", "ice", "steel"
    };

    private void Start()
    {
        for (int i = 0; i < types.Length; i++)
        {
            var btn = Instantiate(typeButtonPrefab, parent);
            btn.Initialize(types[i], typeIcons[i], OnTypeClicked);
        }
    }

    private void OnTypeClicked(string type)
    {
        Debug.Log("Filter clicked: " + type);
        pokedex.FilterByType(type);
    }
}
