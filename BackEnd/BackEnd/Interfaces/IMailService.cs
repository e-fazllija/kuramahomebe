using BackEnd.Models.InputModels;
using BackEnd.Models.MailModels;

namespace BackEnd.Interfaces;

public interface IMailService
{
    Task SendEmailAsync(MailRequest mailRequest);
    Task SendEvaluationRequestAsync(SendRequestModel mailRequest);
    Task SendWorkWithUsRequestAsync(SendRequestModel mailRequest);
    Task SendRequestAsync(SendRequestModel mailRequest);
    Task InformationRequestAsync(SendRequestModel mailRequest);
}