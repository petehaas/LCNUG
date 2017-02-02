namespace TacoBot.Services
{
    using System;
    using System.Collections.Generic;

    public interface IRepository<T>
    {
        PagedResult<T> RetrievePage(int pageNumber, int pageSize, Func<T, bool> predicate = default(Func<T, bool>));

        T GetByName(string name);

        T GetByID(int ids);

        IEnumerable<T> GetAll();
    }
}