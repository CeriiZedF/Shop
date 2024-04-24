using Shop.DAL.Repository.IRepository;
using Shop.DAL.Data;
using Shop.Models;

namespace Shop.DAL.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly DataContext _db;

        public CategoryRepository(DataContext db) : base(db)
        {
            _db = db;
        }

        public async Task Update(Category category)
        {
            var categoryFromDb = await base.FirstOrDefault(x => x.Id == category.Id);
            if (categoryFromDb is not null)
            {
                categoryFromDb.Name = category.Name;
                categoryFromDb.DisplayOrder = category.DisplayOrder;
                _db.Update(categoryFromDb);
            }
        }
    }
}
