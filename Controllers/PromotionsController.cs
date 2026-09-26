using HospitalAdminPanel.Configuration;
using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
public class PromotionsController : Controller
{
    private readonly IApiFactory _apiFactory;
    private readonly ApiSettings _apiSettings;

    public PromotionsController(IApiFactory apiFactory, IOptions<ApiSettings> apiSettings)
    {
        _apiFactory = apiFactory;
        _apiSettings = apiSettings.Value;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await _apiFactory.Promotions.GetAllAsync(cancellationToken);
            return View(new PromotionListViewModel
            {
                Promotions = promotions,
                ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
            });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new PromotionListViewModel
            {
                ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not load promotions: {ex.Message}";
            return View(new PromotionListViewModel
            {
                ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
            });
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new PromotionFormViewModel
        {
            IsActive = true,
            DurationSeconds = 5,
            ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PromotionFormViewModel model, IFormFile? image, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ModelState.AddModelError(nameof(model.Title), "Title is required");
        }

        if (image == null || image.Length == 0)
        {
            ModelState.AddModelError("image", "Promotion image is required");
        }

        if (!ModelState.IsValid)
        {
            model.ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/');
            return View("Form", model);
        }

        try
        {
            var content = await BuildMultipartAsync(model, image, cancellationToken);
            await _apiFactory.Promotions.CreateAsync(content, cancellationToken);
            TempData["Success"] = "Promotion created.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            model.ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/');
            return View("Form", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not create promotion: {ex.Message}";
            model.ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/');
            return View("Form", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var all = await _apiFactory.Promotions.GetAllAsync(cancellationToken);
            var item = all.FirstOrDefault(p => p.PromotionId == id);
            if (item == null)
            {
                TempData["Error"] = "Promotion not found.";
                return RedirectToAction(nameof(Index));
            }

            return View("Form", new PromotionFormViewModel
            {
                PromotionId = item.PromotionId,
                Title = item.Title,
                SortOrder = item.SortOrder,
                DurationSeconds = item.DurationSeconds,
                IsActive = item.IsActive,
                StartAt = item.StartAt,
                EndAt = item.EndAt,
                ExistingImageUrl = item.ImageUrl,
                ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
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
    public async Task<IActionResult> Edit(int id, PromotionFormViewModel model, IFormFile? image, CancellationToken cancellationToken)
    {
        model.PromotionId = id;
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            ModelState.AddModelError(nameof(model.Title), "Title is required");
        }

        if (!ModelState.IsValid)
        {
            model.ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/');
            return View("Form", model);
        }

        try
        {
            var content = await BuildMultipartAsync(model, image, cancellationToken);
            await _apiFactory.Promotions.UpdateAsync(id, content, cancellationToken);
            TempData["Success"] = "Promotion updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            model.ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/');
            return View("Form", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not update promotion: {ex.Message}";
            model.ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/');
            return View("Form", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _apiFactory.Promotions.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Promotion deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private static async Task<MultipartFormDataContent> BuildMultipartAsync(
        PromotionFormViewModel model,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(model.Title?.Trim() ?? string.Empty), "title" },
            { new StringContent(model.SortOrder.ToString()), "sortOrder" },
            { new StringContent((model.DurationSeconds <= 0 ? 5 : model.DurationSeconds).ToString()), "durationSeconds" },
            { new StringContent(model.IsActive ? "true" : "false"), "isActive" },
        };

        if (model.StartAt.HasValue)
        {
            content.Add(new StringContent(model.StartAt.Value.ToString("o")), "startAt");
        }

        if (model.EndAt.HasValue)
        {
            content.Add(new StringContent(model.EndAt.Value.ToString("o")), "endAt");
        }

        if (image != null && image.Length > 0)
        {
            using var stream = image.OpenReadStream();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, cancellationToken);
            var fileContent = new ByteArrayContent(buffer.ToArray());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(image.ContentType) ? "application/octet-stream" : image.ContentType);
            content.Add(fileContent, "image", image.FileName);
        }

        return content;
    }
}
