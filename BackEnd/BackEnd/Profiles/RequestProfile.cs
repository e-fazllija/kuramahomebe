using BackEnd.Entities;
using BackEnd.Models.RequestModels;

namespace BackEnd.Profiles
{
    public class RequestProfile : AutoMapper.Profile
    {
        public RequestProfile()
        {
            CreateMap<Request, RequestCreateModel>();
            CreateMap<Request, RequestUpdateModel>();
            CreateMap<Request, RequestSelectModel>();
            CreateMap<RequestSelectModel, RequestUpdateModel>();
            CreateMap<RequestUpdateModel, RequestSelectModel>();

            CreateMap<RequestCreateModel, Request>();
            CreateMap<RequestUpdateModel, Request>();
            CreateMap<RequestSelectModel, Request>();

        }
    }
}
