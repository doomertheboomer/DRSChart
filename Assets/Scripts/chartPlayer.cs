using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

public class chartPlayer : MonoBehaviour
{
    public AudioSource audioSource;

    IEnumerator LoadAudioFile(string path)
    {
        using (WWW www = new WWW(path))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                // Assign the downloaded audio clip to the AudioSource component
                audioSource.clip = www.GetAudioClip();
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Error loading audio file: " + www.error);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Try to get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // If it's not present, add it dynamically
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Specify the path to the audio file (change this according to your setup)
        string audioFileName = "chart.wav";
        string audioFilePath = Path.Combine(Environment.CurrentDirectory, audioFileName);
        audioFilePath = audioFilePath.Replace('\\', '/');


        Debug.Log(audioFilePath);

        // Load the audio file using UnityWebRequest
        StartCoroutine(LoadAudioFile(audioFilePath));
    }

    // Update is called once per frame
    void Update()
    {
        float currentTimestamp = audioSource.time;
        transform.position = new Vector3(0, currentTimestamp, 0);
    }
}
