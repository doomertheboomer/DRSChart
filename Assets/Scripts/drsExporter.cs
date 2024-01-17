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

    private List<GameObject> notes = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            notes.AddRange(GameObject.FindGameObjectsWithTag("noteL"));
            notes.AddRange(GameObject.FindGameObjectsWithTag("noteR"));


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
                writer.WriteString("8");
                writer.WriteEndElement();

                writer.WriteStartElement("info");
                writer.WriteStartElement("tick");
                writer.WriteString("480");
                writer.WriteEndElement();

                writer.WriteStartElement("bpm_info");
                writer.WriteStartElement("bpm");
                writer.WriteElementString("time", "0");
                writer.WriteElementString("delta_time", "0");
                writer.WriteElementString("bpm", "69420");
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("measure_info");
                writer.WriteStartElement("measure");
                writer.WriteElementString("time", "0");
                writer.WriteElementString("delta_time", "0");
                writer.WriteElementString("num", "4");
                writer.WriteElementString("denomi", "4");
                writer.WriteEndElement();
                writer.WriteEndElement();

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

                    Debug.Log("time_ms = " + time_ms + " pos_left = " + pos_left + " pos_right = " + pos_right);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            // Code to execute when the space key is pressed
            Debug.Log("Space key pressed!");
        }
    }
}
