using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xpress_backend_V2.Data;
using Xpress_backend_V2.Models;
using System.Threading.Tasks;
using Xpress_backend_V2.Interface;

// Define a simple model to receive data from the frontend
public class PublicActionDto
{
    public string ActionType { get; set; }
    public string RequestId { get; set; }
    public string IntendedActorEmail { get; set; }
    public int? OptionId { get; set; } // Optional, for ticket selection
    public string Comments { get; set; } // Optional, for rejection
}

[ApiController]
[Route("api/public-actions")]
[AllowAnonymous] // IMPORTANT: This allows access without a login token
public class PublicActionController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly IAuditLogServices _auditLogService; // Assuming you have this service
    private readonly ILogger<PublicActionController> _logger;


    public PublicActionController(ApiDbContext context, IAuditLogServices auditLogService, ILogger<PublicActionController> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;

    }

    [HttpPost("perform")]
    public async Task<IActionResult> PerformAction([FromBody] PublicActionDto actionDto)
    {
        _logger.LogInformation("Public action received: {Action} for ReqID {ReqId} by {Actor}",
            actionDto.ActionType, actionDto.RequestId, actionDto.IntendedActorEmail);

        var travelRequest = await _context.TravelRequests
            .Include(tr => tr.Project) // Make sure you have a navigation property to RMT
            .FirstOrDefaultAsync(tr => tr.RequestId == actionDto.RequestId);

        if (travelRequest == null)
        {
            return NotFound(new { message = "Travel Request not found." });
        }

        // --- THE MINIMAL SECURITY CHECK ---
        string expectedEmail = null;
        if (actionDto.ActionType.StartsWith("manager-"))
        {
            expectedEmail = travelRequest.Project?.ProjectManagerEmail;
        }
        else if (actionDto.ActionType.StartsWith("duhead-"))
        {
            expectedEmail = travelRequest.Project?.DuHeadEmail;
        }

        if (expectedEmail == null || !expectedEmail.Equals(actionDto.IntendedActorEmail, System.StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SECURITY FAIL: Actor {Actual} tried to perform action '{Action}' on ReqID {ReqId}, but expected actor was {Expected}",
                actionDto.IntendedActorEmail, actionDto.ActionType, actionDto.RequestId, expectedEmail);
            return Forbid("You are not authorized to perform this action for this request.");
        }
        // --- END SECURITY CHECK ---

        // Find the user ID of the actor to log it correctly
        var actorUser = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeEmail == actionDto.IntendedActorEmail);
        if (actorUser == null)
        {
            return BadRequest(new { message = "Acting user not found in the system." });
        }

        // Perform the state change and create the audit log
        // This will trigger your AuditLogHandlerService automatically
        var oldStatusId = travelRequest.CurrentStatusId;
        int newStatusId;
        string auditActionType;

        switch (actionDto.ActionType)
        {
            case "manager-approve":
                newStatusId = 2; // VerifiedByManager
                auditActionType = "ManagerApproved";
                break;
            case "manager-reject":
                newStatusId = 12; // Rejected
                auditActionType = "ManagerRejected";
                break;
            case "duhead-approve":
                newStatusId = 5; // DuApprovedByDuHead
                auditActionType = "DuHeadApproved";
                break;
            case "duhead-reject":
                newStatusId = 12; // Rejected
                auditActionType = "DuHeadRejected";
                break;
            // Add case "select-ticket" if needed
            default:
                return BadRequest(new { message = "Invalid action type." });
        }

        // Update the request
        travelRequest.CurrentStatusId = newStatusId;
        _context.TravelRequests.Update(travelRequest);

        // Create the Audit Log entry
        var auditLog = new AuditLog
        {
            RequestId = travelRequest.RequestId,
            UserId = actorUser.UserId,
            ActionType = auditActionType,
            OldStatusId = oldStatusId,
            NewStatusId = newStatusId,
            Comments = actionDto.Comments ?? string.Empty
        };

        // This will save the changes and create the audit log,
        // which will then be picked up by your AuditLogHandlerService to send the *next* round of emails.
        await _auditLogService.CreateAuditLogAsync(auditLog);

        return Ok(new { message = $"Action '{actionDto.ActionType}' performed successfully!" });
    }
}