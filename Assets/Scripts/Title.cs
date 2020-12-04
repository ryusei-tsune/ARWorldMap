using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// タイトルシーンからの移動
/// </summary>

public class Title : MonoBehaviour
{
    private int Sceneflag = 0;

    public void OnAdminiButton()
    {
        if (Sceneflag == 0)
        {
            SceneManager.LoadScene("TestAdminiScene", LoadSceneMode.Single);
            Sceneflag = 1;
        }
    }


    public void OnUserButton()
    {
        if (Sceneflag == 0)
        {
            SceneManager.LoadScene("TestUserScene", LoadSceneMode.Single);
            Sceneflag = 1;
        }
    }

    public void OnReturnButton()
    {
        if (Sceneflag == 0)
        {
            SceneManager.LoadScene("TestTitleScene", LoadSceneMode.Single);
            Sceneflag = 1;
        }
    }
}
