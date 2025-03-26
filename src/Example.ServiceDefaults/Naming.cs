namespace Example.ServiceDefaults;

public readonly ref struct ConnectionStrings
{
    public const string AuthCache = "authcache";
    public const string AuthOutboxServer = "auth-outbox-server";
    public const string Seq = "seq";
    public const string AuthDb = "authdb";
    public const string NotificationDb = "notificationdb";
    public const string NatsServer = "nats";
    public const string RabbitMq = "rabbitmq";
}

public readonly ref struct ProjectNames
{
    public const string AuthApi = "authapi";
    public const string AuthDbWorker = "auth-db-worker";
    public const string EmailService = "emailservice";
    public const string NotificationApi = "notificationApi";
}

public readonly ref struct DatabaseNames
{
    public const string AuthDatabase = "authdatabase";
    public const string AuthOutboxDatabase = "auth-outbox-db";
    public const string NotificationDatabase = "notificationdatabase";
}
