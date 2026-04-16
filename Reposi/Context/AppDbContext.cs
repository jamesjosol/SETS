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
        }
    }
}
