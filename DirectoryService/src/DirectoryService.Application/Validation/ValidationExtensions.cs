using FluentValidation.Results;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Validation
{
    public static class ValidationExtensions
    {
        public static Errors ToList(this ValidationResult validationResult)
        {
            var validationErrors = validationResult.Errors;

            var errors = from validationError in validationErrors
                         let errorMessage = validationError.ErrorMessage
                         let error = Error.Deserialize(errorMessage)
                         select Error.Validation(error.Code, error.Message, validationError.PropertyName);

            return errors.ToList();
        }
    }
}
