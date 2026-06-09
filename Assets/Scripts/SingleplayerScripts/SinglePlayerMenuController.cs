using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SinglePlayerMenuSectionController : MonoBehaviour {
    [FormerlySerializedAs("mainSection")]
    [Header("Menu Sections")]
    [SerializeField] private GameObject menuSection;
    [SerializeField] private GameObject exitConfirmationSection;
    [SerializeField] private GameObject menuButton;

    private void Start()
    {
        ActiveMenuButton();
    }

    public void OpenMenu() {
        menuSection.SetActive(true);
        exitConfirmationSection.SetActive(false);
        menuButton.SetActive(false);
    }
        
    public void OpenConfirmationMenu() {
        menuSection.SetActive(false);
        exitConfirmationSection.SetActive(true);
        menuButton.SetActive(false);
    }
        
    public void ActiveMenuButton() {
        menuSection.SetActive(false);
        exitConfirmationSection.SetActive(false);
        menuButton.SetActive(true);
    }
}
