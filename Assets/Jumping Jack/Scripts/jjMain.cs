using UnityEngine;
using System.Collections;

/*
 * Todo:
 *  + wrap position aroud edges
 *  + hide character behind lines
 *  + add upper & lower shadows for gap objects
 *  + add gui elements
 *  + use [ExecuteInEditMode] for guis; if debug || Application.isPlaying
 *  + put character in front of lines & gaps
 *  + matching 3d position with position on line
 *  + detect gaps below & above
 *  + add jumping through gaps
 *  + add falling through floors
 *  + add gaps when jumping through gaps
 *  x add backgroud music from the video
 *  + animations: jumping, falling, crashing, stunned
 *  + fill in gui elements
 *  + add crashing into ceiling
 *  + add game over
 *  + add win
 *  + add stunned
 *  + sounds: standing, running, jumping, falling, crashing, stunned, winning, losing
 *  + hazard jumps forward .3 width on invisible line
 *  - move score & highScore & lives to levelMgr
 *  - reinit entire level on 'R'
 *  - connect multiple levels
 *  - add initial jumping jack splash screen [Tribute to Jumping Jack by Adrian Gasinski]
 *  - readup story
 *  - white flash on crash
 *  - red flash on kill, audio for kill
 *  - keep track of score & high score, flash on high score
 * 
 * Design:
 *  - main // gather main components, singletons
 *  - input
 *  - level
 *    - player
 *    - npcs[]
 *    - scene (lines)
 *    - input handler (to be registered with main input)
 *  - ? handling events, e.g. get up after being stunned
 * 
 * Technical:
 *  - import frame sprite animation & apply via texture to object
 *  - use line drawer
 * 
 * Animations:
 *  - Jack
 *    - Standing
 *    - Running
 *    - Stunned
 *    - Jumping up a lane
 *    - Falling down
 *    - Crashing into ceiling
 *    - ? caught by enemy
 *  - Enemies? 7 types?
 * 
 * Tech:
 *  - using Invoke
 *  - using StartCouroutine() .. yield return new WaitForSeconds(.1f);
 * 
 */

public class jjMain : MonoBehaviour {

    private static float maxDeltaTime = 0.3f;

    public static float deltaTime { get { return Mathf.Min(maxDeltaTime, Time.deltaTime); } }

    public static jjLevel level;
    public static jjPlayer player;
    public static jjLevelMgr levelMgr;

    void Awake()
    {
        Random.seed = System.DateTime.Now.Millisecond;
    }
}
