using UnityEngine;
using System.Collections;
using Lomont;
using System;
using System.Collections.Generic;

public class AudioReader : MonoBehaviour {
    AudioSource audioSource;
    LomontFFT fft;
	AudioClip clip;


    float secondInMilliseconds = 1000;
    float samplingInterval = 20; //in ms
    int samplingWindowSize = 512;
    int steps;


    public GameObject[] g;
    public float[] band;

    Dictionary<int, Vector3[]> positions;

    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource>();
        clip = audioSource.clip;
        fft = new LomontFFT();
		float[] samples = new float[clip.samples * clip.channels];
		clip.GetData (samples, 0);
        //convert float to double
        double[] samplesD = new double[samples.Length];
        for (int i = 0; i < samplesD.Length; i++) {
            samplesD[i] = (double)samples[i];
        }

        Debug.Log("clip samples: " + clip.samples); //debug
        Debug.Log("clip channels: " + clip.channels); //debug
        Debug.Log("clip duration: " + clip.length); //debug

        
        steps = Mathf.RoundToInt(samplingInterval / secondInMilliseconds * clip.frequency);
        Debug.Log("steps: " + steps); //debug




        
        int n = samplingWindowSize;
        int k = 0;
        for (int j = 0; j < samplingWindowSize; j++)
        {
            n = n / 2;
            if (n <= 0)
                break;
            k++;
        }
        band = new float[k + 1];
        g = new GameObject[k + 1];
        positions = new Dictionary<int, Vector3[]>();

        for (int i = 0; i < band.Length; i++)
        {
            band[i] = 0;
            g[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g[i].GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
            g[i].transform.position = new Vector3(i, 0, 0);
        }



        for (int i = 0; i < samplesD.Length; i = i + steps)
        {
            double[] samplingWindow = new double[samplingWindowSize];
            if (i > samplingWindowSize)
            {
                Array.Copy(samplesD, i - samplingWindowSize / 2, samplingWindow, 0, samplingWindowSize);
            } else //i = 0
            {
                Array.Copy(samplesD, i, samplingWindow, 0, samplingWindowSize);
            }

            fft.FFT(samplingWindow, true);

            //convert double to float
            float[] samplingWindowF = new float[samplingWindow.Length];
            for (int j = 0; j < samplingWindow.Length; j++)
            {
                samplingWindowF[j] = (float)samplingWindow[j];
            }

            checkWindow(i, samplingWindowF);

            /*
            if (i > 70000)
            {
                break;
            }*/
        }



        /*
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\wgwong\Desktop\out2.txt"))
        {
            foreach (var entry in positions)
            {
                file.WriteLine("position: " + entry.Key);
                for (int i = 0; i < entry.Value.Length; i++) {
                    var element = entry.Value[i];
                    file.WriteLine("element[" + i + "]: " + element.x + ", " + element.y + ", " + element.z);
                }
                file.WriteLine("\n");
            }
        }*/

        Debug.Log("positions keys: " + positions.Keys.ToString());

        audioSource.Play();
        
	}
	
    void checkWindow(int pos, float[] freqData)
    {
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

                if (positions.ContainsKey(pos))
                {
                    positions[pos][k] = tmp;
                } else
                {
                    positions.Add(pos, new Vector3[g.Length]);
                }
            }
        }
    }

	// Update is called once per frame
	void Update () {

        int currentSample = audioSource.timeSamples;
        int pos = Mathf.FloorToInt( ((float) currentSample) / steps);
        int curStep = pos * steps * clip.channels;
        
        /*
        Debug.Log("currentSample: " + currentSample);
        Debug.Log("pos: " + pos);
        Debug.Log("curStep: " + curStep);
        */

        Vector3[] displayPositions = positions[curStep];

        for (int i = 0; i < displayPositions.Length; i++)
        {
            g[i].transform.position = displayPositions[i];
        }
    }
}
