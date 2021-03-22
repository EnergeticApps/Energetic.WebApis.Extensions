using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;

namespace Hellang.Middleware.ProblemDetails
{
    public static class ProblemDetailsOptionsExtensions
    {
        public static ProblemDetailsOptions AddProblemDetailsExceptionMappingForWhenItsOurFault(this ProblemDetailsOptions options)
        {
            options.Map<InvalidOperationException>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));
            options.Map<NotImplementedException>(ex => new StatusCodeProblemDetails(StatusCodes.Status501NotImplemented));
            options.Map<HttpRequestException>(ex => new StatusCodeProblemDetails(StatusCodes.Status503ServiceUnavailable));
            options.Map<Exception>(ex => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));

            return options;
        }


        public static ProblemDetailsOptions AddSecurityProblemDetailsExceptionMapping(this ProblemDetailsOptions options)
        {
            options.Map<InvalidCredentialException>(ex => new StatusCodeProblemDetails(StatusCodes.Status401Unauthorized));
            return options;
        }


        public static ProblemDetailsOptions AddInvalidInputProblemDetailsExceptionMapping(this ProblemDetailsOptions options)
        {
            options.Map<FormatException>(ex => new StatusCodeProblemDetails(StatusCodes.Status400BadRequest));
            options.Map<JsonException>(ex => new StatusCodeProblemDetails(StatusCodes.Status400BadRequest));
            options.Map<ValidationException>(ex => new StatusCodeProblemDetails(StatusCodes.Status400BadRequest));

            return options;
        }


        public static ProblemDetailsOptions AddDatabaseProblemDetailsExceptionMapping(this ProblemDetailsOptions options)
        {
            options.Map<SqlException>(sqlEx => GetResponseToSqlException(sqlEx));
            options.Map<DbUpdateException>(dbEx => GetResponseToDbUpdateException(dbEx));

            return options;
        }

        private static StatusCodeProblemDetails GetResponseToSqlException(SqlException ex)
        {
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                // It's a violation of an unique index or a unique constraint. E.g. primary key violation
                return new StatusCodeProblemDetails(StatusCodes.Status409Conflict);
            }

            // Lord knows what it is.
            return new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError);
        }

        private static StatusCodeProblemDetails GetResponseToDbUpdateException(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
            {
                return GetResponseToSqlException(sqlEx);
            }

            // Lord knows what it is.
            return new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError);
        }
    }
}
