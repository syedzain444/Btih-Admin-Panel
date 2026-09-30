using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Helpers;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.Api;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
[AdminPermission(AdminModules.SupportContent)]
public class SupportContentController : Controller
{
    private readonly IApiFactory _apiFactory;

    public SupportContentController(IApiFactory apiFactory) => _apiFactory = apiFactory;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new SupportContentViewModel();
        try
        {
            model.Contact = await _apiFactory.SupportContent.GetContactAsync(cancellationToken)
                ?? new SupportContactApiModel();
            model.Faqs = await _apiFactory.SupportContent.GetFaqsAsync(cancellationToken);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not load help content: {ex.Message}";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveContact(SupportContactApiModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.HospitalName) ||
            string.IsNullOrWhiteSpace(model.Phone) ||
            string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Address) ||
            string.IsNullOrWhiteSpace(model.WorkingHours))
        {
            TempData["Error"] = "All contact fields are required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _apiFactory.SupportContent.UpdateContactAsync(model, cancellationToken);
            TempData["Success"] = "Call Us / Email Us / Visit Us / Operating Hours updated for the mobile app.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not save contact details: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateFaq()
    {
        return View("FaqForm", new FaqFormViewModel { IsActive = true, Category = "General" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFaq(FaqFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ValidateFaq(model))
        {
            return View("FaqForm", model);
        }

        try
        {
            await _apiFactory.SupportContent.CreateFaqAsync(ToApiModel(model), cancellationToken);
            TempData["Success"] = "FAQ created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View("FaqForm", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not create FAQ: {ex.Message}";
            return View("FaqForm", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditFaq(int id, CancellationToken cancellationToken)
    {
        try
        {
            var faqs = await _apiFactory.SupportContent.GetFaqsAsync(cancellationToken);
            var item = faqs.FirstOrDefault(f => f.FaqId == id);
            if (item == null)
            {
                TempData["Error"] = "FAQ not found.";
                return RedirectToAction(nameof(Index));
            }

            return View("FaqForm", new FaqFormViewModel
            {
                FaqId = item.FaqId,
                Category = item.Category,
                QuestionEn = item.QuestionEn,
                AnswerEn = item.AnswerEn,
                QuestionUr = item.QuestionUr,
                AnswerUr = item.AnswerUr,
                SortOrder = item.SortOrder,
                IsActive = item.IsActive,
            });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditFaq(int id, FaqFormViewModel model, CancellationToken cancellationToken)
    {
        model.FaqId = id;
        if (!ValidateFaq(model))
        {
            return View("FaqForm", model);
        }

        try
        {
            await _apiFactory.SupportContent.UpdateFaqAsync(id, ToApiModel(model), cancellationToken);
            TempData["Success"] = "FAQ updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View("FaqForm", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not update FAQ: {ex.Message}";
            return View("FaqForm", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFaq(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiFactory.SupportContent.DeleteFaqAsync(id, cancellationToken);
            TempData["Success"] = "FAQ deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private bool ValidateFaq(FaqFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.QuestionEn))
        {
            ModelState.AddModelError(nameof(model.QuestionEn), "English question is required");
        }

        if (string.IsNullOrWhiteSpace(model.AnswerEn))
        {
            ModelState.AddModelError(nameof(model.AnswerEn), "English answer is required");
        }

        return ModelState.IsValid;
    }

    private static FaqAdminApiModel ToApiModel(FaqFormViewModel model) => new()
    {
        FaqId = model.FaqId ?? 0,
        Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category.Trim(),
        QuestionEn = model.QuestionEn?.Trim() ?? string.Empty,
        AnswerEn = model.AnswerEn?.Trim() ?? string.Empty,
        QuestionUr = string.IsNullOrWhiteSpace(model.QuestionUr) ? null : model.QuestionUr.Trim(),
        AnswerUr = string.IsNullOrWhiteSpace(model.AnswerUr) ? null : model.AnswerUr.Trim(),
        SortOrder = model.SortOrder,
        IsActive = model.IsActive,
    };
}
