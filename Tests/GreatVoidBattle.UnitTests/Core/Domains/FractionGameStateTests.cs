using GreatVoidBattle.Core.Domains.GameState;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains;

public class FractionGameStateTests
{
    private static FractionGameState CreateTestState(int researchSlots = 3)
    {
        return new FractionGameState
        {
            Id = Guid.NewGuid().ToString(),
            FractionId = "test-fraction",
            CurrentTier = 1,
            ResearchSlots = researchSlots,
            ResearchedTechnologiesInCurrentTier = 0
        };
    }

    // Helper to get tier from technology ID (simulates TechnologyConfig)
    private static int GetTechTier(string techId)
    {
        // Pattern: "tech-tier{N}-{suffix}" -> returns N
        if (techId.StartsWith("tech-tier"))
        {
            var tierPart = techId.Split('-')[1].Replace("tier", "");
            if (int.TryParse(tierPart, out var tier))
                return tier;
        }
        return 1; // default tier
    }

    #region GetUsedResearchSlots Tests

    [Fact]
    public void GetUsedResearchSlots_WithNoPendingRequests_ShouldReturnZero()
    {
        // Arrange
        var state = CreateTestState();

        // Act
        var usedSlots = state.GetUsedResearchSlots(GetTechTier);

        // Assert
        usedSlots.ShouldBe(0);
    }

    [Fact]
    public void GetUsedResearchSlots_WithTier1Requests_ShouldReturn1PerRequest()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier1-laser", 
            Status = ResearchRequestStatus.Pending 
        });
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier1-shield", 
            Status = ResearchRequestStatus.Pending 
        });

        // Act
        var usedSlots = state.GetUsedResearchSlots(GetTechTier);

        // Assert
        usedSlots.ShouldBe(2); // 2 tier-1 requests = 2 slots
    }

    [Fact]
    public void GetUsedResearchSlots_WithTier2Request_ShouldReturn2Slots()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier2-advanced", 
            Status = ResearchRequestStatus.Pending 
        });

        // Act
        var usedSlots = state.GetUsedResearchSlots(GetTechTier);

        // Assert
        usedSlots.ShouldBe(2); // 1 tier-2 request = 2 slots
    }

    [Fact]
    public void GetUsedResearchSlots_WithMixedTierRequests_ShouldSumCorrectly()
    {
        // Arrange
        var state = CreateTestState(researchSlots: 10);
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier1-a", 
            Status = ResearchRequestStatus.Pending 
        });
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier2-b", 
            Status = ResearchRequestStatus.Pending 
        });
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier3-c", 
            Status = ResearchRequestStatus.Pending 
        });

        // Act
        var usedSlots = state.GetUsedResearchSlots(GetTechTier);

        // Assert
        usedSlots.ShouldBe(6); // 1 + 2 + 3 = 6 slots
    }

    [Fact]
    public void GetUsedResearchSlots_ShouldIgnoreApprovedRequests()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier1-pending", 
            Status = ResearchRequestStatus.Pending 
        });
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier1-approved", 
            Status = ResearchRequestStatus.Approved 
        });
        state.ResearchRequests.Add(new ResearchRequest 
        { 
            TechnologyId = "tech-tier1-rejected", 
            Status = ResearchRequestStatus.Rejected 
        });

        // Act
        var usedSlots = state.GetUsedResearchSlots(GetTechTier);

        // Assert
        usedSlots.ShouldBe(1); // Only pending request counts
    }

    #endregion

    #region GetResearchedCountForTier Tests

    [Fact]
    public void GetResearchedCountForTier_WithNoTechnologies_ShouldReturnZero()
    {
        // Arrange
        var state = CreateTestState();

        // Act
        var count = state.GetResearchedCountForTier(GetTechTier, 1);

        // Assert
        count.ShouldBe(0);
    }

    [Fact]
    public void GetResearchedCountForTier_ShouldCountOnlyResearchedTechnologies()
    {
        // Arrange
        var state = CreateTestState();
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "tech-tier1-a", 
            Source = TechnologySource.Research 
        });
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "tech-tier1-b", 
            Source = TechnologySource.Research 
        });
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "tech-tier1-c", 
            Source = TechnologySource.Trade // Not research
        });

        // Act
        var count = state.GetResearchedCountForTier(GetTechTier, 1);

        // Assert
        count.ShouldBe(2); // Only Research source counts
    }

    [Fact]
    public void GetResearchedCountForTier_ShouldCountOnlySpecificTier()
    {
        // Arrange
        var state = CreateTestState();
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "tech-tier1-a", 
            Source = TechnologySource.Research 
        });
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "tech-tier2-b", 
            Source = TechnologySource.Research 
        });
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "tech-tier1-c", 
            Source = TechnologySource.Research 
        });

        // Act
        var tier1Count = state.GetResearchedCountForTier(GetTechTier, 1);
        var tier2Count = state.GetResearchedCountForTier(GetTechTier, 2);

        // Assert
        tier1Count.ShouldBe(2);
        tier2Count.ShouldBe(1);
    }

    #endregion

    #region CanResearchTier Tests

    [Fact]
    public void CanResearchTier_Tier1_ShouldAlwaysReturnTrue()
    {
        // Arrange
        var state = CreateTestState();

        // Act
        var canResearch = state.CanResearchTier(GetTechTier, 1);

        // Assert
        canResearch.ShouldBeTrue();
    }

    [Fact]
    public void CanResearchTier_Tier2_WithoutEnoughTier1Research_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        // Add only 10 researched technologies from tier 1 (need 15)
        for (int i = 0; i < 10; i++)
        {
            state.Technologies.Add(new FractionTechnology 
            { 
                TechnologyId = $"tech-tier1-{i}", 
                Source = TechnologySource.Research 
            });
        }

        // Act
        var canResearch = state.CanResearchTier(GetTechTier, 2);

        // Assert
        canResearch.ShouldBeFalse();
    }

    [Fact]
    public void CanResearchTier_Tier2_WithEnoughTier1Research_ShouldReturnTrue()
    {
        // Arrange
        var state = CreateTestState();
        // Add 15 researched technologies from tier 1
        for (int i = 0; i < 15; i++)
        {
            state.Technologies.Add(new FractionTechnology 
            { 
                TechnologyId = $"tech-tier1-{i}", 
                Source = TechnologySource.Research 
            });
        }

        // Act
        var canResearch = state.CanResearchTier(GetTechTier, 2);

        // Assert
        canResearch.ShouldBeTrue();
    }

    [Fact]
    public void CanResearchTier_Tier3_RequiresTier2Research()
    {
        // Arrange
        var state = CreateTestState();
        // Add 15 researched tier 1 technologies
        for (int i = 0; i < 15; i++)
        {
            state.Technologies.Add(new FractionTechnology 
            { 
                TechnologyId = $"tech-tier1-{i}", 
                Source = TechnologySource.Research 
            });
        }
        // Add only 10 researched tier 2 technologies (need 15)
        for (int i = 0; i < 10; i++)
        {
            state.Technologies.Add(new FractionTechnology 
            { 
                TechnologyId = $"tech-tier2-{i}", 
                Source = TechnologySource.Research 
            });
        }

        // Act
        var canResearchTier3 = state.CanResearchTier(GetTechTier, 3);

        // Assert
        canResearchTier3.ShouldBeFalse();
    }

    [Fact]
    public void CanResearchTier_TradedTechnologies_ShouldNotCount()
    {
        // Arrange
        var state = CreateTestState();
        // Add 15 traded technologies from tier 1 (should not count)
        for (int i = 0; i < 15; i++)
        {
            state.Technologies.Add(new FractionTechnology 
            { 
                TechnologyId = $"tech-tier1-{i}", 
                Source = TechnologySource.Trade 
            });
        }

        // Act
        var canResearch = state.CanResearchTier(GetTechTier, 2);

        // Assert
        canResearch.ShouldBeFalse(); // Trade doesn't count
    }

    #endregion

    #region CanViewTier Tests

    [Fact]
    public void CanViewTier_ShouldAlwaysViewTier1()
    {
        // Arrange
        var state = CreateTestState();

        // Act
        var canView = state.CanViewTier(GetTechTier, 1);

        // Assert
        canView.ShouldBeTrue();
    }

    [Fact]
    public void CanViewTier_ShouldViewNextTierAsPreview()
    {
        // Arrange
        var state = CreateTestState();
        // Can research tier 1, so should be able to view tier 2 as preview

        // Act
        var canViewTier2 = state.CanViewTier(GetTechTier, 2);

        // Assert
        canViewTier2.ShouldBeTrue(); // Can view as preview
    }

    [Fact]
    public void CanViewTier_ShouldNotViewTwoTiersAhead()
    {
        // Arrange
        var state = CreateTestState();
        // Can research tier 1, should NOT see tier 3

        // Act
        var canViewTier3 = state.CanViewTier(GetTechTier, 3);

        // Assert
        canViewTier3.ShouldBeFalse();
    }

    [Fact]
    public void CanViewTier_WithUnlockedTier2_ShouldSeeTier3()
    {
        // Arrange
        var state = CreateTestState();
        // Unlock tier 2 by researching 15 tier 1 technologies
        for (int i = 0; i < 15; i++)
        {
            state.Technologies.Add(new FractionTechnology 
            { 
                TechnologyId = $"tech-tier1-{i}", 
                Source = TechnologySource.Research 
            });
        }

        // Act
        var canViewTier2 = state.CanViewTier(GetTechTier, 2);
        var canViewTier3 = state.CanViewTier(GetTechTier, 3);
        var canViewTier4 = state.CanViewTier(GetTechTier, 4);

        // Assert
        canViewTier2.ShouldBeTrue(); // Can research
        canViewTier3.ShouldBeTrue(); // Preview
        canViewTier4.ShouldBeFalse(); // Too far ahead
    }

    #endregion

    #region CanAdvanceToNextTier Tests

    [Fact]
    public void CanAdvanceToNextTier_WithLessThan15Researched_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchedTechnologiesInCurrentTier = 14;

        // Act
        var canAdvance = state.CanAdvanceToNextTier();

        // Assert
        canAdvance.ShouldBeFalse();
    }

    [Fact]
    public void CanAdvanceToNextTier_WithExactly15Researched_ShouldReturnTrue()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchedTechnologiesInCurrentTier = 15;

        // Act
        var canAdvance = state.CanAdvanceToNextTier();

        // Assert
        canAdvance.ShouldBeTrue();
    }

    [Fact]
    public void CanAdvanceToNextTier_WithMoreThan15Researched_ShouldReturnTrue()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchedTechnologiesInCurrentTier = 20;

        // Act
        var canAdvance = state.CanAdvanceToNextTier();

        // Assert
        canAdvance.ShouldBeTrue();
    }

    #endregion

    #region ResearchSlots Default Value Tests

    [Fact]
    public void NewFractionGameState_ShouldHaveDefaultResearchSlots()
    {
        // Arrange & Act
        var state = new FractionGameState();

        // Assert
        state.ResearchSlots.ShouldBe(3); // Default is 3 slots
    }

    [Fact]
    public void ResearchSlots_CanBeModified()
    {
        // Arrange
        var state = CreateTestState(researchSlots: 5);

        // Assert
        state.ResearchSlots.ShouldBe(5);
    }

    #endregion
}

