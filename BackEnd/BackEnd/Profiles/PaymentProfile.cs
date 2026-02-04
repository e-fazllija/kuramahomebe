using AutoMapper;
using BackEnd.Entities;
using BackEnd.Models.PaymentModels;

namespace BackEnd.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            // Entity -> SelectModel (fallback: se CreationDate/UpdateDate sono default, usa PaymentDate per visualizzazione)
            CreateMap<Payment, PaymentSelectModel>()
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src =>
                    src.CreationDate == default ? src.PaymentDate : src.CreationDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src =>
                    src.UpdateDate == default ? src.PaymentDate : src.UpdateDate));

            // CreateModel -> Entity
            CreateMap<PaymentCreateModel, Payment>();

            // UpdateModel -> Entity
            CreateMap<PaymentUpdateModel, Payment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}

