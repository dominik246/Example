﻿namespace Example.ServiceDefaults.Consts;

public readonly ref struct AuthConsts
{
    public const int ConfirmEmailHexLength = 32;
    public const int PasswordRecoveryHexLength = 128;
    public const int RefreshTokenHexLength = 32;
}
