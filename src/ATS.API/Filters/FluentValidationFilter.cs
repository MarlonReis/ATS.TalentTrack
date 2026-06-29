namespace ATS.API.Filters;

using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
/// Filtro global que executa o validator FluentValidation correspondente
/// para cada argumento de action que possua um IValidator registrado.
/// </summary>
public sealed class FluentValidationFilter : IAsyncActionFilter
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                foreach (var failure in result.Errors)
                {
                    context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
                }
            }
        }

        if (!context.ModelState.IsValid)
        {
            var details = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Um ou mais erros de validação ocorreram."
            };

            // Escreve diretamente para garantir content-type sem charset (consistente com ExceptionHandlingMiddleware)
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.HttpContext.Response.ContentType = "application/problem+json";
            await JsonSerializer.SerializeAsync(
                context.HttpContext.Response.Body,
                details,
                _jsonOptions,
                context.HttpContext.RequestAborted);

            context.Result = new EmptyResult();
            return;
        }

        await next();
    }
}
