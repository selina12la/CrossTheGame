using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class FigureControl : MonoBehaviour
{
    [SerializeField] float stepSize = 1f;        
    [SerializeField] float moveDuration = 0.14f;    
    [SerializeField] float jumpHeight = 0.45f;     
    private FigureInputAction inputAction;
    private InputAction moveAction;
    private bool isMoving = false;

    private void Awake()
    {
        inputAction = new FigureInputAction();
        inputAction.Enable();
        moveAction = inputAction.Figure.Move;
    }

    private void Update()
    {
        if (!isMoving)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();

            if (input != Vector2.zero)
            {
                Vector3 dir = Vector3.zero;

                if (input.y > 0) dir = Vector3.forward;
                else if (input.y < 0) dir = Vector3.back;
                else if (input.x < 0) dir = Vector3.left;
                else if (input.x > 0) dir = Vector3.right;

                StartCoroutine(Move(dir));
            }
        }
    }

    private IEnumerator Move(Vector3 direction)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = start + direction * stepSize;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = Vector3.Lerp(start, end, t) + Vector3.up * height;

            yield return null;
        }

        transform.position = end;
        isMoving = false;
    }
}