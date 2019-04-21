using AutoMapper;
using PlevenLab.Data.DTO;
using PlevenLab.Data.Entities;

namespace PlevenLab.Data
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();

            CreateMap<Event, EventDTO>();
            CreateMap<EventDTO, Event>();
        }
    }
}
