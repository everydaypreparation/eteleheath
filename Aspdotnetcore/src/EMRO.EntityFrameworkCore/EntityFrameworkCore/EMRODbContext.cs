using Abp.Zero.EntityFrameworkCore;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.AuditReport;
using EMRO.Authorization.Roles;
using EMRO.Authorization.Users;
using EMRO.CMSContents;
using EMRO.CommonSetting;
using EMRO.Coupon;
using EMRO.DiagnosticsCases;
using EMRO.DiagnosticsRequestTest;
using EMRO.DoctorRequest;
using EMRO.InternalMessages;
using EMRO.InviteUsers;
using EMRO.Master;
using EMRO.MeetingLogs;
using EMRO.MultiTenancy;
using EMRO.OncologyConsultReports;
using EMRO.Patients.IntakeForm;
using EMRO.Payment;
using EMRO.PayPal;
using EMRO.ReportTypes;
using EMRO.Specialty;
using EMRO.SubSpecialty;
using EMRO.SurveyForm;
using EMRO.UserConsents;
using EMRO.UserCoupon;
using EMRO.UserDocuments;
using EMRO.UserNote;
using EMRO.UsersMetaInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Data;

namespace EMRO.EntityFrameworkCore
{
    public class EMRODbContext : AbpZeroDbContext<Tenant, Role, User, EMRODbContext>
    {
        /* Define a DbSet for each entity of the application */
        //public virtual DbSet<Appointments> Appointments { get; set; }
        //public virtual DbSet<AppointmentParticipants> AppointmentParticipants { get; set; }
        public virtual DbSet<CMSContent> CMSContents { get; set; }

        public virtual DbSet<RequestTest> RequestTests { get; set; }

        //public virtual DbSet<Country> Countries { get; set; }
        //public virtual DbSet<State> States { get; set; }
        //public virtual DbSet<City> Cities { get; set; }
        //public virtual DbSet<PostalCode> PostalCodes { get; set; }
        //public virtual DbSet<Patient> Patients { get; set; }
        //public virtual DbSet<Consultation> Consultations { get; set; }
        //public virtual DbSet<FamilyDoctor> FamilyDoctors { get; set; }
        public virtual DbSet<UserConsent> UserConsents { get; set; }

        //public virtual DbSet<ConsultationReport> ConsultationReports { get; set; }
        public virtual DbSet<ReportType> ReportTypes { get; set; }

        public virtual DbSet<SpecialtyMaster> SpecialtyMasters { get; set; }
        public virtual DbSet<SubSpecialtyMaster> SubSpecialtyMasters { get; set; }

        public virtual DbSet<UserMetaDetails> UserMetaDetails { get; set; }

        public virtual DbSet<DoctorAppointment> DoctorAppointment { get; set; }
        public virtual DbSet<AuditEvent> AuditEvent { get; set; }
        public virtual DbSet<MeetingLogDetails> MeetingLogDetails { get; set; }
        public virtual DbSet<DoctorAppointmentSlot> DoctorAppointmentSlot { get; set; }

        public virtual DbSet<UserConsentPatientsDetails> UserConsentPatientsDetails { get; set; }
       // public virtual DbSet<UserConsentFamilyDoctorsDetails> UserConsentFamilyDoctorsDetails { get; set; }
        public virtual DbSet<UserDocument> UserDocuments { get; set; }
        public virtual DbSet<ConsentFormsMaster> ConsentFormsMasters { get; set; }
        public virtual DbSet<UserConsentForm> UserConsentForms { get; set; }
        public virtual DbSet<UserMessages> UserMessages { get; set; }
        public virtual DbSet<UserMessagesAttachment> UserMessagesAttachments { get; set; }
        public virtual DbSet<UserNotes> UserNotes { get; set; }
        public virtual DbSet<TimeZones> TimeZones { get; set; }
        public virtual DbSet<OncologyConsultReport> OncologyConsultReports { get; set; }
        public virtual DbSet<CouponMaster> CouponMaster { get; set; }
        public virtual DbSet<UserAppointmentPayment> UserAppointmentPayment { get; set; }
        public virtual DbSet<UserCouponTransaction> UserCouponTransaction { get; set; }
        public virtual DbSet<RequestConsultant> RequestConsultants { get; set; }
        public virtual DbSet<InviteUser> InviteUser { get; set; }
        public virtual DbSet<DiagnosticsCase> DiagnosticsCase { get; set; }

        public virtual DbSet<UserAppointmentPayPal> UserAppointmentPayPal { get; set; }
        public virtual DbSet<CostConfiguration> CostConfigurations { get; set; }

        public virtual DbSet<CronHistory> CronHistories { get; set; }
        public virtual DbSet<RoleNotification> RoleNotification { get; set; }
        public virtual DbSet<SurveyQuestionMaster> SurveyQuestionMaster { get; set; }
        public virtual DbSet<SurveyResponse> SurveyResponse { get; set; }

        [DbFunction(Name = "emro_sym_encrypt", Schema = "public")]
        public static byte[] DbSymEncrypt(string t, string Secratkey, string algotype) => throw new NotImplementedException();

        [DbFunction(Name = "emro_sym_decrypt", Schema = "public")]
        public static string DbSymDecrypt(byte[] t, string Secratkey, string algotype) => throw new NotImplementedException();

        private byte[] EncryptMe(string text)
        {
            using (var dbContext = new EMRODbContext(option))
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "emro_sym_encrypt";
                command.Parameters.Add(
                    new Npgsql.NpgsqlParameter("t", NpgsqlTypes.NpgsqlDbType.Text) { Value = text });
                command.Parameters.Add(
                    new Npgsql.NpgsqlParameter("secret", NpgsqlTypes.NpgsqlDbType.Text) { Value = _dataEncryptionSetting.SecretKey });
                command.Parameters.Add(
                    new Npgsql.NpgsqlParameter("algotype", NpgsqlTypes.NpgsqlDbType.Text) { Value = _dataEncryptionSetting.Algotype });
                if (command.Connection.State == ConnectionState.Closed)
                {
                    command.Connection.Open();
                }

                var encrypted = (byte[])command.ExecuteScalar();
                return encrypted;
            }
        }

        private string DecryptMe(byte[] cipher)
        {
            using (var dbContext = new EMRODbContext(option))
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "emro_sym_decrypt";
                command.Parameters.Add(
                    new Npgsql.NpgsqlParameter("t", NpgsqlTypes.NpgsqlDbType.Bytea) { Value = cipher });
                command.Parameters.Add(
                   new Npgsql.NpgsqlParameter("secret", NpgsqlTypes.NpgsqlDbType.Text) { Value = _dataEncryptionSetting.SecretKey });
                command.Parameters.Add(
                   new Npgsql.NpgsqlParameter("algotype", NpgsqlTypes.NpgsqlDbType.Text) { Value = _dataEncryptionSetting.Algotype });
                if (command.Connection.State == ConnectionState.Closed)
                {
                    command.Connection.Open();
                }

                var decrypted = (string)command.ExecuteScalar();
                return decrypted;
            }
        }
        private readonly DbContextOptions<EMRODbContext> option = null;
        private readonly DataEncryptionSetting _dataEncryptionSetting;
        public EMRODbContext(DbContextOptions<EMRODbContext> options)
            : base(options)
        {


        }
        public EMRODbContext(DbContextOptions<EMRODbContext> options, IOptions<DataEncryptionSetting> dataEncryptionSetting)
        : base(options)
        {

            option = options;
            _dataEncryptionSetting = dataEncryptionSetting.Value;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(p => p.UniqueUserId).IsUnique();

            modelBuilder.Entity<User>().Property(p => p.Name).HasConversion(
              val => this.EncryptMe(val),
              val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.Surname).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.Gender).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.PhoneNumber).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.DateOfBirth).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.Address).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.City).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.State).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.Country).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<User>().Property(p => p.PostalCode).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.FirstName).HasConversion(
               val => this.EncryptMe(val),
               val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.LastName).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.Gender).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.TelePhone).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.DateOfBirth).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.Address).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.City).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.State).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.Country).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            modelBuilder.Entity<UserConsentPatientsDetails>().Property(p => p.PostalCode).HasConversion(
                val => this.EncryptMe(val),
                val => this.DecryptMe(val));

            

            base.OnModelCreating(modelBuilder);

            modelBuilder.ChangeAbpTablePrefix<Tenant, Role, User>("");

        }



    }
}
