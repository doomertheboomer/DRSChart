using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class drsExporter : MonoBehaviour
{
    static int msToTick(int ms, int bpm)
    {
        double gradient = ((double)bpm / 100d) * 0.008d;
        return (int)((double)ms * gradient);
    }
    static void sortNotes(List<GameObject> arr)
    {
        int n = arr.Count;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                // Swap if the element found is greater than the next element
                if (arr[j].transform.position.y > arr[j + 1].transform.position.y)
                {
                    GameObject temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
            }
        }
    }

    private List<GameObject> notes = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            notes.AddRange(GameObject.FindGameObjectsWithTag("noteL"));
            notes.AddRange(GameObject.FindGameObjectsWithTag("noteR"));

            sortNotes(notes);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true,
                ConformanceLevel = ConformanceLevel.Auto
            };

            using (XmlWriter writer = XmlWriter.Create("exported.xml", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("data");

                writer.WriteStartElement("seq_version");
                writer.WriteAttributeString("__type", "s32");
                writer.WriteString("8");
                writer.WriteEndElement();

                writer.WriteStartElement("info");

                writer.WriteStartElement("tick");
                writer.WriteAttributeString("__type", "s32");
                writer.WriteString("480");
                writer.WriteEndElement();

                writer.WriteStartElement("bpm_info");
                writer.WriteStartElement("bpm");

                string[] name = { "time", "delta_time", "bpm", "time", "delta_time", "num", "denomi" };
                string[] value = { "0", "0", "69420", "0", "0", "4", "4" };

                for (int i = 0; i < 3; i++)
                {
                    writer.WriteStartElement(name[i]);
                    writer.WriteAttributeString("__type", "s32");
                    writer.WriteString(value[i]);
                    writer.WriteEndElement();
                }
                /*
                writer.WriteElementString("time", "0");
                writer.WriteElementString("delta_time", "0");
                writer.WriteElementString("bpm", "69420");
                */
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("measure_info");
                writer.WriteStartElement("measure");

                for (int i = 3; i < value.Length; i++)
                {
                    writer.WriteStartElement(name[i]);
                    writer.WriteAttributeString("__type", "s32");
                    writer.WriteString(value[i]);
                    writer.WriteEndElement();
                }

                /*
                writer.WriteElementString("time", "0");
                writer.WriteElementString("delta_time", "0");
                writer.WriteElementString("num", "4");
                writer.WriteElementString("denomi", "4");
                */

                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement(); // end info

                writer.WriteStartElement("sequence_data");
                foreach (GameObject go in notes)
                {
                    writer.WriteStartElement("step");

                    float center = ((43690.7f * go.transform.position.x) + 65536f) / 2f;
                    float width = 0f;
                    if (go.transform.parent != null)
                    {
                        width = (go.transform.localScale.x * go.transform.parent.localScale.x) * 209903.6765f;
                    }
                    else
                    {
                        width = (go.transform.localScale.x) * 209903.6765f;
                    }

                    float pos_left = Mathf.Round(center - width);
                    float pos_right = Mathf.Round(center + width);
                    float time_ms = Mathf.Round((1000f * go.transform.position.y) + 5000f);

                    string[] names = { "stime_ms", "etime_ms", "stime_dt", "etime_dt", "category", "pos_left", "pos_right", "kind", "var", "player_id" };
                    string[] values = { time_ms.ToString(), time_ms.ToString(), msToTick((int)time_ms, 69420).ToString(),
                        msToTick((int)time_ms, 69420).ToString(), "0", pos_left.ToString(), pos_right.ToString(), "1", "0", "0"};

                    for (int i = 0; i < 2; i++)
                    {
                        writer.WriteStartElement(names[i]);
                        writer.WriteAttributeString("__type", "s64");
                        writer.WriteString(values[i]);
                        writer.WriteEndElement();
                    }
                    for (int i = 2; i < names.Length; i++)
                    {
                        writer.WriteStartElement(names[i]);
                        writer.WriteAttributeString("__type", "s32");
                        writer.WriteString(values[i]);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    /*
                    writer.WriteElementString("stime_ms", time_ms.ToString());
                    writer.WriteElementString("etime_ms", time_ms.ToString());
                    writer.WriteElementString("stime_dt", msToTick((int)time_ms, 69420).ToString()); 
                    writer.WriteElementString("etime_dt", msToTick((int)time_ms, 69420).ToString()); 
                    writer.WriteElementString("category", "0");   
                    writer.WriteElementString("pos_left", pos_left.ToString());
                    writer.WriteElementString("pos_right", pos_right.ToString());
                    writer.WriteElementString("kind", "1");
                    writer.WriteElementString("var", "0");
                    writer.WriteElementString("player_id", "0"); 
                    */

                    Debug.Log("time_ms = " + time_ms + " pos_left = " + pos_left + " pos_right = " + pos_right);
                }

                writer.WriteEndDocument();
            }

            // Code to execute when the space key is pressed
            Debug.Log("Chart exported to exported.xml!");
        }
    }
}
