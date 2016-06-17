using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Animator))]
public class jjPlayer : MonoBehaviour {

    public float speed = 0.6f;
    public float jumpDuration = 0.5f;
    public float crashDuration = 0.25f;
    public float fallDuration = 0.5f;
    public float killDuration = 0.1f;
    //public float shortStunDuration = 0.5f;
    //public float longStunDuration = 1.5f;
    public int floor { get; private set; }
    private float wrappingTolerance;

    private Coroutine currentCoroutine = null;
    private AudioSource[] audioSources;
    private Sound activeSound = Sound.None;

    public enum Sound {
        None,
        FallAndLongStun,
        FallAndLose,
        FallAndShortStun,
        Jump,
        JumpAndCrashAndLongStun,
        JumpAndCrashAndLose,
        Kill,
        Run,
        Stand,
        Win,
        NumSounds
    }

	// Use this for initialization
    void Awake()
    {
        jjMain.player = this;
    }

	void Start ()
    {
        wrappingTolerance = jjMain.level.spacing / 4.0f;
        floor = 0;
        jjLevel level = jjMain.level;
        Vector3 spriteHalfSize = GetComponent<SpriteRenderer>().sprite.bounds.extents;
        Vector3 position = transform.position;
        position.x -= 0.33f;
        position.y = level.bottom - level.spacing + 0.5f * level.lineWidth + spriteHalfSize.y * transform.localScale.y;
        position.z = -0.1f;
        transform.position = position;

        GetComponent<jjWrappingSprite>().Init();

        LoadSounds();

        // assume initial state standing
        PlaySound(Sound.Stand);
	}

    void LoadSounds()
    {
        audioSources = new AudioSource[(int)Sound.NumSounds];
        for (int i = 0; i < (int)Sound.NumSounds; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = Resources.Load<AudioClip>(string.Format("Sounds/{0}", ((Sound)i).ToString()));
            audioSources[i].playOnAwake = false;
        }

        audioSources[(int)Sound.Stand].loop = true;
        audioSources[(int)Sound.Run].loop = true;

        //audioSources[(int)Sound.Step].volume = 0.5f;
        //audioSources[(int)Sound.Step].pitch = 1.5f;
    }

    void PlaySound(Sound sound)
    {
        if (sound != activeSound || !audioSources[(int)sound].loop)
        {
            audioSources[(int)activeSound].Stop();
            activeSound = sound;
            audioSources[(int)activeSound].Play();
        }
    }

    void StopSound(Sound sound) { PlaySound(Sound.None); }

    float SoundLength(Sound sound) { return audioSources[(int)sound].clip.length; }

    float RemainingSoundLength() { return audioSources[(int)activeSound].clip.length - audioSources[(int)activeSound].time; }

    // Update is called once per frame
    void Update()
    {
        if (!jjMain.level.isPlaying) return;

        if (state == State.StandingOrRunning || state == State.Stunned)
        {

            // Check if needs to fall
            if (jjMain.level.mustFall(transform.position))
            {
                // Fall
                state = State.Falling;
                GetComponent<Animator>().SetInteger("Movement", 0);
                GetComponent<Animator>().SetTrigger("Fall");
                if (floor == 1)
                {
                    if (jjMain.levelMgr.numLives == 1) jjMain.level.Lose();
                    PlaySound(jjMain.levelMgr.numLives == 1 ? Sound.FallAndLose : Sound.FallAndLongStun);
                }
                else
                    PlaySound(Sound.FallAndShortStun);
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);
                currentCoroutine = StartCoroutine(UpdateFalling());
            }
        }

        if (state == State.StandingOrRunning)
        {
            // check for a kill
            if (jjMain.level.isInHazardRange(transform.position))
            {
                state = State.Kill;
                PlaySound(Sound.Kill);
                currentCoroutine = StartCoroutine(UpdateKilled());
            }
        }

        if (state == State.StandingOrRunning)
        {

            int movement = 0;
            if (Input.GetKey(KeyCode.UpArrow))
            {

                if (jjMain.level.canJump(transform.position))
                {
                    // Jump
                    GetComponent<Animator>().SetInteger("Movement", 0);
                    GetComponent<Animator>().SetTrigger("Jump");

                    // Start coroutine
                    state = State.Jumping;
                    jjMain.level.AddRandomGap();
                    jjMain.levelMgr.score += 5;
                    PlaySound(floor == 7 ? Sound.Win : Sound.Jump);
                    if (floor == 7) jjMain.level.Win();
                    currentCoroutine = StartCoroutine(UpdateJumping());
                }
                else
                {
                    // hit ceiling
                    GetComponent<Animator>().SetInteger("Movement", 0);
                    GetComponent<Animator>().SetTrigger("Crash");

                    // Start coroutine
                    state = State.Crashing;
                    PlaySound(jjMain.levelMgr.numLives == 1 && floor == 0 ? Sound.JumpAndCrashAndLose : Sound.JumpAndCrashAndLongStun);
                    if (jjMain.levelMgr.numLives == 1 && floor == 0) jjMain.level.Lose();
                    currentCoroutine = StartCoroutine(UpdateCrashing());
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftArrow)) movement--;
                if (Input.GetKey(KeyCode.RightArrow)) movement++;
                GetComponent<Animator>().SetInteger("Movement", movement);

                // Move the player game object
                Vector3 position = transform.position;
                position.x += jjMain.deltaTime * speed * movement;

                jjLevel level = jjMain.level;
                float levelWidth = level.right - level.left;
                // Those not needed with UpdateWrapping()
                if (position.x < level.left) position.x += levelWidth;
                if (position.x >= level.right) position.x -= levelWidth;
                transform.position = position;

                // Sound
                PlaySound(movement == 0 ? Sound.Stand : Sound.Run);

                if (position.x < level.left + wrappingTolerance)
                {
                    state = State.WrappingFloor;
                    currentCoroutine = StartCoroutine(UpdateWrapping(false));
                }
                if (position.x > level.right - wrappingTolerance)
                {
                    state = State.WrappingFloor;
                    currentCoroutine = StartCoroutine(UpdateWrapping(true));
                }

            }

            // Cheats:
            if (Input.GetKeyDown(KeyCode.U) && floor < 8)
            {
                floor++;
                transform.position = transform.position + Vector3.up * jjMain.level.spacing;
            }
            if (Input.GetKeyDown(KeyCode.D) && floor > 0)
            {
                floor--;
                transform.position = transform.position - Vector3.up * jjMain.level.spacing;
            }

        }

    }

    IEnumerator UpdateWrapping(bool moveRight)
    {
        float elapsedTime = 0.0f;
        Vector3 startPosition = transform.position;

        float movementMultiplier = 2.0f;
        float distanceToTravel = wrappingTolerance * 2.5f;
        float distanceTraveled = 0.0f;

        while (true)
        {
            elapsedTime += jjMain.deltaTime;
            elapsedTime = Mathf.Min(elapsedTime, jumpDuration);
            float deltaDistance = jjMain.deltaTime * speed * movementMultiplier;
            distanceTraveled += deltaDistance;

            jjLevel level = jjMain.level;
            float levelWidth = level.right - level.left;
            Vector3 position = transform.position + (moveRight ? Vector3.right : Vector3.left) * deltaDistance;
            if (position.x < level.left) position.x += levelWidth;
            if (position.x >= level.right) position.x -= levelWidth;
            transform.position = position;

            if (distanceTraveled < distanceToTravel)
            {
                yield return null;//  new WaitForSeconds(Time.fixedDeltaTime);
            }
            else
            {
                currentCoroutine = null;
                state = State.StandingOrRunning;
                yield break;
            }
        }
    }


    IEnumerator UpdateJumping()
    {
        float elapsedTime = 0.0f;
        Vector3 startPosition = transform.position;

        while(true)
        {
            elapsedTime += jjMain.deltaTime;
            elapsedTime = Mathf.Min(elapsedTime, jumpDuration);
            transform.position = startPosition + elapsedTime / jumpDuration * Vector3.up * jjMain.level.spacing;

            if (elapsedTime < jumpDuration)
            {
                yield return null;//  new WaitForSeconds(Time.fixedDeltaTime);
            }
            else
            {
                currentCoroutine = null;
                floor++;
                if (jjMain.level.isPlaying)
                {
                    state = State.StandingOrRunning;
                    GetComponent<Animator>().SetTrigger("Recover");
                }
                yield break;
            }
        }
    }

    IEnumerator UpdateCrashing()
    {
        float elapsedTime = 0.0f;
        Vector3 startPosition = transform.position;

        while (true)
        {
            elapsedTime += jjMain.deltaTime;
            elapsedTime = Mathf.Min(elapsedTime, crashDuration);
            transform.position = startPosition + elapsedTime / crashDuration * Vector3.up * 0.4f * jjMain.level.spacing;

            if (elapsedTime < crashDuration)
            {
                yield return null;//  new WaitForSeconds(Time.fixedDeltaTime);
            }
            else
            {
                state = State.Stunned;
                if (floor == 0) jjMain.levelMgr.numLives--;
                transform.position = startPosition;
                GetComponent<Animator>().SetTrigger("Stun");
                currentCoroutine = StartCoroutine(UpdateStunned(true));
                yield break;
            }
        }
    }

    IEnumerator UpdateFalling()
    {
        float elapsedTime = 0.0f;
        Vector3 startPosition = transform.position;

        while (true)
        {
            elapsedTime += jjMain.deltaTime;
            elapsedTime = Mathf.Min(elapsedTime, fallDuration);
            transform.position = startPosition - elapsedTime / fallDuration * Vector3.up * jjMain.level.spacing;

            if (elapsedTime < fallDuration)
            {
                yield return null;//  new WaitForSeconds(Time.fixedDeltaTime);
            }
            else
            {
                state = State.Stunned;
                floor--;
                if (floor == 0) jjMain.levelMgr.numLives--;
                GetComponent<Animator>().SetTrigger("Stun");
                currentCoroutine = StartCoroutine(UpdateStunned(floor == 0));
                yield break;
            }
        }
    }

    IEnumerator UpdateKilled()
    {
        float elapsedTime = 0.0f;

        while (true)
        {
            elapsedTime += jjMain.deltaTime;
            elapsedTime = Mathf.Min(elapsedTime, killDuration);

            if (elapsedTime < killDuration)
            {
                yield return null;//  new WaitForSeconds(Time.fixedDeltaTime);
            }
            else
            {
                state = State.Falling;
                GetComponent<Animator>().SetInteger("Movement", 0);
                GetComponent<Animator>().SetTrigger("Fall");
                if (floor == 1)
                {
                    if (jjMain.levelMgr.numLives == 1) jjMain.level.Lose();
                    PlaySound(jjMain.levelMgr.numLives == 1 ? Sound.FallAndLose : Sound.FallAndLongStun);
                }
                else
                    PlaySound(Sound.FallAndShortStun);
                currentCoroutine = StartCoroutine(UpdateFalling());
                yield break;
            }
        }
    }


    IEnumerator UpdateStunned(bool longStun)
    {
        float elapsedTime = 0.0f;
        Vector3 startPosition = transform.position;

        //float stunDuration = longStun ? longStunDuration : shortStunDuration;
        float stunDuration = RemainingSoundLength();

        while (true)
        {
            elapsedTime += jjMain.deltaTime;
            elapsedTime = Mathf.Min(elapsedTime, stunDuration);

            if (elapsedTime < stunDuration)
            {
                yield return null;//  new WaitForSeconds(Time.fixedDeltaTime);
            }
            else
            {
                currentCoroutine = null;
                if (jjMain.level.isPlaying)
                {
                    GetComponent<Animator>().SetTrigger("Recover");
                    state = State.StandingOrRunning;
                }
                yield break;
            }
        }
    }

    private enum State
    {
        StandingOrRunning, // The only responsive state
        Wrapping, // Wrapping around the edge
        Jumping,
        Falling,
        Stunned,
        Crashing,
        WrappingFloor,
        Kill,
        // Caught by hazard
    }

    private State state = State.StandingOrRunning;

}
