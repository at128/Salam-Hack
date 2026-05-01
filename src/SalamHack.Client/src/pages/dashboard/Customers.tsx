import { useEffect, useMemo, useState } from "react";
import { Edit, Eye, Loader2, Plus, RefreshCw, Search, Trash2, Users } from "lucide-react";
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

type ClientType = "Company" | "Individual";

type CustomerListItem = {
  id: string;
  customerName: string;
  email: string;
  phone: string;
  clientType: ClientType;
  companyName?: string | null;
  projectsCount: number;
  createdAtUtc: string;
};

type Customer = CustomerListItem & {
  notes?: string | null;
  lastModifiedUtc?: string;
};

type PaginatedList<T> = {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  items: T[];
};

type CustomerForm = {
  customerName: string;
  email: string;
  phone: string;
  clientType: ClientType;
  companyName: string;
  notes: string;
};

type ValidationErrors = Partial<Record<keyof CustomerForm | "general", string[]>>;

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const CUSTOMERS_API_URL = `${API_BASE_URL}/api/v1/customers`;
const EMPTY_FORM: CustomerForm = {
  customerName: "",
  email: "",
  phone: "",
  clientType: "Individual",
  companyName: "",
  notes: "",
};

function buildQuery(params: Record<string, string | number | undefined>) {
  const searchParams = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === "") continue;
    searchParams.set(key, String(value));
  }

  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

async function customersRequest<T>(path = "", init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${CUSTOMERS_API_URL}${path}`, {
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

function normalizeValidationErrors(error: unknown): ValidationErrors {
  if (!error || typeof error !== "object") return {};

  const errors = (error as { errors?: unknown }).errors;
  if (!errors) return {};

  const normalized: ValidationErrors = {};

  if (Array.isArray(errors)) {
    const messages = errors
      .map((item) => {
        if (!item || typeof item !== "object") return null;
        return (item as { message?: string; description?: string }).message ?? (item as { description?: string }).description ?? null;
      })
      .filter((message): message is string => !!message);

    if (messages.length) normalized.general = messages;
    return normalized;
  }

  if (typeof errors === "object") {
    for (const [key, messages] of Object.entries(errors as Record<string, unknown>)) {
      if (!Array.isArray(messages)) continue;
      const field = key.charAt(0).toLowerCase() + key.slice(1);
      normalized[field as keyof CustomerForm] = messages.filter((message): message is string => typeof message === "string");
    }
  }

  return normalized;
}

function getErrorText(error: unknown, fallback: string) {
  return getApiErrorMessage(error, fallback);
}

function clientTypeLabel(type: ClientType | string) {
  return normalizeClientType(type) === "Company" ? "شركة" : "فرد";
}

function normalizeClientType(value: unknown): ClientType {
  if (value === "Company" || value === "company" || value === 0) return "Company";
  return "Individual";
}

function normalizeCustomerListItem(customer: CustomerListItem): CustomerListItem {
  return {
    ...customer,
    clientType: normalizeClientType(customer.clientType),
  };
}

function normalizeCustomer(customer: Customer): Customer {
  return {
    ...customer,
    clientType: normalizeClientType(customer.clientType),
  };
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";

  return new Intl.DateTimeFormat("ar", {
    dateStyle: "medium",
  }).format(date);
}

function ErrorText({ messages }: { messages?: string[] }) {
  if (!messages?.length) return null;
  return <p className="text-xs leading-relaxed text-danger">{messages[0]}</p>;
}

function FieldLabel({ htmlFor, children, required }: { htmlFor?: string; children: React.ReactNode; required?: boolean }) {
  return (
    <Label htmlFor={htmlFor} className="flex items-center gap-1 text-navy">
      <span>{children}</span>
      {required ? (
        <span className="text-danger">*</span>
      ) : (
        <span className="text-xs font-normal text-muted-foreground">اختياري</span>
      )}
    </Label>
  );
}

export default function CustomersPage() {
  const [customers, setCustomers] = useState<PaginatedList<CustomerListItem> | null>(null);
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [clientType, setClientType] = useState<"all" | ClientType>("all");
  const [pageNumber, setPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [formErrors, setFormErrors] = useState<ValidationErrors>({});
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [isDetailsLoading, setIsDetailsLoading] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<CustomerListItem | null>(null);
  const [form, setForm] = useState<CustomerForm>(EMPTY_FORM);

  const pageSize = 10;
  const clientTypeQuery = clientType === "all" ? undefined : clientType;

  const loadCustomers = async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = buildQuery({
        search: appliedSearch.trim(),
        clientType: clientTypeQuery,
        pageNumber,
        pageSize,
      });
      const result = await customersRequest<PaginatedList<CustomerListItem>>(query);
      setCustomers({
        ...result,
        items: result.items.map(normalizeCustomerListItem),
      });
    } catch (err) {
      setError(getErrorText(err, "تعذر تحميل العملاء."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadCustomers();
  }, [appliedSearch, clientTypeQuery, pageNumber]);

  const stats = useMemo(() => {
    const items = customers?.items ?? [];
    return {
      total: customers?.totalCount ?? 0,
      companies: items.filter((customer) => customer.clientType === "Company").length,
      individuals: items.filter((customer) => customer.clientType === "Individual").length,
    };
  }, [customers]);

  const openCreateForm = () => {
    setEditingCustomer(null);
    setForm(EMPTY_FORM);
    setFormErrors({});
    setMessage("");
    setIsFormOpen(true);
  };

  const openEditForm = async (customer: CustomerListItem) => {
    setFormErrors({});
    setMessage("");
    setIsFormOpen(true);

    try {
      const fullCustomer = normalizeCustomer(await customersRequest<Customer>(`/${customer.id}`));
      setEditingCustomer(fullCustomer);
      setForm({
        customerName: fullCustomer.customerName ?? "",
        email: fullCustomer.email ?? "",
        phone: fullCustomer.phone ?? "",
        clientType: fullCustomer.clientType,
        companyName: fullCustomer.companyName ?? "",
        notes: fullCustomer.notes ?? "",
      });
    } catch (err) {
      setIsFormOpen(false);
      setError(getErrorText(err, "تعذر تحميل بيانات العميل."));
    }
  };

  const openDetails = async (customer: CustomerListItem) => {
    setError("");
    setSelectedCustomer(normalizeCustomer(customer as Customer));
    setIsDetailsLoading(true);

    try {
      const fullCustomer = normalizeCustomer(await customersRequest<Customer>(`/${customer.id}`));
      setSelectedCustomer(fullCustomer);
    } catch (err) {
      setError(getErrorText(err, "تعذر تحميل تفاصيل العميل."));
    } finally {
      setIsDetailsLoading(false);
    }
  };

  const setField = (field: keyof CustomerForm) => (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setForm((prev) => ({ ...prev, [field]: event.target.value }));
    setFormErrors((prev) => ({ ...prev, [field]: undefined, general: undefined }));
  };

  const submitSearch = (event: React.FormEvent) => {
    event.preventDefault();
    setPageNumber(1);
    setAppliedSearch(search);
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsSaving(true);
    setFormErrors({});
    setError("");
    setMessage("");

    const body = {
      customerName: form.customerName.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
      clientType: form.clientType,
      companyName: form.companyName.trim() || null,
      notes: form.notes.trim() || null,
    };

    try {
      if (editingCustomer) {
        await customersRequest<Customer>(`/${editingCustomer.id}`, {
          method: "PUT",
          body: JSON.stringify(body),
        });
        setMessage("تم تحديث بيانات العميل بنجاح.");
      } else {
        await customersRequest<Customer>("", {
          method: "POST",
          body: JSON.stringify(body),
        });
        setMessage("تم إنشاء العميل بنجاح.");
      }

      setIsFormOpen(false);
      setEditingCustomer(null);
      setForm(EMPTY_FORM);
      await loadCustomers();
    } catch (err) {
      const validationErrors = normalizeValidationErrors(err);
      setFormErrors(
        Object.keys(validationErrors).length
          ? validationErrors
          : { general: [getErrorText(err, "تعذر حفظ بيانات العميل.")] },
      );
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;

    setIsDeleting(true);
    setError("");
    setMessage("");

    try {
      await customersRequest<object | null>(`/${deleteTarget.id}`, {
        method: "DELETE",
      });
      setMessage("تم حذف العميل بنجاح.");
      setDeleteTarget(null);
      await loadCustomers();
    } catch (err) {
      setError(getErrorText(err, "تعذر حذف العميل."));
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="إدارة العملاء" desc="إضافة وتحديث وحذف العملاء المرتبطين بحسابك." />
        <Button onClick={openCreateForm} className="w-full rounded-xl bg-teal font-bold text-white hover:bg-teal/90 sm:w-auto">
          <Plus className="ml-2 h-4 w-4" />
          عميل جديد
        </Button>
      </div>

      <section className="grid gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{customers?.totalCount ?? 0}</div>
          <div className="mt-1 text-xs text-muted-foreground">إجمالي العملاء</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.companies}</div>
          <div className="mt-1 text-xs text-muted-foreground">شركات في هذه الصفحة</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.individuals}</div>
          <div className="mt-1 text-xs text-muted-foreground">أفراد في هذه الصفحة</div>
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

      <section className="min-w-0 rounded-2xl border border-border/70 bg-card p-4 shadow-card sm:p-5">
        <div className="mb-4 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <form onSubmit={submitSearch} className="flex min-w-0 flex-1 flex-col gap-2 sm:flex-row">
            <div className="relative flex-1">
              <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="ابحث بالاسم أو البريد أو الهاتف"
                className="h-11 rounded-xl border-border/70 bg-white pr-10"
              />
            </div>
            <Button type="submit" variant="outline" className="h-11 rounded-xl">
              بحث
            </Button>
          </form>

          <div className="flex flex-wrap gap-2">
            <Select
              dir="rtl"
              value={clientType}
              onValueChange={(value) => {
                setClientType(value as "all" | ClientType);
                setPageNumber(1);
              }}
            >
              <SelectTrigger className="h-11 w-full rounded-xl bg-white text-right sm:w-40 [&>span]:w-full [&>span]:text-right">
                <SelectValue placeholder="نوع العميل" />
              </SelectTrigger>
              <SelectContent dir="rtl" className="text-right">
                <SelectItem value="all">كل العملاء</SelectItem>
                <SelectItem value="Company">الشركات</SelectItem>
                <SelectItem value="Individual">الأفراد</SelectItem>
              </SelectContent>
            </Select>
            <Button type="button" variant="outline" className="h-11 rounded-xl" onClick={() => void loadCustomers()}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
            <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
            جاري تحميل العملاء...
          </div>
        ) : customers?.items.length ? (
          <>
            <div className="overflow-x-auto rounded-xl border border-border/70">
              <table className="w-full min-w-[820px] text-right text-sm">
                <thead className="bg-muted/50 text-xs text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">العميل</th>
                    <th className="px-4 py-3 font-semibold">النوع</th>
                    <th className="px-4 py-3 font-semibold">التواصل</th>
                    <th className="px-4 py-3 font-semibold">المشاريع</th>
                    <th className="px-4 py-3 font-semibold">تاريخ الإضافة</th>
                    <th className="px-4 py-3 text-center font-semibold">إجراءات</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/70">
                  {customers.items.map((customer) => (
                    <tr key={customer.id}>
                      <td className="px-4 py-3">
                        <div className="font-semibold text-navy">{customer.customerName}</div>
                        <div className="text-xs text-muted-foreground">{customer.companyName || "بدون شركة"}</div>
                      </td>
                      <td className="px-4 py-3">
                        <span className="inline-flex rounded-full bg-teal-soft px-2 py-1 text-xs font-bold text-teal">
                          {clientTypeLabel(customer.clientType)}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
                        <div>{customer.email}</div>
                        <div className="text-xs">{customer.phone}</div>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{customer.projectsCount}</td>
                      <td className="px-4 py-3 text-muted-foreground">{formatDate(customer.createdAtUtc)}</td>
                      <td className="px-4 py-3">
                        <div className="flex justify-center gap-2">
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl"
                            onClick={() => void openDetails(customer)}
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl"
                            onClick={() => void openEditForm(customer)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl text-danger hover:text-danger"
                            onClick={() => setDeleteTarget(customer)}
                          >
                            <Trash2 className="h-4 w-4" />
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
                صفحة {customers.pageNumber} من {customers.totalPages || 1}
              </span>
              <div className="flex gap-2">
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={pageNumber <= 1}
                  onClick={() => setPageNumber((value) => Math.max(1, value - 1))}
                >
                  السابق
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  className="rounded-xl"
                  disabled={pageNumber >= (customers.totalPages || 1)}
                  onClick={() => setPageNumber((value) => value + 1)}
                >
                  التالي
                </Button>
              </div>
            </div>
          </>
        ) : (
          <div className="grid place-items-center rounded-xl bg-muted/40 p-10 text-center">
            <Users className="mb-3 h-8 w-8 text-muted-foreground" />
            <p className="font-semibold text-navy">لا توجد عملاء للعرض</p>
            <p className="mt-1 text-sm text-muted-foreground">ابدأ بإضافة عميل جديد أو غيّر معايير البحث.</p>
          </div>
        )}
      </section>

      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right">
            <DialogTitle>{editingCustomer ? "تعديل العميل" : "عميل جديد"}</DialogTitle>
            <DialogDescription>املأ بيانات العميل ثم احفظ التغييرات.</DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmit} className="space-y-4">
            {formErrors.general && (
              <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
                {formErrors.general[0]}
              </div>
            )}

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <FieldLabel htmlFor="customerName" required>اسم العميل</FieldLabel>
                <Input
                  id="customerName"
                  value={form.customerName}
                  onChange={setField("customerName")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.customerName} />
              </div>

              <div className="space-y-2">
                <FieldLabel htmlFor="clientType" required>نوع العميل</FieldLabel>
                <Select
                  dir="rtl"
                  value={form.clientType}
                  onValueChange={(value) => {
                    setForm((prev) => ({ ...prev, clientType: value as ClientType }));
                    setFormErrors((prev) => ({ ...prev, clientType: undefined, general: undefined }));
                  }}
                >
                  <SelectTrigger id="clientType" className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    <SelectItem value="Individual">فرد</SelectItem>
                    <SelectItem value="Company">شركة</SelectItem>
                  </SelectContent>
                </Select>
                <ErrorText messages={formErrors.clientType} />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <FieldLabel htmlFor="email" required>البريد الإلكتروني</FieldLabel>
                <Input
                  id="email"
                  type="email"
                  value={form.email}
                  onChange={setField("email")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.email} />
              </div>

              <div className="space-y-2">
                <FieldLabel htmlFor="phone" required>رقم الهاتف</FieldLabel>
                <Input
                  id="phone"
                  value={form.phone}
                  onChange={setField("phone")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.phone} />
              </div>
            </div>

            {form.clientType === "Company" && (
              <div className="space-y-2">
                <FieldLabel htmlFor="companyName">اسم الشركة</FieldLabel>
                <Input
                  id="companyName"
                  value={form.companyName}
                  onChange={setField("companyName")}
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.companyName} />
              </div>
            )}

            <div className="space-y-2">
              <FieldLabel htmlFor="notes">ملاحظات</FieldLabel>
              <Textarea
                id="notes"
                value={form.notes}
                onChange={setField("notes")}
                className="min-h-24 rounded-xl bg-white"
              />
              <ErrorText messages={formErrors.notes} />
            </div>

            <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
              <Button type="submit" disabled={isSaving} className="rounded-xl bg-teal font-bold text-white hover:bg-teal/90">
                {isSaving ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                حفظ
              </Button>
              <Button type="button" variant="outline" className="rounded-xl" onClick={() => setIsFormOpen(false)}>
                إلغاء
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog
        open={!!selectedCustomer || isDetailsLoading}
        onOpenChange={(open) => {
          if (!open) setSelectedCustomer(null);
        }}
      >
        <DialogContent className="max-w-2xl text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>تفاصيل العميل</DialogTitle>
            <DialogDescription>كل البيانات المسجلة عن العميل المحدد.</DialogDescription>
          </DialogHeader>

          {isDetailsLoading && !selectedCustomer ? (
            <div className="flex items-center justify-center rounded-xl bg-muted/40 p-8 text-sm text-muted-foreground">
              <Loader2 className="ml-2 h-4 w-4 animate-spin" />
              جاري تحميل التفاصيل...
            </div>
          ) : selectedCustomer ? (
            <div className="grid gap-3 text-sm sm:grid-cols-2">
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">اسم العميل</div>
                <div className="mt-1 font-semibold text-navy">{selectedCustomer.customerName}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">نوع العميل</div>
                <div className="mt-1 font-semibold text-navy">{clientTypeLabel(selectedCustomer.clientType)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">البريد الإلكتروني</div>
                <div className="mt-1 break-all font-semibold text-navy">{selectedCustomer.email || "-"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">الهاتف</div>
                <div className="mt-1 font-semibold text-navy">{selectedCustomer.phone || "-"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">الشركة</div>
                <div className="mt-1 font-semibold text-navy">{selectedCustomer.companyName || "-"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">عدد المشاريع</div>
                <div className="mt-1 font-semibold text-navy">{selectedCustomer.projectsCount}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">تاريخ الإضافة</div>
                <div className="mt-1 font-semibold text-navy">{formatDate(selectedCustomer.createdAtUtc)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">آخر تعديل</div>
                <div className="mt-1 font-semibold text-navy">{selectedCustomer.lastModifiedUtc ? formatDate(selectedCustomer.lastModifiedUtc) : "-"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3 sm:col-span-2">
                <div className="text-xs text-muted-foreground">ملاحظات</div>
                <div className="mt-1 whitespace-pre-wrap text-navy">{selectedCustomer.notes || "-"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3 sm:col-span-2">
                <div className="text-xs text-muted-foreground">المعرف</div>
                <div className="mt-1 break-all font-mono text-xs text-navy">{selectedCustomer.id}</div>
              </div>
            </div>
          ) : null}
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(open) => !open && setDeleteTarget(null)}>
        <DialogContent className="text-right" dir="rtl">
          <DialogHeader className="text-right">
            <DialogTitle>حذف العميل</DialogTitle>
            <DialogDescription>
              هل تريد حذف {deleteTarget?.customerName}؟ لا يمكن التراجع عن هذا الإجراء من هذه الشاشة.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
            <Button
              type="button"
              disabled={isDeleting}
              className="rounded-xl bg-danger text-danger-foreground hover:bg-danger/90"
              onClick={() => void handleDelete()}
            >
              {isDeleting ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : <Trash2 className="ml-2 h-4 w-4" />}
              حذف
            </Button>
            <Button type="button" variant="outline" className="rounded-xl" onClick={() => setDeleteTarget(null)}>
              إلغاء
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
