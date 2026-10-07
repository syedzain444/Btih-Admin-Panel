using HospitalAdminPanel.Configuration;
using HospitalAdminPanel.Factories;
using HospitalAdminPanel.Helpers;
using HospitalAdminPanel.Middleware;
using HospitalAdminPanel.Models.ViewModels;
using HospitalAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HospitalAdminPanel.Controllers;

[AdminAuthorize]
[AdminPermission(AdminModules.Offers)]
public class OffersController : Controller
{
    private readonly IApiFactory _apiFactory;
    private readonly ApiSettings _apiSettings;

    public OffersController(IApiFactory apiFactory, IOptions<ApiSettings> apiSettings)
    {
        _apiFactory = apiFactory;
        _apiSettings = apiSettings.Value;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var offers = await _apiFactory.Offers.GetAllAsync(cancellationToken);
            return View(new OfferListViewModel
            {
                Offers = offers,
                ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
            });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new OfferListViewModel
            {
                ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not load offers: {ex.Message}";
            return View(new OfferListViewModel
            {
                ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
            });
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new OfferFormViewModel
        {
            IsActive = true,
            Category = "Package",
            Currency = "PKR",
            CtaLabel = "Enquire",
            ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OfferFormViewModel model, IFormFile? image, CancellationToken cancellationToken)
    {
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
            await _apiFactory.Offers.CreateAsync(content, cancellationToken);
            TempData["Success"] = "Offer / package created.";
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
            TempData["Error"] = $"Could not create offer: {ex.Message}";
            model.ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/');
            return View("Form", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var all = await _apiFactory.Offers.GetAllAsync(cancellationToken);
            var item = all.FirstOrDefault(o => o.OfferId == id);
            if (item == null)
            {
                TempData["Error"] = "Offer not found.";
                return RedirectToAction(nameof(Index));
            }

            return View("Form", new OfferFormViewModel
            {
                OfferId = item.OfferId,
                Title = item.Title,
                Subtitle = item.Subtitle,
                Description = item.Description,
                Category = item.Category,
                OriginalPrice = item.OriginalPrice,
                OfferPrice = item.OfferPrice,
                Currency = item.Currency,
                Highlights = item.Highlights,
                CtaLabel = item.CtaLabel,
                CtaPhone = item.CtaPhone,
                SortOrder = item.SortOrder,
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
    public async Task<IActionResult> Edit(int id, OfferFormViewModel model, IFormFile? image, CancellationToken cancellationToken)
    {
        model.OfferId = id;
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
            await _apiFactory.Offers.UpdateAsync(id, content, cancellationToken);
            TempData["Success"] = "Offer / package updated.";
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
            TempData["Error"] = $"Could not update offer: {ex.Message}";
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
            await _apiFactory.Offers.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "Offer deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private static async Task<MultipartFormDataContent> BuildMultipartAsync(
        OfferFormViewModel model,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(model.Title?.Trim() ?? string.Empty), "title" },
            { new StringContent(model.Subtitle?.Trim() ?? string.Empty), "subtitle" },
            { new StringContent(model.Description?.Trim() ?? string.Empty), "description" },
            { new StringContent(string.IsNullOrWhiteSpace(model.Category) ? "Package" : model.Category.Trim()), "category" },
            { new StringContent(model.OriginalPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty), "originalPrice" },
            { new StringContent(model.OfferPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty), "offerPrice" },
            { new StringContent(string.IsNullOrWhiteSpace(model.Currency) ? "PKR" : model.Currency.Trim()), "currency" },
            { new StringContent(model.Highlights?.Trim() ?? string.Empty), "highlights" },
            { new StringContent(string.IsNullOrWhiteSpace(model.CtaLabel) ? "Enquire" : model.CtaLabel.Trim()), "ctaLabel" },
            { new StringContent(model.CtaPhone?.Trim() ?? string.Empty), "ctaPhone" },
            { new StringContent(model.SortOrder.ToString()), "sortOrder" },
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
