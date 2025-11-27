using GreatVoidBattle.Application.Services;
using GreatVoidBattle.Core.Domains.GameState;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Services;

public class TechnologyValidationResultTests
{
    [Fact]
    public void Success_ShouldCreateValidResult()
    {
        // Arrange
        var fractionState = new FractionGameState
        {
            Id = Guid.NewGuid().ToString(),
            FractionId = "test-fraction"
        };
        var technology = new Technology
        {
            Id = "test-tech",
            Name = "Test Technology"
        };
        var ownedTechIds = new HashSet<string> { "tech-1", "tech-2" };

        // Act
        var result = TechnologyValidationResult.Success(fractionState, technology, ownedTechIds);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.ErrorMessage.ShouldBeNull();
        result.FractionState.ShouldBe(fractionState);
        result.Technology.ShouldBe(technology);
        result.OwnedTechnologyIds.ShouldBe(ownedTechIds);
        result.MissingRequirements.Count.ShouldBe(0);
    }

    [Fact]
    public void Failure_ShouldCreateInvalidResult()
    {
        // Arrange
        var errorMessage = "Fraction not found";

        // Act
        var result = TechnologyValidationResult.Failure(errorMessage);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Fraction not found");
        result.FractionState.ShouldBeNull();
        result.Technology.ShouldBeNull();
    }

    [Fact]
    public void MissingRequirementsFailure_ShouldCreateInvalidResultWithRequirements()
    {
        // Arrange
        var errorMessage = "Missing requirements for advanced-laser";
        var missingReqs = new List<string> { "basic-laser", "power-systems" };

        // Act
        var result = TechnologyValidationResult.MissingRequirementsFailure(errorMessage, missingReqs);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("Missing requirements for advanced-laser");
        result.MissingRequirements.Count.ShouldBe(2);
        result.MissingRequirements.ShouldContain("basic-laser");
        result.MissingRequirements.ShouldContain("power-systems");
    }

    [Fact]
    public void Success_ShouldHaveEmptyMissingRequirements()
    {
        // Arrange
        var fractionState = new FractionGameState { FractionId = "test" };
        var technology = new Technology { Id = "tech" };
        var ownedTechIds = new HashSet<string>();

        // Act
        var result = TechnologyValidationResult.Success(fractionState, technology, ownedTechIds);

        // Assert
        result.MissingRequirements.ShouldNotBeNull();
        result.MissingRequirements.Count.ShouldBe(0);
    }

    [Fact]
    public void Failure_ShouldHaveEmptyMissingRequirements()
    {
        // Act
        var result = TechnologyValidationResult.Failure("Some error");

        // Assert
        result.MissingRequirements.ShouldNotBeNull();
        result.MissingRequirements.Count.ShouldBe(0);
    }

    [Fact]
    public void MissingRequirementsFailure_WithEmptyList_ShouldWork()
    {
        // Act
        var result = TechnologyValidationResult.MissingRequirementsFailure("Error", new List<string>());

        // Assert
        result.IsValid.ShouldBeFalse();
        result.MissingRequirements.Count.ShouldBe(0);
    }
}

