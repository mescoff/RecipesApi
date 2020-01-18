using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipesApi
{
    public class RecipesContext : DbContext
    {
        public RecipesContext(DbContextOptions<RecipesContext> options)
        : base(options)
        {
        }

        public DbSet<RecipeBase> Recipes { get; set; }
        public DbSet<Unit> Units { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RecipeBase>(entity =>
            {
                entity.HasKey(e => e.Recipe_Id);
                entity.Property(e => e.TitleShort).IsRequired();
                entity.Property(e => e.AuditDate).IsRequired();
                entity.Property(e => e.CreationDate).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.LastModifier).IsRequired();
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.HasKey(e => e.Unit_Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Symbol).IsRequired();
            });

            //modelBuilder.Entity<Book>(entity =>
            //{
            //    entity.HasKey(e => e.ISBN);
            //    entity.Property(e => e.Title).IsRequired();
            //    entity.HasOne(d => d.Publisher)
            //      .WithMany(p => p.Books);
            //});
        }
    }
}
