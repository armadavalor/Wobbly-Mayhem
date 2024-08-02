using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class GifAnimator : MonoBehaviour
{
    public Image image; // Unity UI Image component
    public float frameRate = 0.1f; // Time between frames
    private Sprite[] frames;
    public int FotografSayısı;

     private bool isWorking = true;
    private bool workOneMoreTime = false;
    private bool workOneMoreTimeUpdate = false;

   public  List<Sprite> sprites;

    // public UnityEvent eventtt;
    public GameObject deActiveObject;

    private void Update()
    {
        if(deActiveObject.active == true && !workOneMoreTimeUpdate)
        {

            frameRate = 0.0025f;
            workOneMoreTime = true;
            workOneMoreTimeUpdate = true;
            Kapanış();

        }
    }

    void Start()
    {
        Debug.Log("sadasd");
        // Load all sprites from ozlem/Resources folder
        frames = new Sprite[FotografSayısı];
        for (int i = 0; i < FotografSayısı; i++)
        {
            frames[i] = sprites[i];
            //frames[i] = Resources.Load<Sprite>($"wobblymayhemgif-{i} (sürüklenen)");

             //Debug.Log(Resources.Load<Sprite>($"wobblymayhemgif-{i} (sürüklenen)"));
       
        }

        // Start the animation
        StartCoroutine(PlayGif(0));
    }

    // 58 de yok olmaya başlıyor

    IEnumerator PlayGif(int startIndex)
    {
        int index = startIndex;
        isWorking = true;
        while (isWorking)
        {

            if(index >= 56 && !workOneMoreTime)
            {
                // burda ilk başladıgında durdugu yer
                workOneMoreTime = true;
                isWorking = false;
                yield return null;
            }

            image.sprite = frames[index];
            Debug.Log(frames[index]);
            index = (index + 1) % frames.Length;

            if(index >= 74 && workOneMoreTime)
            {
                // burda geri çıktıgı
                isWorking = false;
                image.gameObject.SetActive(false);
                gameObject.SetActive(false);
                yield return null;
            }

            yield return new WaitForSeconds(frameRate);
        }
    }

    [ContextMenu("Kapanış")]
    private void Kapanış()
    {

        isWorking = true;
        StartCoroutine(PlayGif(59));


    }


}