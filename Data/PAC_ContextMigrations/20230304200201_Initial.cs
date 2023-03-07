using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NiagaraCollegeProject.Data.PAC_ContextMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicDivisions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DivisionName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicDivisions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    UploadedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Report", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Salutations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalutationTitle = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salutations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeRole",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    CommitteeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeRole", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CommitteeRole_AcademicDivisions_CommitteeID",
                        column: x => x.CommitteeID,
                        principalTable: "AcademicDivisions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAC",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PACName = table.Column<string>(type: "TEXT", nullable: false),
                    AcademicDivisionID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAC", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PAC_AcademicDivisions_AcademicDivisionID",
                        column: x => x.AcademicDivisionID,
                        principalTable: "AcademicDivisions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    EducationSummary = table.Column<string>(type: "TEXT", nullable: true),
                    OccupationalSummary = table.Column<string>(type: "TEXT", nullable: true),
                    StreetAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Province = table.Column<int>(type: "INTEGER", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: true),
                    NCGraduate = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignUpDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReNewDate_ = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RenewalDueBy = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MemberStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyStreetAddress = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyProvince = table.Column<int>(type: "INTEGER", nullable: false),
                    CompanyCity = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyPostalCode = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyPhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyEmail = table.Column<string>(type: "TEXT", nullable: true),
                    PreferredContact = table.Column<int>(type: "INTEGER", nullable: false),
                    CompanyPositionTitle = table.Column<string>(type: "TEXT", nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcademicDivisionID = table.Column<int>(type: "INTEGER", nullable: true),
                    PACID = table.Column<int>(type: "INTEGER", nullable: true),
                    SalutationID = table.Column<int>(type: "INTEGER", nullable: true),
                    CommitteeRoleID = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Members_AcademicDivisions_AcademicDivisionID",
                        column: x => x.AcademicDivisionID,
                        principalTable: "AcademicDivisions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Members_CommitteeRole_CommitteeRoleID",
                        column: x => x.CommitteeRoleID,
                        principalTable: "CommitteeRole",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Members_PAC_PACID",
                        column: x => x.PACID,
                        principalTable: "PAC",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Members_Salutations_SalutationID",
                        column: x => x.SalutationID,
                        principalTable: "Salutations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Meeting",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeetingTopicName = table.Column<string>(type: "TEXT", nullable: false),
                    MeetingStartTimeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MeetingNotes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: true),
                    AcademicDivisionID = table.Column<int>(type: "INTEGER", nullable: true),
                    PACID = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meeting", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Meeting_AcademicDivisions_AcademicDivisionID",
                        column: x => x.AcademicDivisionID,
                        principalTable: "AcademicDivisions",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Meeting_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Meeting_PAC_PACID",
                        column: x => x.PACID,
                        principalTable: "PAC",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "MemberPhotos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberPhotos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MemberPhotos_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PushEndpoint = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PushP256DH = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PushAuth = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionItems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgendaName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Completed = table.Column<bool>(type: "INTEGER", nullable: false),
                    MeetingID = table.Column<int>(type: "INTEGER", nullable: true),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionItems", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ActionItems_Meeting_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "Meeting",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_ActionItems_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingAttendees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeetingID = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingAttendees", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetingAttendees_Meeting_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "Meeting",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingAttendees_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingMinutes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MeetingDiscussion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    MeetingID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingMinutes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetingMinutes_Meeting_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "Meeting",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgendaItem",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgendaName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UploadDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false),
                    MeetingID = table.Column<int>(type: "INTEGER", nullable: false),
                    MeetingMinuteID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaItem", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AgendaItem_Meeting_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "Meeting",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgendaItem_MeetingMinutes_MeetingMinuteID",
                        column: x => x.MeetingMinuteID,
                        principalTable: "MeetingMinutes",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AgendaItem_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFile",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    ActionItemID = table.Column<int>(type: "INTEGER", nullable: true),
                    AgendaID = table.Column<int>(type: "INTEGER", nullable: true),
                    AgendaItemID = table.Column<int>(type: "INTEGER", nullable: true),
                    MeetingID = table.Column<int>(type: "INTEGER", nullable: true),
                    MeetingMinuteID = table.Column<int>(type: "INTEGER", nullable: true),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: true),
                    ReportID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFile", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UploadedFile_ActionItems_ActionItemID",
                        column: x => x.ActionItemID,
                        principalTable: "ActionItems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UploadedFile_AgendaItem_AgendaItemID",
                        column: x => x.AgendaItemID,
                        principalTable: "AgendaItem",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_UploadedFile_Meeting_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "Meeting",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadedFile_MeetingMinutes_MeetingMinuteID",
                        column: x => x.MeetingMinuteID,
                        principalTable: "MeetingMinutes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadedFile_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadedFile_Report_ReportID",
                        column: x => x.ReportID,
                        principalTable: "Report",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileContent",
                columns: table => new
                {
                    FileContentID = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileContent", x => x.FileContentID);
                    table.ForeignKey(
                        name: "FK_FileContent_UploadedFile_FileContentID",
                        column: x => x.FileContentID,
                        principalTable: "UploadedFile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_MeetingID",
                table: "ActionItems",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_ActionItems_MemberID",
                table: "ActionItems",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItem_MeetingID",
                table: "AgendaItem",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItem_MeetingMinuteID",
                table: "AgendaItem",
                column: "MeetingMinuteID");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaItem_MemberID",
                table: "AgendaItem",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeRole_CommitteeID",
                table: "CommitteeRole",
                column: "CommitteeID");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_AcademicDivisionID",
                table: "Meeting",
                column: "AcademicDivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_MemberID",
                table: "Meeting",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Meeting_PACID",
                table: "Meeting",
                column: "PACID");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendees_MeetingID",
                table: "MeetingAttendees",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendees_MemberID",
                table: "MeetingAttendees",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingMinutes_MeetingID",
                table: "MeetingMinutes",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPhotos_MemberID",
                table: "MemberPhotos",
                column: "MemberID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_AcademicDivisionID",
                table: "Members",
                column: "AcademicDivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_CommitteeRoleID",
                table: "Members",
                column: "CommitteeRoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Email",
                table: "Members",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_PACID",
                table: "Members",
                column: "PACID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_SalutationID",
                table: "Members",
                column: "SalutationID");

            migrationBuilder.CreateIndex(
                name: "IX_PAC_AcademicDivisionID",
                table: "PAC",
                column: "AcademicDivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_MemberID",
                table: "Subscriptions",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_ActionItemID",
                table: "UploadedFile",
                column: "ActionItemID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_AgendaItemID",
                table: "UploadedFile",
                column: "AgendaItemID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_MeetingID",
                table: "UploadedFile",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_MeetingMinuteID",
                table: "UploadedFile",
                column: "MeetingMinuteID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_MemberID",
                table: "UploadedFile",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_ReportID",
                table: "UploadedFile",
                column: "ReportID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileContent");

            migrationBuilder.DropTable(
                name: "MeetingAttendees");

            migrationBuilder.DropTable(
                name: "MemberPhotos");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "UploadedFile");

            migrationBuilder.DropTable(
                name: "ActionItems");

            migrationBuilder.DropTable(
                name: "AgendaItem");

            migrationBuilder.DropTable(
                name: "Report");

            migrationBuilder.DropTable(
                name: "MeetingMinutes");

            migrationBuilder.DropTable(
                name: "Meeting");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "CommitteeRole");

            migrationBuilder.DropTable(
                name: "PAC");

            migrationBuilder.DropTable(
                name: "Salutations");

            migrationBuilder.DropTable(
                name: "AcademicDivisions");
        }
    }
}
