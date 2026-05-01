import { useEffect, useMemo, useState } from "react";
import { Download, Eye, Loader2, Plus, Printer, RefreshCw, Send, Trash2, Wallet } from "lucide-react";
import { useNavigate } from "react-router-dom";
import Swal from "sweetalert2";
import "sweetalert2/dist/sweetalert2.min.css";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
import { getApiErrorMessage, getCurrentUser, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type InvoiceStatus = "Draft" | "Sent" | "PartiallyPaid" | "Paid" | "Overdue" | "Cancelled";
type PaymentMethod = "BankTransfer" | "Cash" | "CreditCard" | "PayPal" | "Other";

export type InvoiceListItem = {
  id: string;
  projectId: string;
  projectName: string;
  customerId: string;
  customerName: string;
  invoiceNumber: string;
  totalWithTax: number;
  paidAmount: number;
  remainingAmount: number;
  status: InvoiceStatus | string;
  issueDate: string;
  dueDate: string;
  currency: string;
};

export type PaymentDto = {
  id: string;
  invoiceId: string;
  amount: number;
  method: PaymentMethod;
  paymentDate: string;
  notes?: string | null;
  currency: string;
  createdAtUtc: string;
};

export type InvoiceDto = InvoiceListItem & {
  totalAmount: number;
  taxAmount: number;
  advanceAmount: number;
  advanceRemainingAmount: number;
  notes?: string | null;
  payments: PaymentDto[];
  createdAtUtc: string;
  lastModifiedUtc: string;
};

type BankTransferInfo = {
  bankName?: string | null;
  bankAccountName?: string | null;
  bankIban?: string | null;
};

type ProjectOption = {
  id: string;
  projectName: string;
  customerName: string;
  suggestedPrice?: number;
};

type PaginatedList<T> = {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  items: T[];
};

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const INVOICES_API_URL = `${API_BASE_URL}/api/v1/invoices`;
const PROJECTS_API_URL = `${API_BASE_URL}/api/v1/projects`;

function buildQuery(params: Record<string, string | number | undefined>) {
  const searchParams = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === "") continue;
    searchParams.set(key, String(value));
  }
  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

async function apiRequest<T>(url: string, init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(url, {
    ...init,
    headers: {
      Accept: "application/json",
      Authorization: `Bearer ${token}`,
      ...(init?.body ? { "Content-Type": "application/json" } : {}),
      ...init?.headers,
    },
  });

  const payload = await response.json().catch(() => null);

  if (!response.ok) throw payload ?? new Error(getApiErrorMessage(payload, "تعذر تنفيذ الطلب."));

  return unwrapApiResponse<T>(payload);
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("ar", { dateStyle: "medium" }).format(date);
}

function toDateInput(value: string) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString().slice(0, 10);
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function formatCurrency(amount: number | undefined, currency: string) {
  const normalized = currency?.trim() || "SAR";
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency: normalized,
    maximumFractionDigits: 2,
  }).format(amount ?? 0);
}

function formatNumberInputValue(amount: number) {
  if (!Number.isFinite(amount)) return "";

  return new Intl.NumberFormat("en-US", {
    useGrouping: false,
    maximumFractionDigits: 2,
  }).format(amount);
}

function normalizeNumberInputValue(value: string) {
  return value
    .replace(/[٠-٩]/g, (digit) => String(digit.charCodeAt(0) - 0x0660))
    .replace(/[۰-۹]/g, (digit) => String(digit.charCodeAt(0) - 0x06f0))
    .replace(/[^\d.]/g, "");
}

function isPaymentCurrencyMismatch(message: string) {
  return message.toLowerCase().includes("payment currency must match the invoice currency");
}

function wait(ms: number) {
  return new Promise((resolve) => window.setTimeout(resolve, ms));
}

function normalizeStatus(status: InvoiceStatus | string) {
  const normalized = status.toLowerCase();
  if (normalized === "draft" || status === "مسودة") return "Draft";
  if (normalized === "sent" || status === "مرسلة") return "Sent";
  if (normalized === "partiallypaid" || normalized === "partially paid" || status === "مدفوعة جزئيا" || status === "مدفوعة جزئياً") return "PartiallyPaid";
  if (normalized === "paid" || status === "مدفوعة") return "Paid";
  if (normalized === "overdue" || status === "متأخرة") return "Overdue";
  if (normalized === "cancelled" || normalized === "canceled" || status === "ملغاة") return "Cancelled";
  return status;
}

function statusLabel(status: InvoiceStatus | string) {
  switch (normalizeStatus(status)) {
    case "Draft":
      return "مسودة";
    case "Sent":
      return "مرسلة";
    case "PartiallyPaid":
      return "مدفوعة جزئيا";
    case "Paid":
      return "مدفوعة";
    case "Overdue":
      return "متأخرة";
    case "Cancelled":
      return "ملغاة";
    default:
      return status;
  }
}

function statusClass(status: InvoiceStatus | string) {
  switch (normalizeStatus(status)) {
    case "Paid":
      return "bg-success-soft text-success";
    case "Sent":
      return "bg-teal-soft text-teal";
    case "Overdue":
      return "bg-warning-soft text-warning";
    case "Cancelled":
      return "bg-danger-soft text-danger";
    case "PartiallyPaid":
      return "bg-warning-soft text-warning";
    case "Draft":
    default:
      return "bg-muted text-muted-foreground";
  }
}

function methodLabel(method: PaymentMethod | string) {
  switch (method) {
    case "BankTransfer":
      return "تحويل بنكي";
    case "Cash":
      return "نقدا";
    case "CreditCard":
      return "بطاقة";
    case "PayPal":
      return "PayPal";
    case "Other":
      return "أخرى";
    default:
      return method;
  }
}

function escapeHtml(value: string | number | null | undefined) {
  return String(value ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}

function getBankTransferInfo(): BankTransferInfo {
  return getCurrentUser() ?? {};
}

function hasBankTransferInfo(info: BankTransferInfo) {
  return !!(info.bankName?.trim() || info.bankAccountName?.trim() || info.bankIban?.trim());
}

function bankTransferLines(info: BankTransferInfo) {
  if (!hasBankTransferInfo(info)) {
    return ["أضف بيانات حسابك البنكي في إعدادات الحساب."];
  }

  return [
    info.bankName ? `البنك: ${info.bankName}` : null,
    info.bankAccountName ? `اسم المستفيد: ${info.bankAccountName}` : null,
    info.bankIban ? `الآيبان / رقم الحساب: ${info.bankIban}` : null,
  ].filter((line): line is string => !!line);
}

function invoiceItemName(invoice: InvoiceDto) {
  return invoice.projectName || invoice.notes || "خدمة مستقلة";
}

function buildInvoicePrintHtml(invoice: InvoiceDto, autoPrint = false) {
  const currency = invoice.currency || "SAR";
  const itemName = invoiceItemName(invoice);
  const safeInvoiceNumber = escapeHtml(invoice.invoiceNumber);
  const bankLines = bankTransferLines(getBankTransferInfo())
    .map((line) => `<span class="bank-line">${escapeHtml(line)}</span>`)
    .join("");

  return `<!DOCTYPE html>
<html lang="ar" dir="rtl">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>فاتورة ضريبية - ${safeInvoiceNumber}</title>
  <style>
    @page { size: A4 portrait; margin: 8mm 10mm; }
    * { box-sizing: border-box; }
    html, body { margin: 0; padding: 0; }
    body {
      font-family: Tahoma, Arial, sans-serif;
      color: #000;
      background: #e9e9e9;
      -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
    }
    .screen-toolbar {
      position: fixed;
      top: 14px;
      left: 14px;
      z-index: 9999;
      display: flex;
      gap: 8px;
      padding: 10px;
      background: rgba(255,255,255,0.96);
      border: 1px solid #cfcfcf;
      border-radius: 10px;
      box-shadow: 0 8px 20px rgba(0,0,0,0.14);
    }
    .toolbar-btn {
      border: none;
      border-radius: 8px;
      padding: 8px 14px;
      cursor: pointer;
      font-size: 13px;
      font-weight: 700;
    }
    .toolbar-btn-primary { background: #0f766e; color: #fff; }
    .toolbar-btn-secondary { background: #f4f4f4; color: #000; border: 1px solid #cfcfcf; }
    .page {
      width: 190mm;
      min-height: 278mm;
      margin: 8px auto;
      background: #fff;
      padding: 8mm 6mm 10mm;
      display: flex;
      flex-direction: column;
    }
    .header-grid {
      direction: ltr;
      display: grid;
      grid-template-columns: 26% 48% 26%;
      align-items: start;
      gap: 12px;
      border-bottom: 1px solid #000;
      padding-bottom: 8px;
    }
    .header-left, .header-center, .header-right { min-height: 150px; }
    .header-left { text-align: center; font-size: 15px; font-weight: 700; }
    .qr-box {
      width: 120px;
      height: 120px;
      margin: 8px auto 0;
      border: 1px solid #000;
      display: grid;
      place-items: center;
      font-size: 11px;
      color: #555;
    }
    .header-center {
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }
    .invoice-title { font-size: 22px; font-weight: 800; }
    .invoice-subtitle { margin-top: 8px; font-size: 13px; font-weight: 700; }
    .header-right {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      justify-content: flex-start;
      padding-top: 5px;
      text-align: right;
    }
    .brand-box {
      width: 150px;
      min-height: 64px;
      border: 1px solid #000;
      display: grid;
      place-items: center;
      font-size: 20px;
      font-weight: 800;
    }
    .company-vat { margin-top: 8px; font-size: 13px; font-weight: 700; }
    .meta {
      width: 100%;
      border-collapse: collapse;
      margin-top: 9px;
      font-size: 13px;
    }
    .meta td { padding: 4px 5px; vertical-align: top; }
    .meta .label, .meta .customer-label { font-weight: 700; white-space: nowrap; }
    .meta .label { width: 11%; }
    .meta .value { width: 16%; }
    .meta .customer-label { width: 12%; }
    .meta .customer-value { width: 35%; }
    .items {
      width: 100%;
      border-collapse: collapse;
      margin-top: 8px;
      font-size: 12px;
    }
    .items th, .items td {
      border: 1px solid #000;
      padding: 6px 4px;
      text-align: center;
      vertical-align: middle;
    }
    .items thead th { font-weight: 700; background: #fff; }
    .items td.item-name { text-align: right; line-height: 1.45; }
    .after-items {
      display: grid;
      grid-template-columns: 170px 1fr;
      gap: 16px;
      margin-top: 8px;
      direction: ltr;
    }
    .totals-box {
      width: 100%;
      border-collapse: collapse;
      font-size: 12px;
      direction: rtl;
    }
    .totals-box td { border: 1px solid #000; padding: 5px 7px; }
    .totals-box .t-label { font-weight: 700; text-align: right; width: 58%; }
    .totals-box .t-value { text-align: center; width: 42%; direction: ltr; }
    .lower-section { margin-top: 26px; }
    .signatures {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 80px;
      text-align: center;
      font-size: 13px;
      padding-top: 10px;
    }
    .sign-box { min-height: 60px; padding-top: 22px; }
    .bottom-wrapper { margin-top: auto; }
    .bank-info { text-align: right; font-size: 13px; line-height: 2; }
    .bank-line { display: block; margin-bottom: 2px; font-weight: 700; }
    .footer {
      margin-top: 0;
      padding-top: 8px;
      text-align: center;
      font-size: 11px;
      border-top: 1px solid #000;
    }
    @media print {
      html, body { background: #fff; margin: 0; padding: 0; }
      .screen-toolbar { display: none !important; }
      .page {
        margin: 0 auto;
        width: 100%;
        min-height: 278mm;
        padding: 8mm 6mm 10mm;
      }
      .lower-section, .bank-info, .signatures, .footer, .bottom-wrapper { page-break-inside: avoid; }
    }
  </style>
</head>
<body>
  <div class="screen-toolbar">
    <button type="button" class="toolbar-btn toolbar-btn-primary" onclick="window.print()">طباعة / حفظ PDF</button>
    <button type="button" class="toolbar-btn toolbar-btn-secondary" onclick="window.close()">إغلاق</button>
  </div>

  <div class="page">
    <div class="header-grid">
      <div class="header-left">
        <div>فاتورة مبيعات ضريبية</div>
      </div>
      <div class="header-center">
        <div class="invoice-title">فاتورة ضريبية</div>
        <div class="invoice-subtitle">Tax Invoice</div>
      </div>
      <div class="header-right">
        <div class="brand-box">منصة مالي</div>
        <div class="company-vat">الرقم الضريبي: غير محدد</div>
      </div>
    </div>

    <table class="meta">
      <tr>
        <td class="label">رقم الفاتورة</td>
        <td class="value">${safeInvoiceNumber}</td>
        <td class="customer-label">اسم العميل</td>
        <td class="customer-value">${escapeHtml(invoice.customerName)}</td>
        <td class="label">حالة الفاتورة</td>
        <td class="value">${escapeHtml(statusLabel(invoice.status))}</td>
      </tr>
      <tr>
        <td class="label">تاريخ الفاتورة</td>
        <td class="value">${escapeHtml(formatDate(invoice.issueDate))}</td>
        <td class="customer-label">طريقة الدفع</td>
        <td class="customer-value">حسب الاتفاق</td>
        <td class="label">تاريخ الاستحقاق</td>
        <td class="value">${escapeHtml(formatDate(invoice.dueDate))}</td>
      </tr>
      <tr>
        <td class="label">المشروع</td>
        <td class="value" colspan="5">${escapeHtml(invoice.projectName)}</td>
      </tr>
    </table>

    <table class="items">
      <thead>
        <tr>
          <th style="width:8%;">م</th>
          <th style="width:62%;">اسم المشروع</th>
          <th style="width:30%;">تكلفة المشروع</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>1</td>
          <td class="item-name">${escapeHtml(itemName)}</td>
          <td style="text-align:center;">${escapeHtml(formatCurrency(invoice.totalAmount, currency))}</td>
        </tr>
      </tbody>
    </table>

    <div class="after-items">
      <table class="totals-box">
        <tr>
          <td class="t-label">دفعة مقدمة</td>
          <td class="t-value">${escapeHtml(formatCurrency(invoice.advanceAmount, currency))}</td>
        </tr>
        <tr>
          <td class="t-label">المجموع</td>
          <td class="t-value">${escapeHtml(formatCurrency(invoice.totalAmount, currency))}</td>
        </tr>
        <tr>
          <td class="t-label">الضريبة</td>
          <td class="t-value">${escapeHtml(formatCurrency(invoice.taxAmount, currency))}</td>
        </tr>
        <tr>
          <td class="t-label">صافي الفاتورة</td>
          <td class="t-value">${escapeHtml(formatCurrency(invoice.totalWithTax, currency))}</td>
        </tr>
        <tr>
          <td class="t-label">المدفوع</td>
          <td class="t-value">${escapeHtml(formatCurrency(invoice.paidAmount, currency))}</td>
        </tr>
        <tr>
          <td class="t-label">المتبقي</td>
          <td class="t-value">${escapeHtml(formatCurrency(invoice.remainingAmount, currency))}</td>
        </tr>
      </table>
      <div></div>
    </div>

    <div class="lower-section">
      <div class="signatures">
        <div class="sign-box">توقيع المستقل</div>
        <div class="sign-box">توقيع العميل</div>
      </div>
    </div>

    <div class="bottom-wrapper">
      <div class="bank-info">
        <span class="bank-line">بيانات التحويل البنكي:</span>
        ${bankLines}
      </div>
      <div class="footer">
        شكرا لتعاملكم معنا - تم إنشاء هذه الفاتورة عبر منصة مالي
      </div>
    </div>
  </div>
  ${autoPrint ? "<script>window.addEventListener('load', function () { window.print(); });</script>" : ""}
</body>
</html>`;
}

type PaymentForm = {
  amount: string;
  method: PaymentMethod;
  paymentDate: string;
  currency: string;
  notes: string;
};

const EMPTY_PAYMENT_FORM: PaymentForm = {
  amount: "",
  method: "BankTransfer",
  paymentDate: new Date().toISOString().slice(0, 10),
  currency: "SAR",
  notes: "",
};

type InvoiceForm = {
  projectId: string;
  invoiceNumber: string;
  totalAmount: string;
  advanceAmount: string;
  issueDate: string;
  dueDate: string;
  currency: string;
  notes: string;
};

const today = new Date().toISOString().slice(0, 10);
const EMPTY_INVOICE_FORM: InvoiceForm = {
  projectId: "",
  invoiceNumber: "",
  totalAmount: "",
  advanceAmount: "0",
  issueDate: today,
  dueDate: today,
  currency: "SAR",
  notes: "",
};

const CURRENCY_OPTIONS = [
  { value: "SAR", label: "ريال سعودي" },
  { value: "USD", label: "دولار أمريكي" },
  { value: "EUR", label: "يورو" },
  { value: "AED", label: "درهم إماراتي" },
  { value: "KWD", label: "دينار كويتي" },
  { value: "BHD", label: "دينار بحريني" },
  { value: "QAR", label: "ريال قطري" },
  { value: "OMR", label: "ريال عماني" },
  { value: "EGP", label: "جنيه مصري" },
  { value: "JOD", label: "دينار أردني" },
];

export function InvoicePrintStyles() {
  return (
    <style>{`
      .invoice-print-area ~ .grid,
      .invoice-print-area ~ .rounded-xl {
        display: none !important;
      }

      @media print {
        body * {
          visibility: hidden !important;
        }

        .invoice-print-area,
        .invoice-print-area * {
          visibility: visible !important;
        }

        .invoice-print-area {
          position: absolute !important;
          inset: 0 auto auto 0 !important;
          width: 100% !important;
          max-width: none !important;
          margin: 0 !important;
          padding: 0 !important;
          box-shadow: none !important;
        }

        .invoice-no-print {
          display: none !important;
        }
      }
    `}</style>
  );
}

export function InvoicePrintPreview({ invoice }: { invoice: InvoiceDto }) {
  const currency = invoice.currency || "SAR";
  const bankLines = bankTransferLines(getBankTransferInfo());

  return (
    <div className="invoice-print-area mx-auto flex min-h-[278mm] max-w-[820px] flex-col bg-white p-4 text-black shadow-sm" dir="rtl">
      <div className="grid grid-cols-[26%_1fr_26%] items-start gap-3 border-b border-black pb-3">
        <div className="text-center text-sm font-bold">
          <div>فاتورة مبيعات ضريبية</div>
        </div>
        <div className="flex min-h-32 flex-col items-center justify-center text-center">
          <div className="text-2xl font-extrabold">فاتورة ضريبية</div>
          <div className="mt-2 text-sm font-bold">Tax Invoice</div>
        </div>
        <div className="flex flex-col items-end text-right">
          <div className="grid min-h-16 w-36 place-items-center border border-black text-lg font-extrabold">
            منصة مالي
          </div>
          <div className="mt-2 text-xs font-bold">الرقم الضريبي: غير محدد</div>
        </div>
      </div>

      <table className="mt-3 w-full text-xs">
        <tbody>
          <tr>
            <td className="py-1 font-bold">رقم الفاتورة</td>
            <td className="py-1">{invoice.invoiceNumber}</td>
            <td className="py-1 font-bold">اسم العميل</td>
            <td className="py-1">{invoice.customerName}</td>
            <td className="py-1 font-bold">حالة الفاتورة</td>
            <td className="py-1">{statusLabel(invoice.status)}</td>
          </tr>
          <tr>
            <td className="py-1 font-bold">تاريخ الفاتورة</td>
            <td className="py-1">{formatDate(invoice.issueDate)}</td>
            <td className="py-1 font-bold">طريقة الدفع</td>
            <td className="py-1">حسب الاتفاق</td>
            <td className="py-1 font-bold">تاريخ الاستحقاق</td>
            <td className="py-1">{formatDate(invoice.dueDate)}</td>
          </tr>
          <tr>
            <td className="py-1 font-bold">المشروع</td>
            <td className="py-1" colSpan={5}>{invoice.projectName}</td>
          </tr>
        </tbody>
      </table>

      <table className="mt-3 w-full border-collapse text-xs">
        <thead>
          <tr>
            <th className="w-[8%] border border-black p-2">م</th>
            <th className="w-[62%] border border-black p-2">اسم المشروع</th>
            <th className="w-[30%] border border-black p-2 text-center">تكلفة المشروع</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td className="border border-black p-2 text-center">1</td>
            <td className="border border-black p-2 text-right">{invoiceItemName(invoice)}</td>
            <td className="border border-black p-2 text-center">{formatCurrency(invoice.totalAmount, currency)}</td>
          </tr>
        </tbody>
      </table>

      <div className="mt-3 grid grid-cols-[180px_1fr] gap-4" dir="ltr">
        <table className="w-full border-collapse text-xs" dir="rtl">
          <tbody>
            {[
              ["دفعة مقدمة", invoice.advanceAmount],
              ["المجموع", invoice.totalAmount],
              ["الضريبة", invoice.taxAmount],
              ["صافي الفاتورة", invoice.totalWithTax],
              ["المدفوع", invoice.paidAmount],
              ["المتبقي", invoice.remainingAmount],
            ].map(([label, value]) => (
              <tr key={String(label)}>
                <td className="border border-black p-2 text-right font-bold">{label}</td>
                <td className="border border-black p-2 text-center">{formatCurrency(Number(value), currency)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <div />
      </div>

      <div className="mt-8 grid grid-cols-2 gap-20 text-center text-sm">
        <div className="min-h-16 pt-6">توقيع المستقل</div>
        <div className="min-h-16 pt-6">توقيع العميل</div>
      </div>

      <div className="mt-auto">
        <div className="text-right text-sm font-bold leading-8">
          <span className="block">بيانات التحويل البنكي:</span>
          {bankLines.map((line) => (
            <span key={line} className="block">
              {line}
            </span>
          ))}
        </div>

        <div className="mt-3 border-t border-black pt-2 text-center text-xs">
          شكرا لتعاملكم معنا - تم إنشاء هذه الفاتورة عبر منصة مالي
        </div>
      </div>
    </div>
  );
}

export default function InvoicesTable() {
  const navigate = useNavigate();
  const [invoices, setInvoices] = useState<PaginatedList<InvoiceListItem> | null>(null);
  const [projects, setProjects] = useState<ProjectOption[]>([]);
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [status, setStatus] = useState<"all" | InvoiceStatus>("all");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [pageNumber, setPageNumber] = useState(1);

  const [isLoading, setIsLoading] = useState(true);
  const [isBusy, setIsBusy] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const [selectedInvoice, setSelectedInvoice] = useState<InvoiceDto | null>(null);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [invoiceForm, setInvoiceForm] = useState<InvoiceForm>(EMPTY_INVOICE_FORM);

  const [deleteTarget, setDeleteTarget] = useState<InvoiceListItem | null>(null);

  const [paymentTarget, setPaymentTarget] = useState<InvoiceListItem | null>(null);
  const [isPaymentOpen, setIsPaymentOpen] = useState(false);
  const [paymentForm, setPaymentForm] = useState<PaymentForm>(EMPTY_PAYMENT_FORM);
  const [isAdvancePayment, setIsAdvancePayment] = useState(false);

  const pageSize = 10;

  const stats = useMemo(() => {
    const items = invoices?.items ?? [];
    return {
      total: invoices?.totalCount ?? 0,
      overdue: items.filter((i) => normalizeStatus(i.status) === "Overdue").length,
      paid: items.filter((i) => normalizeStatus(i.status) === "Paid").length,
    };
  }, [invoices]);

  const remainingAfterPayment = useMemo(() => {
    if (!paymentTarget) return 0;
    if (isAdvancePayment) return paymentTarget.remainingAmount;

    const paymentAmount = Number(paymentForm.amount || 0);
    return Math.max(paymentTarget.remainingAmount - (Number.isFinite(paymentAmount) ? paymentAmount : 0), 0);
  }, [isAdvancePayment, paymentForm.amount, paymentTarget]);

  const loadInvoices = async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = buildQuery({
        search: appliedSearch.trim(),
        status: status === "all" ? undefined : status,
        fromDate: fromDate ? dateInputToIso(fromDate) : undefined,
        toDate: toDate ? dateInputToIso(toDate) : undefined,
        pageNumber,
        pageSize,
      });
      const result = await apiRequest<PaginatedList<InvoiceListItem>>(`${INVOICES_API_URL}${query}`);
      setInvoices(result);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل الفواتير."));
    } finally {
      setIsLoading(false);
    }
  };

  const loadProjects = async () => {
    try {
      const result = await apiRequest<PaginatedList<ProjectOption>>(`${PROJECTS_API_URL}?pageSize=100`);
      setProjects(result.items ?? []);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل المشاريع لإنشاء الفاتورة."));
    }
  };

  useEffect(() => {
    void loadInvoices();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [appliedSearch, status, fromDate, toDate, pageNumber]);

  useEffect(() => {
    void loadProjects();
  }, []);

  const submitSearch = (event: React.FormEvent) => {
    event.preventDefault();
    setPageNumber(1);
    setAppliedSearch(search);
  };

  const openCreateInvoice = () => {
    setInvoiceForm(EMPTY_INVOICE_FORM);
    setError("");
    setMessage("");
    setIsCreateOpen(true);
    if (!projects.length) void loadProjects();
  };

  const submitInvoice = async (event: React.FormEvent) => {
    event.preventDefault();
    setError("");
    setMessage("");

    // Warn if selected currency differs from existing invoices for this project
    if (invoiceForm.projectId) {
      const existingForProject = invoices?.items.find(
        (inv) => inv.projectId === invoiceForm.projectId,
      );
      if (
        existingForProject &&
        existingForProject.currency.toUpperCase() !== invoiceForm.currency.toUpperCase()
      ) {
        const result = await Swal.fire({
          icon: "warning",
          title: "تنبيه: عملة مختلفة",
          html: `الفواتير السابقة لهذا المشروع بعملة <strong>${existingForProject.currency}</strong>،<br/>وقد اخترت <strong>${invoiceForm.currency}</strong>.<br/><br/>هل تريد المتابعة؟`,
          confirmButtonText: "نعم، تأكيد",
          cancelButtonText: "إلغاء",
          showCancelButton: true,
          customClass: {
            popup: "rounded-2xl",
            confirmButton: "rounded-xl",
            cancelButton: "rounded-xl",
          },
        });
        if (!result.isConfirmed) return;
      }
    }

    setIsBusy(true);

    try {
      await apiRequest<InvoiceDto>(INVOICES_API_URL, {
        method: "POST",
        body: JSON.stringify({
          projectId: invoiceForm.projectId,
          invoiceNumber: invoiceForm.invoiceNumber.trim(),
          totalAmount: Number(invoiceForm.totalAmount),
          advanceAmount: Number(invoiceForm.advanceAmount || 0),
          issueDate: dateInputToIso(invoiceForm.issueDate),
          dueDate: dateInputToIso(invoiceForm.dueDate),
          currency: invoiceForm.currency.trim() || "SAR",
          notes: invoiceForm.notes.trim() || null,
        }),
      });

      setMessage("تم إنشاء الفاتورة بنجاح.");
      setIsCreateOpen(false);
      setInvoiceForm(EMPTY_INVOICE_FORM);
      await loadInvoices();
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر إنشاء الفاتورة. تحقق من البيانات وحاول مرة أخرى."));
    } finally {
      setIsBusy(false);
    }
  };

  const openDetails = async (invoice: InvoiceListItem) => {
    navigate(`/dashboard/invoices/${invoice.id}`);
  };

  const printInvoice = async (invoice: InvoiceListItem | InvoiceDto) => {
    setIsBusy(true);
    setError("");
    setMessage("");

    try {
      const full =
        "taxAmount" in invoice
          ? invoice
          : await apiRequest<InvoiceDto>(`${INVOICES_API_URL}/${invoice.id}`);

      setSelectedInvoice(full);
      setIsDetailsOpen(true);
      window.setTimeout(() => window.print(), 150);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تجهيز الفاتورة للطباعة."));
    } finally {
      setIsBusy(false);
    }
  };

  const runAction = async (action: "send" | "cancel" | "mark-overdue", invoiceId: string) => {
    setIsBusy(true);
    setError("");
    setMessage("");

    const url =
      action === "send"
        ? `${INVOICES_API_URL}/${invoiceId}/send`
        : action === "cancel"
          ? `${INVOICES_API_URL}/${invoiceId}/cancel`
          : `${INVOICES_API_URL}/${invoiceId}/mark-overdue`;

    try {
      await apiRequest<InvoiceDto>(url, { method: "POST" });
      setMessage("تم تحديث حالة الفاتورة بنجاح.");
      await loadInvoices();
      if (isDetailsOpen && selectedInvoice?.id === invoiceId) {
        const refreshed = await apiRequest<InvoiceDto>(`${INVOICES_API_URL}/${invoiceId}`);
        setSelectedInvoice(refreshed);
      }
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تنفيذ الإجراء."));
    } finally {
      setIsBusy(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;

    setIsBusy(true);
    setError("");
    setMessage("");

    try {
      await apiRequest<object | null>(`${INVOICES_API_URL}/${deleteTarget.id}`, { method: "DELETE" });
      setMessage("تم حذف الفاتورة بنجاح.");
      setDeleteTarget(null);
      await loadInvoices();
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر حذف الفاتورة."));
    } finally {
      setIsBusy(false);
    }
  };

  const openPaymentDialog = (invoice: InvoiceListItem, kind: "payment" | "advance") => {
    setPaymentTarget(invoice);
    setIsAdvancePayment(kind === "advance");
    setPaymentForm((prev) => ({
      ...EMPTY_PAYMENT_FORM,
      currency: invoice.currency || prev.currency || "SAR",
    }));
    setError("");
    setMessage("");
    setIsPaymentOpen(true);
  };

  const submitPayment = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!paymentTarget) return;

    const selectedCurrency = paymentForm.currency.trim() || paymentTarget.currency;
    if (selectedCurrency.toUpperCase() !== paymentTarget.currency.toUpperCase()) {
      setIsPaymentOpen(false);
      await wait(150);
      await Swal.fire({
        icon: "error",
        title: "عملة الدفعة غير مطابقة",
        text: "يجب أن تكون عملة الدفعة نفس عملة الفاتورة.",
        confirmButtonText: "حسنا",
        customClass: {
          popup: "rounded-2xl",
          confirmButton: "rounded-xl",
        },
      });
      setIsPaymentOpen(true);
      return;
    }

    setIsBusy(true);
    setError("");
    setMessage("");

    try {
      if (isAdvancePayment) {
        await apiRequest<InvoiceDto>(`${INVOICES_API_URL}/${paymentTarget.id}/advance-payment`, {
          method: "POST",
          body: JSON.stringify({
            method: paymentForm.method,
            paymentDate: dateInputToIso(paymentForm.paymentDate),
            currency: selectedCurrency,
            notes: paymentForm.notes.trim() || null,
          }),
        });
        setMessage("تم تسجيل الدفعة المقدمة بنجاح.");
      } else {
        await apiRequest<InvoiceDto>(`${INVOICES_API_URL}/${paymentTarget.id}/payments`, {
          method: "POST",
          body: JSON.stringify({
            amount: Number(paymentForm.amount || 0),
            method: paymentForm.method,
            paymentDate: dateInputToIso(paymentForm.paymentDate),
            currency: selectedCurrency,
            notes: paymentForm.notes.trim() || null,
          }),
        });
        setMessage("تم تسجيل الدفعة بنجاح.");
      }

      setIsPaymentOpen(false);
      setPaymentTarget(null);
      await loadInvoices();

      if (isDetailsOpen && selectedInvoice?.id === paymentTarget.id) {
        const refreshed = await apiRequest<InvoiceDto>(`${INVOICES_API_URL}/${paymentTarget.id}`);
        setSelectedInvoice(refreshed);
      }
    } catch (err) {
      const errorMessage = getApiErrorMessage(err, "تعذر تسجيل الدفعة.");
      setError(errorMessage);

      if (isPaymentCurrencyMismatch(errorMessage)) {
        setIsPaymentOpen(false);
        await wait(150);
        await Swal.fire({
          icon: "error",
          title: "عملة الدفعة غير مطابقة",
          text: "يجب أن تكون عملة الدفعة نفس عملة الفاتورة.",
          confirmButtonText: "حسنا",
          customClass: {
            popup: "rounded-2xl",
            confirmButton: "rounded-xl",
          },
        });
        setIsPaymentOpen(true);
      }
    } finally {
      setIsBusy(false);
    }
  };

  return (
    <>
      <InvoicePrintStyles />

      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="الفواتير" desc="جميع فواتيرك مع حالاتها وتفاصيل العملاء." />
        <Button onClick={openCreateInvoice} className="rounded-xl bg-teal font-bold text-white hover:bg-teal/90">
          <Plus className="ml-2 h-4 w-4" />
          فاتورة جديدة
        </Button>
      </div>

      <section className="grid gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.total}</div>
          <div className="mt-1 text-xs text-muted-foreground">إجمالي الفواتير</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.overdue}</div>
          <div className="mt-1 text-xs text-muted-foreground">متأخرة في هذه الصفحة</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.paid}</div>
          <div className="mt-1 text-xs text-muted-foreground">مدفوعة في هذه الصفحة</div>
        </div>
      </section>

      {(message || error) && (
        <div
          className={`rounded-xl border p-3 text-sm ${
            error ? "border-danger/30 bg-danger-soft text-danger" : "border-success/30 bg-success-soft text-success"
          }`}
        >
          {error || message}
        </div>
      )}

      <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
        <div className="mb-4 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <form onSubmit={submitSearch} className="flex flex-1 gap-2">
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="ابحث برقم الفاتورة أو المشروع أو العميل"
              className="h-11 rounded-xl border-border/70 bg-white"
            />
            <Button type="submit" variant="outline" className="h-11 rounded-xl">
              بحث
            </Button>
          </form>

          <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
            <Select
              dir="rtl"
              value={status}
              onValueChange={(value) => {
                setStatus(value as "all" | InvoiceStatus);
                setPageNumber(1);
              }}
            >
              <SelectTrigger className="h-11 w-44 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                <SelectValue placeholder="حالة الفاتورة" />
              </SelectTrigger>
              <SelectContent dir="rtl" className="text-right">
                <SelectItem value="all">كل الحالات</SelectItem>
                <SelectItem value="Draft">مسودة</SelectItem>
                <SelectItem value="Sent">مرسلة</SelectItem>
                <SelectItem value="PartiallyPaid">مدفوعة جزئيا</SelectItem>
                <SelectItem value="Paid">مدفوعة</SelectItem>
                <SelectItem value="Overdue">متأخرة</SelectItem>
                <SelectItem value="Cancelled">ملغاة</SelectItem>
              </SelectContent>
            </Select>

            <div className="flex gap-2">
              <Input
                type="date"
                value={fromDate}
                onChange={(e) => {
                  setFromDate(e.target.value);
                  setPageNumber(1);
                }}
                className="h-11 w-40 rounded-xl border-border/70 bg-white"
              />
              <Input
                type="date"
                value={toDate}
                onChange={(e) => {
                  setToDate(e.target.value);
                  setPageNumber(1);
                }}
                className="h-11 w-40 rounded-xl border-border/70 bg-white"
              />
              <Button type="button" variant="outline" className="h-11 rounded-xl" onClick={() => void loadInvoices()} disabled={isLoading || isBusy}>
                <RefreshCw className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
            <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
            جاري تحميل الفواتير...
          </div>
        ) : invoices?.items?.length ? (
          <>
            <div className="overflow-x-auto rounded-xl border border-border/70">
              <table className="w-full min-w-[1040px] table-fixed text-right text-sm">
                <colgroup>
                  <col className="w-[11%]" />
                  <col className="w-[13%]" />
                  <col className="w-[13%]" />
                  <col className="w-[11%]" />
                  <col className="w-[9%]" />
                  <col className="w-[9%]" />
                  <col className="w-[10%]" />
                  <col className="w-[12%]" />
                  <col className="w-[12%]" />
                </colgroup>
                <thead className="bg-muted/50 text-xs text-muted-foreground">
                  <tr>
                    <th className="px-3 py-3 font-semibold">رقم الفاتورة</th>
                    <th className="px-3 py-3 font-semibold">العميل</th>
                    <th className="px-3 py-3 font-semibold">المشروع</th>
                    <th className="px-3 py-3 font-semibold">الإجمالي</th>
                    <th className="px-3 py-3 font-semibold">مدفوع</th>
                    <th className="px-3 py-3 font-semibold">متبقي</th>
                    <th className="px-3 py-3 font-semibold">الحالة</th>
                    <th className="px-3 py-3 font-semibold">التواريخ</th>
                    <th className="px-3 py-3 text-center font-semibold">إجراءات</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/70">
                  {invoices.items.map((inv) => (
                    <tr key={inv.id}>
                      <td className="max-w-0 truncate px-3 py-3 font-semibold text-navy" title={inv.invoiceNumber}>{inv.invoiceNumber}</td>
                      <td className="max-w-0 truncate px-3 py-3 text-muted-foreground" title={inv.customerName}>{inv.customerName}</td>
                      <td className="max-w-0 truncate px-3 py-3 text-muted-foreground" title={inv.projectName}>{inv.projectName}</td>
                      <td className="px-3 py-3 text-muted-foreground">{formatCurrency(inv.totalWithTax, inv.currency)}</td>
                      <td className="px-3 py-3 text-muted-foreground">{formatCurrency(inv.paidAmount, inv.currency)}</td>
                      <td className="px-3 py-3 font-semibold text-navy">{formatCurrency(inv.remainingAmount, inv.currency)}</td>
                      <td className="px-3 py-3">
                        <span className={`inline-flex rounded-full px-2 py-1 text-xs font-bold ${statusClass(inv.status)}`}>
                          {statusLabel(inv.status)}
                        </span>
                      </td>
                      <td className="px-3 py-3 text-muted-foreground">
                        <div className="text-xs">إصدار: {formatDate(inv.issueDate)}</div>
                        <div className="text-xs">استحقاق: {formatDate(inv.dueDate)}</div>
                      </td>
                      <td className="px-3 py-3">
                        <div className="flex justify-center gap-1">
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-8 w-8 rounded-lg"
                            onClick={() => void openDetails(inv)}
                            disabled={isBusy}
                            title="عرض"
                          >
                            <Eye className="h-3.5 w-3.5" />
                          </Button>

                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-8 w-8 rounded-lg"
                            onClick={() => openPaymentDialog(inv, "payment")}
                            disabled={isBusy}
                            title="تسجيل دفعة"
                          >
                            <Wallet className="h-3.5 w-3.5" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-8 w-8 rounded-lg"
                            onClick={() => void runAction("send", inv.id)}
                            disabled={isBusy || normalizeStatus(inv.status) === "Cancelled" || normalizeStatus(inv.status) === "Paid"}
                            title="إرسال"
                          >
                            <Send className="h-3.5 w-3.5" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-8 w-8 rounded-lg text-danger hover:text-danger"
                            onClick={() => setDeleteTarget(inv)}
                            disabled={isBusy}
                            title="حذف"
                          >
                            <Trash2 className="h-3.5 w-3.5" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="mt-4 flex items-center justify-between text-sm text-muted-foreground">
              <span>
                صفحة {invoices.pageNumber} من {invoices.totalPages || 1}
              </span>
              <div className="flex gap-2">
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy || pageNumber <= 1}
                  onClick={() => setPageNumber((value) => Math.max(1, value - 1))}
                >
                  السابق
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy || pageNumber >= (invoices.totalPages || 1)}
                  onClick={() => setPageNumber((value) => value + 1)}
                >
                  التالي
                </Button>
              </div>
            </div>
          </>
        ) : (
          <div className="rounded-xl bg-muted/40 p-10 text-center text-sm text-muted-foreground">لا توجد فواتير للعرض.</div>
        )}
      </div>

      <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right">
            <DialogTitle>فاتورة جديدة</DialogTitle>
            <DialogDescription>أنشئ فاتورة مرتبطة بمشروع موجود.</DialogDescription>
          </DialogHeader>

          <form onSubmit={submitInvoice} className="space-y-4">
            <div className="space-y-2">
              <Label className="flex items-center gap-1 text-navy">
                المشروع <span className="text-danger">*</span>
              </Label>
              <Select
                dir="rtl"
                value={invoiceForm.projectId}
                onValueChange={(value) => {
                  const selectedProject = projects.find((project) => project.id === value);
                  setInvoiceForm((prev) => ({
                    ...prev,
                    projectId: value,
                    totalAmount:
                      typeof selectedProject?.suggestedPrice === "number"
                        ? formatNumberInputValue(selectedProject.suggestedPrice)
                        : prev.totalAmount,
                  }));
                }}
              >
                <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                  <SelectValue placeholder="اختر المشروع" />
                </SelectTrigger>
                <SelectContent dir="rtl" className="text-right">
                  {projects.map((project) => (
                    <SelectItem key={project.id} value={project.id}>
                      {project.projectName} - {project.customerName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="invoiceNumber" className="flex items-center gap-1 text-navy">
                  رقم الفاتورة <span className="text-danger">*</span>
                </Label>
                <Input
                  id="invoiceNumber"
                  value={invoiceForm.invoiceNumber}
                  onChange={(event) => setInvoiceForm((prev) => ({ ...prev, invoiceNumber: event.target.value }))}
                  required
                  className="h-11 rounded-xl bg-white"
                />
              </div>

              <div className="space-y-2">
                <Label className="flex items-center gap-1 text-navy">
                  العملة <span className="text-danger">*</span>
                </Label>
                <Select
                  dir="rtl"
                  value={invoiceForm.currency}
                  onValueChange={(value) => setInvoiceForm((prev) => ({ ...prev, currency: value }))}
                >
                  <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue placeholder="اختر العملة" />
                  </SelectTrigger>
                  <SelectContent align="end" className="text-right">
                    {CURRENCY_OPTIONS.map((option) => (
                      <SelectItem key={option.value} value={option.value} className="justify-end text-right">
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="totalAmount" className="flex items-center gap-1 text-navy">
                  المبلغ قبل الضريبة <span className="text-danger">*</span>
                </Label>
                <Input
                  id="totalAmount"
                  type="text"
                  inputMode="decimal"
                  dir="rtl"
                  lang="en"
                  value={invoiceForm.totalAmount}
                  onChange={(event) =>
                    setInvoiceForm((prev) => ({ ...prev, totalAmount: normalizeNumberInputValue(event.target.value) }))
                  }
                  required
                  className="h-11 rounded-xl bg-white text-right"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="advanceAmount" className="flex items-center gap-1 text-navy">
                  دفعة مقدمة <span className="text-xs font-normal text-muted-foreground">اختياري</span>
                </Label>
                <Input
                  id="advanceAmount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={invoiceForm.advanceAmount}
                  onChange={(event) => setInvoiceForm((prev) => ({ ...prev, advanceAmount: event.target.value }))}
                  className="h-11 rounded-xl bg-white"
                />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="issueDate" className="flex items-center gap-1 text-navy">
                  تاريخ الإصدار <span className="text-danger">*</span>
                </Label>
                <Input
                  id="issueDate"
                  type="date"
                  value={invoiceForm.issueDate}
                  onChange={(event) => setInvoiceForm((prev) => ({ ...prev, issueDate: event.target.value }))}
                  required
                  className="h-11 rounded-xl bg-white"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="dueDate" className="flex items-center gap-1 text-navy">
                  تاريخ الاستحقاق <span className="text-danger">*</span>
                </Label>
                <Input
                  id="dueDate"
                  type="date"
                  value={invoiceForm.dueDate}
                  onChange={(event) => setInvoiceForm((prev) => ({ ...prev, dueDate: event.target.value }))}
                  required
                  className="h-11 rounded-xl bg-white"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="invoiceNotes" className="flex items-center gap-1 text-navy">
                ملاحظات <span className="text-xs font-normal text-muted-foreground">اختياري</span>
              </Label>
              <Textarea
                id="invoiceNotes"
                value={invoiceForm.notes}
                onChange={(event) => setInvoiceForm((prev) => ({ ...prev, notes: event.target.value }))}
                className="min-h-24 rounded-xl bg-white"
              />
            </div>

            <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
              <Button type="submit" disabled={isBusy} className="rounded-xl bg-teal font-bold text-white hover:bg-teal/90">
                {isBusy ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                إنشاء الفاتورة
              </Button>
              <Button type="button" variant="outline" disabled={isBusy} className="rounded-xl" onClick={() => setIsCreateOpen(false)}>
                إلغاء
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={isDetailsOpen} onOpenChange={setIsDetailsOpen}>
        <DialogContent className="max-h-[90vh] w-[calc(100vw-2rem)] max-w-4xl overflow-y-auto overflow-x-hidden text-right" dir="rtl">
          <DialogHeader className="text-right">
            <DialogTitle>عرض الفاتورة</DialogTitle>
            <DialogDescription>معاينة الفاتورة بنفس تنسيق الطباعة، مع إمكانية الطباعة أو الحفظ PDF.</DialogDescription>
          </DialogHeader>

          {selectedInvoice ? (
            <div className="space-y-4">
              <div className="flex flex-wrap justify-end gap-2">
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy}
                  onClick={() => window.print()}
                >
                  <Printer className="h-4 w-4" />
                  طباعة
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy}
                  onClick={() => window.print()}
                >
                  <Download className="h-4 w-4" />
                  حفظ PDF
                </Button>
              </div>

              <InvoicePrintPreview invoice={selectedInvoice} />

              <div className="grid gap-3 rounded-xl border border-border/70 bg-muted/20 p-4 sm:grid-cols-2">
                <div>
                  <div className="text-xs text-muted-foreground">رقم الفاتورة</div>
                  <div className="font-semibold text-navy">{selectedInvoice.invoiceNumber}</div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">الحالة</div>
                  <span className={`inline-flex rounded-full px-2 py-1 text-xs font-bold ${statusClass(selectedInvoice.status)}`}>
                    {statusLabel(selectedInvoice.status)}
                  </span>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">العميل</div>
                  <div className="font-semibold text-navy">{selectedInvoice.customerName}</div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">المشروع</div>
                  <div className="font-semibold text-navy">{selectedInvoice.projectName}</div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">معرف العميل</div>
                  <div className="break-all font-mono text-xs text-navy">{selectedInvoice.customerId}</div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">معرف المشروع</div>
                  <div className="break-all font-mono text-xs text-navy">{selectedInvoice.projectId}</div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">تاريخ الإصدار</div>
                  <div className="text-muted-foreground">{formatDate(selectedInvoice.issueDate)}</div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">تاريخ الاستحقاق</div>
                  <div className="text-muted-foreground">{formatDate(selectedInvoice.dueDate)}</div>
                </div>
              </div>

              <div className="grid gap-3 sm:grid-cols-3">
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">الإجمالي قبل الضريبة</div>
                  <div className="mt-1 text-lg font-bold text-navy">{formatCurrency(selectedInvoice.totalAmount, selectedInvoice.currency)}</div>
                </div>
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">الضريبة</div>
                  <div className="mt-1 text-lg font-bold text-navy">{formatCurrency(selectedInvoice.taxAmount, selectedInvoice.currency)}</div>
                </div>
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">الإجمالي (شامل الضريبة)</div>
                  <div className="mt-1 text-lg font-bold text-navy">
                    {formatCurrency(selectedInvoice.totalWithTax, selectedInvoice.currency)}
                  </div>
                </div>
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">مدفوع</div>
                  <div className="mt-1 text-lg font-bold text-navy">{formatCurrency(selectedInvoice.paidAmount, selectedInvoice.currency)}</div>
                </div>
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">متبقي</div>
                  <div className="mt-1 text-lg font-bold text-navy">
                    {formatCurrency(selectedInvoice.remainingAmount, selectedInvoice.currency)}
                  </div>
                </div>
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">الدفعة المقدمة</div>
                  <div className="mt-1 text-lg font-bold text-navy">{formatCurrency(selectedInvoice.advanceAmount, selectedInvoice.currency)}</div>
                </div>
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">المتبقي من المقدمة</div>
                  <div className="mt-1 text-lg font-bold text-navy">{formatCurrency(selectedInvoice.advanceRemainingAmount, selectedInvoice.currency)}</div>
                </div>
              </div>

              {selectedInvoice.notes ? (
                <div className="rounded-xl border border-border/70 bg-card p-4">
                  <div className="text-xs text-muted-foreground">ملاحظات</div>
                  <p className="mt-1 whitespace-pre-wrap text-sm text-muted-foreground">{selectedInvoice.notes}</p>
                </div>
              ) : null}

              <div className="grid gap-3 rounded-xl border border-border/70 bg-muted/20 p-4 sm:grid-cols-2">
                <div>
                  <div className="text-xs text-muted-foreground">تاريخ الإضافة</div>
                  <div className="font-semibold text-navy">{formatDate(selectedInvoice.createdAtUtc)}</div>
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">آخر تعديل</div>
                  <div className="font-semibold text-navy">{formatDate(selectedInvoice.lastModifiedUtc)}</div>
                </div>
                <div className="sm:col-span-2">
                  <div className="text-xs text-muted-foreground">معرف الفاتورة</div>
                  <div className="break-all font-mono text-xs text-navy">{selectedInvoice.id}</div>
                </div>
              </div>

              <div className="rounded-xl border border-border/70 bg-card p-4">
                <div className="mb-2 font-semibold text-navy">المدفوعات</div>
                {selectedInvoice.payments?.length ? (
                  <div className="grid gap-3 sm:grid-cols-2">
                    {selectedInvoice.payments.map((p) => (
                      <div key={p.id} className="rounded-xl border border-border/70 bg-muted/20 p-3">
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <div className="text-xs text-muted-foreground">التاريخ</div>
                            <div className="font-semibold text-navy">{formatDate(p.paymentDate)}</div>
                          </div>
                          <div className="text-left">
                            <div className="text-xs text-muted-foreground">المبلغ</div>
                            <div className="font-bold text-navy">{formatCurrency(p.amount, p.currency)}</div>
                          </div>
                        </div>
                        <div className="mt-3 grid gap-2 text-xs text-muted-foreground sm:grid-cols-2">
                          <div>
                            <span className="block">الطريقة</span>
                            <span className="font-semibold text-navy">{methodLabel(p.method)}</span>
                          </div>
                          <div>
                            <span className="block">تاريخ الإضافة</span>
                            <span className="font-semibold text-navy">{formatDate(p.createdAtUtc)}</span>
                          </div>
                        </div>
                        <div className="mt-3 text-xs text-muted-foreground">
                          <span className="block">ملاحظات</span>
                          <span className="whitespace-pre-wrap text-navy">{p.notes || "-"}</span>
                        </div>
                        <div className="mt-3 text-xs text-muted-foreground">
                          <span className="block">المعرف</span>
                          <span className="block break-all font-mono text-[11px] text-navy">{p.id}</span>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="rounded-lg bg-muted/40 p-4 text-sm text-muted-foreground">لا توجد مدفوعات مسجلة.</div>
                )}
              </div>

              <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy}
                  onClick={() => void runAction("mark-overdue", selectedInvoice.id)}
                >
                  تعيين كمتأخرة
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy || normalizeStatus(selectedInvoice.status) === "Cancelled" || normalizeStatus(selectedInvoice.status) === "Paid"}
                  onClick={() => void runAction("send", selectedInvoice.id)}
                >
                  إرسال
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy}
                  onClick={() => window.print()}
                >
                  حفظ PDF
                </Button>
                <Button type="button" className="rounded-xl bg-teal font-bold text-white hover:bg-teal/90" disabled={isBusy} onClick={() => openPaymentDialog(selectedInvoice, "payment")}>
                  تسجيل دفعة
                </Button>
                <Button type="button" variant="outline" className="rounded-xl" disabled={isBusy} onClick={() => openPaymentDialog(selectedInvoice, "advance")}>
                  دفعة مقدمة
                </Button>
                <Button type="button" variant="outline" className="rounded-xl" onClick={() => setIsDetailsOpen(false)}>
                  إغلاق
                </Button>
              </DialogFooter>
            </div>
          ) : (
            <div className="flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
              <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
              جاري تحميل التفاصيل...
            </div>
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={isPaymentOpen} onOpenChange={setIsPaymentOpen}>
        <DialogContent className="text-right" dir="rtl">
          <DialogHeader className="text-right">
            <DialogTitle>{isAdvancePayment ? "تسجيل دفعة مقدمة" : "تسجيل دفعة"}</DialogTitle>
            <DialogDescription>
              {paymentTarget ? `الفاتورة: ${paymentTarget.invoiceNumber}` : "اختر فاتورة لتسجيل الدفعة."}
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={submitPayment} className="space-y-4">
            {!isAdvancePayment ? (
              <div className="space-y-2">
                <Label htmlFor="amount">المبلغ</Label>
                <Input
                  id="amount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={paymentForm.amount}
                  onChange={(e) => setPaymentForm((p) => ({ ...p, amount: e.target.value }))}
                  required
                  className="h-11 rounded-xl bg-white"
                />
              </div>
            ) : null}

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>الطريقة</Label>
                <Select
                  dir="rtl"
                  value={paymentForm.method}
                  onValueChange={(value) => setPaymentForm((p) => ({ ...p, method: value as PaymentMethod }))}
                >
                  <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    <SelectItem value="BankTransfer">تحويل بنكي</SelectItem>
                    <SelectItem value="Cash">نقدا</SelectItem>
                    <SelectItem value="CreditCard">بطاقة</SelectItem>
                    <SelectItem value="PayPal">PayPal</SelectItem>
                    <SelectItem value="Other">أخرى</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="paymentDate">التاريخ</Label>
                <Input
                  id="paymentDate"
                  type="date"
                  value={paymentForm.paymentDate}
                  onChange={(e) => setPaymentForm((p) => ({ ...p, paymentDate: e.target.value }))}
                  required
                  className="h-11 rounded-xl bg-white"
                />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>العملة</Label>
                <Select
                  dir="rtl"
                  value={paymentForm.currency}
                  onValueChange={(value) => setPaymentForm((p) => ({ ...p, currency: value }))}
                >
                  <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue placeholder="اختر العملة" />
                  </SelectTrigger>
                  <SelectContent align="end" className="text-right">
                    {CURRENCY_OPTIONS.map((option) => (
                      <SelectItem key={option.value} value={option.value} className="justify-end text-right">
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label>المتبقي</Label>
                <Input
                  value={
                    paymentTarget ? formatCurrency(remainingAfterPayment, paymentTarget.currency) : ""
                  }
                  readOnly
                  className="h-11 rounded-xl bg-muted/40"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="notes">ملاحظات</Label>
              <Textarea
                id="notes"
                value={paymentForm.notes}
                onChange={(e) => setPaymentForm((p) => ({ ...p, notes: e.target.value }))}
                className="min-h-24 rounded-xl bg-white"
              />
            </div>

            <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
              <Button type="submit" disabled={isBusy} className="rounded-xl bg-teal font-bold text-white hover:bg-teal/90">
                {isBusy ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                حفظ
              </Button>
              <Button type="button" variant="outline" disabled={isBusy} className="rounded-xl" onClick={() => setIsPaymentOpen(false)}>
                إلغاء
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(open) => !open && setDeleteTarget(null)}>
        <DialogContent className="text-right" dir="rtl">
          <DialogHeader className="text-right">
            <DialogTitle>حذف الفاتورة</DialogTitle>
            <DialogDescription>هل تريد حذف {deleteTarget?.invoiceNumber}؟</DialogDescription>
          </DialogHeader>
          <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
            <Button
              type="button"
              disabled={isBusy}
              className="rounded-xl bg-danger text-danger-foreground hover:bg-danger/90"
              onClick={() => void handleDelete()}
            >
              {isBusy ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : <Trash2 className="ml-2 h-4 w-4" />}
              حذف
            </Button>
            <Button type="button" variant="outline" disabled={isBusy} className="rounded-xl" onClick={() => setDeleteTarget(null)}>
              إلغاء
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
