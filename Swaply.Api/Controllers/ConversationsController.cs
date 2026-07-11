using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swaply.Application.ConversationManagement;
using System.Security.Claims;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationsController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetCurrentUserId();
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetConversationById(Guid id)
    {
        var userId = GetCurrentUserId();
        var conversation = await _conversationService.GetConversationByIdAsync(id, userId);
        if (conversation == null)
            return NotFound(new { message = "Conversation not found or access denied." });
        return Ok(conversation);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetConversationMessages(Guid id)
    {
        var userId = GetCurrentUserId();
        try
        {
            var messages = await _conversationService.GetConversationMessagesAsync(id, userId);
            return Ok(messages);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetCurrentUserId();
        try
        {
            var conversation = await _conversationService.CreateConversationAsync(userId, request);
            return CreatedAtAction(nameof(GetConversationById), new { id = conversation.Id }, conversation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] CreateMessageRequest request)
    {
        var userId = GetCurrentUserId();
        try
        {
            var message = await _conversationService.SendMessageAsync(id, userId, request);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
