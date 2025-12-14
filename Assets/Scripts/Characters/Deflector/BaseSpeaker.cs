public class BaseSpeaker : BaseCharacter
{
    public HealthComponent healthComponent;
    public DeflectManager deflectManager;
    public override void DeactivatePlayer()
    {
        base.DeactivatePlayer();
        deflectManager.gameObject.SetActive(false);
    }
    public override void ActivatePlayer()
    {
        base.ActivatePlayer();
        deflectManager.gameObject.SetActive(true);
    }
}
