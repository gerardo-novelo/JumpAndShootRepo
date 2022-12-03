using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]
    byte HP { get; set; }

    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool isDead { get; set; }

    bool isInitialized = false;

    const byte StartingHP = 5;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    public MeshRenderer bodyMeshRenderer;
    Color defaultMeshBodyColor;

    public GameObject playerModel;
    public GameObject deathGameObjectPrefab;

    public bool skipSettingStartValues = false;

    //Other components
    HitboxRoot hitboxRoot;
    CharacterMovementHandler characterMovementHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    private void Awake()
    {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!skipSettingStartValues)
        {
            HP = StartingHP;
            isDead = false;
        }

        defaultMeshBodyColor = bodyMeshRenderer.material.color;

        isInitialized = true;
    }

    IEnumerator OnHitCO()
    {
        bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
            uiOnHitImage.color = uiOnHitColor;

        yield return new WaitForSeconds(0.2f);

        bodyMeshRenderer.material.color = defaultMeshBodyColor;

        if (Object.HasInputAuthority && !isDead)
            uiOnHitImage.color = new Color(0, 0, 0, 0);
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2.0f);

        characterMovementHandler.RequestRespawn();
    }

    //Function only called on the server
    public void OnTakenDamage(string damageCauseByPlayerNickname)
    {
        //Only take damage while alive
        if (isDead)
            return;

        HP -= 1;

        Debug.Log($"{Time.time} {transform.name} took damage got {HP} left ");

        //Player died
        if (HP <= 0)
        {
            networkInGameMessages.SendInGameRPCMessage(damageCauseByPlayerNickname, $"Killed <b>{networkPlayer.nickName.ToString()}</b>");
            networkInGameMessages.SendInGameRPCKillMessage(damageCauseByPlayerNickname);

            Debug.Log($"{Time.time} {transform.name} died");

            StartCoroutine(ServerReviveCO());

            isDead = true;
        }
    }

    static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");

        byte newHP = changed.Behaviour.HP;

        //Load the old value
        changed.LoadOld();

        byte oldHP = changed.Behaviour.HP;

        //Check if the HP has been decreased
        if (newHP < oldHP)
            changed.Behaviour.OnHPReduced();
    }

    private void OnHPReduced()
    {
        if (!isInitialized)
            return;

        StartCoroutine(OnHitCO());
    }

    static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged isDead {changed.Behaviour.isDead}");

        bool isDeadCurrent = changed.Behaviour.isDead;

        //Load the old value
        changed.LoadOld();

        bool isDeadOld = changed.Behaviour.isDead;

        //Handle on death for the player. Also check if the player was dead but is now alive in that case revive the player.
        if (isDeadCurrent)
            changed.Behaviour.OnDeath();
        else if (!isDeadCurrent && isDeadOld)
            changed.Behaviour.OnRevive();
    }

    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");

        playerModel.gameObject.SetActive(false);
        hitboxRoot.HitboxRootActive = false;
        characterMovementHandler.SetCharacterControllerEnabled(false);

        Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        Debug.Log($"{Time.time} OnRevive");

        if (Object.HasInputAuthority)
            uiOnHitImage.color = new Color(0, 0, 0, 0);

        playerModel.gameObject.SetActive(true);
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        //Reset variables
        HP = StartingHP;
        isDead = false;
    }
}
