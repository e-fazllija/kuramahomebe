using BackEnd.Entities;
using BackEnd.Models.DocumentsTabModelModels;
using BackEnd.Models.DocumentsTabModels;

namespace BackEnd.Profiles
{
    public class DocumentsTabProfile : AutoMapper.Profile
    {
        public DocumentsTabProfile()
        {
            CreateMap<DocumentsTab, DocumentsTabCreateModel>();
            CreateMap<DocumentsTab, DocumentsTabUpdateModel>();
            CreateMap<DocumentsTab, DocumentsTabSelectModel>();
            CreateMap<DocumentsTabSelectModel, DocumentsTabUpdateModel>();
            CreateMap<DocumentsTabUpdateModel, DocumentsTabSelectModel>();

            CreateMap<DocumentsTabCreateModel, DocumentsTab>();
            CreateMap<DocumentsTabUpdateModel, DocumentsTab>();
            CreateMap<DocumentsTabSelectModel, DocumentsTab>();

        }
    }
}
