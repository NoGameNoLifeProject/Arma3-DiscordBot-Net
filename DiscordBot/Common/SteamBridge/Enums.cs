namespace DiscordBot.Common.SteamBridge
{
    public enum LoginResult
    {
        OK,
        RateLimitedExceeded,
        WrongInformation,
        WaitingForSteamGuard,
        SteamGuardCodeWrong,
        TwoFactorWrong,
        ExpiredCode,
        AlreadyLoggedIn,
        SteamGuardNotSupported,
        Timeout
    }

    public enum SteamExitReason
    {
        NothingSpecial,
        NonEnglishCharachers
    }

    public enum UpdateStateStage
    {
        Validating,
        Downloading,
        Commiting
    }
}
