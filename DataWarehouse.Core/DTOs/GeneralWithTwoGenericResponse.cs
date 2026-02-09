using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataWarehouse.Core.DTOs
{
  
        public class GeneralWithTwoGenericResponse<T,TMeta>
        {
            public bool Success { get; set; }                // هل العملية نجحت
            public string Message { get; set; }              // رسالة توضيحية (نجاح / خطأ)
            public T? Data { get; set; }                     // البيانات الراجعة (اختياري)
            public TMeta Meta { get; set; }   
         public List<string>? Errors { get; set; }        // لو في أكتر من خطأ

            public GeneralWithTwoGenericResponse() { }

            // ✅ للنجاح
            public static GeneralWithTwoGenericResponse<T,TMeta> SuccessResponse(T data,TMeta meta, string message = "Operation succeeded")
            {
                return new GeneralWithTwoGenericResponse<T,TMeta>
                {
                    Success = true,
                    Message = message,
                    Data = data,
                    Meta = meta
                   
                };
            }

            // ⚠️ للفشل برسالة فقط
            public static GeneralWithTwoGenericResponse<T,TMeta> FailResponse(string message, List<string>? errors = null)
            {
                return new GeneralWithTwoGenericResponse<T,TMeta>
                {
                    Success = false,
                    Message = message,
                    Errors = errors
                };
            }

            public static GeneralWithTwoGenericResponse<bool,TMeta> ErrorResponse(string v)
            {
                throw new NotImplementedException();
            }
        }
    
}
