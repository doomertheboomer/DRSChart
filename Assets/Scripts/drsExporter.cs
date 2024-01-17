using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class drsExporter : MonoBehaviour
{
    private List<GameObject> notes = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            notes.AddRange(GameObject.FindGameObjectsWithTag("noteL"));
            notes.AddRange(GameObject.FindGameObjectsWithTag("noteR"));
            
            foreach (GameObject go in notes)
            {
                float center = ((43690.7f * go.transform.position.x) + 65536f) / 2f;
                float width = 0;
                if (go.transform.parent != null)
                {
                    width = (go.transform.localScale.x * go.transform.parent.localScale.x) * 209903.6765f;
                } else
                {
                    width = (go.transform.localScale.x) * 209903.6765f;
                }

                Debug.Log("pos_left = " + (center - width) + " pos_right = " + (center + width));
            }

            // Code to execute when the space key is pressed
            Debug.Log("Space key pressed!");
        }
    }
}
