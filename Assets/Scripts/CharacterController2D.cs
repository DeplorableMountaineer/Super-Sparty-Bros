using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // include so we can load new scenes

public class CharacterController2D : MonoBehaviour
{
    // player controls
    [Range(0.0f, 10.0f)] // create a slider in the editor and set limits on moveSpeed
    public float moveSpeed = 3f;

    public float jumpForce = 600f;

    // player health
    public int playerHealth = 1;

    // LayerMask to determine what is considered ground for the player
    public LayerMask whatIsGround;

    // Transform just below feet for checking if player is grounded
    public Transform groundCheck;

    // player can move?
    // we want this public so other scripts can access it but we don't want to show in editor as it might confuse designer
    [HideInInspector] public bool playerCanMove = true;

    // SFXs
    public AudioClip coinSfx;
    public AudioClip deathSfx;
    public AudioClip fallSfx;
    public AudioClip jumpSfx;
    public AudioClip victorySfx;

    // private variables below

    // store references to components on the gameObject
    private Transform _transform;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private AudioSource _audio;

    // hold player motion in this timestep
    private float _velocityX;
    private float _velocityY;

    // player tracking
    private bool _facingRight = true;
    private bool _isGrounded;
    private bool _isRunning;
    private bool _canDoubleJump;

    // store the layer the player is on (setup in Awake)
    int _playerLayer;

    // number of layer that Platforms are on (setup in Awake)
    int _platformLayer;
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Victory1 = Animator.StringToHash("Victory");
    private static readonly int Respawn1 = Animator.StringToHash("Respawn");

    private void Awake()
    {
        // get a reference to the components we are going to be changing and store a reference for efficiency purposes
        _transform = GetComponent<Transform>();

        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null) // if Rigidbody is missing
            Debug.LogError("Rigidbody2D component missing from this gameobject");

        _animator = GetComponent<Animator>();
        if (_animator == null) // if Animator is missing
            Debug.LogError("Animator component missing from this gameobject");

        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        {
            // if AudioSource is missing
            Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
            // let's just add the AudioSource component dynamically
            _audio = gameObject.AddComponent<AudioSource>();
        }

        // determine the player's specified layer
        _playerLayer = this.gameObject.layer;

        // determine the platform's specified layer
        _platformLayer = LayerMask.NameToLayer("Platform");
    }

    // this is where most of the player controller magic happens each game event loop
    private void Update()
    {
        // exit update if player cannot move or game is paused
        if (!playerCanMove || (Time.timeScale == 0f))
            return;

        // determine horizontal
        // velocity change based on the horizontal input
        _velocityX = Input.GetAxisRaw("Horizontal");

        // Determine if running based on the horizontal movement
        _isRunning = _velocityX != 0;

        // set the running animation state
        _animator.SetBool(Running, _isRunning);

        // get the current vertical velocity from the rigidbody component
        _velocityY = _rigidbody.velocity.y;

        // Check to see if character is grounded by raycasting from the middle of the player
        // down to the groundCheck position and see if collected with gameobjects on the
        // whatIsGround layer
        _isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);

        // Set the grounded animation states
        _animator.SetBool(Grounded, _isGrounded);

        if (_isGrounded) _canDoubleJump = true;

        if (_isGrounded && Input.GetButtonDown("Jump")
        ) // If grounded AND jump button pressed, then allow the player to jump
        {
            DoJump();
        }
        else if (_canDoubleJump && Input.GetButtonDown("Jump")
        ) // If can double jump AND jump button pressed, then allow the player to jump
        {
            _canDoubleJump = false;
            DoJump();
        }

        // If the player stops jumping mid jump and player is not yet falling
        // then set the vertical velocity to 0 (he will start to fall from gravity)
        if (Input.GetButtonUp("Jump") && _velocityY > 0f)
        {
            _velocityY = 0f;
        }

        // Change the actual velocity on the rigidbody
        _rigidbody.velocity = new Vector2(_velocityX * moveSpeed, _velocityY);

        // if moving up then don't collide with platform layer
        // this allows the player to jump up through things on the platform layer
        // NOTE: requires the platforms to be on a layer named "Platform"
        Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_velocityY > 0.0f));
    }

    private void DoJump()
    {
        // reset current vertical motion to 0 prior to jump
        _velocityY = 0f;
        // add a force in the up direction
        _rigidbody.AddForce(new Vector2(0, jumpForce));
        // play the jump sound
        PlaySound(jumpSfx);
    }

    // Checking to see if the sprite should be flipped
    // this is done in LateUpdate since the Animator may override the localScale
    // this code will flip the player even if the animator is controlling scale
    private void LateUpdate()
    {
        // get the current scale
        Vector3 localScale = _transform.localScale;

        if (_velocityX > 0) // moving right so face right
        {
            _facingRight = true;
        }
        else if (_velocityX < 0)
        {
            // moving left so face left
            _facingRight = false;
        }

        // check to see if scale x is right for the player
        // if not, multiple by -1 which is an easy way to flip a sprite
        if (((_facingRight) && (localScale.x < 0)) || ((!_facingRight) && (localScale.x > 0)))
        {
            localScale.x *= -1;
        }

        // update the scale
        _transform.localScale = localScale;
    }

    // if the player collides with a MovingPlatform, then make it a child of that platform
    // so it will go for a ride on the MovingPlatform
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform"))
        {
            this.transform.parent = other.transform;
        }
    }

    // if the player exits a collision with a moving platform, then unchild it
    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform"))
        {
            this.transform.parent = null;
        }
    }

    // do what needs to be done to freeze the player
    void FreezeMotion()
    {
        playerCanMove = false;
        _rigidbody.velocity = new Vector2(0, 0);
        _rigidbody.isKinematic = true;
    }

    // do what needs to be done to unfreeze the player
    void UnFreezeMotion()
    {
        playerCanMove = true;
        _rigidbody.isKinematic = false;
    }

    // play sound through the audiosource on the gameobject
    void PlaySound(AudioClip clip)
    {
        _audio.PlayOneShot(clip);
    }

    // public function to apply damage to the player
    public void ApplyDamage(int damage)
    {
        if (playerCanMove)
        {
            playerHealth -= damage;

            if (playerHealth <= 0)
            {
                // player is now dead, so start dying
                PlaySound(deathSfx);
                StartCoroutine(KillPlayer());
            }
        }
    }

    // public function to kill the player when they have a fall death
    public void FallDeath()
    {
        if (playerCanMove)
        {
            playerHealth = 0;
            PlaySound(fallSfx);
            StartCoroutine(KillPlayer());
        }
    }

    // coroutine to kill the player
    IEnumerator KillPlayer()
    {
        if (playerCanMove)
        {
            // freeze the player
            FreezeMotion();

            // play the death animation
            _animator.SetTrigger(Death);

            // After waiting tell the GameManager to reset the game
            yield return new WaitForSeconds(2.0f);

            if (GameManager.Gm) // if the gameManager is available, tell it to reset the game
                GameManager.Gm.ResetGame();
            else // otherwise, just reload the current level
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void CollectCoin(int amount)
    {
        PlaySound(coinSfx);

        if (GameManager.Gm) // add the points through the game manager, if it is available
            GameManager.Gm.AddPoints(amount);
    }

    // public function on victory over the level
    public void Victory()
    {
        PlaySound(victorySfx);
        FreezeMotion();
        _animator.SetTrigger(Victory1);

        if (GameManager.Gm) // do the game manager level compete stuff, if it is available
            GameManager.Gm.LevelCompete();
    }

    // public function to respawn the player at the appropriate location
    public void Respawn(Vector3 spawnloc)
    {
        UnFreezeMotion();
        playerHealth = 1;
        _transform.parent = null;
        _transform.position = spawnloc;
        _animator.SetTrigger(Respawn1);
    }

    public void EnemyBounce()
    {
        DoJump();
    }
}