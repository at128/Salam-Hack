import { useEffect, useMemo, useState } from "react";
import { AlertTriangle, Banknote, CalendarDays, Loader2, Plus, RefreshCw, Search, Wallet } from "lucide-react";
import { PageHeader } from "@/components/dashboard/DashboardLayout";
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
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type PaymentMethod = "BankTransfer" | "Cash" | "CreditCard" | "PayPal" | "Other";
type InvoiceStatus = "Draft" | "Sent" | "PartiallyPaid" | "Paid" | "Overdue" | "Cancelled";

type PaymentDto = {
  id: string;
  invoiceId: string;
  amount: number;
  method: PaymentMethod | string;
  paymentDate: string;
  notes?: string | null;
  currency: string;
  createdAtUtc: string;
};

type PaymentSummary = {
  totalInvoiced: number;
  totalCollected: number;
  totalOutstanding: number;
  totalOverdue: number;
  collectionRatePercent: number;
  paidInvoiceCount: number;
  pendingInvoiceCount: number;
  overdueInvoiceCount: number;
  overdueInvoices: OverdueInvoice[];
};

type OverdueInvoice = {
  invoiceId: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  remainingAmount: number;
  dueDate: string;
  daysOverdue: number;
  currency: string;
};

type InvoiceListItem = {
  id: string;
  invoiceNumber: string;
  customerName: string;
  projectName: string;
  totalWithTax: number;
  paidAmount: number;
  remainingAmount: number;
  status: InvoiceStatus | string;
  dueDate: string;
  currency: string;
};

type PaginatedList<T> = {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  items: T[];
};

type PaymentForm = {
  invoiceId: string;
  amount: string;
  method: PaymentMethod;
  paymentDate: string;
  currency: string;
  notes: string;
};

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const PAYMENTS_API_URL = `${API_BASE_URL}/api/v1/payments`;
const INVOICES_API_URL = `${API_BASE_URL}/api/v1/invoices`;

const today = new Date().toISOString().slice(0, 10);

const EMPTY_PAYMENT_FORM: PaymentForm = {
  invoiceId: "",
  amount: "",
  method: "BankTransfer",
  paymentDate: today,
  currency: "SAR",
  notes: "",
};

const METHOD_OPTIONS: { value: PaymentMethod; label: string }[] = [
  { value: "BankTransfer", label: "تحويل بنكي" },
  { value: "Cash", label: "نقدا" },
  { value: "CreditCard", label: "بطاقة" },
  { value: "PayPal", label: "PayPal" },
  { value: "Other", label: "أخرى" },
];

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

  if (!response.ok) {
    throw payload ?? new Error(getApiErrorMessage(payload, "تعذر تنفيذ الطلب."));
  }

  return unwrapApiResponse<T>(payload);
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("ar", { dateStyle: "medium" }).format(date);
}

function formatCurrency(amount: number | undefined, currency = "SAR") {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency: currency?.trim() || "SAR",
    maximumFractionDigits: 2,
  }).format(amount ?? 0);
}

function formatPercent(value: number | undefined) {
  return new Intl.NumberFormat("ar", {
    maximumFractionDigits: 1,
  }).format(value ?? 0);
}

function methodLabel(method: string) {
  return METHOD_OPTIONS.find((option) => option.value === method)?.label ?? method;
}

function statusLabel(status: string) {
  switch (normalizeStatus(status)) {
    case "sent":
      return "مرسلة";
    case "partiallypaid":
      return "مدفوعة جزئيا";
    case "paid":
      return "مدفوعة";
    case "overdue":
      return "متأخرة";
    case "cancelled":
      return "ملغاة";
    default:
      return "مسودة";
  }
}

function normalizeStatus(status: string) {
  const normalized = status.toLowerCase();
  if (normalized === "sent" || status === "مرسلة") return "sent";
  if (normalized === "partiallypaid" || normalized === "partially paid" || status === "مدفوعة جزئيا" || status === "مدفوعة جزئياً") return "partiallypaid";
  if (normalized === "paid" || status === "مدفوعة") return "paid";
  if (normalized === "overdue" || status === "متأخرة") return "overdue";
  if (normalized === "cancelled" || normalized === "canceled" || status === "ملغاة") return "cancelled";
  return "draft";
}

export default function PaymentsPage() {
  const [payments, setPayments] = useState<PaginatedList<PaymentDto> | null>(null);
  const [summary, setSummary] = useState<PaymentSummary | null>(null);
  const [invoices, setInvoices] = useState<InvoiceListItem[]>([]);
  const [invoiceId, setInvoiceId] = useState("all");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isPaymentOpen, setIsPaymentOpen] = useState(false);
  const [paymentForm, setPaymentForm] = useState<PaymentForm>(EMPTY_PAYMENT_FORM);

  const pageSize = 10;

  const invoiceById = useMemo(() => {
    return new Map(invoices.map((invoice) => [invoice.id, invoice]));
  }, [invoices]);

  const filteredPayments = useMemo(() => {
    const term = search.trim().toLowerCase();
    const items = payments?.items ?? [];
    if (!term) return items;

    return items.filter((payment) => {
      const invoice = invoiceById.get(payment.invoiceId);
      return [
        payment.id,
        payment.invoiceId,
        payment.notes ?? "",
        payment.currency,
        methodLabel(payment.method),
        invoice?.invoiceNumber ?? "",
        invoice?.customerName ?? "",
        invoice?.projectName ?? "",
      ]
        .join(" ")
        .toLowerCase()
        .includes(term);
    });
  }, [invoiceById, payments?.items, search]);

  const selectedInvoice = paymentForm.invoiceId ? invoiceById.get(paymentForm.invoiceId) : undefined;

  const loadPayments = async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = buildQuery({
        invoiceId: invoiceId === "all" ? undefined : invoiceId,
        fromDate: fromDate ? dateInputToIso(fromDate) : undefined,
        toDate: toDate ? dateInputToIso(toDate) : undefined,
        pageNumber,
        pageSize,
      });

      const [paymentsResult, summaryResult] = await Promise.all([
        apiRequest<PaginatedList<PaymentDto>>(`${PAYMENTS_API_URL}${query}`),
        apiRequest<PaymentSummary>(`${PAYMENTS_API_URL}/summary`),
      ]);

      setPayments(paymentsResult);
      setSummary(summaryResult);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل المدفوعات."));
    } finally {
      setIsLoading(false);
    }
  };

  const loadInvoices = async () => {
    try {
      const result = await apiRequest<PaginatedList<InvoiceListItem>>(`${INVOICES_API_URL}?pageSize=100`);
      setInvoices(result.items ?? []);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل الفواتير."));
    }
  };

  useEffect(() => {
    void loadInvoices();
  }, []);

  useEffect(() => {
    void loadPayments();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [invoiceId, fromDate, toDate, pageNumber]);

  const openPaymentDialog = (invoice?: InvoiceListItem) => {
    setPaymentForm({
      ...EMPTY_PAYMENT_FORM,
      invoiceId: invoice?.id ?? "",
      amount: invoice ? String(invoice.remainingAmount) : "",
      currency: invoice?.currency ?? "SAR",
    });
    setMessage("");
    setError("");
    setIsPaymentOpen(true);
  };

  const submitPayment = async (event: React.FormEvent) => {
    event.preventDefault();
    const invoice = invoiceById.get(paymentForm.invoiceId);
    if (!invoice) return;

    const currency = paymentForm.currency.trim() || invoice.currency;
    if (currency.toUpperCase() !== invoice.currency.toUpperCase()) {
      setError("يجب أن تكون عملة الدفعة نفس عملة الفاتورة.");
      return;
    }

    setIsSaving(true);
    setError("");
    setMessage("");

    try {
      await apiRequest(`${INVOICES_API_URL}/${invoice.id}/payments`, {
        method: "POST",
        body: JSON.stringify({
          amount: Number(paymentForm.amount),
          method: paymentForm.method,
          paymentDate: dateInputToIso(paymentForm.paymentDate),
          currency,
          notes: paymentForm.notes.trim() || null,
        }),
      });

      setMessage("تم تسجيل الدفعة بنجاح.");
      setIsPaymentOpen(false);
      setPaymentForm(EMPTY_PAYMENT_FORM);
      await Promise.all([loadInvoices(), loadPayments()]);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تسجيل الدفعة."));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="المدفوعات" desc="متابعة التحصيل، الدفعات المسجلة، والفواتير المتأخرة." />
        <Button onClick={() => openPaymentDialog()} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
          <Plus className="ml-2 h-4 w-4" />
          تسجيل دفعة
        </Button>
      </div>

      {(message || error) && (
        <div
          className={`rounded-xl border p-3 text-sm ${
            error ? "border-danger/30 bg-danger-soft text-danger" : "border-success/30 bg-success-soft text-success"
          }`}
        >
          {error || message}
        </div>
      )}

      <section className="grid gap-4 md:grid-cols-4">
        <SummaryCard icon={Wallet} label="إجمالي المفوتر" value={formatCurrency(summary?.totalInvoiced)} />
        <SummaryCard icon={Banknote} label="المحصل" value={formatCurrency(summary?.totalCollected)} />
        <SummaryCard icon={CalendarDays} label="المتبقي" value={formatCurrency(summary?.totalOutstanding)} />
        <SummaryCard icon={AlertTriangle} label="متأخر" value={formatCurrency(summary?.totalOverdue)} tone="warning" />
      </section>

      <section className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-4 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <h3 className="font-bold text-navy">سجل المدفوعات</h3>
              <p className="mt-1 text-xs text-muted-foreground">
                نسبة التحصيل: {formatPercent(summary?.collectionRatePercent)}%
              </p>
            </div>
            <Button type="button" variant="outline" className="h-10 w-10 rounded-xl p-0" onClick={() => void loadPayments()}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>

          <div className="mb-4 grid gap-3 lg:grid-cols-[minmax(0,1fr)_180px_150px_150px]">
            <div className="relative">
              <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="ابحث برقم الفاتورة أو العميل"
                className="h-11 rounded-xl bg-white pr-10"
              />
            </div>

            <Select
              dir="rtl"
              value={invoiceId}
              onValueChange={(value) => {
                setInvoiceId(value);
                setPageNumber(1);
              }}
            >
              <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                <SelectValue placeholder="الفاتورة" />
              </SelectTrigger>
              <SelectContent dir="rtl" className="text-right">
                <SelectItem value="all">كل الفواتير</SelectItem>
                {invoices.map((invoice) => (
                  <SelectItem key={invoice.id} value={invoice.id}>
                    {invoice.invoiceNumber}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Input type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} className="h-11 rounded-xl bg-white" />
            <Input type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} className="h-11 rounded-xl bg-white" />
          </div>

          {isLoading ? (
            <div className="flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
              <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
              جاري تحميل المدفوعات...
            </div>
          ) : filteredPayments.length ? (
            <>
              <div className="overflow-hidden rounded-xl border border-border/70">
                <table className="w-full text-right text-sm">
                  <thead className="bg-muted/50 text-xs text-muted-foreground">
                    <tr>
                      <th className="px-4 py-3 font-semibold">الفاتورة</th>
                      <th className="px-4 py-3 font-semibold">العميل</th>
                      <th className="px-4 py-3 font-semibold">المبلغ</th>
                      <th className="px-4 py-3 font-semibold">الطريقة</th>
                      <th className="px-4 py-3 font-semibold">التاريخ</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border/70">
                    {filteredPayments.map((payment) => {
                      const invoice = invoiceById.get(payment.invoiceId);
                      return (
                        <tr key={payment.id}>
                          <td className="px-4 py-3">
                            <div className="font-semibold text-navy">{invoice?.invoiceNumber ?? payment.invoiceId}</div>
                            {payment.notes ? <div className="mt-1 text-xs text-muted-foreground">{payment.notes}</div> : null}
                          </td>
                          <td className="px-4 py-3 text-muted-foreground">{invoice?.customerName ?? "-"}</td>
                          <td className="px-4 py-3 font-bold text-teal">{formatCurrency(payment.amount, payment.currency)}</td>
                          <td className="px-4 py-3 text-muted-foreground">{methodLabel(payment.method)}</td>
                          <td className="px-4 py-3 text-muted-foreground">{formatDate(payment.paymentDate)}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              <div className="mt-4 flex items-center justify-between text-sm text-muted-foreground">
                <span>
                  صفحة {payments?.pageNumber ?? 1} من {payments?.totalPages || 1}
                </span>
                <div className="flex gap-2">
                  <Button type="button" variant="outline" className="rounded-xl" disabled={pageNumber <= 1} onClick={() => setPageNumber((value) => Math.max(1, value - 1))}>
                    السابق
                  </Button>
                  <Button type="button" variant="outline" className="rounded-xl" disabled={pageNumber >= (payments?.totalPages || 1)} onClick={() => setPageNumber((value) => value + 1)}>
                    التالي
                  </Button>
                </div>
              </div>
            </>
          ) : (
            <div className="rounded-xl bg-muted/40 p-10 text-center text-sm text-muted-foreground">
              لا توجد مدفوعات مطابقة.
            </div>
          )}
        </div>

        <aside className="space-y-4">
          <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
            <h3 className="font-bold text-navy">حالة الفواتير</h3>
            <div className="mt-4 grid grid-cols-3 gap-3 text-center text-sm">
              <MiniStat label="مدفوعة" value={summary?.paidInvoiceCount ?? 0} />
              <MiniStat label="معلقة" value={summary?.pendingInvoiceCount ?? 0} />
              <MiniStat label="متأخرة" value={summary?.overdueInvoiceCount ?? 0} />
            </div>
          </div>

          <div className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
            <h3 className="font-bold text-navy">فواتير متأخرة</h3>
            <div className="mt-4 space-y-3">
              {summary?.overdueInvoices?.length ? (
                summary.overdueInvoices.map((invoice) => (
                  <button
                    key={invoice.invoiceId}
                    type="button"
                    className="w-full rounded-xl border border-border/70 bg-muted/30 p-3 text-right transition hover:border-teal/50"
                    onClick={() => openPaymentDialog(invoiceById.get(invoice.invoiceId))}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <div className="font-semibold text-navy">{invoice.invoiceNumber}</div>
                        <div className="mt-1 text-xs text-muted-foreground">{invoice.customerName}</div>
                      </div>
                      <span className="rounded-full bg-warning-soft px-2 py-1 text-xs font-bold text-warning">
                        {invoice.daysOverdue} يوم
                      </span>
                    </div>
                    <div className="mt-3 font-bold text-danger">{formatCurrency(invoice.remainingAmount, invoice.currency)}</div>
                  </button>
                ))
              ) : (
                <div className="rounded-xl bg-muted/40 p-6 text-center text-sm text-muted-foreground">
                  لا توجد فواتير متأخرة.
                </div>
              )}
            </div>
          </div>
        </aside>
      </section>

      <Dialog open={isPaymentOpen} onOpenChange={setIsPaymentOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>تسجيل دفعة</DialogTitle>
            <DialogDescription>اختر الفاتورة وسجل مبلغ الدفعة بنفس عملة الفاتورة.</DialogDescription>
          </DialogHeader>

          <form onSubmit={submitPayment} className="space-y-4">
            <div className="space-y-2">
              <Label>الفاتورة</Label>
              <Select
                dir="rtl"
                value={paymentForm.invoiceId}
                onValueChange={(value) => {
                  const invoice = invoiceById.get(value);
                  setPaymentForm((prev) => ({
                    ...prev,
                    invoiceId: value,
                    amount: invoice ? String(invoice.remainingAmount) : prev.amount,
                    currency: invoice?.currency ?? prev.currency,
                  }));
                }}
              >
                <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                  <SelectValue placeholder="اختر الفاتورة" />
                </SelectTrigger>
                <SelectContent dir="rtl" className="text-right">
                  {invoices
                    .filter((invoice) => invoice.remainingAmount > 0 && !["paid", "cancelled"].includes(normalizeStatus(invoice.status)))
                    .map((invoice) => (
                      <SelectItem key={invoice.id} value={invoice.id}>
                        {invoice.invoiceNumber} - {invoice.customerName}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            </div>

            {selectedInvoice ? (
              <div className="rounded-xl border border-border/70 bg-muted/40 p-3 text-sm">
                <div className="flex items-center justify-between gap-3">
                  <span className="text-muted-foreground">المتبقي</span>
                  <span className="font-bold text-navy">{formatCurrency(selectedInvoice.remainingAmount, selectedInvoice.currency)}</span>
                </div>
                <div className="mt-2 flex items-center justify-between gap-3">
                  <span className="text-muted-foreground">الحالة</span>
                  <span className="font-semibold text-navy">{statusLabel(selectedInvoice.status)}</span>
                </div>
              </div>
            ) : null}

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="amount">المبلغ</Label>
                <Input
                  id="amount"
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={paymentForm.amount}
                  onChange={(event) => setPaymentForm((prev) => ({ ...prev, amount: event.target.value }))}
                  required
                  className="rounded-xl bg-white"
                />
              </div>
              <div className="space-y-2">
                <Label>طريقة الدفع</Label>
                <Select dir="rtl" value={paymentForm.method} onValueChange={(value) => setPaymentForm((prev) => ({ ...prev, method: value as PaymentMethod }))}>
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    {METHOD_OPTIONS.map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="paymentDate">التاريخ</Label>
                <Input
                  id="paymentDate"
                  type="date"
                  value={paymentForm.paymentDate}
                  onChange={(event) => setPaymentForm((prev) => ({ ...prev, paymentDate: event.target.value }))}
                  required
                  className="rounded-xl bg-white"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="currency">العملة</Label>
                <Input
                  id="currency"
                  value={paymentForm.currency}
                  onChange={(event) => setPaymentForm((prev) => ({ ...prev, currency: event.target.value.toUpperCase() }))}
                  required
                  className="rounded-xl bg-white"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="notes">ملاحظات</Label>
              <Textarea
                id="notes"
                value={paymentForm.notes}
                onChange={(event) => setPaymentForm((prev) => ({ ...prev, notes: event.target.value }))}
                className="rounded-xl bg-white"
              />
            </div>

            <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
              <Button type="submit" disabled={isSaving || !paymentForm.invoiceId} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
                {isSaving ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                حفظ الدفعة
              </Button>
              <Button type="button" variant="outline" className="rounded-xl" onClick={() => setIsPaymentOpen(false)}>
                إلغاء
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </>
  );
}

function SummaryCard({
  icon: Icon,
  label,
  value,
  tone = "default",
}: {
  icon: typeof Wallet;
  label: string;
  value: string;
  tone?: "default" | "warning";
}) {
  return (
    <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
      <div className={`grid h-11 w-11 place-items-center rounded-xl ${tone === "warning" ? "bg-warning-soft text-warning" : "bg-teal-soft text-teal"}`}>
        <Icon className="h-5 w-5" />
      </div>
      <div className="mt-4 text-xl font-bold text-navy">{value}</div>
      <div className="mt-1 text-xs text-muted-foreground">{label}</div>
    </div>
  );
}

function MiniStat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl bg-muted/40 p-3">
      <div className="text-xl font-bold text-navy">{value}</div>
      <div className="mt-1 text-xs text-muted-foreground">{label}</div>
    </div>
  );
}
