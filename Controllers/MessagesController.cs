using HospitalAdminPanel.Factories;

using HospitalAdminPanel.Helpers;

using HospitalAdminPanel.Middleware;

using HospitalAdminPanel.Models.Api;

using HospitalAdminPanel.Models.ViewModels;

using HospitalAdminPanel.Services;

using Microsoft.AspNetCore.Mvc;



namespace HospitalAdminPanel.Controllers;



[AdminAuthorize]

public class MessagesController : Controller

{

    private readonly IApiFactory _apiFactory;

    private readonly ITokenSessionService _tokenSession;



    public MessagesController(IApiFactory apiFactory, ITokenSessionService tokenSession)

    {

        _apiFactory = apiFactory;

        _tokenSession = tokenSession;

    }



    public async Task<IActionResult> Index(string? search, string? status, CancellationToken cancellationToken)

    {

        var filtered = FilterThreads(await _apiFactory.Messages.GetThreadsAsync(cancellationToken), search, status);



        ViewData["MsgCount"] = filtered.AllCount;

        return View(new MessageListViewModel

        {

            Threads = filtered.Threads,

            Search = search,

            StatusFilter = status,

            AllCount = filtered.AllCount,

            UnreadCount = filtered.UnreadCount,

            MissingThreadMessagesApi = false,

        });

    }



    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)

    {

        var threads = await _apiFactory.Messages.GetThreadsAsync(cancellationToken);

        var thread = threads.FirstOrDefault(t => t.ThreadId == id);

        if (thread == null)

        {

            return NotFound();

        }



        var messages = await _apiFactory.Messages.GetThreadMessagesAsync(id, cancellationToken);

        var filtered = FilterThreads(threads, null, null);

        PatientProfileApiModel? profile = null;

        try { profile = await _apiFactory.Users.GetByMrNoAsync(thread.MrNo, cancellationToken); }
        catch (Exception) { /* patient profile is optional */ }



        return View(new MessageReplyViewModel

        {

            Thread = thread,

            Threads = filtered.Threads,

            Messages = messages,

            PatientProfile = profile,

            AllCount = filtered.AllCount,

            UnreadCount = filtered.UnreadCount,

            StaffName = _tokenSession.GetUser()?.DisplayName ?? _tokenSession.GetUser()?.Username,

        });

    }



    [HttpPost]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Reply(MessageReplyViewModel model, CancellationToken cancellationToken)

    {

        if (model.Thread.ThreadId <= 0)

        {

            if (IsAjaxRequest())

            {

                return BadRequest(new { success = false, error = "Invalid message thread." });

            }



            model.ErrorMessage = "Invalid message thread.";

            model.Messages = await _apiFactory.Messages.GetThreadMessagesAsync(model.Thread.ThreadId, cancellationToken);

            model.Threads = (await _apiFactory.Messages.GetThreadsAsync(cancellationToken)).OrderByDescending(t => t.UpdatedAt).ToList();

            return View("Details", model);

        }



        if (string.IsNullOrWhiteSpace(model.Body))

        {

            if (IsAjaxRequest())

            {

                return BadRequest(new { success = false, error = "Message body is required." });

            }



            model.ErrorMessage = "Message body is required.";

            model.Messages = await _apiFactory.Messages.GetThreadMessagesAsync(model.Thread.ThreadId, cancellationToken);

            model.Threads = (await _apiFactory.Messages.GetThreadsAsync(cancellationToken)).OrderByDescending(t => t.UpdatedAt).ToList();

            return View("Details", model);

        }



        try

        {

            var sent = await _apiFactory.Messages.ReplyAsync(new AdminReplyRequest

            {

                ThreadId = model.Thread.ThreadId,

                Body = model.Body.Trim(),

                StaffName = model.StaffName,

            }, cancellationToken);



            if (IsAjaxRequest())

            {

                return Json(new

                {

                    success = true,

                    message = sent ?? new MessageItemApiModel

                    {

                        ThreadId = model.Thread.ThreadId,

                        Body = model.Body.Trim(),

                        SenderName = model.StaffName,

                        SenderType = "STAFF",

                        CreatedAt = DateTime.Now,

                    },

                });

            }



            return RedirectToAction(nameof(Details), new { id = model.Thread.ThreadId });

        }

        catch (ApiException ex)

        {

            if (IsAjaxRequest())

            {

                return BadRequest(new { success = false, error = ex.Message });

            }



            model.ErrorMessage = ex.Message;

            model.Messages = await _apiFactory.Messages.GetThreadMessagesAsync(model.Thread.ThreadId, cancellationToken);

            model.Threads = (await _apiFactory.Messages.GetThreadsAsync(cancellationToken)).OrderByDescending(t => t.UpdatedAt).ToList();

            return View("Details", model);

        }

    }



    [HttpGet]

    public async Task<IActionResult> Poll(int id, int afterMessageId, CancellationToken cancellationToken)

    {

        var messages = await _apiFactory.Messages.GetThreadMessagesAsync(id, cancellationToken);

        var incoming = messages

            .Where(m => m.MessageId > afterMessageId)

            .OrderBy(m => m.CreatedAt)

            .ToList();



        return Json(new { success = true, messages = incoming });

    }



    private bool IsAjaxRequest() =>

        string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);



    private static (List<MessageThreadApiModel> Threads, int AllCount, int UnreadCount) FilterThreads(

        List<MessageThreadApiModel> threads,

        string? search,

        string? status)

    {

        IEnumerable<MessageThreadApiModel> query = threads;



        if (!string.IsNullOrWhiteSpace(search))

        {

            query = query.Where(t =>

                t.Subject.Contains(search, StringComparison.OrdinalIgnoreCase) ||

                t.MrNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||

                (t.LastMessagePreview?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

        }



        var searched = query.ToList();

        var allCount = searched.Count;

        var unreadCount = searched.Count(t => t.UnreadCount > 0);



        if (string.Equals(status, "UNREAD", StringComparison.OrdinalIgnoreCase))

        {

            searched = searched.Where(t => t.UnreadCount > 0).ToList();

        }

        else if (!string.IsNullOrWhiteSpace(status))

        {

            searched = searched.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

        }



        return (searched.OrderByDescending(t => t.UpdatedAt).ToList(), allCount, unreadCount);

    }

}


