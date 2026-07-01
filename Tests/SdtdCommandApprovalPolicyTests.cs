using NUnit.Framework;

namespace PolyhydraGames.SdtdSharp.Tests;

[TestFixture]
public sealed class SdtdCommandApprovalPolicyTests
{
    [TestCase("listplayers", SdtdCommandRisks.ReadOnly, SdtdCommandDispositions.Allowed, false)]
    [TestCase("say \"hello\"", SdtdCommandRisks.Communication, SdtdCommandDispositions.ApprovalRequired, true)]
    [TestCase("kick 171 bad behavior", SdtdCommandRisks.Moderation, SdtdCommandDispositions.ApprovalRequired, true)]
    [TestCase("buff godmode", SdtdCommandRisks.PlayerEffect, SdtdCommandDispositions.ApprovalRequired, true)]
    [TestCase("teleport 171 1 2 3", SdtdCommandRisks.Movement, SdtdCommandDispositions.ApprovalRequired, true)]
    [TestCase("spawnentity 171 zombieBoe", SdtdCommandRisks.Spawn, SdtdCommandDispositions.ApprovalRequired, true)]
    [TestCase("settime 12000", SdtdCommandRisks.WorldMutation, SdtdCommandDispositions.ApprovalRequired, true)]
    [TestCase("givequest quest_BasicSurvival1", SdtdCommandRisks.Progression, SdtdCommandDispositions.ApprovalRequired, true)]
    [TestCase("shutdown", SdtdCommandRisks.Lifecycle, SdtdCommandDispositions.Denied, true)]
    [TestCase("unknowncommand", SdtdCommandRisks.Unknown, SdtdCommandDispositions.Denied, true)]
    public void Classify_ReturnsRiskAndDefaultDisposition(string command, string risk, string disposition, bool isMutating)
    {
        var classification = SdtdCommandApprovalPolicy.Classify(command);

        Assert.Multiple(() =>
        {
            Assert.That(classification.Risk, Is.EqualTo(risk));
            Assert.That(classification.DefaultDisposition, Is.EqualTo(disposition));
            Assert.That(classification.IsMutating, Is.EqualTo(isMutating));
        });
    }

    [Test]
    public void Evaluate_DeniesUnknownCommands()
    {
        var result = SdtdCommandApprovalPolicy.Evaluate(new SdtdCommandApprovalRequest("debugmenu", Actor: "chat"));

        Assert.Multiple(() =>
        {
            Assert.That(result.CanExecute, Is.False);
            Assert.That(result.Disposition, Is.EqualTo(SdtdCommandDispositions.Denied));
            Assert.That(result.Risk, Is.EqualTo(SdtdCommandRisks.Unknown));
        });
    }

    [Test]
    public void Evaluate_DryRunNeverExecutesMutatingCommand()
    {
        var result = SdtdCommandApprovalPolicy.Evaluate(new SdtdCommandApprovalRequest("spawnentity 171 zombieBoe", DryRun: true, Actor: "stream"));

        Assert.Multiple(() =>
        {
            Assert.That(result.CanExecute, Is.False);
            Assert.That(result.IsDryRun, Is.True);
            Assert.That(result.Disposition, Is.EqualTo(SdtdCommandDispositions.DryRun));
            Assert.That(result.Risk, Is.EqualTo(SdtdCommandRisks.Spawn));
        });
    }

    [Test]
    public void Evaluate_RequiresApprovalForMutatingCommand()
    {
        var result = SdtdCommandApprovalPolicy.Evaluate(new SdtdCommandApprovalRequest("teleport 171 1 2 3", Actor: "ai"));

        Assert.Multiple(() =>
        {
            Assert.That(result.CanExecute, Is.False);
            Assert.That(result.Disposition, Is.EqualTo(SdtdCommandDispositions.ApprovalRequired));
            Assert.That(result.Risk, Is.EqualTo(SdtdCommandRisks.Movement));
        });
    }

    [Test]
    public void Evaluate_AllowsApprovedMutatingCommand()
    {
        var result = SdtdCommandApprovalPolicy.Evaluate(new SdtdCommandApprovalRequest("say \"hello survivors\"", Approved: true, Actor: "operator"));

        Assert.Multiple(() =>
        {
            Assert.That(result.CanExecute, Is.True);
            Assert.That(result.Disposition, Is.EqualTo(SdtdCommandDispositions.Allowed));
            Assert.That(result.Risk, Is.EqualTo(SdtdCommandRisks.Communication));
        });
    }

    [Test]
    public void VersionCoverageMap_TagsEstablishedPackageLayers()
    {
        var repoRoot = TestContext.CurrentContext.TestDirectory;
        while (repoRoot is not null && !File.Exists(Path.Combine(repoRoot, "README.md")))
        {
            repoRoot = Directory.GetParent(repoRoot)?.FullName;
        }

        Assert.That(repoRoot, Is.Not.Null);

        var coverage = File.ReadAllText(Path.Combine(repoRoot!, "docs", "features", "version-coverage-map.md"));

        Assert.That(coverage, Does.Contain("established_versions: [V2, V3]"));
        Assert.That(coverage, Does.Contain("not_applicable_versions: [V0, V1]"));
        Assert.That(coverage, Does.Contain("blocked_versions: [V5]"));
        Assert.That(coverage, Does.Contain("dry-run never executes a mutating command"));
        Assert.That(coverage, Does.Contain("production mutation blocked"));
    }
}
