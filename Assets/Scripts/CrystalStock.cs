using UnityEngine;
using System.Collections;

public class CrystalStock
{
  private CrystalPack[] stock;

  public delegate void OnStockAmountChangedEvent(Crystal crystal, uint amount);
  public OnStockAmountChangedEvent OnStockAmountChanged;

  public CrystalStock()
  {
    stock = new CrystalPack[3];

    stock[0] = new CrystalPack();
    stock[0].type = Crystal.Yellow;
    stock[0].amount = 0;

    stock[1] = new CrystalPack();
    stock[1].type = Crystal.Purple;
    stock[1].amount = 0;

    stock[2] = new CrystalPack();
    stock[2].type = Crystal.Cian;
    stock[2].amount = 0;
  }

  public void AddToStock(Crystal crystal, uint amount)
  {
    for (int i = 0; i < stock.Length; i++)
    {
        if (stock[i].type == crystal)
        {
            stock[i].amount += amount;
            if (OnStockAmountChanged != null)
                OnStockAmountChanged(stock[i].type, stock[i].amount);
            break;
        }
    }
  }
}
