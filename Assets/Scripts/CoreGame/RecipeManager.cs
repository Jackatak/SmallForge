using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [SerializeField] private AlloyRecipeSO alloyRecipeSO;
    [SerializeField] private int totalRounds = 10;

    private List<RoundRecipe> roundRecipes = new List<RoundRecipe>();
    private int currentRoundIndex = 0;

    public RoundRecipe CurrentRound => roundRecipes[currentRoundIndex];

    private void Start()
    {
        GenerateRoundsFromRecipe();
    }

    // This method is called to generate the rounds based on the recipe
    private void GenerateRoundsFromRecipe()
    {
        roundRecipes.Clear();
        for (int i = 0; i < totalRounds; i++)
        {
            roundRecipes.Add(new RoundRecipe());
        }

        // Build a flat list of individual metals (e.g., 58 Cu → 58 entries of Cu)
        List<ResourceObjectSO> allMetalUnits = new List<ResourceObjectSO>();
        foreach (MetalAmount metal in alloyRecipeSO.recipe)
        {
            for (int i = 0; i < metal.amount; i++)
            {
                allMetalUnits.Add(metal.metal);
            }
        }

        // Shuffle the full list
        Shuffle(allMetalUnits);

        // Distribute 1-by-1 into rounds
        for (int i = 0; i < allMetalUnits.Count; i++)
        {
            int roundIndex = i % totalRounds;
            AddMetalToRound(roundRecipes[roundIndex], allMetalUnits[i], 1);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    private void AddMetalToRound(RoundRecipe round, ResourceObjectSO metal, int amount)
    {
        var existing = round.metalsThisRound.Find(m => m.metal == metal);
        if (existing != null)
        {
            existing.amount += amount;
        }
        else
        {
            round.metalsThisRound.Add(new MetalAmount
            {
                metal = metal,
                amount = amount
            });
        }
    }

    public void AdvanceToNextRound()
    {
        currentRoundIndex = Mathf.Min(currentRoundIndex + 1, totalRounds - 1);
    }

    public int GetCurrentRoundNumber() => currentRoundIndex + 1;
}
