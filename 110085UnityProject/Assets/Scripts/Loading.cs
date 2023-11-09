using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour {

    [SerializeField]
    AudioClip click;

    [SerializeField]
    GameObject barGo, btnGo;

    [SerializeField]
    Image i;
    [SerializeField]
    Text t;

    int progress = 0;

    private AudioSource audioSource;

    public static bool isLoad = false;

    void Awake()
    {
        audioSource = GameObject.Find("GameManager").GetComponent<AudioSource>();
        audioSource.volume = Game.volumOpen ? 1 : 0;

        if (Loading.isLoad)
        {
            barGo.SetActive(false);
            btnGo.SetActive(true);
        }
        else
            StartCoroutine(load());
    }

    IEnumerator load()
    {
        while (progress < 100)
        {
            progress += Random.Range(0, 7);
            if (progress > 100)
                progress = 100;
            i.fillAmount = (float)progress / 100;
            t.text = string.Format("{0}%", progress);
            yield return new WaitForSeconds(0.1f);
        }

        barGo.SetActive(false);
        btnGo.SetActive(true);

        Loading.isLoad = true;
    }

    public void onStart()
    {
        audioSource.PlayOneShot(click);
        SceneManager.LoadSceneAsync("GameScene");
    }
}
