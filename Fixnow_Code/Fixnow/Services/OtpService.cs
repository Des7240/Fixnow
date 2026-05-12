using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Services;

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;

    public OtpService(AppDbContext context, IEmailService emailService, ILogger<OtpService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(string email, OtpType type, string purpose)
    {
        // 1. Mark old OTPs as used/invalid for this email and type
        var oldOtps = await _context.OtpCodes
            .Where(o => o.Email == email && o.Type == type && !o.IsUsed)
            .ToListAsync();
        
        foreach (var old in oldOtps)
        {
            old.IsUsed = true;
        }

        // 2. Generate new 6-digit OTP
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();

        // 3. Save to database
        var otpEntry = new OtpCode
        {
            Email = email,
            Code = code,
            Type = type,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _context.OtpCodes.Add(otpEntry);
        await _context.SaveChangesAsync();

        // 4. Send Email
        await _emailService.SendOtpEmailAsync(email, code, purpose);

        _logger.LogInformation("Generated OTP for {Email}, Purpose: {Purpose}", email, purpose);
        return code;
    }

    public async Task<bool> VerifyOtpAsync(string email, string code, OtpType type, bool markAsUsed = true)
    {
        var otpEntry = await _context.OtpCodes
            .Where(o => o.Email == email && o.Code == code && o.Type == type && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpEntry == null)
        {
            _logger.LogWarning("OTP verification failed for {Email}: Invalid code or used", email);
            return false;
        }

        if (otpEntry.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("OTP verification failed for {Email}: Expired", email);
            return false;
        }

        if (markAsUsed)
        {
            // Mark as used
            otpEntry.IsUsed = true;
            await _context.SaveChangesAsync();
            _logger.LogInformation("OTP verified and marked as used for {Email}", email);
        }
        else
        {
            _logger.LogInformation("OTP verified (not marked as used) for {Email}", email);
        }

        return true;
    }
}
