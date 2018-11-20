using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{

  // Config params
  [SerializeField] float runSpeed = 5f;
  [SerializeField] float jumpSpeed = 5f;
  [SerializeField] float climbSpeed = 5f;
  [SerializeField] Vector2 deathKick = new Vector2(25f, 25f);

  // State
  bool isAlive = true;
  bool isGrounded = true;

  // Cached references
  Rigidbody2D myRigidBody;
  Animator myAnimator;
  CapsuleCollider2D myBodyCollider;
  BoxCollider2D myFeetCollider;
  SpriteRenderer mySpriteRenderer;
  float gravityScale;
  

  // Use this for initialization
  void Start()
  {
    myRigidBody = GetComponent<Rigidbody2D>();
    myAnimator = GetComponent<Animator>();
    myBodyCollider = GetComponent<CapsuleCollider2D>();
    myFeetCollider = GetComponent<BoxCollider2D>();
    gravityScale = myRigidBody.gravityScale;
  }

  // Update is called once per frame
  void Update()
  {
    if (!isAlive)
    {
      return;
    }
    else
    {
      Run();
      ClimbLadder();
      Jump();
      Die();
      FlipSprite();
    }
  }

    private void Run()
      {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); // value is between -1 to +1
        Vector2 playerVelocity = new Vector2((controlThrow * runSpeed), myRigidBody.velocity.y);
        myRigidBody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("Running", playerHasHorizontalSpeed);
      }

  private void ClimbLadder()
  {
    if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
    {
      myAnimator.SetBool("Climbing", false);
      myRigidBody.gravityScale = gravityScale;
      return;
    }

    float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
    Vector2 climbVelocity = new Vector2(myRigidBody.velocity.x, (controlThrow * climbSpeed));
    myRigidBody.velocity = climbVelocity;
    myRigidBody.gravityScale = 0f;

    bool playerHasVerticalSpeed = Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon;
    myAnimator.SetBool("Climbing", playerHasVerticalSpeed);
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.gameObject.tag == "Ground")
    {
      isGrounded = true;
      myAnimator.SetBool("Jumping", false);
    }
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.gameObject.tag == "Climbing")
    {
      myAnimator.SetBool("Jumping", false);
    }
  }

  private void Jump()
  {
    if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
    {
      return;
    }

    if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
    {
      return;
    }

    if (CrossPlatformInputManager.GetButtonDown("Jump") && isGrounded)
    {
      Vector2 jumpVelocity = new Vector2(0f, jumpSpeed);
      myRigidBody.velocity += jumpVelocity;
      isGrounded = false;
      myAnimator.SetBool("Jumping", true);
    }
  }

  private void Die()
  {
    if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy", "Obstacles")))
    {
      isAlive = false;
      myAnimator.SetTrigger("Dying");
      GetComponent<Rigidbody2D>().velocity = deathKick;
      //FindObjectOfType<GameSession>().ProcessPlayerDeath();
    }
  }

  private void FlipSprite()
  {
    bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
    if (playerHasHorizontalSpeed)
    {
      transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
    }
  }
}
