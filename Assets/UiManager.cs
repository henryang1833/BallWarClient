using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public TMP_InputField userNameText;
    public TMP_InputField passText;
    public TMP_Text tipText;
    
    public GameObject siginPanel;
    public GameObject welcomePanel;
    public Main main;
    public Button goButton;
    // Start is called before the first frame update
    void Start()
    {
        welcomePanel = transform.GetChild(0).gameObject;
        siginPanel = welcomePanel.transform.Find("SigInPanel").gameObject;
        userNameText = siginPanel.transform.Find("UserNameInput").GetComponent<TMP_InputField>();
        passText = siginPanel.transform.Find("PasswdInput").GetComponent<TMP_InputField>();
        goButton = transform.GetChild(0).Find("GoButton").GetComponent<Button>();
        tipText = siginPanel.transform.Find("TipText").GetComponent<TMP_Text>();

        Debug.Assert(welcomePanel!=null);
        Debug.Assert(siginPanel!=null);
        Debug.Assert(userNameText!=null);
        Debug.Assert(passText!=null);
        Debug.Assert(goButton!=null);
        Debug.Assert(tipText!=null);
        siginPanel.SetActive(false);
        goButton.interactable = false;
    }

    public void OnCancelSingIn()
    {
        siginPanel.SetActive(false);
        goButton.gameObject.SetActive(true);
    }

    public void OnGoGame()
    {
        Debug.Log("OnGoGame");
        siginPanel.SetActive(true);
        goButton.gameObject.SetActive(false);
    }

    public void OnSingIn()
    {
        string userName = userNameText.text;
        string passWord = passText.text;
        main.OnClickToSingIn(userName, passWord);
    }
}
