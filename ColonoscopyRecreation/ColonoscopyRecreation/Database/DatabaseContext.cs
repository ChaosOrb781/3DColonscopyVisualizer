using ColonoscopyRecreation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.Database
{
    public class DatabaseContext : DbContext
    {
        public string SQLiteDatabasePath { get; set; }
        public DbSet<Frame> Frames { get; set; }
        public DbSet<Video> Videos { get; set; }

        public DatabaseContext(string databaspath) 
        { 
            SQLiteDatabasePath = databaspath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={SQLiteDatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var videomodel = modelBuilder.Entity<Video>();
            videomodel.HasKey(v => v.Id);
            videomodel.Ignore(v => v.Mask);
            videomodel.HasMany(v => v.Frames)
                .WithOne(f => f.Video)
                .HasForeignKey(f => f.VideoId)
                .OnDelete(DeleteBehavior.Cascade);
            videomodel.Property(v => v.Width).IsRequired();
            videomodel.Property(v => v.Height).IsRequired();


            var framemodel = modelBuilder.Entity<Frame>();
            framemodel.HasKey(f => f.Id);
            framemodel.HasOne(f => f.Video)
                .WithMany(v => v.Frames)
                .OnDelete(DeleteBehavior.NoAction);
            framemodel.HasMany(f => f.KeyPoints)
                .WithOne(k => k.Frame)
                .OnDelete(DeleteBehavior.Cascade);
            framemodel.Property(f => f.FrameIndex).IsRequired();
            framemodel.Property(f => f.Content).IsRequired();


            var keypointmodel = modelBuilder.Entity<KeyPoint>();
            keypointmodel.HasKey(k => k.Id);
            keypointmodel.HasOne(k => k.Frame)
                .WithMany(f => f.KeyPoints)
                .OnDelete(DeleteBehavior.NoAction);
            keypointmodel.Property(k => k.X).IsRequired();
            keypointmodel.Property(k => k.Y).IsRequired();
            keypointmodel.Property(k => k.Octave).IsRequired();
            keypointmodel.Property(k => k.Size).IsRequired();
            keypointmodel.Property(k => k.Angle).IsRequired();
            keypointmodel.Property(k => k.ClassId).IsRequired();
            keypointmodel.Property(k => k.Response).IsRequired();
            keypointmodel.Property(k => k.Descriptors).IsRequired();
        }
    }
}
