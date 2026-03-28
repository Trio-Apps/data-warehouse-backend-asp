using DataWarehouse.Domain.Entities.Processes.IGenericDto;
using System;

namespace DataWarehouse.Domain.Helpers
{
    public static class VatCalculationHelper
    {
        public static void Apply(IOrderItem item)
        {
            if (item == null)
            {
                return;
            }

            if (!item.UnitPrice.HasValue)
            {
                item.VatAmount = null;
                item.LineTotalBeforeVat = null;
                item.LineTotalAfterVat = null;
                return;
            }

            var vatPercent = item.VatPercent ?? 0m;
            var lineTotalBeforeVat = item.Quantity * item.UnitPrice.Value;
            var vatAmount = lineTotalBeforeVat * (vatPercent / 100m);
            var lineTotalAfterVat = lineTotalBeforeVat + vatAmount;

            item.VatPercent = vatPercent;
            item.LineTotalBeforeVat = Math.Round(lineTotalBeforeVat, 2, MidpointRounding.AwayFromZero);
            item.VatAmount = Math.Round(vatAmount, 2, MidpointRounding.AwayFromZero);
            item.LineTotalAfterVat = Math.Round(lineTotalAfterVat, 2, MidpointRounding.AwayFromZero);
        }
    }
}
