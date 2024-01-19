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
                string[] value = { "0", "0", "12500", "0", "0", "4", "4" };

                for (int i = 0; i < 3; i++)
                {
                    writer.WriteStartElement(name[i]);
                    writer.WriteAttributeString("__type", "s32");
                    writer.WriteString(value[i]);
                    writer.WriteEndElement();
                }
 
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

                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement(); // end info

                writer.WriteStartElement("sequence_data");
                foreach (GameObject go in notes)
                {
                    if (go.GetComponent<noteMover>().isPoint)
                    {
                        continue; // skip holds
                    } 

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
                    float stime_ms = Mathf.Round((1000f * go.transform.position.y) + 5000f);
                    float etime_ms = Mathf.Round((1000f * go.transform.position.y) + 5000f);
                    string category = "0";

                    List<GameObject> long_points = new List<GameObject>();

                    foreach (Transform tr in go.GetComponentsInChildren<Transform>())
                    {
                        if (tr.tag == "noteL" || tr.tag == "noteR")
                        {
                            long_points.Add(tr.gameObject);
                        }
                    }
                    long_points.RemoveAt(0); // first object is the parent WHYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
                    sortNotes(long_points);

                    if (long_points.Count > 0) { etime_ms = Mathf.Round((1000f * long_points[^1].transform.position.y) + 5000f); category = "1"; }

                    Debug.Log("long_points: " + long_points.Count);

                    // start writing step data
                    writer.WriteStartElement("step");

                    string[] names = { "stime_ms", "etime_ms", "stime_dt", "etime_dt", "category", "pos_left", "pos_right", "kind", "var", "player_id" };
                    string[] values = { stime_ms.ToString(), etime_ms.ToString(), msToTick((int)stime_ms, 12500).ToString(),
                        msToTick((int)etime_ms, 12500).ToString(), category, pos_left.ToString(), pos_right.ToString(), go.GetComponent<noteMover>().kind.ToString(), "0", "0"};

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

                    Debug.Log("time_ms = " + stime_ms + " pos_left = " + pos_left + " pos_right = " + pos_right);

                    if (long_points.Count > 0)
                    {
                        writer.WriteStartElement("long_point");
                        foreach (GameObject point in long_points)
                        {
                            if (point.GetComponent<noteMover>().isSkidEnd) { continue; } // ignore skid ends, they are not "points"
                            // if (point.GetComponent<noteMover>().isSkidStart) { continue; } // temporarily disable skids

                            center = ((43690.7f * point.transform.position.x) + 65536f) / 2f;
                            width = 0f;
                            width = (point.transform.localScale.x * go.transform.localScale.x) * 209903.6765f;

                            pos_left = Mathf.Round(center - width);
                            pos_right = Mathf.Round(center + width);

                            float pos_lend = 0; float pos_rend = 0;

                            int pointAttributes = 3;
                            if (point.GetComponent<noteMover>().isSkidStart)
                            {
                                Debug.Log("is a skid!");

                                center = ((43690.7f * point.GetComponent<noteMover>().skidEnd.transform.position.x) + 65536f) / 2f;
                                width = 0f;
                                width = (point.GetComponent<noteMover>().skidEnd.transform.localScale.x * go.transform.localScale.x * 209903.6765f);

                                pos_lend = Mathf.Round(center - width);
                                pos_rend = Mathf.Round(center + width);

                                pointAttributes = 5;
                            }

                            float point_time = Mathf.Round((1000f * point.transform.position.y) + 5000f);

                            string[] titles = { "point_time", "pos_left", "pos_right", "pos_lend", "pos_rend" };
                            string[] content = { point_time.ToString(), pos_left.ToString(), pos_right.ToString(), pos_lend.ToString(), pos_rend.ToString() };

                            writer.WriteStartElement("point");

                            writer.WriteStartElement(titles[0]);
                            writer.WriteAttributeString("__type", "s64");
                            writer.WriteString(content[0]);
                            writer.WriteEndElement();

                            for (int i = 1; i < pointAttributes; i++)
                            {
                                writer.WriteStartElement(titles[i]);
                                writer.WriteAttributeString("__type", "s32");
                                writer.WriteString(content[i]);
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement(); // end point

                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                }

                writer.WriteEndDocument();
            }

            // Code to execute when the space key is pressed
            Debug.Log("Chart exported to exported.xml!");
        }
    }
}
