using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Dodge Settings")]
    public Transform opponent;
    public float dodgeAngle = 45f;        // degrees to strafe per dodge
    public float dodgeDuration = 0.3f;    // seconds to complete the arc

    private bool isDodging = false;

    NetworkSpellCaster spellCaster;

    void Start()
    {
        spellCaster = GetComponent<NetworkSpellCaster>();
    }

    // void Update()
    // {
    //     if (!IsOwner) return;

    //     if (Input.GetKeyDown(KeyCode.LeftArrow) && !isDodging)
    //         StartCoroutine(DodgeArc(+dodgeAngle)); // left
    //     if (Input.GetKeyDown(KeyCode.RightArrow) && !isDodging)
    //         StartCoroutine(DodgeArc(-dodgeAngle)); // right
    // }
    

    public void Dodge(float? totalAngle = null)
    {
        if (totalAngle == null)
        {
            totalAngle = dodgeAngle;
        }
        StartCoroutine(DodgeArc(totalAngle.Value));
    }

    System.Collections.IEnumerator DodgeArc(float totalAngle)
    {
        isDodging = true;

        float elapsed = 0f;
        float angleRotated = 0f;

        //spellCaster.characterAnimator.SetTrigger("DodgeLeft1");

        while (elapsed < dodgeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dodgeDuration;

            // How many degrees to apply THIS frame
            float targetAngle = Mathf.Lerp(0f, totalAngle, t);
            float deltaAngle = targetAngle - angleRotated;
            angleRotated = targetAngle;

            // Rotate the radius arm around opponent
            Vector3 arm = transform.position - opponent.position;
            arm = Quaternion.AngleAxis(deltaAngle, Vector3.up) * arm;

            // Move player to new position on the circle
            transform.position = opponent.position + arm;

            // Always face opponent
            //FaceOpponent();

            yield return null;
        }

        // Enforce exact combat distance after every dodge
        Vector3 toPlayer = (transform.position - opponent.position).normalized;
        transform.position = opponent.position + toPlayer * Constants.COMBAT_RADIUS;

        // Snap clean at the end
        //FaceOpponent();
        isDodging = false;
    }

    // public void FaceOpponent()
    // {
    //     Vector3 dir = opponent.position - transform.position;
    //     dir.y = 0f;
    //     if (dir != Vector3.zero)
    //         transform.rotation = Quaternion.LookRotation(dir);
    // }
}
// ```

// **Why this works:**
// ```
//          Opponent (center)
//               O
//              /|
//         arm / |
//            /  |
//     You --*   *-- You after dodge left
    
//     |arm| stays constant = distance preserved ✓