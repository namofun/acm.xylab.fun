using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xylab.BricksService.OjUpdate
{
    public class RecordV1Storage<TContext> : IRecordStorage
        where TContext : DbContext
    {
        private TContext Context { get; }
        
        public RecordV1Storage(TContext context)
        {
            Context = context;
        }

        public async Task<IReadOnlyList<IRecord>> GetAllAsync(RecordType type)
        {
            return await Context.Set<RecordV1>()
                .Where(s => s.Category == type)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task UpdateAsync(IRecord rec, int? result)
        {
            RecordV1 record = (RecordV1)rec;
            return Context.Set<RecordV1>()
                .Where(r => r.Id == record.Id)
                .BatchUpdateAsync(_ => new() { Result = result });
        }

        public Task UpdateAsync(IRecord rec, CreateRecordModel properties)
        {
            RecordV1 record = (RecordV1)rec;
            return Context.Set<RecordV1>()
                .Where(r => r.Id == record.Id)
                .BatchUpdateAsync(_ => new RecordV1
                {
                    Account = properties.Account,
                    Category = properties.Category,
                    NickName = properties.NickName,
                    Grade = properties.Grade,
                    Result = null,
                });
        }

        public Task<List<OjAccount>> ListAsync(RecordType type, int? grade)
        {
            return Context.Set<RecordV1>()
                .Where(s => s.Category == type)
                .WhereIf(grade.HasValue, s => s.Grade == grade)
                .Select(s => new OjAccount(s.Account, s.NickName, s.Result, s.Grade))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<IRecord>> ListAsync()
        {
            return await Context.Set<RecordV1>()
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<IRecord> FindAsync(string id)
        {
            if (int.TryParse(id, out int iid))
            {
                return await Context.Set<RecordV1>()
                    .AsNoTracking()
                    .Where(s => s.Id == iid)
                    .SingleOrDefaultAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task CreateAsync(List<CreateRecordModel> records)
        {
            if (records.Count == 0) return;
            Context.Set<RecordV1>().AddRange(
                records.Select(r => new RecordV1
                {
                    Account = r.Account,
                    Grade = r.Grade,
                    Category = r.Category,
                    NickName = r.NickName,
                }));

            await Context.SaveChangesAsync();
        }

        public Task<int> DeleteAsync(RecordType type, string[] ids)
        {
            List<int> parsedIds = new(capacity: ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                if (int.TryParse(ids[i], out int iid))
                {
                    parsedIds.Add(iid);
                }
            }

            return Context.Set<RecordV1>()
                .Where(r => r.Category == type && parsedIds.Contains(r.Id))
                .BatchDeleteAsync();
        }
    }
}
