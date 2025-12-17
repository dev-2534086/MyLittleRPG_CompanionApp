using UnityEngine;

public class UIPageManager : MonoBehaviour
{
    public GameObject pageQuests;
    public GameObject pageCharacter;
    public GameObject pagePokedex;
    public GameObject pageClassement;
    public GameObject pageBadges;

    private void Start()
    {
        SetActivePage(pageCharacter);
    }

    public void ShowQuests()    => SetActivePage(pageQuests);
    public void ShowCharacter() => SetActivePage(pageCharacter);
    public void ShowPokedex()   => SetActivePage(pagePokedex);

    private void SetActivePage(GameObject target)
    {
        pageQuests.SetActive(false);
        pageCharacter.SetActive(false);
        pagePokedex.SetActive(false);
        pageClassement.SetActive(false);
        pageBadges.SetActive(false);

        target.SetActive(true);
    }
}
