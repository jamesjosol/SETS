using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Model.SETSDB;

namespace Reposi.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User_Master> User_Master { get; set; }
        public DbSet<Branch_Master> Branch_Master { get; set; }
        public DbSet<Section_Master> Section_Master { get; set; }
        public DbSet<User_Section> User_Section { get; set; }
        public DbSet<PC_Master> PC_Master { get; set; }
        public DbSet<PC_Section> PC_Section { get; set; }
        public DbSet<Sample_Type> Sample_Type { get; set; }
        public DbSet<Batch_Header> Batch_Header { get; set; }
        public DbSet<Batch_Specimen> Batch_Specimen { get; set; }
        public DbSet<Batch_NonBarcoded> Batch_NonBarcoded { get; set; }
        public DbSet<Batch_Specimen_Receiving> Batch_Specimen_Receiving { get; set; }
        public DbSet<Section_TestGroup> Section_TestGroup { get; set; }
        public DbSet<Specimen_Section_Header> Specimen_Section_Header { get; set; }
        public DbSet<Specimen_Section_Test> Specimen_Section_Test { get; set; }
        public DbSet<Test_Group> Test_Group { get; set; }
        public DbSet<Test_RunningDay> Test_RunningDay { get; set; }
        public DbSet<Tat_Section> Tat_Section { get; set; }
        public DbSet<Tat_Cycle_Log> Tat_Cycle_Log { get; set; }
        public DbSet<OnSite_Section_Header> OnSite_Section_Header { get; set; }
        public DbSet<OnSite_Section_Test> OnSite_Section_Test { get; set; }
        public DbSet<OnSite_AllowedLabNo> OnSite_AllowedLabNo { get; set; }
        public DbSet<OnSite_Settings> OnSite_Settings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User_Master
            modelBuilder.Entity<User_Master>().ToTable("User_Master");
            modelBuilder.Entity<User_Master>().HasKey(u => u.UserID);

            // Branch_Master
            modelBuilder.Entity<Branch_Master>().ToTable("Branch_Master");
            modelBuilder.Entity<Branch_Master>().HasKey(b => b.Code);

            // Section_Master
            modelBuilder.Entity<Section_Master>().ToTable("Section_Master");
            modelBuilder.Entity<Section_Master>().HasKey(s => s.Code);
            modelBuilder.Entity<Section_Master>()
                .HasOne<Branch_Master>()
                .WithMany()
                .HasForeignKey(s => s.BranchCode);

            // User_Section
            modelBuilder.Entity<User_Section>().ToTable("User_Section");
            modelBuilder.Entity<User_Section>().HasKey(us => us.Id);
            modelBuilder.Entity<User_Section>()
                .HasIndex(us => new { us.UserID, us.SectionCode })
                .IsUnique();
            modelBuilder.Entity<User_Section>()
                .HasOne<User_Master>()
                .WithMany()
                .HasForeignKey(us => us.UserID);
            modelBuilder.Entity<User_Section>()
                .HasOne<Section_Master>()
                .WithMany()
                .HasForeignKey(us => us.SectionCode);

            // PC_Master
            modelBuilder.Entity<PC_Master>().ToTable("PC_Master");
            modelBuilder.Entity<PC_Master>().HasKey(p => p.Id);

            // PC_Section
            modelBuilder.Entity<PC_Section>().ToTable("PC_Section");
            modelBuilder.Entity<PC_Section>().HasKey(ps => ps.Id);
            modelBuilder.Entity<PC_Section>()
                .HasOne<PC_Master>()
                .WithMany()
                .HasForeignKey(ps => ps.PCId);
            modelBuilder.Entity<PC_Section>()
                .HasOne<Section_Master>()
                .WithMany()
                .HasForeignKey(ps => ps.SectionCode);

            // Sample_Type
            modelBuilder.Entity<Sample_Type>().ToTable("Sample_Type");
            modelBuilder.Entity<Sample_Type>().HasKey(s => s.Code);

            // Batch_Header
            modelBuilder.Entity<Batch_Header>().ToTable("Batch_Header");
            modelBuilder.Entity<Batch_Header>().HasKey(b => b.BatchNo);

            // Batch_Specimen
            modelBuilder.Entity<Batch_Specimen>().ToTable("Batch_Specimen");
            modelBuilder.Entity<Batch_Specimen>().HasKey(b => b.Id);
            modelBuilder.Entity<Batch_Specimen>()
                .HasOne<Batch_Header>()
                .WithMany()
                .HasForeignKey(b => b.BatchNo);

            // Batch_NonBarcoded
            modelBuilder.Entity<Batch_NonBarcoded>().ToTable("Batch_NonBarcoded");
            modelBuilder.Entity<Batch_NonBarcoded>().HasKey(b => b.ItemID);
            modelBuilder.Entity<Batch_NonBarcoded>()
                .HasOne<Batch_Header>()
                .WithMany()
                .HasForeignKey(b => b.BatchNo);

            // Ref_Codes
            modelBuilder.Entity<Ref_Codes>().ToTable("Ref_Codes");
            modelBuilder.Entity<Ref_Codes>().HasKey(r => r.No);

            // Batch_Specimen_Receiving
            modelBuilder.Entity<Batch_Specimen_Receiving>().ToTable("Batch_Specimen_Receiving");
            modelBuilder.Entity<Batch_Specimen_Receiving>().HasKey(b => b.Id);
            modelBuilder.Entity<Batch_Specimen_Receiving>()
                .HasIndex(b => new { b.SpecimenNo, b.BatchNo })
                .IsUnique();
            modelBuilder.Entity<Batch_Specimen_Receiving>()
                .HasOne<Batch_Header>()
                .WithMany()
                .HasForeignKey(b => b.BatchNo);

            // Section_TestGroup
            modelBuilder.Entity<Section_TestGroup>().ToTable("Section_TestGroup");
            modelBuilder.Entity<Section_TestGroup>().HasKey(s => s.Id);
            modelBuilder.Entity<Section_TestGroup>()
                .HasIndex(s => new { s.SectionCode, s.TestGroupCode })
                .IsUnique();
            modelBuilder.Entity<Section_TestGroup>()
                .HasOne<Section_Master>()
                .WithMany()
                .HasForeignKey(s => s.SectionCode);

            // Specimen_Section_Header
            modelBuilder.Entity<Specimen_Section_Header>().ToTable("Specimen_Section_Header");
            modelBuilder.Entity<Specimen_Section_Header>().HasKey(s => s.Id);
            modelBuilder.Entity<Specimen_Section_Header>()
                .HasIndex(s => new { s.SpecimenNo, s.TestGroupCode })
                .IsUnique();
            modelBuilder.Entity<Specimen_Section_Header>()
                .HasOne<Section_Master>()
                .WithMany()
                .HasForeignKey(s => s.SectionCode);

            // Specimen_Section_Test
            modelBuilder.Entity<Specimen_Section_Test>().ToTable("Specimen_Section_Test");
            modelBuilder.Entity<Specimen_Section_Test>().HasKey(s => s.Id);
            modelBuilder.Entity<Specimen_Section_Test>()
                .HasIndex(s => new { s.HeaderId, s.TestCode })
                .IsUnique();
            modelBuilder.Entity<Specimen_Section_Test>()
                .HasOne<Specimen_Section_Header>()
                .WithMany()
                .HasForeignKey(s => s.HeaderId);

            // Test_Group
            modelBuilder.Entity<Test_Group>().ToTable("Test_Group");
            modelBuilder.Entity<Test_Group>().HasKey(r => r.Code);

            // Test_RunningDay
            modelBuilder.Entity<Test_RunningDay>().ToTable("Test_RunningDay");
            modelBuilder.Entity<Test_RunningDay>().HasKey(t => t.Id);
            modelBuilder.Entity<Test_RunningDay>()
                .HasIndex(t => t.TestCode)
                .IsUnique();

            // Tat_Section
            modelBuilder.Entity<Tat_Section>().ToTable("Tat_Section");
            modelBuilder.Entity<Tat_Section>().HasKey(r => r.SectionCode);

            // Tat_Cycle_Log
            modelBuilder.Entity<Tat_Cycle_Log>().ToTable("Tat_Cycle_Log");
            modelBuilder.Entity<Tat_Cycle_Log>().HasKey(r => r.Id);

            // OnSite_Section_Header
            modelBuilder.Entity<OnSite_Section_Header>(entity =>
            {
                entity.ToTable("OnSite_Section_Header");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SpecimenNo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SectionCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TestGroupCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.SampleTypeCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PatientName).HasMaxLength(200);
                entity.Property(e => e.PID).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(5);
                entity.Property(e => e.Remarks).HasMaxLength(200);
                entity.Property(e => e.ReceivedBy).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            });

            // OnSite_Section_Test
            modelBuilder.Entity<OnSite_Section_Test>(entity =>
            {
                entity.ToTable("OnSite_Section_Test");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TestCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TestName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(5);
                entity.Property(e => e.ScheduleTag).HasMaxLength(10);
                entity.Property(e => e.AssignedRMT).HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).HasMaxLength(50);
                entity.HasOne<OnSite_Section_Header>()
                      .WithMany()
                      .HasForeignKey(e => e.HeaderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // OnSite_AllowedLabNo
            modelBuilder.Entity<OnSite_AllowedLabNo>(entity =>
            {
                entity.ToTable("OnSite_AllowedLabNo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Prefix).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
            });

            // OnSite_Settings
            modelBuilder.Entity<OnSite_Settings>(entity =>
            {
                entity.ToTable("OnSite_Settings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            });

        }
    }
}
