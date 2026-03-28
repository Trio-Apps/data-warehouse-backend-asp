using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataWarehouse.Domain.Validations
{
   

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NotFutureDateAttribute : ValidationAttribute
    {
        public NotFutureDateAttribute()
        {
            ErrorMessage = "Posting date cannot be in the future.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is not DateTime dateValue)
                return new ValidationResult("Invalid date value.");

            if (dateValue.Date > DateTime.Today)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
