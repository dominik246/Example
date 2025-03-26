namespace Example.AuthApi.Localization;

public readonly ref struct LocalizerConsts
{
    public readonly ref struct UserRegisterCommandHandler
    {
        public const string EmailSubject = "email.subjects.confirm-email";
        public const string EmailTemplate = "email.templates.confirm-email";
    }
    public readonly ref struct PasswordRecoveryCommandHandler
    {
        public const string EmailSubject = "email.subjects.password-recovery";
        public const string EmailTemplate = "email.templates.password-recovery";
    }
}
