namespace Example.ServiceDefaults;

public readonly ref struct ConnectionStrings
{
    public const string AuthCache = "authcache";
    public const string Seq = "seq";
    public const string AuthDb = "authdb";
    public const string NotificationDb = "notificationdb";
    public const string NatsServer = "nats";
}

public readonly ref struct ProjectNames
{
    public const string AuthApi = "authapi";
    public const string EmailServiceApi = "emailserviceapi";
    public const string NotificationApi = "notificationApi";
}

public readonly ref struct DatabaseNames
{
    public const string AuthDatabase = "authdatabase";
    public const string NotificationDatabase = "notificationdatabase";
}

public readonly ref struct NatsStreams
{
    public const string EmailStream = "email-stream";
}

public readonly ref struct NatsConsumers
{
    public const string EmailConsumer = "email-consumer-1";
}

public readonly ref struct NatsEvents
{
    public const string EmailPasswordRecoverEvent = "email.password.recover";
}