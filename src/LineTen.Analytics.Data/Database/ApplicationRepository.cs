using LineTen.DataAccess.EntityFramework.Repository;

namespace LineTen.Analytics.Data.Database
{
    /// <inheritdoc />
    public class ApplicationRepository<TEntity> :Repository<TEntity, ApplicationDbContext> where TEntity : class, new()
    {
        /// <inheritdoc />
        public ApplicationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
