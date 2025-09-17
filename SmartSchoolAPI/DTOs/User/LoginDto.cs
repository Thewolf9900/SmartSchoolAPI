﻿using System.ComponentModel.DataAnnotations;

namespace SmartSchoolAPI.DTOs.User
{
    public class LoginDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        public string Password { get; set; } = string.Empty;
    }
}