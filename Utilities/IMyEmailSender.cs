﻿using NiagaraCollegeProject.ViewModels;
using System.Net.Mail;

namespace NiagaraCollegeProject.Utilities
{

    /// <summary>
    /// Interface for my own email service
    /// </summary>
    public interface IMyEmailSender
    {
        Task SendOneAsync(string name, string email, string subject, string htmlMessage);
        Task SendToManyAsync(EmailMessage emailMessage);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
