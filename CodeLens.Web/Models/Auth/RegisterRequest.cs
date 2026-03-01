using System.ComponentModel.DataAnnotations;

namespace CodeLens.Web.Models.Auth;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display name is required.")]
    [MinLength(2, ErrorMessage = "Display name must be at least 2 characters.")]
    [MaxLength(50, ErrorMessage = "Display name must not exceed 50 characters.")]
    public string DisplayName { get; set; } = string.Empty;
}
