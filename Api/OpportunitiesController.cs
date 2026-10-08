using AcxiomCRM.Constants;
using AcxiomCRM.DTOs;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Api
{
    [Authorize(Policy = "CrmUser")]
    [Route("api/opportunities")]
    public class OpportunitiesApiController : ApiControllerBase
    {
        private readonly IOpportunityService _opportunities;

        public OpportunitiesApiController(IOpportunityService opportunities)
        {
            _opportunities = opportunities;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? stage, [FromQuery] string? status, [FromQuery] int page = 1)
        {
            var (items, total) = await _opportunities.GetOpportunitiesAsync(search, stage, status, null, page, 20, CurrentUserId, CurrentRole);
            return Ok(new { total, page, items = items.Select(DtoMapper.ToDto) });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OpportunityWriteDto dto)
        {
            if (!ModelState.IsValid) return ApiError(400, "Validation failed.");
            var errors = OpportunityRules.Validate(dto.Amount, dto.Probability, dto.ExpectedCloseDate, dto.Status);
            if (errors.Count > 0) return ApiError(400, errors[0]);

            var opportunity = new Opportunity
            {
                OpportunityName = dto.OpportunityName,
                CustomerId = dto.CustomerId,
                LeadId = dto.LeadId,
                Amount = dto.Amount,
                Stage = dto.Stage,
                Probability = dto.Probability,
                ExpectedCloseDate = dto.ExpectedCloseDate,
                Status = dto.Status,
                AssignedToUserId = CurrentRole == AppRoles.SalesExecutive ? CurrentUserId : dto.AssignedToUserId,
                Notes = dto.Notes
            };

            try
            {
                var created = await _opportunities.CreateAsync(opportunity, CurrentUserId, CurrentUserName, ClientIp);
                return StatusCode(201, DtoMapper.ToDto(created));
            }
            catch (ArgumentException ex)
            {
                return ApiError(400, ex.Message);
            }
        }
    }
}
