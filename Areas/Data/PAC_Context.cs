using Microsoft.EntityFrameworkCore;
using MVC_Music.Models;
using NiagaraCollegeProject.Models;

namespace NiagaraCollegeProject.Data
{
    public class PAC_Context : DbContext
    {

        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public PAC_Context(DbContextOptions<PAC_Context> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
                UserName ??= "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }

        public DbSet<Member> Members { get; set; }
      
        public DbSet<AcademicDivision> AcademicDivisions { get; set; }
        public DbSet<PAC> PAC { get; set; }
        public DbSet<MeetingAttendees> MeetingAttendees { get; set; }

        public DbSet<Salutation> Salutations { get; set; }
        public DbSet<ActionItem> ActionItems { get; set; }
        public DbSet<ActionDocuments> ActionDocuments { get; set; }
        public DbSet<UploadedFile> UploadedFile { get; set; }
        public DbSet<MemberDocuments> MemberDocuments { get; set; }
        public DbSet<ReportDocuments> ReportDocuments { get; set; }
        public DbSet<MeetingDocuments> MeetingDocuments { get; set; }
        public DbSet<MeetingMinute> MeetingMinutes { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

        public DbSet<CommitteeRole> CommitteeRole { get; set; }
        public DbSet<Meeting> Meeting { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<MemberPhoto> MemberPhotos { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("MO");
            //Add a unique index to the Member Email
            modelBuilder.Entity<Member>()
            .HasIndex(a => new { a.Email })
            .IsUnique();

            modelBuilder.Entity<MeetingAttendees>()
            .HasKey(p => new { p.ID });

            //PAC to Academic Divisions
            modelBuilder.Entity<AcademicDivision>()
                .HasMany<PAC>(d => d.PACs)
                .WithOne(p => p.AcademicDivision)
                .HasForeignKey(p => p.AcademicDivisionID)
                .OnDelete(DeleteBehavior.Restrict);

            //Member to Academic Divisions
            modelBuilder.Entity<PAC>()
                .HasMany<Member>(d => d.Members)
                .WithOne(p => p.PAC)
                .HasForeignKey(p => p.PACID)
                .OnDelete(DeleteBehavior.Restrict);

            //Member to Committee
            modelBuilder.Entity<AcademicDivision>()
                .HasMany<Member>(d => d.Members)
                .WithOne(p => p.AcademicDivision)
                .HasForeignKey(p => p.AcademicDivisionID)
                .OnDelete(DeleteBehavior.Restrict);

            //Member to Salution
            modelBuilder.Entity<Salutation>()
                .HasMany<Member>(d => d.Members)
                .WithOne(p => p.Salutation)
                .HasForeignKey(p => p.SalutationID)
                .OnDelete(DeleteBehavior.Restrict);
       
            //Member to Member Renewal
            modelBuilder.Entity<Member>()
                .HasMany<ActionItem>(d => d.ActionItems)
                .WithOne(p => p.Member)
                .HasForeignKey(p => p.MemberID)
                .OnDelete(DeleteBehavior.Restrict);

            //Member to Member Renewal
            modelBuilder.Entity<ActionItem>()
                .HasMany<ActionDocuments>(d => d.ActionDocuments)
                .WithOne(p => p.ActionItem)
                .HasForeignKey(p => p.ActionItemID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }

        public DbSet<NiagaraCollegeProject.Models.AgendaItem> AgendaItem { get; set; }
    }
}