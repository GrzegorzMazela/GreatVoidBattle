using GreatVoidBattle.Core.Domains.GameState;

namespace GreatVoidBattle.Application.Services;

/// <summary>
/// Result of technology validation
/// </summary>
public class TechnologyValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public FractionGameState? FractionState { get; private set; }
    public FractionGameState? State => FractionState; // Alias for convenience
    public Technology? Technology { get; private set; }
    public HashSet<string> OwnedTechnologyIds { get; private set; } = new();
    public List<string> MissingRequirements { get; private set; } = new();

    public static TechnologyValidationResult Success(
        FractionGameState fractionState, 
        Technology technology,
        HashSet<string> ownedTechIds)
    {
        return new TechnologyValidationResult
        {
            IsValid = true,
            FractionState = fractionState,
            Technology = technology,
            OwnedTechnologyIds = ownedTechIds
        };
    }

    public static TechnologyValidationResult Failure(string errorMessage)
    {
        return new TechnologyValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }

    public static TechnologyValidationResult MissingRequirementsFailure(
        string errorMessage, 
        List<string> missingRequirements)
    {
        return new TechnologyValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            MissingRequirements = missingRequirements
        };
    }
}

