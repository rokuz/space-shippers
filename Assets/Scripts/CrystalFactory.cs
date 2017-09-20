using UnityEngine;
using System.Collections;

public class CrystalFactory : MonoBehaviour
{
    public Crystal crystalType = Crystal.Red;
    public float productionSpeed = 1.0f;
    public uint amount = 0;

    private bool initialAmountChanged = true;
    private float producingAmount = 0.0f;
    private CrystalStock stock;

    public delegate void OnAmountChangedEvent(Crystal crystal, uint amount);
    public OnAmountChangedEvent OnAmountChanged;

    public CrystalStock Stock
    {
        set { stock = value; }
    }

	public void Update()
    {
        if (initialAmountChanged)
        {
            if (OnAmountChanged != null)
                OnAmountChanged(crystalType, amount);
            initialAmountChanged = false;
        }
	}

    public void OnGameTick(float dt)
    {
        producingAmount += productionSpeed * dt;

        bool produced = false;
        while (producingAmount >= 1.0f)
        {
            producingAmount -= 1.0f;
            amount++;
            produced = true;
        }

        if (produced && OnAmountChanged != null)
            OnAmountChanged(crystalType, amount);
    }

    public CrystalPack TakeCrystals(uint crystalAmount)
    {
        CrystalPack pack = new CrystalPack();
        pack.type = crystalType;
        pack.amount = System.Math.Min(crystalAmount, amount);

        if (pack.amount == 0)
            return pack;

        amount -= pack.amount;
        if (OnAmountChanged != null)
            OnAmountChanged(crystalType, amount);

        return pack;
    }

    public void ApplyCargo(CrystalPack pack)
    {
        if (pack.type != crystalType)
        {
            Crystal type = GetCompoundCrystalType(pack.type, crystalType);
            uint newAmount = System.Math.Min(pack.amount, amount);
            amount -= newAmount;
            stock.AddToStock(type, newAmount);
            if (OnAmountChanged != null)
                OnAmountChanged(crystalType, amount);
        }
        else
        {
            amount += pack.amount;
            if (OnAmountChanged != null)
                OnAmountChanged(crystalType, amount);
        }
    }

    public Crystal GetCompoundCrystalType(Crystal crystal1, Crystal crystal2)
    {
        switch (crystal1)
        {
            case Crystal.Red:
                if (crystal2 == Crystal.Red)
                    return Crystal.Red;
                if (crystal2 == Crystal.Green)
                    return Crystal.Yellow;    
                if (crystal2 == Crystal.Blue)
                    return Crystal.Purple;
                break;

            case Crystal.Green:
                if (crystal2 == Crystal.Red)
                    return Crystal.Yellow;
                if (crystal2 == Crystal.Green)
                    return Crystal.Green;    
                if (crystal2 == Crystal.Blue)
                    return Crystal.Cian;  
                break;

            case Crystal.Blue:
                if (crystal2 == Crystal.Red)
                    return Crystal.Purple;
                if (crystal2 == Crystal.Green)
                    return Crystal.Cian;    
                if (crystal2 == Crystal.Blue)
                    return Crystal.Blue; 
                break;
        }

        throw new UnityException("Incorrect game logic");
    }
}
