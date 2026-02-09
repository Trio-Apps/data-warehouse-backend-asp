using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs
{
    public class SapDto
    {
        public int SapId { get; set; }
        public string SapUrl { get; set; }
        public string CompanyDB { get; set; }
        public string Name { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public int CompanyId { get; set; }

        public bool IsActive { get; set; }
    }
    public class SapByCompanyIdDto
    {
        public int SapId { get; set; }
        public string Name { get; set; }
    }
    public class AddSapDto
    {
        [Required(ErrorMessage = "Sap Url is required")]
        public string SapUrl { get; set; }
        [Required(ErrorMessage = "CompanyDB is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "CompanyDB must be between 3 and 100 characters")]
        public string CompanyDB { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]

        public string Name { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "UserName must be between 3 and 100 characters")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; }

        public int CompanyId { get; set; }

    }

    public class UpdateSapDto
    {

        [Required(ErrorMessage = "Sap Id is required")]
        public int SapId { get; set; }
        [Required(ErrorMessage = "Sap Url is required")]
      
        public string CompanyDB { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]

        public string Name { get; set; }
        [Required(ErrorMessage = "UserName is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "UserName must be between 3 and 100 characters")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; }
        public int CompanyId { get; set; }

    }

    public class SapConnectionTestResult
    {
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }

        public static SapConnectionTestResult Success()
            => new() { IsSuccess = true };

        public static SapConnectionTestResult Fail(string error)
            => new() { IsSuccess = false, Error = error };
    }

}
