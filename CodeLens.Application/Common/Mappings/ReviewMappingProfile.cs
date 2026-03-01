using AutoMapper;
using CodeLens.Application.DTOs;
using CodeLens.Domain.Entities;

namespace CodeLens.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that defines all mappings from Domain entities to Application DTOs.
/// Registered automatically via <c>AddAutoMapper(Assembly.GetExecutingAssembly())</c>.
/// </summary>
public sealed class ReviewMappingProfile : Profile
{
    /// <summary>Configures all entity-to-DTO mappings for the review domain.</summary>
    public ReviewMappingProfile()
    {
        CreateMap<ReviewIssue, ReviewIssueDto>();

        CreateMap<CodeReview, CodeReviewDto>()
            .ForMember(d => d.IssueCount, opt => opt.MapFrom(s => s.IssueCount));

        CreateMap<CodeReview, CodeReviewDetailDto>()
            .ForMember(d => d.IssueCount, opt => opt.MapFrom(s => s.IssueCount))
            .ForMember(d => d.Issues, opt => opt.MapFrom(s => s.Issues));
    }
}
