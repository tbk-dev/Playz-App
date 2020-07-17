using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadScene("Main"));
    }

    IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncOper = SceneManager.LoadSceneAsync(sceneName);
         asyncOper.allowSceneActivation = false;

        while (!asyncOper.isDone)
        {
            yield return null;
            Debug.Log(asyncOper.progress);

            if (asyncOper.progress >= 0.9f)
            {
                Debug.Log("loading complete");
                break;
            }
        }

        while (!goNextScene)
        {
            yield return null;

            if (goNextScene)
            {
                asyncOper.allowSceneActivation = true;
            }
        }
    }

    bool goNextScene = false;
    public void NextScene()
    {
        Debug.Log("log >>> NextScene");

        goNextScene = true;
    }
}