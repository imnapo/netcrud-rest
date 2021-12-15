using Nv.Example.Models;
using Nv.AspNetCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Nv.Example.Controllers
{
    public class UserParameters : GetAllQueryStringParameters<User>
    {
        //public int? MinAge { get; set; }
        //public override IQueryable<User> ApplyFilter(IQueryable<User> query)
        //{
        //    if(MinAge.HasValue)
        //    {
        //        //query = query.Where($"(Age == 53 and Name.Contains(\"Elizan\")) or (Age == 19 and Name.Contains(\"Sonia\"))");
        //        //query = query.Where($"UserGames.Any(x => x.Game.Id == 14)");

        //        query = query.Where(x => x.Age >= MinAge);
        //    }
        //    return base.ApplyFilter(query);
        //}
    }
}
