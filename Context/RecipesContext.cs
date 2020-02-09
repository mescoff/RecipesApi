using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;

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
        public DbSet<Ingredient> Ingredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RecipeBase>();

            modelBuilder.Entity<Unit>();

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(e => e.RecipeIng_Id);
                entity.Property(e => e.Recipe_Id).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Unit_Id).IsRequired();
            });
            //modelBuilder.Entity<Ingredient>().HasOne(d => d.Unit);

            //modelBuilder.Entity<RecipeBase>(

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
