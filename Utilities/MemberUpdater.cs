using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NiagaraCollegeProject.Data;
using NiagaraCollegeProject.Models;
using NiagaraCollegeProject.ViewModels;

namespace NiagaraCollegeProject.Utilities
{
    public class MemberUpdater
    {
        private readonly PAC_Context _dbContext;
        private readonly IMyEmailSender _emailSender;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public MemberUpdater(PAC_Context context,
            IMyEmailSender emailSender,
            IBackgroundJobClient backgroundJobClient)
        {
            _dbContext = context;
            _emailSender = emailSender;
            _backgroundJobClient = backgroundJobClient;
        }

        public void UpdateMemberIsArchived()
        {
            _backgroundJobClient.Enqueue<MemberUpdater>(x => x.UpdateNotifyMember());
            _backgroundJobClient.Schedule<MemberUpdater>(x => x.UpdateNotifyMember(), TimeSpan.FromSeconds(5));
        }

        public async Task UpdateNotifyMember()
        {
            // Your logic to update the isArchived field goes here
            // For example, you can retrieve all members from your data store
            // and update their isArchived field based on certain conditions

            // Example logic:
            var members = _dbContext.Members.ToList(); // Replace with your actual Member retrieval logic

            foreach (var m in members)
            {
                // Update the isArchived field based on your conditions
                if (DateTime.Now >= m.RenewalDueBy.AddDays(-7))
                {
                    m.IsArchived = true;
                    await Notification(m.ID);
                }
            }

            _dbContext.SaveChanges();
        }

        public async Task Notification(int id)
        {
            var member = _dbContext.Members.Find(id);

            await _emailSender.SendOneAsync(member.FullName, member.Email, "Dear " + member.FullName +
                "Your PAC account is expiring in 7 day", MemberRenewalNotify.MemberMinTemp(member.Email));
        }
    }
}
