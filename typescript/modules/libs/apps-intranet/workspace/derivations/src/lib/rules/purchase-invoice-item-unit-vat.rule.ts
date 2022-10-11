import { isBefore, isAfter } from 'date-fns';
import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import {
  OrderAdjustment,
  PurchaseInvoiceItem,
} from '@allors/default/workspace/domain';

export class PurchaseInvoiceItemUnitVatRule
  implements IRule<PurchaseInvoiceItem>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.PurchaseInvoiceItem;
    this.roleType = m.PurchaseInvoiceItem.UnitVat;
  }

  derive(match: PurchaseInvoiceItem) {
    const invoice = match.PurchaseInvoiceWherePurchaseInvoiceItem;
    let unitBasePrice = 0;
    let unitDiscount = 0;
    let unitSurcharge = 0;

    const supplierOffering = match.Part?.SupplierOfferingsWherePart?.find(
      (v) =>
        v.Supplier == invoice.BilledFrom &&
        isBefore(new Date(v.FromDate), invoice.InvoiceDate) &&
        (v.ThroughDate == null ||
          isAfter(new Date(v.ThroughDate), invoice.InvoiceDate))
    );

    const vatRegime = match.AssignedVatRegime ?? invoice?.DerivedVatRegime;
    const vatRate = vatRegime?.VatRates.find(
      (v) =>
        isBefore(new Date(v.FromDate), invoice.InvoiceDate) &&
        (v.ThroughDate == null ||
          isAfter(new Date(v.ThroughDate), invoice.InvoiceDate))
    );

    if (match.AssignedUnitPrice != null) {
      unitBasePrice = parseFloat(match.AssignedUnitPrice);
    } else if (supplierOffering != null) {
      unitBasePrice = parseFloat(supplierOffering?.Price);
    }

    if (unitBasePrice > 0) {
      match.DiscountAdjustments.forEach((v: OrderAdjustment) => {
        unitDiscount +=
          v.Percentage != null
            ? (unitBasePrice * parseFloat(v.Percentage)) / 100
            : parseFloat(v.Amount ?? '0');
      });

      match.SurchargeAdjustments.forEach((v: OrderAdjustment) => {
        unitSurcharge +=
          v.Percentage != null
            ? (unitBasePrice * parseFloat(v.Percentage)) / 100
            : parseFloat(v.Amount ?? '0');
      });

      const unitPrice = unitBasePrice - unitDiscount + unitSurcharge;

      return ((unitPrice * parseFloat(vatRate?.Rate ?? '0')) / 100).toFixed(2);
    }

    return '0';
  }
}
