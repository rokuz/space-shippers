using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrystalFactory : MonoBehaviour
{
  public Crystal crystalType = Crystal.Red;
  public float productionSpeed = 1.0f;
  public uint amount = 0;
  private uint initialAmount = 0;

  private bool initialAmountChanged = true;
  private float producingAmount = 0.0f;
  private CrystalStock stock;

  public delegate void OnAmountChangedEvent(Crystal crystal, uint amount);
  public OnAmountChangedEvent OnAmountChanged;

  public delegate void OnProgressEvent(Crystal crystal, float progress);
  public OnProgressEvent OnProgress;

  public delegate void OnWaitingEvent(Crystal crystal, bool isWaiting);
  public OnWaitingEvent OnWaiting;

  private class CompoundCrystalTask
  {
   public Crystal crystalType = Crystal.Yellow;
   public uint amount = 0;
  }
  private List<CompoundCrystalTask> tasks = new List<CompoundCrystalTask>();
  private CompoundCrystalTask currentTask = null;

  public CrystalStock Stock
  {
    set { stock = value; }
  }

  public void Start()
  {
    initialAmount = amount;
  }

	public void Update()
  {
    if (initialAmountChanged)
    {
      if (OnProgress != null)
      {
          OnProgress(crystalType, 0.0f);
          OnProgress(Crystal.Yellow, 0.0f);
          OnProgress(Crystal.Cian, 0.0f);
          OnProgress(Crystal.Purple, 0.0f);
      }
      if (OnWaiting != null)
      {
          OnWaiting(crystalType, false);
          OnWaiting(Crystal.Yellow, false);
          OnWaiting(Crystal.Cian, false);
          OnWaiting(Crystal.Purple, false);
      }
      if (OnAmountChanged != null)
        OnAmountChanged(crystalType, amount);
      initialAmountChanged = false;
    }
	}

  public void Reset()
  {
    initialAmountChanged = true;
    tasks.Clear();
    currentTask = null;
    amount = initialAmount;
    producingAmount = 0.0f;
  }

  public void OnGameTick(float dt)
  {
      producingAmount += productionSpeed * dt;

      if (OnWaiting != null)
      {
          foreach(CompoundCrystalTask task in tasks)
              OnWaiting(task.crystalType, true);  
      }

      // Task executing.
      if (currentTask != null)
      {
          if (OnWaiting != null)
          {
              OnWaiting(crystalType, true);
              OnWaiting(currentTask.crystalType, false);
          }

          uint compoundAmount = 0;
          while (producingAmount >= 1.0f)
          {
              producingAmount -= 1.0f;
              compoundAmount++;
          }
          if (OnProgress != null)
              OnProgress(currentTask.crystalType, producingAmount);

          if (compoundAmount > 0)
          {
              uint newAmount = System.Math.Min(currentTask.amount, compoundAmount);
              amount -= newAmount;
              currentTask.amount -= newAmount;
              stock.AddToStock(currentTask.crystalType, newAmount);
              if (OnAmountChanged != null)
                OnAmountChanged(crystalType, amount);
          }

          if (currentTask == null || currentTask.amount == 0)
          {
              currentTask = null;
              producingAmount = 0.0f;
              if (tasks.Count > 0 && amount >= tasks[0].amount)
              {
                  currentTask = tasks[0];
                  tasks.RemoveAt(0);
              }
          }
          return;
      }

      if (OnWaiting != null)
          OnWaiting(crystalType, false);

      bool produced = false;
      while (producingAmount >= 1.0f)
      {
          producingAmount -= 1.0f;
          amount++;
          produced = true;
      }

      if (OnProgress != null)
          OnProgress(crystalType, producingAmount);

      if (produced && OnAmountChanged != null)
          OnAmountChanged(crystalType, amount);

      if (produced && tasks.Count > 0)
      {
          if (amount >= tasks[0].amount)
          {
              currentTask = tasks[0];
              tasks.RemoveAt(0);
              producingAmount = 0.0f;
          }
      }
  }

  public CrystalPack TakeCrystals(uint crystalAmount)
  {
    if (currentTask != null)
        return new CrystalPack();

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
        CompoundCrystalTask task = new CompoundCrystalTask();
        task.crystalType = GetCompoundCrystalType(pack.type, crystalType);
        task.amount = pack.amount;
        tasks.Add(task);
    }
    else
    {
        throw new UnityException("Incorrect game logic");
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
