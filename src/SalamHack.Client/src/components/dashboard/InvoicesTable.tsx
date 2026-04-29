import { useEffect, useMemo, useState } from "react";
import { Download, Eye, Loader2, Plus, RefreshCw, Send, Trash2, Wallet } from "lucide-react";
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
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type InvoiceStatus = "Draft" | "Sent" | "PartiallyPaid" | "Paid" | "Overdue" | "Cancelled";
type PaymentMethod = "BankTransfer" | "Cash" | "CreditCard" | "PayPal" | "Other";

type InvoiceListItem = {
  id: string;
  projectId: string;
  projectName: string;
  customerId: string;
  customerName: string;
  invoiceNumber: string;
  totalWithTax: number;
  paidAmount: number;
  remainingAmount: number;
  status: InvoiceStatus;
  issueDate: string;
  dueDate: string;
  currency: string;
};

type PaymentDto = {
  id: string;
  invoiceId: string;
  amount: number;
  method: PaymentMethod;
  paymentDate: string;
  notes?: string | null;
  currency: string;
  createdAtUtc: string;
};

type InvoiceDto = InvoiceListItem & {
  totalAmount: number;
  taxAmount: number;
  advanceAmount: number;
  advanceRemainingAmount: number;
  notes?: string | null;
  payments: PaymentDto[];
  createdAtUtc: string;
  lastModifiedUtc: string;
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

function statusLabel(status: InvoiceStatus | string) {
  switch (status) {
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
  switch (status) {
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

async function downloadInvoicePdf(invoiceId: string) {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${INVOICES_API_URL}/${invoiceId}/pdf`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const payload = await response.json().catch(() => null);
    throw payload ?? new Error(getApiErrorMessage(payload, "تعذر تصدير ملف PDF."));
  }

  const blob = await response.blob();
  const fileName = getFileNameFromContentDisposition(response.headers.get("content-disposition")) ?? `invoice-${invoiceId}.pdf`;
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.URL.revokeObjectURL(url);
}

function getFileNameFromContentDisposition(value: string | null) {
  if (!value) return null;
  const match = /filename\*?=(?:UTF-8''|")?([^;"\n]+)"?/i.exec(value);
  if (!match?.[1]) return null;
  try {
    return decodeURIComponent(match[1]);
  } catch {
    return match[1];
  }
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

export default function InvoicesTable() {
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
      overdue: items.filter((i) => i.status === "Overdue").length,
      paid: items.filter((i) => i.status === "Paid").length,
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
    setIsBusy(true);
    setError("");
    setMessage("");

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
    setIsBusy(true);
    setError("");
    setMessage("");

    try {
      const full = await apiRequest<InvoiceDto>(`${INVOICES_API_URL}/${invoice.id}`);
      setSelectedInvoice(full);
      setIsDetailsOpen(true);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل تفاصيل الفاتورة."));
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
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="الفواتير" desc="جميع فواتيرك مع حالاتها وتفاصيل العملاء." />
        <Button onClick={openCreateInvoice} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
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
            <div className="rounded-xl border border-border/70 overflow-hidden">
              <table className="w-full table-fixed text-right text-sm">
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
                            onClick={() =>
                              void (async () => {
                                setIsBusy(true);
                                setError("");
                                setMessage("");
                                try {
                                  await downloadInvoicePdf(inv.id);
                                } catch (err) {
                                  setError(getApiErrorMessage(err, "تعذر تنزيل ملف PDF."));
                                } finally {
                                  setIsBusy(false);
                                }
                              })()
                            }
                            disabled={isBusy}
                            title="PDF"
                          >
                            <Download className="h-3.5 w-3.5" />
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
                            disabled={isBusy || inv.status === "Cancelled" || inv.status === "Paid"}
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
              <Button type="submit" disabled={isBusy} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
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
            <DialogTitle>تفاصيل الفاتورة</DialogTitle>
            <DialogDescription>عرض تفاصيل الفاتورة والمدفوعات المرتبطة بها.</DialogDescription>
          </DialogHeader>

          {selectedInvoice ? (
            <div className="space-y-4">
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
                  disabled={isBusy || selectedInvoice.status === "Cancelled" || selectedInvoice.status === "Paid"}
                  onClick={() => void runAction("send", selectedInvoice.id)}
                >
                  إرسال
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={isBusy}
                  onClick={() =>
                    void (async () => {
                      setIsBusy(true);
                      setError("");
                      setMessage("");
                      try {
                        await downloadInvoicePdf(selectedInvoice.id);
                      } catch (err) {
                        setError(getApiErrorMessage(err, "تعذر تنزيل ملف PDF."));
                      } finally {
                        setIsBusy(false);
                      }
                    })()
                  }
                >
                  تنزيل PDF
                </Button>
                <Button type="button" className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90" disabled={isBusy} onClick={() => openPaymentDialog(selectedInvoice, "payment")}>
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
              <Button type="submit" disabled={isBusy} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
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
