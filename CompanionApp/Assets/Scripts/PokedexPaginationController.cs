using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

[System.Serializable]
public class MonsterPageResponse
{
    public MonsterData[] items;
    public int total;
}

public class PokedexPaginationController : MonoBehaviour
{
    [Header("UI")]
    public Transform contentGrid;
    public GameObject monsterCardPrefab;
    public Button btnPrevious;
    public Button btnNext;
    public TMP_Text pageInfo;

    [Header("Pagination Settings")]
    public int limit = 9; // 3x3 grid
    private int offset = 0;
    private int total = 0;

    public MonsterService service;

    private void Start()
    {
        btnPrevious.onClick.AddListener(OnPreviousPage);
        btnNext.onClick.AddListener(OnNextPage);

        LoadPage();
    }

    private async void LoadPage()
    {
        MonsterPageResponse result = await service.GetPokemons(offset, limit);

        if (result == null || result.items == null)
            return;

        total = result.total;

        RefreshGrid(result.items);
        UpdatePaginationUI();
    }

    private void RefreshGrid(MonsterData[] items)
    {
        foreach (Transform child in contentGrid)
            Destroy(child.gameObject);

        foreach (var monster in items)
        {
            var card = Instantiate(monsterCardPrefab, contentGrid);
            card.GetComponent<MonsterCard>().Setup(monster);
        }
    }

    private void UpdatePaginationUI()
    {
        int currentPage = (offset / limit) + 1;
        int maxPage = Mathf.CeilToInt((float)total / limit);

        pageInfo.text = $"Page {currentPage} / {maxPage}";

        btnPrevious.interactable = offset > 0;
        btnNext.interactable = offset + limit < total;
    }

    private void OnNextPage()
    {
        offset += limit;
        LoadPage();
    }

    private void OnPreviousPage()
    {
        offset -= limit;
        if (offset < 0) offset = 0;
        LoadPage();
    }
}
