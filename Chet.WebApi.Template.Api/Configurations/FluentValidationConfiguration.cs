using Chet.WebApi.Template.Shared;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Chet.WebApi.Template.Api.Configurations;

public static class FluentValidationConfiguration
{
    public static void ConfigureFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(options =>
        {
            options.DisableDataAnnotationsValidation = true;
        });
        
        services.AddValidatorsFromAssembly(Assembly.Load("Chet.WebApi.Template.DTOs"));
        
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray()
                    );

                var response = new ValidationErrorResponse
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors,
                    Timestamp = DateTime.Now
                };

                return new BadRequestObjectResult(response)
                {
                    ContentTypes = { "application/json" }
                };
            };
        });
    }
}
