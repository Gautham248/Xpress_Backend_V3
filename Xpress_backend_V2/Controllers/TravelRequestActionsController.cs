using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xpress_backend_V2.Interface;
using Xpress_backend_V2.Models;
using Xpress_backend_V2.Models.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Xpress_backend_V2.Controllers
{
    [ApiController]
    [Route("api/travelrequests")]
    public class TravelRequestActionsController : ControllerBase
    {
        private readonly ITravelRequestRepo _travelRequestRepo;
        protected APIResponse _response;

        private const int PendingReviewStatusId = 1;
        private const int ModifiedStatusId = 13;

        private static readonly Dictionary<string, string> UserFriendlyFieldNames = new Dictionary<string, string>
        {
            { "TravelModeId", "Travel Mode" },
            { "IsInternational", "International Travel" },
            { "IsRoundTrip", "Round Trip" },
            { "ProjectCode", "Project" },
            { "SourcePlace", "Origin City" },
            { "SourceCountry", "Origin Country" },
            { "DestinationPlace", "Destination City" },
            { "DestinationCountry", "Destination Country" },
            { "OutboundDepartureDate", "Outbound Departure Date" },
            { "OutboundArrivalDate", "Outbound Arrival Date" },
            { "ReturnDepartureDate", "Return Departure Date" },
            { "ReturnArrivalDate", "Return Arrival Date" },
            { "IsAccommodationRequired", "Accommodation Required" },
            { "IsDropOffRequired", "Drop-off Required" },
            { "DropOffPlace", "Drop-off Location" },
            { "IsPickUpRequired", "Pick-up Required" },
            { "PickUpPlace", "Pick-up Location" },
            { "Comments", "General Comments" },
            { "PurposeOfTravel", "Purpose of Travel" },
            { "IsVegetarian", "Vegetarian Preference" },
            { "FoodComment", "Food Preferences" },
            { "AttendedCCT", "Attended CCT Training" },
            { "LDCertificatePath", "L&D Certificate" },
            { "SelectedTicketOptionId", "Selected Ticket Option" },
            { "TravelAgencyName", "Travel Agency" },
            { "TravelAgencyExpense", "Travel Agency Quote" },
            { "TotalExpense", "Total Estimated Expense" },
            { "TicketDocumentPath", "E-Ticket Document" },
            { "AccomodationDocumentPath", "Accommodation Document" },
            { "InsuranceDocumentPath", "Insurance Document" },
            { "TravelFeedback", "Travel Feedback" }
        };

        public TravelRequestActionsController(ITravelRequestRepo travelRequestRepo)
        {
            _travelRequestRepo = travelRequestRepo;
            _response = new APIResponse();
        }

        [HttpPost("{requestId}/edit")]
        [ProducesResponseType(typeof(APIResponse), 200)]
        [ProducesResponseType(typeof(APIResponse), 400)]
        [ProducesResponseType(typeof(APIResponse), 404)]
        [ProducesResponseType(typeof(APIResponse), 500)]
        public async Task<ActionResult<APIResponse>> EditTravelRequest(string requestId, [FromBody] EditedTravelRequestDto editDto)
        {
            try
            {
                var existingRequest = await _travelRequestRepo.GetByIdAsync(requestId);

                if (existingRequest == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add($"Travel request with ID '{requestId}' not found.");
                    return NotFound(_response);
                }

                var changes = new StringBuilder();
                var modificationTime = DateTime.UtcNow;
                var originalStatusId = existingRequest.CurrentStatusId;

                // --- 1. Update fields based on input DTO (EditedTravelRequestDto) ---
                CompareAndLog(changes, nameof(existingRequest.TravelModeId), existingRequest.TravelModeId, editDto.TravelModeId);
                existingRequest.TravelModeId = editDto.TravelModeId;

                CompareAndLog(changes, nameof(existingRequest.IsInternational), existingRequest.IsInternational, editDto.IsInternational);
                existingRequest.IsInternational = editDto.IsInternational;

                CompareAndLog(changes, nameof(existingRequest.IsRoundTrip), existingRequest.IsRoundTrip, editDto.IsRoundTrip);
                existingRequest.IsRoundTrip = editDto.IsRoundTrip;

                CompareAndLog(changes, nameof(existingRequest.ProjectCode), existingRequest.ProjectCode, editDto.ProjectCode);
                existingRequest.ProjectCode = editDto.ProjectCode;

                CompareAndLog(changes, nameof(existingRequest.SourcePlace), existingRequest.SourcePlace, editDto.SourcePlace);
                existingRequest.SourcePlace = editDto.SourcePlace;

                CompareAndLog(changes, nameof(existingRequest.SourceCountry), existingRequest.SourceCountry, editDto.SourceCountry);
                existingRequest.SourceCountry = editDto.SourceCountry;

                CompareAndLog(changes, nameof(existingRequest.DestinationPlace), existingRequest.DestinationPlace, editDto.DestinationPlace);
                existingRequest.DestinationPlace = editDto.DestinationPlace;

                CompareAndLog(changes, nameof(existingRequest.DestinationCountry), existingRequest.DestinationCountry, editDto.DestinationCountry);
                existingRequest.DestinationCountry = editDto.DestinationCountry;

                CompareAndLog(changes, nameof(existingRequest.OutboundDepartureDate), existingRequest.OutboundDepartureDate, editDto.OutboundDepartureDate);
                existingRequest.OutboundDepartureDate = editDto.OutboundDepartureDate.ToUniversalTime();

                CompareAndLog(changes, nameof(existingRequest.OutboundArrivalDate), existingRequest.OutboundArrivalDate, editDto.OutboundArrivalDate);
                existingRequest.OutboundArrivalDate = editDto.OutboundArrivalDate?.ToUniversalTime();

                CompareAndLog(changes, nameof(existingRequest.ReturnDepartureDate), existingRequest.ReturnDepartureDate, editDto.ReturnDepartureDate);
                existingRequest.ReturnDepartureDate = editDto.ReturnDepartureDate?.ToUniversalTime();

                CompareAndLog(changes, nameof(existingRequest.ReturnArrivalDate), existingRequest.ReturnArrivalDate, editDto.ReturnArrivalDate);
                existingRequest.ReturnArrivalDate = editDto.ReturnArrivalDate?.ToUniversalTime();

                CompareAndLog(changes, nameof(existingRequest.IsAccommodationRequired), existingRequest.IsAccommodationRequired, editDto.IsAccommodationRequired);
                existingRequest.IsAccommodationRequired = editDto.IsAccommodationRequired;

                CompareAndLog(changes, nameof(existingRequest.IsDropOffRequired), existingRequest.IsDropOffRequired, editDto.IsDropOffRequired);
                existingRequest.IsDropOffRequired = editDto.IsDropOffRequired;

                CompareAndLog(changes, nameof(existingRequest.DropOffPlace), existingRequest.DropOffPlace, editDto.DropOffPlace);
                existingRequest.DropOffPlace = editDto.DropOffPlace;

                CompareAndLog(changes, nameof(existingRequest.IsPickUpRequired), existingRequest.IsPickUpRequired, editDto.IsPickUpRequired);
                existingRequest.IsPickUpRequired = editDto.IsPickUpRequired;

                CompareAndLog(changes, nameof(existingRequest.PickUpPlace), existingRequest.PickUpPlace, editDto.PickUpPlace);
                existingRequest.PickUpPlace = editDto.PickUpPlace;

                CompareAndLog(changes, nameof(existingRequest.Comments), existingRequest.Comments, editDto.Comments);
                existingRequest.Comments = editDto.Comments;

                CompareAndLog(changes, nameof(existingRequest.PurposeOfTravel), existingRequest.PurposeOfTravel, editDto.PurposeOfTravel);
                existingRequest.PurposeOfTravel = editDto.PurposeOfTravel;

                CompareAndLog(changes, nameof(existingRequest.IsVegetarian), existingRequest.IsVegetarian, editDto.IsVegetarian);
                existingRequest.IsVegetarian = editDto.IsVegetarian;

                CompareAndLog(changes, nameof(existingRequest.FoodComment), existingRequest.FoodComment, editDto.FoodComment);
                existingRequest.FoodComment = editDto.FoodComment;

                CompareAndLog(changes, nameof(existingRequest.LDCertificatePath), existingRequest.LDCertificatePath, editDto.LDCertificatePath);
                existingRequest.LDCertificatePath = editDto.LDCertificatePath;

                CompareAndLog(changes, nameof(existingRequest.AttendedCCT), existingRequest.AttendedCCT, editDto.AttendedCCT);
                if (editDto.AttendedCCT.HasValue)
                {
                    existingRequest.AttendedCCT = editDto.AttendedCCT.Value;
                }
                else
                {
                    if (existingRequest.AttendedCCT != false)
                    {
                        changes.AppendLine($"{GetUserFriendlyFieldName(nameof(existingRequest.AttendedCCT))} was reset to 'No'.");
                    }
                    existingRequest.AttendedCCT = false;
                }

                // --- 2. Explicitly reset fields NOT present in the input DTO ---
                CompareAndLog(changes, nameof(existingRequest.SelectedTicketOptionId), existingRequest.SelectedTicketOptionId, null);
                existingRequest.SelectedTicketOptionId = null;

                CompareAndLog(changes, nameof(existingRequest.TravelAgencyName), existingRequest.TravelAgencyName, null);
                existingRequest.TravelAgencyName = null;

                CompareAndLog(changes, nameof(existingRequest.TravelAgencyExpense), existingRequest.TravelAgencyExpense, null);
                existingRequest.TravelAgencyExpense = null;

                CompareAndLog(changes, nameof(existingRequest.TotalExpense), existingRequest.TotalExpense, null);
                existingRequest.TotalExpense = null;

                CompareAndLog(changes, nameof(existingRequest.TicketDocumentPath), existingRequest.TicketDocumentPath, null);
                existingRequest.TicketDocumentPath = null;

                CompareAndLog(changes, nameof(existingRequest.AccomodationDocumentPath), existingRequest.AccomodationDocumentPath, null);
                existingRequest.AccomodationDocumentPath = null;

                CompareAndLog(changes, nameof(existingRequest.InsuranceDocumentPath), existingRequest.InsuranceDocumentPath, null);
                existingRequest.InsuranceDocumentPath = null;

                CompareAndLog(changes, nameof(existingRequest.TravelFeedback), existingRequest.TravelFeedback, null);
                existingRequest.TravelFeedback = null;

                // --- End of Changes Detection and Application ---

                if (changes.Length == 0 && originalStatusId == PendingReviewStatusId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("No changes were detected in the submitted data, and the request is already in Pending Review status. No action taken.");
                    return BadRequest(_response);
                }
                else if (changes.Length == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("No content changes were detected in the submitted data.");
                    return BadRequest(_response);
                }

                var modificationLog = new AuditLog
                {
                    RequestId = requestId,
                    UserId = existingRequest.UserId,
                    ActionType = "Modified",
                    ActionDate = modificationTime,
                    OldStatusId = originalStatusId,
                    NewStatusId = ModifiedStatusId,
                    ChangeDescription = changes.ToString().TrimEnd(),
                    Timestamp = modificationTime
                };
                _travelRequestRepo.AddAuditLog(modificationLog);

                existingRequest.CurrentStatusId = PendingReviewStatusId;
                existingRequest.UpdatedAt = modificationTime;
                _travelRequestRepo.Update(existingRequest);

                var statusChangeLog = new AuditLog
                {
                    RequestId = requestId,
                    UserId = existingRequest.UserId,
                    ActionType = "Status Change",
                    ActionDate = modificationTime,
                    OldStatusId = ModifiedStatusId,
                    NewStatusId = PendingReviewStatusId,
                    ChangeDescription = "Request resubmitted for review after modification.",
                    Timestamp = modificationTime
                };
                _travelRequestRepo.AddAuditLog(statusChangeLog);

                await _travelRequestRepo.SaveChangesAsync();

                var updatedRequestWithLogs = await _travelRequestRepo.GetByIdAsync(requestId);

                if (updatedRequestWithLogs == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.InternalServerError;
                    _response.ErrorMessages.Add("Failed to retrieve the updated request after saving.");
                    return StatusCode(StatusCodes.Status500InternalServerError, _response);
                }

                var responseDto = new EditedTravelRequestDto
                {
                    RequestId = updatedRequestWithLogs.RequestId,
                    UserId = updatedRequestWithLogs.UserId.ToString(),
                    CurrentStatusId = updatedRequestWithLogs.CurrentStatusId,
                    UpdatedAt = updatedRequestWithLogs.UpdatedAt,

                    TravelModeId = updatedRequestWithLogs.TravelModeId,
                    IsInternational = updatedRequestWithLogs.IsInternational,
                    IsRoundTrip = updatedRequestWithLogs.IsRoundTrip,
                    ProjectCode = updatedRequestWithLogs.ProjectCode,
                    SourcePlace = updatedRequestWithLogs.SourcePlace,
                    SourceCountry = updatedRequestWithLogs.SourceCountry,
                    DestinationPlace = updatedRequestWithLogs.DestinationPlace,
                    DestinationCountry = updatedRequestWithLogs.DestinationCountry,
                    OutboundDepartureDate = updatedRequestWithLogs.OutboundDepartureDate,
                    OutboundArrivalDate = updatedRequestWithLogs.OutboundArrivalDate,
                    ReturnDepartureDate = updatedRequestWithLogs.ReturnDepartureDate,
                    ReturnArrivalDate = updatedRequestWithLogs.ReturnArrivalDate,
                    IsAccommodationRequired = updatedRequestWithLogs.IsAccommodationRequired,
                    IsDropOffRequired = updatedRequestWithLogs.IsDropOffRequired,
                    DropOffPlace = updatedRequestWithLogs.DropOffPlace,
                    IsPickUpRequired = updatedRequestWithLogs.IsPickUpRequired,
                    PickUpPlace = updatedRequestWithLogs.PickUpPlace,
                    Comments = updatedRequestWithLogs.Comments,
                    PurposeOfTravel = updatedRequestWithLogs.PurposeOfTravel,
                    IsVegetarian = updatedRequestWithLogs.IsVegetarian,
                    FoodComment = updatedRequestWithLogs.FoodComment,
                    AttendedCCT = updatedRequestWithLogs.AttendedCCT,
                    LDCertificatePath = updatedRequestWithLogs.LDCertificatePath,

                    AuditLogs = updatedRequestWithLogs.AuditLogs?
                                    .Select(log => new EditedRequestAuditLogDto
                                    {
                                        Id = log.LogId,
                                        ActionType = log.ActionType,
                                        ActionDate = log.ActionDate,
                                        OldStatusId = log.OldStatusId,
                                        NewStatusId = log.NewStatusId,
                                        ChangeDescription = log.ChangeDescription
                                    }).ToList() ?? new List<EditedRequestAuditLogDto>()
                };

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = responseDto;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        private void CompareAndLog<T>(StringBuilder sb, string fieldName, T oldValue, T newValue)
        {
            // Normalize empty strings to null for consistent display of "Not specified"
            if (oldValue is string oldStr && string.IsNullOrEmpty(oldStr)) oldValue = default(T);
            if (newValue is string newStr && string.IsNullOrEmpty(newStr)) newValue = default(T);

            if (!object.Equals(oldValue, newValue))
            {
                string displayFieldName = GetUserFriendlyFieldName(fieldName);
                string oldValFormatted = FormatValueForDisplay(fieldName, oldValue);
                string newValFormatted = FormatValueForDisplay(fieldName, newValue);

                // Determine the most appropriate message for the change
                if (newValue == null) // This covers all "reset" scenarios (e.g., to null)
                {
                    // Removed " (previously: {oldValFormatted})" part
                    sb.AppendLine($"{displayFieldName} was cleared.");
                }
                else if (oldValue == null) // Value was set for the first time or from being cleared
                {
                    sb.AppendLine($"{displayFieldName} was set to '{newValFormatted}'.");
                }
                else // Value changed from one non-null/non-empty to another
                {
                    sb.AppendLine($"{displayFieldName} changed from '{oldValFormatted}' to '{newValFormatted}'.");
                }
            }
        }

        private string GetUserFriendlyFieldName(string internalName)
        {
            return UserFriendlyFieldNames.GetValueOrDefault(internalName, internalName);
        }

        private string FormatValueForDisplay<T>(string fieldName, T value)
        {
            // Normalize empty strings to null for consistent display of "Not specified"
            if (value is string strVal && string.IsNullOrEmpty(strVal))
            {
                value = default(T); // Set to default to trigger the null check below
            }

            if (value == null)
            {
                return "Not specified";
            }

            // Specific handling for boolean values
            if (value is bool boolVal)
            {
                return boolVal ? "Yes" : "No";
            }

            // Specific handling for DateTime values (both nullable and non-nullable)
            if (value is DateTime exactDateTime) // Handles non-nullable DateTime
            {
                return exactDateTime.ToLocalTime().ToString("MMM dd, yyyy h:mm tt");
            }
           

            // Specific handling for document paths (jsonb string arrays)
            if (fieldName.EndsWith("DocumentPath") || fieldName == "LDCertificatePath")
            {
                if (value is string pathString && !string.IsNullOrEmpty(pathString))
                {
                    // If the path contains 'http', it's likely a Cloudinary/URL path indicating an upload
                    if (pathString.Contains("http://") || pathString.Contains("https://"))
                        return "Uploaded";
                    else if (pathString.Length > 0)
                        return "Provided";
                }
                return "No file";
            }

            // Specific handling for expense values (assuming currency)
            if (fieldName == "TravelAgencyExpense" || fieldName == "TotalExpense")
            {
                if (value is decimal decVal)
                {
                    return decVal.ToString("C", CultureInfo.CurrentCulture);
                }
            }

            // Default handling for other types
            return value.ToString();
        }
    }
}