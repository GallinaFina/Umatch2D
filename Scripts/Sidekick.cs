using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Player;

public class Sidekick : BaseUnit
{
    public string sidekickName;
    public MonoBehaviour owner;

    public void Initialize(SidekickData data, MonoBehaviour owner)
    {
        sidekickName = data.name;
        this.owner = owner;
        base.Initialize(data.health, data.combatType);
    }

    public Card SelectCardForDefense()
    {
        if (owner is Player playerOwner)
        {
            return playerOwner.SelectCardForDefense();
        }
        else if (owner is Enemy enemyOwner)
        {
            return enemyOwner.SelectDefenseCard();
        }
        return null;
    }
}
