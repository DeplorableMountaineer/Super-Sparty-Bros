﻿using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Stunned1 = Animator.StringToHash("Stunned");
    private static readonly int Stand1 = Animator.StringToHash("Stand");
    [Range(0, 10)] public float moveSpeed = 4f; // enemy move speed when moving
    public int damageAmount = 10; // probably deal a lot of damage to kill player immediately

    [Tooltip("Child game object with collider for detecting stun")]
    public GameObject stunnedCheck; // what gameobject is the stunnedCheck

    public float stunnedTime = 3f; // how long to wait at a waypoint

    public string stunnedLayer = "StunnedEnemy"; // name of the layer to put enemy on when stunned
    public string playerLayer = "Player"; // name of the player layer to ignore collisions with when stunned

    [HideInInspector] public bool isStunned; // flag for isStunned

    public GameObject[] myWaypoints; // to define the movement waypoints

    [Tooltip("Time in seconds to wait at each waypoint")]
    public float waitAtWaypointTime = 1f; // how long to wait at a waypoint

    public bool loopWaypoints = true; // should it loop through the waypoints

    // SFXs
    public AudioClip stunnedSfx;
    public AudioClip attackSfx;
    private Animator _animator;
    private AudioSource _audio;

    // store the layer number the enemy is on (setup in Awake)
    private int _enemyLayer;
    private float _moveTime;
    private bool _moving = true;

    // movement tracking
    private int _myWaypointIndex; // used as index for My_Waypoints
    private Rigidbody2D _rigidbody;

    // store the layer number the enemy should be moved to when stunned
    private int _stunnedLayer;

    // private variables below

    // store references to components on the gameObject
    private Transform _transform;
    private float _velocityX;

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

        if (stunnedCheck == null) Debug.LogError("stunnedCheck child gameobject needs to be setup on the enemy");

        // setup moving defaults
        _moveTime = 0f;
        _moving = true;

        // determine the enemies specified layer
        _enemyLayer = gameObject.layer;

        // determine the stunned enemy layer number
        _stunnedLayer = LayerMask.NameToLayer(stunnedLayer);

        // make sure collision are off between the playerLayer and the stunnedLayer
        // which is where the enemy is placed while stunned
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(playerLayer), _stunnedLayer, true);
    }

    // if not stunned then move the enemy when time is > _moveTime
    private void Update()
    {
        if (!isStunned)
        {
            if (Time.time >= _moveTime)
                EnemyMovement();
            else
                _animator.SetBool(Moving, false);
        }
    }

    // if the Enemy collides with a MovingPlatform, then make it a child of that platform
    // so it will go for a ride on the MovingPlatform
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform")) transform.parent = other.transform;
    }

    // if the enemy exits a collision with a moving platform, then unchild it
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("MovingPlatform")) transform.parent = null;
    }

    // Attack player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isStunned)
        {
            CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
            if (player.playerCanMove)
            {
                // Make sure the enemy is facing the player on attack
                Flip(collision.transform.position.x - _transform.position.x);

                // attack sound
                PlaySound(attackSfx);

                // stop moving
                _rigidbody.velocity = new Vector2(0, 0);

                // apply damage to the player
                player.ApplyDamage(damageAmount);

                // stop to enjoy killing the player
                _moveTime = Time.time + stunnedTime;
            }
        }
    }

    // Move the enemy through its rigidbody based on its waypoints
    private void EnemyMovement()
    {
        // if there isn't anything in My_Waypoints
        if (myWaypoints.Length != 0 && _moving)
        {
            // make sure the enemy is facing the waypoint (based on previous movement)
            Flip(_velocityX);

            // determine distance between waypoint and enemy
            _velocityX = myWaypoints[_myWaypointIndex].transform.position.x - _transform.position.x;

            // if the enemy is close enough to waypoint, make it's new target the next waypoint
            if (Mathf.Abs(_velocityX) <= 0.05f)
            {
                // At waypoint so stop moving
                _rigidbody.velocity = new Vector2(0, 0);

                // increment to next index in array
                _myWaypointIndex++;

                // reset waypoint back to 0 for looping
                if (_myWaypointIndex >= myWaypoints.Length)
                {
                    if (loopWaypoints)
                        _myWaypointIndex = 0;
                    else
                        _moving = false;
                }

                // setup wait time at current waypoint
                _moveTime = Time.time + waitAtWaypointTime;
            }
            else
            {
                // enemy is moving
                _animator.SetBool(Moving, true);

                // Set the enemy's velocity to moveSpeed in the x direction.
                _rigidbody.velocity = new Vector2(_transform.localScale.x * moveSpeed, _rigidbody.velocity.y);
            }
        }
    }

    // flip the enemy to face torward the direction he is moving in
    private void Flip(float vx)
    {
        // get the current scale
        Vector3 localScale = _transform.localScale;

        if (vx > 0f && localScale.x < 0f)
            localScale.x *= -1;
        else if (vx < 0f && localScale.x > 0f)
            localScale.x *= -1;

        // update the scale
        _transform.localScale = localScale;
    }

    // play sound through the audiosource on the gameobject
    private void PlaySound(AudioClip clip)
    {
        _audio.PlayOneShot(clip);
    }

    // setup the enemy to be stunned
    public void Stunned()
    {
        if (!isStunned)
        {
            isStunned = true;

            // provide the player with feedback that enemy is stunned
            PlaySound(stunnedSfx);
            _animator.SetTrigger(Stunned1);

            // stop moving
            _rigidbody.velocity = new Vector2(0, 0);

            // switch layer to stunned layer so no collisions with the player while stunned
            gameObject.layer = _stunnedLayer;
            stunnedCheck.layer = _stunnedLayer;

            // start coroutine to stand up eventually
            StartCoroutine(Stand());
        }
    }

    // coroutine to unstun the enemy and stand back up
    private IEnumerator Stand()
    {
        yield return new WaitForSeconds(stunnedTime);

        // no longer stunned
        isStunned = false;

        // switch layer back to regular layer for regular collisions with the player
        gameObject.layer = _enemyLayer;
        stunnedCheck.layer = _enemyLayer;

        // provide the player with feedback
        _animator.SetTrigger(Stand1);
    }
}