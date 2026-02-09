using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs
{
    public class GeneralResponse<T>
    {
        public bool Success { get; set; }                // هل العملية نجحت
        public string Message { get; set; }              // رسالة توضيحية (نجاح / خطأ)
        public T? Data { get; set; }                     // البيانات الراجعة (اختياري)
        public List<string>? Errors { get; set; }        // لو في أكتر من خطأ

        public GeneralResponse() { }

        // ✅ للنجاح
        public static GeneralResponse<T> SuccessResponse(T data, string message = "Operation succeeded")
        {
            return new GeneralResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        // ⚠️ للفشل برسالة فقط
        public static GeneralResponse<T> FailResponse(string message,List<string>? errors = null)
        {
            return new GeneralResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        public static GeneralResponse<bool> ErrorResponse(string v)
        {
            throw new NotImplementedException();
        }
    }
}
