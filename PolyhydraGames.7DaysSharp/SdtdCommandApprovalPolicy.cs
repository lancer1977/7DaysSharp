namespace PolyhydraGames.SdtdSharp;

public sealed record SdtdCommandApprovalRequest(
    string Command,
    bool Approved = false,
    bool DryRun = false,
    string Actor = "unknown");

public sealed record SdtdCommandApprovalResult(
    string Command,
    string Verb,
    string Risk,
    string Disposition,
    bool CanExecute,
    bool IsDryRun,
    string Message);

public sealed record SdtdCommandClassification(
    string Verb,
    string Risk,
    string DefaultDisposition,
    bool IsMutating);

public static class SdtdCommandRisks
{
    public const string ReadOnly = "read-only";
    public const string Communication = "communication";
    public const string Moderation = "moderation";
    public const string PlayerEffect = "player-effect";
    public const string Movement = "movement";
    public const string Spawn = "spawn";
    public const string WorldMutation = "world-mutation";
    public const string Progression = "progression";
    public const string Lifecycle = "lifecycle";
    public const string Unknown = "unknown";
}

public static class SdtdCommandDispositions
{
    public const string Allowed = "allowed";
    public const string ApprovalRequired = "approval-required";
    public const string DryRun = "dry-run";
    public const string Denied = "denied";
}

public static class SdtdCommandApprovalPolicy
{
    public static SdtdCommandApprovalResult Evaluate(SdtdCommandApprovalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = NormalizeCommand(request.Command);
        var classification = Classify(command);

        if (request.DryRun)
        {
            return new SdtdCommandApprovalResult(
                command,
                classification.Verb,
                classification.Risk,
                SdtdCommandDispositions.DryRun,
                CanExecute: false,
                IsDryRun: true,
                $"Dry run only: {classification.Risk} command was not executed.");
        }

        if (classification.DefaultDisposition == SdtdCommandDispositions.Denied)
        {
            return new SdtdCommandApprovalResult(
                command,
                classification.Verb,
                classification.Risk,
                SdtdCommandDispositions.Denied,
                CanExecute: false,
                IsDryRun: false,
                $"Denied {classification.Risk} command for actor {request.Actor}.");
        }

        if (classification.DefaultDisposition == SdtdCommandDispositions.Allowed || request.Approved)
        {
            return new SdtdCommandApprovalResult(
                command,
                classification.Verb,
                classification.Risk,
                SdtdCommandDispositions.Allowed,
                CanExecute: true,
                IsDryRun: false,
                $"Allowed {classification.Risk} command for actor {request.Actor}.");
        }

        return new SdtdCommandApprovalResult(
            command,
            classification.Verb,
            classification.Risk,
            SdtdCommandDispositions.ApprovalRequired,
            CanExecute: false,
            IsDryRun: false,
            $"Approval required for {classification.Risk} command before execution.");
    }

    public static SdtdCommandClassification Classify(string command)
    {
        var verb = ExtractVerb(command);
        return verb switch
        {
            "listplayers" or "gettime" => new(verb, SdtdCommandRisks.ReadOnly, SdtdCommandDispositions.Allowed, IsMutating: false),
            "say" => new(verb, SdtdCommandRisks.Communication, SdtdCommandDispositions.ApprovalRequired, IsMutating: true),
            "kick" or "ban" => new(verb, SdtdCommandRisks.Moderation, SdtdCommandDispositions.ApprovalRequired, IsMutating: true),
            "buff" or "debuff" => new(verb, SdtdCommandRisks.PlayerEffect, SdtdCommandDispositions.ApprovalRequired, IsMutating: true),
            "teleport" => new(verb, SdtdCommandRisks.Movement, SdtdCommandDispositions.ApprovalRequired, IsMutating: true),
            "spawnentity" or "spawnscouts" or "spawnhorde" => new(verb, SdtdCommandRisks.Spawn, SdtdCommandDispositions.ApprovalRequired, IsMutating: true),
            "settime" or "weather" => new(verb, SdtdCommandRisks.WorldMutation, SdtdCommandDispositions.ApprovalRequired, IsMutating: true),
            "givequest" => new(verb, SdtdCommandRisks.Progression, SdtdCommandDispositions.ApprovalRequired, IsMutating: true),
            "shutdown" => new(verb, SdtdCommandRisks.Lifecycle, SdtdCommandDispositions.Denied, IsMutating: true),
            _ => new(verb, SdtdCommandRisks.Unknown, SdtdCommandDispositions.Denied, IsMutating: true)
        };
    }

    private static string NormalizeCommand(string command)
        => string.IsNullOrWhiteSpace(command)
            ? throw new ArgumentException("Command is required.", nameof(command))
            : command.Trim();

    private static string ExtractVerb(string command)
    {
        var normalized = NormalizeCommand(command);
        var separator = normalized.IndexOf(' ');
        return (separator < 0 ? normalized : normalized[..separator]).ToLowerInvariant();
    }
}
