using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerComponent : MonoBehaviour
{
    [SerializeField] public string targetScene = "";

    public void ChangeScene()
    {
        try
        {
            SceneManager.LoadScene(targetScene);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to change scene! {e.Message}");
        }
    }
}
