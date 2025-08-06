using NUnit.Framework;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private bool isGrounded;


    public float IsRunning = 0f;
    public float Stamina = 100f;
    public float StaminaDrainRate = 1f;
    public float StaminaRegenRate = 2f;



    public Transform CameraTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
    }

    void Update()
    {
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = (CameraTransform.forward * Vertical + CameraTransform.right * Horizontal).normalized;
        direction.y = 0;

        IsRunning = (Mathf.Abs(Horizontal) > 0 || Mathf.Abs(Vertical) > 0) ? 1f : 0f;

        Vector3 newVelocity = direction * moveSpeed;
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;


//Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }



//Stamina and Regeneration
        if (Input.GetKey(KeyCode.LeftShift) && Stamina > 0 && IsRunning > 0f)
        {
            moveSpeed = 10f;
            Stamina -= StaminaDrainRate * Time.deltaTime;
            if (Stamina < 0f)
            {
                Stamina = 0f;
            }
        }
        else
        {
            moveSpeed = 5f;
            Stamina += StaminaRegenRate * Time.deltaTime;
            if (Stamina > 100f)
            {
                Stamina = 100f;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("Player is grounded");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            Debug.Log("Player is not grounded");
        }
    }
}