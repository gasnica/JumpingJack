using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class jjLevel : MonoBehaviour {

    // Layout from the bottom:
    //  54 pixels
    //  (2 pixels for line + 22 pixels for floor space) * 7 floors
    //  2 pixels for line
    //  32 pixels at the top
    //  total: 256

    [System.NonSerialized]
    public float left = -1.0f;
    [System.NonSerialized]
    public float right = 1.0f;
    [System.NonSerialized]
    public float bottom = (54 + 1) / 256.0f * 2.0f - 1.0f;// -0.8f;
    [System.NonSerialized]
    public float spacing = 24.0f / 256.0f * 2.0f; //0.2f;
    [System.NonSerialized]
    public int numFloors = 8;

    [System.NonSerialized]
    public float lineWidth = 2 / 256.0f * 2.0f;

    public float speed = 0.7f;

    [System.NonSerialized]
    public float jumpTolerance = (24.0f / 256.0f * 2.0f);// / 2.0f * 1.5f; // 4.0f * 1.5f; // spacing / 4
    [System.NonSerialized]
    public float fallTolerance = (24.0f / 256.0f * 2.0f) / 4.0f; // spacing / 4
    [System.NonSerialized]
    public float hazardTolerance = (24.0f / 256.0f * 2.0f) / 4.0f; // spacing / 8

    [System.NonSerialized]
    public bool isPlaying = true;

    public int initialNumHazards = 0;


    public class FloorGap
    {
        public FloorGap(float position, bool movesRight, GameObject gameObject) { this.position = position;  this.movesRight = movesRight; this.gameObject = gameObject; }
        public float position;
        public bool movesRight; // or left
        public GameObject gameObject;
    }

    public class Hazard
    {
        public Hazard(float position, GameObject gameObject) { this.position = position; this.gameObject = gameObject; }
        public float position;
        // moves left
        public GameObject gameObject;
    }

    [System.NonSerialized]
    public List<FloorGap> gaps = new List<FloorGap>();

    [System.NonSerialized]
    public List<Hazard> hazards = new List<Hazard>();

    private GameObject gapTemplate;
    private GameObject[] hazardTemplates;

    void Awake()
    {
        jjMain.level = this;
    }

	// Use this for initialization
	void Start ()
    {

        // Render background floor lines
        Vector3[] positions = RenderStaticLine().ToArray();
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetVertexCount(positions.Length);
        lineRenderer.SetPositions(positions);

        // Keep reference to gap template
        gapTemplate = transform.FindChild("Gap").gameObject;
        gapTemplate.transform.position = Vector3.forward * 20.0f;

        Transform hazardContainer = transform.FindChild("Hazards");
        hazardTemplates = new GameObject[] {
            hazardContainer.FindChild("Aeroplane").gameObject,
            hazardContainer.FindChild("Automobile").gameObject,
            hazardContainer.FindChild("Axe").gameObject,
            hazardContainer.FindChild("Ghost").gameObject,
            hazardContainer.FindChild("Ranger").gameObject,
        };

        // Randomly add 2 gaps
        AddRandomGap();
        AddRandomGap();

        // Randomly add hazards
        AddRandomHazards(initialNumHazards);
    }

    public void AddRandomHazards(int numHazards)
    {
        for (int i = 0; i < numHazards; i++) AddRandomHazard();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlaying) return;

        float deltaTime = jjMain.deltaTime;
        float width = right - left;

        foreach (FloorGap g in gaps)
        {
            g.position += deltaTime * speed * (g.movesRight ? 1.0f : -1.0f);
            if (g.position < 0) g.position += width * numFloors;
            if (g.position > width * numFloors) g.position -= width * numFloors;

            g.gameObject.transform.position = positionFloatToVec(g.position);
        }

        foreach (Hazard h in hazards)
        {

            h.position -= deltaTime * speed;
            if (h.position < 0.0f) h.position += width * numFloors;

            // hazard jumps forward .3 width on invisible line
            if (h.position < 0.8 * width && h.position > 0.5f * width) h.position -= 0.3f * width;

            Vector3 spriteHalfSize = h.gameObject.GetComponent<SpriteRenderer>().sprite.bounds.extents;
            spriteHalfSize.Scale(h.gameObject.transform.localScale);
            h.gameObject.transform.position = positionFloatToVec(h.position) + Vector3.up * (0.5f * lineWidth + spriteHalfSize.y);
        }

        // debug positions:
        {
            float pos = positionVecToFloat(jjMain.player.transform.position);
            GameObject.Find("Player Tracker").transform.position = positionFloatToVec(pos);
        }
    }

    public void ResetAndPlay()
    {
        isPlaying = true;
    }

    public void Win()
    {
        isPlaying = false;
        jjMain.levelMgr.Invoke("StartNextLevel", 2);
    }

    public void Lose()
    {
        isPlaying = false;
        jjMain.levelMgr.Invoke("GameOver", 2);
    }

    List<Vector3> RenderStaticLine()
    {
        List<Vector3> list = new List<Vector3>();
        for (int i = 0; i < numFloors; i++)
        {
            // show
            list.Add(new Vector3(left, bottom + i * spacing, 2.0f));

            // draw
            list.Add(new Vector3(left-0.01f, bottom + i * spacing, 0.1f));
            list.Add(new Vector3(left, bottom + i * spacing, 0.1f));
            list.Add(new Vector3(right, bottom + i * spacing, 0.1f));
            list.Add(new Vector3(right+0.01f, bottom + i * spacing, 0.1f));

            // hide
            list.Add(new Vector3(right, bottom + i * spacing, 2.0f));

        }

        return list;
    }

    public void AddRandomGap()
    {
        if (gaps.Count >= 8) return;

        float length = (right - left) * numFloors;
        // check if position is not anywhere close to existing gaps
        bool goesRight = Random.Range(0.0f, 2.0f) > 1.0f;
        float position = 0;

        // Ensure, we're not overlaping this gap with another one
        float gapDistTolerance = spacing * 2.0f;
        int maxAttempts = 100;
        while(maxAttempts-- != 0)
        {
            position = Random.Range(0.0f, length);
            bool gapsOverlapping = false;
            foreach (FloorGap gap in gaps)
                if (Mathf.Abs(gap.position - position) < gapDistTolerance
                     || Mathf.Abs(gap.position - position) > numFloors * (right - left) - gapDistTolerance)
                {
                    gapsOverlapping = true;
                    break;
                }
            if (!gapsOverlapping) break;
        }

        GameObject gapObject = GameObject.Instantiate(gapTemplate);
        gapObject.transform.parent = transform;
        gapObject.GetComponent<jjGap>().Init(goesRight);

        gaps.Add(new FloorGap(position, goesRight, gapObject));
    }

    public void AddRandomHazard()
    {
        if (hazards.Count >= 20) return;

        float length = (right - left) * numFloors;
        // check if position is not anywhere close to existing hazards
        float position = 0;

        // Ensure, we're not overlaping this hazard with another one
        float hazardDistTolerance = spacing * 2.0f;
        int maxAttempts = 100;
        while (maxAttempts-- != 0)
        {
            position = Random.Range(0.0f, length);
            bool hazardsOverlapping = false;
            foreach (Hazard hazard in hazards)
                if (Mathf.Abs(hazard.position - position) < hazardDistTolerance
                     || Mathf.Abs(hazard.position - position) > numFloors * (right - left) - hazardDistTolerance)
                {
                    hazardsOverlapping = true;
                    break;
                }
            if (!hazardsOverlapping) break;
        }

        int hazardIdx = (int)Mathf.Floor(Random.Range(0.0f, hazardTemplates.Length - 0.01f));
        GameObject hazardObject = GameObject.Instantiate(hazardTemplates[hazardIdx]);
        hazardObject.transform.parent = transform;
        hazardObject.GetComponent<jjWrappingSprite>().Init(jjWrappingSprite.MovementDirection.Left);

        hazards.Add(new Hazard(position, hazardObject));

    }


    public float positionVecToFloat(Vector3 point)
    {
        float xValue = Mathf.Clamp(point.x - left, 0.0f, right - left);
        // get floor counting from top. level above topmost ceiling is floor zero
        int maxFloorBelowCharacter = 0;
        for (int i = 0; i <= 8; i++)
        {
            float floorLevel = bottom + (numFloors - 1 - i) * spacing;
            if (floorLevel < point.y) break;
            maxFloorBelowCharacter = i;

        }

        return maxFloorBelowCharacter * (right - left) + xValue;
    }

    public Vector3 positionFloatToVec(float position)
    {
        float width = right - left;

        Vector3 result = Vector3.zero;
        result.y = Mathf.Floor(position / width);
        result.x = position - result.y * width;

        result.y = bottom + (numFloors - 1 - result.y) * spacing;
        result.x = left + result.x;

        return result;
    }

    public bool mustFall(Vector3 point)
    {
        // Look for gaps close & below
        float pos = positionVecToFloat(point) + right - left;
        foreach (FloorGap gap in gaps) if (Mathf.Abs(gap.position - pos) < fallTolerance) return true;
        return false;
    }

    public bool canJump(Vector3 point)
    {
        // Look for gaps close & above
        float pos = positionVecToFloat(point);
        foreach(FloorGap gap in gaps) if (Mathf.Abs(gap.position - pos) < jumpTolerance) return true;
        return false;
    }

    public bool isInHazardRange(Vector3 point)
    {
        float pos = positionVecToFloat(point) + right - left;
        foreach (Hazard hazard in hazards) if (Mathf.Abs(hazard.position - pos) < hazardTolerance) return true;
        return false;

    }

}
