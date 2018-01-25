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

    public ArrayList leftLaneBalls;
    public ArrayList midLaneBalls;
    public ArrayList rightLaneBalls;
    int numLanes = 3;
    public Bumper leftBumper;
    public Bumper midBumper;
    public Bumper rightBumper;

    Dictionary<int, Vector3[]> positions;
    Dictionary<int, bool[]> beatMap;
    Dictionary<int, bool> instantiatedBeatMap; //prevent duplicate beat creation

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
        leftLaneBalls = new ArrayList();
        midLaneBalls = new ArrayList();
        rightLaneBalls = new ArrayList();
        leftBumper = new Bumper(new Vector3(12, 0, 2), Color.red);
        midBumper = new Bumper(new Vector3(14, 0, 2), Color.blue);
        rightBumper = new Bumper(new Vector3(16, 0, 2), Color.green);

        beatMap = new Dictionary<int, bool[]>();
        instantiatedBeatMap = new Dictionary<int, bool>();
        createBeatMap();

        audioSource.Play();
        
	}
	
    void checkWindow(int pos, float[] freqData)
    {
        int k = 0;
        int crossover = 2;
        float scale = .125f;
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
                Vector3 tmp = new Vector3(g[k].transform.position.x, height * scale, g[k].transform.position.z);
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

    void createBeatMap()
    {
        foreach (var entry in positions)
        {
            int timePos = entry.Key;
            Vector3[] objPos = entry.Value;
            bool[] activeBeats = new bool[] { false, false, false };

            float[] threshold = new float[g.Length];
            threshold[0] = 0;
            threshold[1] = 24;
            threshold[2] = 21f;
            threshold[3] = 12;
            threshold[4] = 6.5f;
            threshold[5] = 4.5f;
            threshold[6] = 2;
            threshold[7] = 1;
            threshold[8] = 0.5f;
            threshold[9] = 0;
            //left 3 = leftLane & so forth
            if (objPos[1].y > threshold[1] || objPos[2].y > threshold[2] || objPos[3].y > threshold[3])
            {
                activeBeats[0] = true;
            }
            beatMap.Add(timePos, activeBeats);
        }
    }

	// Update is called once per frame
	void Update () {
        float avgFrameRate = Time.frameCount / Time.time;
        int currentSample = audioSource.timeSamples;
        int pos = Mathf.FloorToInt( ((float) currentSample) / steps);
        int curStep = pos * steps * clip.channels;
        int speed = 10;

        /*
        Debug.Log("currentSample: " + currentSample);
        Debug.Log("pos: " + pos);
        Debug.Log("curStep: " + curStep);
        */
        //Debug.Log("framerate: " + avgFrameRate);

        //visualizer
        Vector3[] displayPositions = positions[curStep];

        for (int i = 0; i < displayPositions.Length; i++)
        {
            g[i].transform.position = displayPositions[i];
        }



        //beats
        //lookahead to see if we need to instantiate future beats to drop down
        int lookAheadSeconds = 3;
        int lookAheadFrames = (clip.frequency * clip.channels) * lookAheadSeconds;
        bool[] beats = beatMap[curStep + lookAheadFrames];
        //make sure we didn't already check this frame
        //checking left lane
        if (beats[0] && !instantiatedBeatMap.ContainsKey(curStep + lookAheadFrames))
        {
            Beat beat = new Beat(new Vector3(12, lookAheadSeconds * speed, 0), new Vector3(0, -speed, 0), Color.red); //30 = how long it takes to drop

            leftLaneBalls.Add(beat);
            instantiatedBeatMap.Add(curStep + lookAheadFrames, true); //prevent duplicate instantiating
        }

        foreach (Beat beat in leftLaneBalls) {
            if (beat.getGameObject().transform.position.y < -5)
            {
                Destroy(beat.getGameObject());
                leftLaneBalls.Remove(beat); //buggy, can't iterate & remove at same time
            }
        }
    }
}
