using AcxiomCRM.Constants;
using AcxiomCRM.DTOs;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Api
{
    [Authorize(Policy = "CrmUser")]
    [Route("api/leads")]
    public class LeadsApiController : ApiControllerBase
    {
        private readonly ILeadService _leads;

        public LeadsApiController(ILeadService leads)
        {
            _leads = leads;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1)
        {
            var (items, total) = await _leads.GetLeadsAsync(search, status, null, page, 20, CurrentUserId, CurrentRole);
            return Ok(new { total, page, items = items.Select(DtoMapper.ToDto) });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeadWriteDto dto)
        {
            if (!ModelState.IsValid) return ApiError(400, "Validation failed.");
            var lead = new Lead
            {
                LeadName = dto.LeadName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Source = dto.Source,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "New" : dto.Status,
                ExpectedValue = dto.ExpectedValue,
                AssignedToUserId = CurrentRole == AppRoles.SalesExecutive ? CurrentUserId : dto.AssignedToUserId
            };

            var created = await _leads.CreateAsync(lead, CurrentUserId, CurrentUserName, ClientIp);
            return StatusCode(201, DtoMapper.ToDto(created));
        }
    }
}
