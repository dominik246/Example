using Example.MassTransit.PasswordRecovery.EventModels;

using MassTransit;

namespace Example.MassTransit.PasswordRecovery;

public sealed class PasswordRecoveryStateMachine : MassTransitStateMachine<PasswordRecoveryState>
{
    public PasswordRecoveryStateMachine()
    {
        Event(() => PasswordRecoveryInitial, x => x.CorrelateById(y => y.Message.UserId));
        Event(() => PersistToDbCompleted, x => x.CorrelateById(y => y.Message.UserId));
        Event(() => EmailSendCompleted, x => x.CorrelateById(y => y.Message.Id));

        Event(() => PasswordRecoveryInitialFailed, x => x.CorrelateById(y => y.Message.Id));
        Event(() => PasswordRecoveryPersistToDbFailed, x => x.CorrelateById(y => y.Message.Id));
        Event(() => PasswordRecoveryEmailSendFailed, x => x.CorrelateById(y => y.Message.Id));

        InstanceState(x => x.CurrentState);

        Initially(
            When(PasswordRecoveryInitial)
            .Then(x =>
            {
                x.Saga.Email = x.Message.Email;
                x.Saga.EmailSubject = x.Message.EmailSubject;
                x.Saga.EmailTemplate = x.Message.EmailTemplate;
                x.Saga.UserId = x.Message.UserId;
                x.Saga.Hash = x.Message.Hash;
            })
            .PublishAsync(x => x.Init<PasswordRecoveryPersist>(new PasswordRecoveryPersist { Hash = x.Saga.Hash, UserId = x.Saga.UserId, Email = x.Saga.Email }))
            .TransitionTo(Persisting),
            When(PasswordRecoveryInitialFailed)
            .TransitionTo(Failed)
            .Finalize()
        );

        During(Persisting,
            When(PersistToDbCompleted)
            .PublishAsync(x =>
            {
                var context = new Dictionary<string, string>() { { "key", x.Saga.Hash } };
                var model = new PasswordRecoveryMailAddressModel
                {
                    Context = context,
                    Id = x.Saga.CorrelationId,
                    SendTo = x.Saga.Email,
                    Subject = x.Saga.EmailSubject,
                    Template = x.Saga.EmailTemplate
                };
                return x.Init<PasswordRecoveryMailAddressModel>(model);
            })
            .TransitionTo(SendingEmail),
            When(PasswordRecoveryPersistToDbFailed)
            .TransitionTo(Failed)
            .Finalize()
        );

        During(SendingEmail,
            When(EmailSendCompleted)
            .TransitionTo(Completed)
            .Finalize(),
            When(PasswordRecoveryEmailSendFailed)
            .TransitionTo(Failed)
            .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    public required State Persisting { get; set; }
    public required State SendingEmail { get; set; }
    public required State Completed { get; set; }
    public required State Failed { get; set; }

    public required Event<PasswordRecoveryModel> PasswordRecoveryInitial { get; set; }
    public required Event<PasswordRecoveryPersistToDbCompleted> PersistToDbCompleted { get; set; }
    public required Event<PasswordRecoveryEmailSentCompleted> EmailSendCompleted { get; set; }

    public required Event<PasswordRecoveryInitialFailed> PasswordRecoveryInitialFailed { get; set; }
    public required Event<PasswordRecoveryPersistToDbFailed> PasswordRecoveryPersistToDbFailed { get; set; }
    public required Event<PasswordRecoveryEmailSendFailed> PasswordRecoveryEmailSendFailed { get; set; }
}
