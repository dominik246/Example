using Example.MassTransit.RegisterNewUser.EventModels;

using MassTransit;

namespace Example.MassTransit.RegisterNewUser;

public sealed class RegisterNewUserStateMachine : MassTransitStateMachine<RegisterNewUserModelState>
{
    public RegisterNewUserStateMachine()
    {
        Event(() => RegisterNewUser, x => x.CorrelateById(y => y.Message.UserId));

        Event(() => PersistToDbCompleted, x => x.CorrelateById(y => y.Message.UserId));
        Event(() => EmailSendCompleted, x => x.CorrelateById(y => y.Message.Id));

        Event(() => RegisterNewUserFailed, x => x.CorrelateById(y => y.Message.Id));
        Event(() => RegisterNewUserPersistToDbFailed, x => x.CorrelateById(y => y.Message.Id));
        Event(() => RegisterNewUserEmailSendFailed, x => x.CorrelateById(y => y.Message.Id));

        InstanceState(x => x.CurrentState);

        Initially(
            When(RegisterNewUser)
            .Then(x =>
            {
                x.Saga.Email = x.Message.Email;
                x.Saga.EmailConfirmHash = x.Message.EmailConfirmHash;
                x.Saga.EmailTemplate = x.Message.EmailTemplate;
                x.Saga.EmailSubject = x.Message.EmailSubject;
            })
            .PublishAsync(x => x.Init<PersistNewUser>(new PersistNewUser() { Email = x.Message.Email, EmailConfirmHash = x.Message.EmailConfirmHash, UserId = x.Message.UserId, PasswordHash = x.Message.PasswordHash }))
            .TransitionTo(Persisting),
            When(RegisterNewUserFailed)
            .TransitionTo(Failed)
            .Finalize()
        );

        During(Persisting,
            When(PersistToDbCompleted)
            .PublishAsync(x =>
            {
                var context = new Dictionary<string, string>() { { "key", x.Saga.EmailConfirmHash } };
                var model = new RegisterNewUserMailAddressModel
                {
                    Context = context,
                    Id = x.Saga.CorrelationId,
                    SendTo = x.Saga.Email,
                    Subject = x.Saga.EmailSubject,
                    Template = x.Saga.EmailTemplate
                };
                return x.Init<RegisterNewUserMailAddressModel>(model);
            })
            .TransitionTo(SendingEmail),
            When(RegisterNewUserPersistToDbFailed)
            .TransitionTo(Failed)
            .Finalize()
        );

        During(SendingEmail,
            When(EmailSendCompleted)
            .TransitionTo(Completed)
            .Finalize(),
            When(RegisterNewUserEmailSendFailed)
            .TransitionTo(Failed)
            .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    public required State Persisting { get; set; }
    public required State SendingEmail { get; set; }
    public required State Completed { get; set; }
    public required State Failed { get; set; }

    public required Event<RegisterNewUserModel> RegisterNewUser { get; set; }

    public required Event<RegisterUserPersistToDbCompleted> PersistToDbCompleted { get; set; }
    public required Event<RegisterUserEmailSentCompleted> EmailSendCompleted { get; set; }

    public required Event<RegisterNewUserFailed> RegisterNewUserFailed { get; set; }
    public required Event<RegisterNewUserPersistToDbFailed> RegisterNewUserPersistToDbFailed { get; set; }
    public required Event<RegisterNewUserEmailSendFailed> RegisterNewUserEmailSendFailed { get; set; }
}
