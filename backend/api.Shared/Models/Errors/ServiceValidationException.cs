using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace api.Shared.Models.Errors
{
    public class ServiceValidationException : Exception
    {
        public ServiceValidationException(string message)
            : base(message)
        { }

        public ServiceValidationException(IEnumerable<ValidationFailure> errors)
            : base($"Validation errors: {string.Join(", ", errors.Select(x => x.ErrorMessage))}")
        { }
    }
}
