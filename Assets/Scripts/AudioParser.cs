using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * DEFUNCT , please refer to AudioReader.cs for updates
 * 
 */
public class AudioParser : MonoBehaviour
{
    AudioSource audioSource;

    public const int numOfSamples = 512;
    public float[] threshold;

    public float[] freqData;
    public float[] band;

    public GameObject[] g;


    public float[] highVals; //debug

    // Use this for initialization
    void Start()
    {
        freqData = new float[numOfSamples];
        int n = freqData.Length;
        int k = 0;
        for (int j = 0; j < freqData.Length; j++)
        {
            n = n / 2;
            if (n <= 0)
                break;
            k++;
        }

        band = new float[k + 1];
        g = new GameObject[k + 1];
        threshold = new float[k + 1];
        threshold[0] = 0;
        threshold[1] = 12;
        threshold[2] = 10.5f;
        threshold[3] = 6;
        threshold[4] = 6.5f;
        threshold[5] = 4.5f;
        threshold[6] = 2;
        threshold[7] = 1;
        threshold[8] = 0.5f;
        threshold[9] = 0;

        highVals = new float[k + 1]; //debug

        for (int i = 0; i < band.Length; i++)
        {
            band[i] = 0;
            g[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g[i].GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
            g[i].transform.position = new Vector3(i, 0, 0);
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    private void check()
    {
        audioSource.GetSpectrumData(freqData, 0, FFTWindow.Rectangular);

        int k = 0;
        int crossover = 2;
        for (int i = 0; i < freqData.Length; i++)
        {
            float d = freqData[i];
            float b = band[k];

            // find the max as the peak value in that frequency band.
            band[k] = (d > b) ? d : b;

            if (i > (crossover - 3))
            {
                k++;
                crossover *= 2; // frequency crossover point for each band.
                float height = band[k] * 32;
                Vector3 tmp = new Vector3(g[k].transform.position.x, height, g[k].transform.position.z);
                g[k].transform.position = tmp;
                band[k] = 0;
                if (height > highVals[k])
                {
                    highVals[k] = height; //debug
                }
            }
        }
    }

    private void display()
    {
        for (int i = 0; i < g.Length; i++)
        {
            if (g[i].transform.position.y > threshold[i])
            {
                g[i].SetActive(true);
            }
            else
            {
                g[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        check();
        //display ();

        String s = ""; //debug
        for (int i = 0; i < highVals.Length; i++)
        {
            float val = highVals[i]; //debug
            s = s + i + ": " + highVals[i] + " "; //debug
        }
        //Debug.Log (s); //debug
    }
}
