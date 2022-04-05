using System;
using System.Collections.Generic;

namespace Xylab.BricksService.OjUpdate
{
    public class ServiceConstants
    {
        public static IUpdateDriver GetDriver(RecordType category)
        {
            return category switch
            {
                RecordType.Hdoj => new HdojDriver(),
                RecordType.Codeforces => new CodeforcesDriver(),
                RecordType.Vjudge => new VjudgeDriver(),
                _ => throw new NotSupportedException(),
            };
        }

        public static IEnumerable<KeyValuePair<RecordType, IUpdateDriver>> GetDrivers()
        {
            yield return new(RecordType.Hdoj, new HdojDriver());
            yield return new(RecordType.Codeforces, new CodeforcesDriver());
            yield return new(RecordType.Vjudge, new VjudgeDriver());
        }
    }
}
