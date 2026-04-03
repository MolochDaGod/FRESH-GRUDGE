//using System.Linq;

// BUFF
public partial struct Buff
{
    public LevelBasedElement[] elementalResistances { get { return data.elementalResistances; } }

    public float GetResistance(ElementTemplate element)
    {
        foreach (var item in elementalResistances)
        {
            if (item.template == element)
                return item.Get(level);
        }
        return 0f; //elementalResistances.FirstOrDefault(x => x.template == element).Get(level); //TODO essaie de suppression de Linq pour les perfs
    }
}