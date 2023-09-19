using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendancePuller
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Attendance>().ToTable("Attendance");
        //    modelBuilder.Entity<Attendance>().Property(a => a.Id).HasColumnName("AttendanceId");

        //    // Add any additional configuration for the Attendance entity

        //    base.OnModelCreating(modelBuilder);
        //}

        public DbSet<Attendance> Attendances { get; set; }
    }
}
