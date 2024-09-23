using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class knight : MonoBehaviour
{
    //---------------------------------Componentes necessários
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer sr;
    public LayerMask plataform;
    public GameObject text;
    public TextMeshProUGUI textEdit;
    public GameObject jump, arrow, fireball, dash, spawn1, spawn2, spawn3;
    private Vector3 position, spawn;
    float direction = 0f;

    //---------------------------------Estados
    enum State { idle, walking, running, dashing, rolling, jumping, falling, attacking, special, resting }

    State state = State.idle;

    //---------------------------------Comandos
    float moveInput = 0f;
    float auxiliarInput = 0f;

    bool dashRollInput = false;
    bool jumpInput = false;
    bool attackingInput = false;
    bool specialInput = false;
    bool restingInput = false;

    //---------------------------------Informacoes especiais
    bool onFloor = false;

    //---------------------------------Outros
    [SerializeField] float raycastDistance = 0.1f;
    [SerializeField] Transform feet;
    Ray raio;
    bool attacking = false;
    bool specialing = false;

    [Header("Status Knight - Walk/Run")]
    [SerializeField] float runVelocity = 14.0f;
    [SerializeField] float smoothTime = 0.5f;
    [SerializeField] float velocity = 9.0f;
    float actualVelocity;
    float walkVelocity;

    [Header("Status Knight - Dash")]
    [SerializeField] float forceDash = 10000f;
    [SerializeField] float dashCooldown = 1.0f;
    [SerializeField] bool canDash = true;
    bool isDashing = false;

    [Header("Status Knight - Jump")]
    [SerializeField] float forceJump = 50f;
    [SerializeField] bool canJump;
    [SerializeField] float yFactor = 0.85f;

    [Header("Status Knight - Roll")]
    [SerializeField] float forceRoll = 50;
    [SerializeField] bool rolling = false;
    [SerializeField] bool canRoll = true;

    //---------------------------------Pegando informacoes iniciais
    void Awake()
    {
        
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        walkVelocity = velocity;
        actualVelocity = walkVelocity;
    }

    //---------------------------------Detectando botoes
    void Update()
    {
        
        onFloor = Physics2D.Raycast(feet.transform.position, Vector2.down, raycastDistance, plataform);
        //Debug.Log(onFloor);
        moveInput = Input.GetAxisRaw("Horizontal");
        auxiliarInput = Input.GetAxisRaw("Vertical");
        dashRollInput = Input.GetKey(KeyCode.LeftShift);
        attackingInput = Input.GetKey(KeyCode.Z);
        specialInput = Input.GetKey(KeyCode.X);
        restingInput = Input.GetKey(KeyCode.C);

        //-------------------------------------- Debugger's
        //-------------------------------------- Floor
        /*Debug.DrawRay(feet.transform.position, Vector2.down * raycastDistance, Color.red);
        if(onFloor ) 
        {
            Debug.Log("floor");
        }*/

        //-------------------------------------- Outra coisa
    }

    private void FixedUpdate()
    {
        if (moveInput > 0f)
        {
            sr.flipX = false;

        }
        else if (moveInput < 0f)
        {
            sr.flipX = true;
        }

        switch (state)
        {
            case State.idle:
                StateIdle();
                break;

            case State.walking:
                StateWalking();
                break;

            case State.running:
                StateRunning();
                break;

            case State.dashing:
                StateDashing();
                break;

            case State.rolling:
                StateRolling();
                break;

            case State.jumping:
                StateJumping();
                break;

            case State.falling:
                StateFalling();
                break;

            case State.attacking:
                StateAttacking();
                break;

            case State.special:
                StateSpecial();
                break;

            case State.resting:
                StateResting();
                break;
        }

        Debug.Log(state);
    }


    //---------------------------------Movimentação básica
    void StateIdle()
    {
        textEdit.text = "Idle";

        // acao
        animator.Play("Idle - Knight");

        //transicoes
        if (onFloor)
        {
            if (restingInput)
            {
                state = State.resting;
            }
            else if (specialInput)
            {
                state = State.special;
            }
            else if (attackingInput)
            {
                state = State.attacking;
            }
            else if (moveInput != 0)
            {
                state = State.walking;
            }
        }
        else
        {
            state = State.falling;
        }

    }

    void StateWalking()
    {
        textEdit.text = "Walking";

        // Animations
        if (rb.velocity.x < runVelocity)
        {
            animator.Play("Walking - Knight");
        }

        // Actions

        velocity = Mathf.SmoothDamp(velocity, runVelocity, ref actualVelocity, smoothTime);
        if (velocity >= 0.85f * runVelocity)
        {
            velocity = runVelocity;
        }
        rb.velocity = new Vector3(velocity * moveInput, rb.velocity.y, 0.0f);

        //Debug.Log(rb.velocity);

        // Transitions

        if (onFloor)
        {
            if (attackingInput)
            {
                velocity = walkVelocity;
                state = State.attacking;
            }
            else if (specialInput)
            {
                velocity = walkVelocity;
                state = State.special;
            }
            else if (rb.velocity.x >= runVelocity || rb.velocity.x <= -runVelocity)
            {
                state = State.running;
            }
            else if (dashRollInput)
            {
                velocity = walkVelocity;
                state = State.rolling;
            }
            else if (moveInput == 0)
            {
                velocity = walkVelocity;
                //Debug.Log(walkVelocity);
                //Debug.Log("aqui");
                state = State.idle;
            }
            else if (auxiliarInput > 0)
            {
                velocity = walkVelocity;
                state = State.jumping;
            }
        }
        else
        {
            velocity = walkVelocity;
            state = State.falling;
        }
    }

    void StateRunning()
    {
        textEdit.text = "Running";

        //Animations

        animator.Play("Running - Knight");

        //Actions

        rb.velocity = new Vector3(velocity * moveInput, rb.velocity.y, 0.0f);

        //Transitions

        if (onFloor)
        {
            if (attackingInput)
            {
                velocity = walkVelocity;
                state = State.attacking;
            }
            else if (specialInput)
            {
                velocity = walkVelocity;
                state = State.special;
            }
            else if (dashRollInput)
            {
                velocity = walkVelocity;
                state = State.dashing;
            }
            else if (moveInput == 0)
            {
                velocity = walkVelocity;
                state = State.idle;
            }
            else if (auxiliarInput > 0)
            {
                //velocity = walkVelocity;
                state = State.jumping;
            }
        }
        else
        {
            velocity = walkVelocity;
            state = State.falling;
        }
    }

    void StateDashing()
    {
        textEdit.text = "Dashing";
        if (isDashing == false && canDash == true)
        {
            isDashing = true;
            canDash = false;

            rb.AddForce(new Vector3(velocity * forceDash * moveInput, 0.0f, 0.0f));

            animator.Play("Dashing - Knight");

            Invoke("EndDash", 0.567f);

        }

        //States
        if (isDashing == false)
        {
            if (onFloor)
            {
                if (moveInput == 0)
                {
                    rb.velocity = new Vector3(0, 0, 0);
                    canDash = true;
                    state = State.idle;
                }
                else if (moveInput != 0)
                {
                    canDash = true;
                    velocity = runVelocity;
                    state = State.running;
                }
            }
            else
            {
                canDash = true;
                state = State.falling;
            }
        }
    }

    void EndDash()
    {
        isDashing = false;
    }

    void StateRolling()
    {
        textEdit.text = "Rolling";

        if (rolling == false && canRoll == true)
        {
            rolling = true;
            canRoll = false;
            rb.AddForce(new Vector3(velocity * forceRoll * moveInput, 0.0f, 0.0f));
            animator.Play("Roll - Knight");
            Invoke("EndRoll", 0.683f);
        }

        if (rolling == false)
        {
            if (onFloor)
            {
                if (moveInput == 0)
                {
                    rb.velocity = new Vector3(0, 0, 0);
                    canRoll = true;
                    state = State.idle;
                }
                else if (moveInput != 0)
                {
                    canRoll = true;
                    state = State.walking;
                }
            }
            else
            {
                canRoll = true;
                state = State.falling;
            }
        }
    }

    private void EndRoll()
    {
        rolling = false;
    }

    void StateJumping()
    {
        textEdit.text = "Jumping";
        //Animation
        animator.Play("Jump A - Knight");

        //Action
        if (onFloor)
        {
            rb.AddForce(new Vector3(0.0f, velocity * forceJump, 0.0f));
        }

        //State
        if (rb.velocity.y < 0)
        {
            velocity = walkVelocity;
            state = State.falling;
        }
    }

    void StateFalling()
    {
        textEdit.text = "Falling";
        //Animation
        if (rb.velocity.y < 0)
        {
            animator.Play("Jump B - Knight");
        }
        else if (rb.velocity.y == 0)
        {
            rb.velocity = new Vector3(0, 0, 0);
            animator.Play("Jump C - Knight");
            Invoke("ChangeState", 0.35f);
        }
    }

    void ChangeState()
    {
        if (rb.velocity.y == 0 && moveInput == 0)
        {
            state = State.idle;
        }
        else if (rb.velocity.y == 0 && moveInput != 0)
        {
            state = State.walking;
        }
    }

    void StateAttacking()
    {
        
        if (moveInput == 0 && auxiliarInput == 0 && attacking == false)
        {
            textEdit.text = "Attacking - attack 1";
            animator.Play("Attack A");
            attacking = true;
            Invoke("EndAttack", 0.2f);
        }
        else if (moveInput != 0 && auxiliarInput == 0 && attacking == false)
        {
            textEdit.text = "Attacking - attack 2";
            animator.Play("Attack C");
            attacking = true;
            Invoke("EndAttack", 0.683f);
        }
        else if (auxiliarInput == 1 && attacking == false)
        {
            textEdit.text = "Attacking - attack 3";
            animator.Play("Attack D");
            attacking = true;
            Invoke("EndAttack", 0.617f);
        }
        else if (auxiliarInput == -1 && attacking == false)
        {
            textEdit.text = "Attacking - attack 4";
            animator.Play("Attack E");
            attacking = true;
            Invoke("EndAttack", 1.083f);
        }
    }

    void EndAttack()
    {
        if (onFloor)
        {
            if (moveInput == 0)
            {
                state = State.idle;
                attacking = false;
            }
            else if (moveInput != 0)
            {
                state = State.walking;
                attacking = false;
            }
        }
        else
        {
            state = State.falling;
            attacking = false;
        }
    }

    void StateSpecial()
    {
        if (moveInput == 0 && auxiliarInput == 0 && specialing == false)
        {
            textEdit.text = "Special - Kick";
            animator.Play("Kick");
            specialing = true;
            Invoke("EndSpecial", 0.417f);
            Debug.Log("1");
        }
        else if (moveInput != 0 && auxiliarInput == 0 && specialing == false)
        {
            textEdit.text = "Special - Arrow";
            Debug.Log("2");
            animator.Play("Shoot Arrow");
            specialing = true;
            Invoke("EndSpecial", 0.517f);
        }
        else if (auxiliarInput == 1 && specialing == false)
        {
            textEdit.text = "Special - Fireball";
            Debug.Log("3");
            animator.Play("Cast Fireball");
            specialing = true;
            Invoke("EndSpecial", 0.483f);
        }
    }

    void EndSpecial()
    {
        if (onFloor)
        {
            if (moveInput == 0)
            {
                state = State.idle;
                specialing = false;
            }
            else if (moveInput != 0)
            {
                state = State.walking;
                specialing = false;
            }
        }
        else
        {
            state = State.falling;
            specialing = false;
        }
    }

    void StateResting()
    {
        //Animation
        if (Input.GetKey(KeyCode.DownArrow))
        {
            textEdit.text = "Resting - Lay";
            animator.Play("Lay - Knight");
            Invoke("EndRest", 0.6f);
        }
        else
        {
            textEdit.text = "Resting - Dance";
            animator.Play("Dance  - Knight");
            Invoke("EndRest", 0.73f);
        }
        //Action
    }

    void EndRest()
    {
        //Transitions
        if (onFloor)
        {
            state = State.idle;
        }
    }
}
//Dar olhada no código, se puder resolver o bug do idle dps da dança