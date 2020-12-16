using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Controls;

public class Player : MonoBehaviour
{
	[SerializeField]
	private float moveSpeed;

	[SerializeField]
	private float jumpForce;

	private Rigidbody2D playerRigidbody2D; 

	private DefControls inputActions;

	private Vector2 moveDirection;

	private bool isGrounded;

	private void Awake()
	{
		this.inputActions = new DefControls();
		this.inputActions.Player.Move.performed += context =>
		{
			var control = context.control;
			var value = context.ReadValue<float>();

			var button = control as ButtonControl;
			if(button != null && button.wasPressedThisFrame)
			{
				this.moveDirection.x = value;
			}
			else
			{
				this.moveDirection.x = 0;
			}
		};
		this.inputActions.Player.Jump.performed += context => this.Jump();
		
		this.playerRigidbody2D = this.GetComponent<Rigidbody2D>();
	}

	private void OnEnable()
	{
		this.inputActions.Player.Enable();
	}

	private void FixedUpdate()
	{
		this.Move(this.moveDirection);
		this.moveDirection.x = 0;
	}

	private void OnDisable()
	{
		this.inputActions.Player.Disable();
	}

	private void Move(Vector2 direction)
	{
		this.playerRigidbody2D.AddForce(direction * this.moveSpeed, ForceMode2D.Impulse);
	}

	private void Jump()
	{
		if(this.isGrounded)
		{
			this.isGrounded = false;
			this.playerRigidbody2D.AddForce(this.transform.up * jumpForce, ForceMode2D.Impulse);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.layer == 9)
		{
			isGrounded = true;
		}
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		var control = context.control;
		var value = context.ReadValue<float>();

		var button = control as ButtonControl;
		if(button != null && button.wasPressedThisFrame)
		{
			this.moveDirection.x = value;
		}
		else
		{
			this.moveDirection.x = 0;
		}
	}
}
