using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;

public class point
{
    public bool isSkid; // only used internally. xml does not contain this
    public long point_time; // time the point starts, game will assume it ends at etime_ms unless otherwise stated by next pt.
    public int pos_left; // indicates landing position of a hold
    public int pos_right;
    public int pos_lend; // only use these if isSkid is true. this indicates the landing position of a slide
    public int pos_rend;
}

public class step
{
    public long stime_ms; // starting time
    public long etime_ms; // ending time
    public int stime_dt; // what the fuck is this (delta time?)
    public int etime_dt; // what the fuck is this (delta time?)
    public int category; // 0 = step, 1 = slide
    public int pos_left;
    public int pos_right;
    public int kind; // note type. 1/2/3/4 l/r/jump/down
    public int player_id; // whatever tf this is yang penting kalo down/jump jadi 4
    public List<point> long_point; // only used when category = 1
}


public class drsParser : MonoBehaviour
{
    static int msToTick(int ms, int bpm)
    {
        double gradient = ((double)bpm / 100d) * 0.008d;
        return (int)((double)ms * gradient);
    }

    static int tickToMs(int tick, int bpm)
    {
        double gradient = 1d / (((double)bpm / 100d) * 0.008d);
        return (int)((double)tick * gradient);
    }

    static point parsePoint(XElement element, int ver = 8, int bpm = 13000)
    {
        point result = new point();
        
        switch (ver)
        {
            case 8:
                result.point_time = long.Parse(element.Element("point_time").Value);
                result.pos_left = int.Parse(element.Element("pos_left").Value);
                result.pos_right = int.Parse(element.Element("pos_right").Value);

                // check if note is a skid by checking for existence of pos_lend and pos_lend
                if (element.Element("pos_lend") != null && element.Element("pos_rend") != null)
                {
                    result.isSkid = true;
                    result.pos_lend = int.Parse(element.Element("pos_lend").Value);
                    result.pos_rend = int.Parse(element.Element("pos_rend").Value);
                }
                else
                {
                    result.isSkid = false;
                }
                break;
            case 9:
                result.point_time = tickToMs(int.Parse(element.Element("tick").Value), bpm);
                result.pos_left = int.Parse(element.Element("left_pos").Value);
                result.pos_right = int.Parse(element.Element("right_pos").Value);

                // check if note is a skid by checking for existence of pos_lend and pos_lend
                if (element.Element("left_end_pos") != null && element.Element("right_end_pos") != null)
                {
                    result.isSkid = true;
                    result.pos_lend = int.Parse(element.Element("left_end_pos").Value);
                    result.pos_rend = int.Parse(element.Element("right_end_pos").Value);
                }
                else
                {
                    result.isSkid = false;
                }
                break;
            default:
                Debug.Log("Chart Format not supported!");
                break;
        }
        return result;
    }

    static step parseStep(XElement element, int ver = 8, int bpm = 13000)
    {
        step result = new step();

        switch (ver)
        {
            case 8:
                result.stime_ms = long.Parse(element.Element("stime_ms").Value);
                result.etime_ms = long.Parse(element.Element("etime_ms").Value);
                result.stime_dt = int.Parse(element.Element("stime_dt").Value);
                result.etime_dt = int.Parse(element.Element("etime_dt").Value);
                result.category = int.Parse(element.Element("category").Value);
                result.pos_left = int.Parse(element.Element("pos_left").Value);
                result.pos_right = int.Parse(element.Element("pos_right").Value);
                result.kind = int.Parse(element.Element("kind").Value);
                result.player_id = int.Parse(element.Element("player_id").Value);

                // check if note is a slide based on category and existence of long_point array
                if (result.category == 1 && element.Element("long_point") != null)
                {
                    result.long_point = element.Element("long_point")?
                    .Elements("point")
                    .Select(point => parsePoint(point, ver, bpm))
                    .ToList();
                }
                break;
            case 9:
                result.stime_dt = int.Parse(element.Element("start_tick").Value);
                result.etime_dt = int.Parse(element.Element("end_tick").Value);
                // convert ticks to ms for internal use of the chart editor
                result.stime_ms = (tickToMs(result.stime_dt, bpm));
                result.etime_ms = (tickToMs(result.etime_dt, bpm));
                result.pos_left = int.Parse(element.Element("left_pos").Value);
                result.pos_right = int.Parse(element.Element("right_pos").Value);
                result.kind = int.Parse(element.Element("kind").Value);
                result.player_id = int.Parse(element.Element("player_id").Value);

                // check if note is a slide based on existence of long_point array
                if (element.Element("long_point") != null)
                {
                    result.long_point = element.Element("long_point")?
                    .Elements("point")
                    .Select(point => parsePoint(point, ver, bpm))
                    .ToList();
                }
                break;
            default:
                Debug.Log("Chart Format not supported!");
                break;
        }
        return result;
    }

    List<step> getSteps(XDocument xdoc)
    {
        int revision = int.Parse(xdoc.Element("data").Element("seq_version").Value);
        int bpm = int.Parse(xdoc.Element("data").Element("info").Element("bpm_info").Element("bpm").Value);

        List<step> result = xdoc.Element("data").Element("sequence_data")?
            .Elements("step")
            .Select(step => parseStep(step, revision, bpm))
            .ToList();

        return result;
    }

    static void PrintAllElements(List<step> steps)
    {
        foreach (var step in steps)
        {
            Debug.Log($"Step: stime_ms={step.stime_ms}, etime_ms={step.etime_ms}, stime_dt={step.stime_dt}, etime_dt={step.etime_dt}, category={step.category}, pos_left={step.pos_left}, pos_right={step.pos_right}, kind={step.kind}, player_id={step.player_id}");

            if (step.long_point != null)
            {
                foreach (var point in step.long_point)
                {
                    if (point.isSkid == true)
                    {
                        Debug.Log($"  Point: point_time={point.point_time}, pos_left={point.pos_left}, pos_right={point.pos_right}, pos_lend={point.pos_lend}, pos_rend={point.pos_rend}");
                    }
                    else
                    {
                        Debug.Log($"  Point: point_time={point.point_time}, pos_left={point.pos_left}, pos_right={point.pos_right}");
                    }
                }
            }
        }
    }

    public GameObject noteL;
    public GameObject noteR;
    public GameObject skidL;
    public GameObject skidR;

    public List<step> stepList;

    public void renderStep(step step)
    {
        /*
         * NOTE LAYERING SYSTEM:
         * 1. L
         * 2. R
         * 3. Down
         * 4. Jump
         * 5. Skid/Hold heads
         * 6. Skid/Hold paths
        */

        GameObject note = noteL; GameObject skid = skidL;
        // if note is right/jump
        if (step.kind == 2 || step.kind == 4)
        {
            note = noteR; skid = skidR;
        }

        // get note positions
        float posX = (((((float)step.pos_left + (float)step.pos_right) / 2f) / 65536f) * 3f) - 1.5f; // get center of note. lane is from -1.5 to 1.5 (simplified to y = (0.0000228882 * (step.pos_left + step.pos_right)) - 1.5)
        float posY = ((float)step.stime_ms / 1000f) - 5f; // where to spawn the note. lane is from -5 to 5 (simplified to y = (0.001 * step.stime_ms) - 5
        float width = 0.1561097f * Mathf.Abs(((float)step.pos_left - (float)step.pos_right) / 65536f); // get width of note in percentage then multiplied by constant (simplified to y = 0.00000238204498291 * width)

        // create step note object
        GameObject noteInstance = Instantiate(note, new Vector3(posX, posY, step.kind), Quaternion.identity); // use note type as layering system
        noteInstance.transform.localScale = new Vector3(width, 0.1084799f, 1.644383f);

        // check if note is a long note and render all longs
        if (step.category == 1 || step.long_point != null)
        {
            for (int i = 0; i < step.long_point.Count; i++)
            {
                point point = step.long_point[i];
                // check if current point is skid or hold
                if (point.isSkid)
                {
                    float skidX, skidY, skidW;
                    // check for skid direction
                    if (point.pos_lend > point.pos_left) // right
                    {
                        skidX = (((((float)point.pos_left + (float)point.pos_rend) / 2f) / 65536f) * 3f) - 1.5f;
                        skidW = 0.1561097f * Mathf.Abs(((float)point.pos_left - (float)point.pos_rend)) / 65536f;
                    }
                    else // left
                    {
                        skidX = (((((float)point.pos_right + (float)point.pos_lend) / 2f) / 65536f) * 3f) - 1.5f;
                        skidW = 0.1561097f * Mathf.Abs(((float)point.pos_right - (float)point.pos_lend)) / 65536f;
                    }
                    skidY = ((float)point.point_time / 1000) - 5;

                    // spawn skid object
                    GameObject skidInstance = Instantiate(note, new Vector3(skidX, skidY, 5), Quaternion.identity); // skids are always behind
                    skidInstance.transform.localScale = new Vector3(skidW, 0.1084799f, 1.644383f);
                    skidInstance.transform.SetParent(noteInstance.transform);

                    // render skid "stem"
                    float stemX = (((((float)point.pos_left + (float)point.pos_right) / 2f) / 65536f) * 3f) - 1.5f;
                    float stemY, stemH;
                    if (i == 0) // if its the first point, take the parent step as stem
                    {
                        stemY = ((((float)point.point_time + (float)step.stime_ms) / 2f) / 1000) - 5;
                        stemH = ((float)point.point_time - (float)step.stime_ms) * 0.00128291f;
                    }
                    else
                    {
                        stemY = ((((float)point.point_time + (float)step.long_point[i - 1].point_time) / 2f) / 1000) - 5;
                        stemH = ((float)point.point_time - (float)step.long_point[i - 1].point_time) * 0.00128291f;

                    }

                    float stemW = 0.1561097f * Mathf.Abs(((float)point.pos_left - (float)point.pos_right)) / 65536f;
                    GameObject stemInstance = Instantiate(skid, new Vector3(stemX, stemY, 6), Quaternion.identity); // stems are always behind
                    stemInstance.transform.localScale = new Vector3(stemW, stemH, 1.644383f);
                    stemInstance.transform.SetParent(skidInstance.transform);
                }
                else
                {
                    // handling for holds, pretty difficult stuff will do later
                    float startTime;
                    float endTime;
                    float startL;
                    float startR;
                    float endL;
                    float endR;

                    if (i == 0) // if its the first point, get parent step as starting pos
                    {
                        startTime = (float)step.stime_ms;
                        startL = (float)step.pos_left;
                        startR = (float)step.pos_right;
                    }
                    else // if its in the middle, get last point as starting pos
                    {
                        startTime = (float)step.long_point[i - 1].point_time;

                        // use lend and rend for parent skid
                        if (step.long_point[i - 1].isSkid)
                        {
                            startL = (float)step.long_point[i - 1].pos_lend;
                            startR = (float)step.long_point[i - 1].pos_rend;
                        }
                        else
                        {
                            startL = (float)step.long_point[i - 1].pos_left;
                            startR = (float)step.long_point[i - 1].pos_right;
                        }
                    }

                    endL = (float)point.pos_left;
                    endR = (float)point.pos_right;
                    endTime = (float)point.point_time;
                    // if its the last point, get parent step as ending time
                    if (i == (step.long_point.Count - 1)) { endTime = (float)step.etime_ms; }

                    // get L R position gradient
                    float lGrad = (endL - startL) / (endTime - startTime);
                    float rGrad = (endR - startR) / (endTime - startTime);

                    // spawn hold head (final point). copied code from above
                    float holdX = ((((endL + endR) / 2f) / 65536f) * 3f) - 1.5f; // get center of note. lane is from -1.5 to 1.5 (simplified to y = (0.0000228882 * (step.pos_left + step.pos_right)) - 1.5)
                    float holdY = (endTime / 1000f) - 5f; // where to spawn the note. lane is from -5 to 5 (simplified to y = (0.001 * step.stime_ms) - 5
                    float holdWidth = 0.1561097f * Mathf.Abs((endL - endR) / 65536f); // get width of note in percentage then multiplied by constant (simplified to y = 0.00000238204498291 * width)
                   
                    GameObject holdParent = Instantiate(note, new Vector3(holdX, holdY, 5), Quaternion.identity); // holds are always behind
                    holdParent.transform.localScale = new Vector3(holdWidth, 0.1084799f, 1.644383f);
                    holdParent.transform.SetParent(noteInstance.transform);

                    // spawn one note each 10ms for the illusion of a "drag"
                    for (float j = startTime; j <= endTime; j += 10f)
                    {
                        // calc L and R pos
                        float left = startL + lGrad * (j - startTime);
                        float right = startR + rGrad * (j - startTime);

                        // code duplicated from step alg
                        holdX = ((((left + right) / 2f) / 65536f) * 3f) - 1.5f; // get center of note. lane is from -1.5 to 1.5
                        holdY = (j / 1000f) - 5f; // where to spawn the note. lane is from -5 to 5
                        holdWidth = 0.1561097f * Mathf.Abs((left - right) / 65536f); // get width of note in percentage then multiplied by constant

                        GameObject holdInstance = Instantiate(skid, new Vector3(holdX, holdY, 6), Quaternion.identity); // holds are always behind
                        holdInstance.transform.localScale = new Vector3(holdWidth, 0.1084799f, 1.644383f);
                        holdInstance.transform.SetParent(holdParent.transform);
                    }
                }
            }
        }
    }

    public void renderSteps(List<step> input)
    {
        foreach (var step in input)
        {
            renderStep(step);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        string filePath = "chart.xml";
        XDocument xdoc = XDocument.Load(filePath);

        stepList = getSteps(xdoc);

        renderSteps(stepList);

        PrintAllElements(stepList);
    }

    // Update is called once per frame
    void Update()
    {

    }
}