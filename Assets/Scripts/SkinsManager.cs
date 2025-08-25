using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsManager : MonoBehaviour
{

    public static SkinsManager INSTANCE { get; private protected set; }

    [SerializeField] Material PlayerMaterial;
    [SerializeField] string selectedSkinId;
    [SerializeField] List<Skin> Skins = new List<Skin>();

    private void Awake()
    {
        if (INSTANCE != null)
            Destroy(gameObject);
        else
        {
            INSTANCE = this;
            DontDestroyOnLoad(gameObject);
        }
    }


}

[Serializable]
public enum Payment
{
    IAP,
    Cheese,
    Gems,
    Free,
}

[Serializable]
public class Skin
{
    public string id;
    public string name;
    public bool isPurchased;
    public Texture texture;
    public Payment payment;
    public int price;
}
