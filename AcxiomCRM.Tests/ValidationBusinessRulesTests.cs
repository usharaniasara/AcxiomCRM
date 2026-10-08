using Xunit;
using AcxiomCRM.Validators;

namespace AcxiomCRM.Tests;

public class ValidationBusinessRulesTests
{
    [Theory]
    [InlineData(0, 50, "Open", true)]
    [InlineData(-1, 50, "Open", true)]
    [InlineData(1000, 50, "Won", false)]
    public void OpportunityValidationRejectsInvalidAmount(decimal amount, int probability, string status, bool shouldError)
    {
        var errors = OpportunityRules.Validate(amount, probability, DateTime.UtcNow.AddDays(10), status);
        var hasAmountError = errors.Any(x => x.Contains("greater than 0") || x.Contains("negative"));
        Assert.Equal(shouldError, hasAmountError);
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(101, true)]
    [InlineData(100, false)]
    public void OpportunityValidationChecksProbabilityRange(int probability, bool shouldError)
    {
        var errors = OpportunityRules.Validate(5000m, probability, DateTime.UtcNow.AddDays(10), "Open");
        Assert.Equal(shouldError, errors.Any(x => x.Contains("between 0 and 100")));
    }

    [Fact]
    public void OpportunityValidationRejectsPastCloseDateForActiveDeal()
    {
        var errors = OpportunityRules.Validate(2500m, 50, DateTime.UtcNow.AddDays(-1), "Open");
        Assert.Contains(errors, x => x.Contains("Expected Close Date cannot be in the past."));
    }

    [Fact]
    public void FollowUpValidationRejectsPastDateForPlannedFollowUp()
    {
        var error = FollowUpRules.ValidatePlannedDate(DateTime.UtcNow.AddDays(-1), "Planned");
        Assert.Equal("Follow-up date cannot be earlier than today.", error);
    }

    [Fact]
    public void FollowUpValidationAllowsTodayForPlannedFollowUp()
    {
        var error = FollowUpRules.ValidatePlannedDate(DateTime.UtcNow, "Planned");
        Assert.Null(error);
    }
}
