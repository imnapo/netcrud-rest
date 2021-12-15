using AutoMapper;
using Nv.Example.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nv.Example.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != default));
        }
    }
}
