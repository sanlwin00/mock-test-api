using AutoMapper;
using MockTestApi.Models;

namespace MockTestApi.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<Subscription, SubscriptionDto>();

        }
    }
}
