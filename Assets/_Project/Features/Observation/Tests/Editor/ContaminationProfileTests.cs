using NUnit.Framework;

public sealed class ContaminationProfileTests
{
    [Test]
    public void NewProfileStartsWithoutCauseData()
    {
        var profile = new ContaminationProfile();

        Assert.IsFalse(profile.HasCauseProfile);

        foreach (ContaminationCause cause in System.Enum.GetValues(typeof(ContaminationCause)))
        {
            Assert.AreEqual(0f, profile.GetAmount(cause));
            Assert.AreEqual(0, profile.GetCount(cause));
        }
    }

    [Test]
    public void RecordStoresAmountCountAndLastCause()
    {
        var profile = new ContaminationProfile();

        profile.Record(3f, ContaminationCause.FastLook);

        Assert.IsTrue(profile.HasCauseProfile);
        Assert.AreEqual(3f, profile.GetAmount(ContaminationCause.FastLook));
        Assert.AreEqual(1, profile.GetCount(ContaminationCause.FastLook));
        Assert.AreEqual(ContaminationCause.FastLook, profile.LastCause);
    }

    [Test]
    public void RecordAccumulatesSameCause()
    {
        var profile = new ContaminationProfile();

        profile.Record(3f, ContaminationCause.FastLook);
        profile.Record(2f, ContaminationCause.FastLook);

        Assert.AreEqual(5f, profile.GetAmount(ContaminationCause.FastLook));
        Assert.AreEqual(2, profile.GetCount(ContaminationCause.FastLook));
    }

    [Test]
    public void DominantCauseUsesHighestAccumulatedAmount()
    {
        var profile = new ContaminationProfile();

        profile.Record(3f, ContaminationCause.FastLook);
        profile.Record(5f, ContaminationCause.RepeatCheck);
        profile.Record(1f, ContaminationCause.TurnAround);

        Assert.AreEqual(ContaminationCause.RepeatCheck, profile.DominantCause);
    }

    [Test]
    public void DominantCauseUsesLastCauseWhenAmountsTie()
    {
        var profile = new ContaminationProfile();

        profile.Record(3f, ContaminationCause.FastLook);
        profile.Record(3f, ContaminationCause.RepeatCheck);

        Assert.AreEqual(ContaminationCause.RepeatCheck, profile.DominantCause);
    }
}
