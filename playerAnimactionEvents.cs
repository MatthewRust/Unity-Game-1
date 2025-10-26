using UnityEngine;

public class playerAnimactionEvents : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void disableJumpAndMovement()
    {
        player.enableJumpAndMovement(false);
    }

    private void enableJumpAndMovement()
    {
        player.enableJumpAndMovement(true);
    }

    private void enableJump()
    {
        player.enableJump(true);
    }

    private void disableJump()
    {
        player.enableJump(false);
    }

    private void damageEnemies()
    {
        player.damageEnemies();
    }

}
