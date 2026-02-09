using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.SAP.Models.Based
{
    public  class GeneralSapResponse<T>
    {
        public bool Success { get; set; }                // هل العملية نجحت
        public string Message { get; set; }              // رسالة توضيحية (نجاح / خطأ)
        public T? Data { get; set; }                     // البيانات الراجعة (اختياري)
        public List<string>? Errors { get; set; }        // لو في أكتر من خطأ

        public GeneralSapResponse() { }

        // ✅ للنجاح
        public static GeneralSapResponse<T> SuccessResponse(T data, string message = "Operation succeeded")
        {
            return new GeneralSapResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        // ⚠️ للفشل برسالة فقط
        public static GeneralSapResponse<T> FailResponse(string message, List<string>? errors = null)
        {
            return new GeneralSapResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
