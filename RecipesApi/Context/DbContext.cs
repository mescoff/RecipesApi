using Microsoft.EntityFrameworkCore;
using RecipesApi.Models;

namespace RecipesApi
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options)
        : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RecipeCategory> RecipeCategories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe>();
                ;
            modelBuilder.Entity<Category>() ;
            modelBuilder.Entity<RecipeCategory>();
                //.HasKey(rc => new { rc.Category_Id, rc.Recipe_Id });
            //modelBuilder.Entity<RecipeCategory>();
            modelBuilder.Entity<Unit>();
            modelBuilder.Entity<Media>();
            //modelBuilder.Entity<Instruction>();
            modelBuilder.Entity<Ingredient>();
            //modelBuilder.Entity<Ingredient>(entity =>
            //{
            //    entity.HasKey(e => e.RecipeIng_Id);
            //    entity.Property(e => e.Recipe_Id).IsRequired();
            //    entity.Property(e => e.Quantity).IsRequired();
            //    entity.Property(e => e.Name).IsRequired();
            //    entity.Property(e => e.Unit_Id).IsRequired();
            //});

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
