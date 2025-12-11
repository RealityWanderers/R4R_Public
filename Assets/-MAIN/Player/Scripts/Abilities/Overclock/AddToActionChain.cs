using UnityEngine;

public class AddToActionChain : MonoBehaviour
{
    [Header("Refs")]
    private PlayerAbilityManager pM;
    private PlayerActionChainData actionChainData; 

    void Awake()
    {
        pM = PlayerAbilityManager.Instance;
    }

    private void Start()
    {
        actionChainData = pM.GetAbility<PlayerActionChainData>();
    }

    public void DoAddToActionChain()
    {
        actionChainData.AddToChain(); 
    }
}
