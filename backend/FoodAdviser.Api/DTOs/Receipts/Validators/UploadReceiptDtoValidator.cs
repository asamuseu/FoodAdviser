using FluentValidation;
using FoodAdviser.Application.Options;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Api.DTOs.Receipts.Validators;

/// <summary>
/// Validator for <see cref="UploadReceiptDto"/>.
/// </summary>
public sealed class UploadReceiptDtoValidator : AbstractValidator<UploadReceiptDto>
{
    public UploadReceiptDtoValidator(IOptions<StorageOptions> storageOptions)
    {
        var maxBytes = storageOptions.Value.MaxReceiptUploadFileSizeBytes;

        // File must be present
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.");

        // When a file is provided, ensure it's not empty, within size limit, allowed content type and extension
        When(x => x.File != null, () =>
        {
            RuleFor(x => x.File!.Length)
                .GreaterThan(0).WithMessage("File is empty.")
                .LessThanOrEqualTo(maxBytes).WithMessage($"File size exceeds the maximum allowed of {maxBytes} bytes.");

            RuleFor(x => x.File!.ContentType)
                .Must(ct => ct == "image/png" || ct == "image/jpeg")
                .WithMessage("Only PNG or JPEG images are allowed.");

            RuleFor(x => x.File!.FileName)
                .Must(name => HasAllowedExtension(name))
                .WithMessage("Only .png or .jpg/.jpeg file extensions are allowed.");
        });
    }

    private static bool HasAllowedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg";
    }
}
