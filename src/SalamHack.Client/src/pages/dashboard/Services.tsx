import { useEffect, useMemo, useState } from "react";
import { Edit, Eye, Loader2, Plus, RefreshCw, Search, Trash2 } from "lucide-react";
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
import { Switch } from "@/components/ui/switch";
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type ServiceCategory = "Design" | "Development" | "Consulting" | "Marketing" | "Content" | "Other";

type ServiceListItem = {
  id: string;
  serviceName: string;
  category: ServiceCategory | string;
  defaultHourlyRate: number;
  defaultRevisions: number;
  isActive: boolean;
  projectsCount: number;
  createdAtUtc: string;
};

type Service = ServiceListItem & {
  lastModifiedUtc?: string;
};

type PaginatedList<T> = {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  items: T[];
};

type ServiceForm = {
  serviceName: string;
  category: ServiceCategory;
  defaultHourlyRate: string;
  defaultRevisions: string;
  isActive: boolean;
};

type ValidationErrors = Partial<Record<keyof ServiceForm | "general", string[]>>;

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const SERVICES_API_URL = `${API_BASE_URL}/api/v1/services`;
const EMPTY_FORM: ServiceForm = {
  serviceName: "",
  category: "Design",
  defaultHourlyRate: "",
  defaultRevisions: "0",
  isActive: true,
};

const CATEGORY_OPTIONS: { value: ServiceCategory; label: string }[] = [
  { value: "Design", label: "تصميم" },
  { value: "Development", label: "تطوير" },
  { value: "Consulting", label: "استشارات" },
  { value: "Marketing", label: "تسويق" },
  { value: "Content", label: "محتوى" },
  { value: "Other", label: "أخرى" },
];

function buildQuery(params: Record<string, string | number | boolean | undefined>) {
  const searchParams = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === "") continue;
    searchParams.set(key, String(value));
  }

  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

async function servicesRequest<T>(path = "", init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing access token.");

  const response = await fetch(`${SERVICES_API_URL}${path}`, {
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

function normalizeCategory(value: unknown): ServiceCategory {
  if (value === "Design" || value === "design" || value === 0) return "Design";
  if (value === "Development" || value === "development" || value === 1) return "Development";
  if (value === "Consulting" || value === "consulting" || value === 2) return "Consulting";
  if (value === "Marketing" || value === "marketing" || value === 3) return "Marketing";
  if (value === "Content" || value === "content" || value === 4) return "Content";
  return "Other";
}

function normalizeService<T extends ServiceListItem>(service: T): T {
  return {
    ...service,
    category: normalizeCategory(service.category),
  };
}

function normalizeValidationErrors(error: unknown): ValidationErrors {
  if (!error || typeof error !== "object") return {};
  const errors = (error as { errors?: unknown }).errors;
  if (!errors) return {};

  if (Array.isArray(errors)) {
    const messages = errors
      .map((item) => {
        if (!item || typeof item !== "object") return null;
        return (item as { message?: string; description?: string }).message ?? (item as { description?: string }).description ?? null;
      })
      .filter((message): message is string => !!message);
    return messages.length ? { general: messages } : {};
  }

  const normalized: ValidationErrors = {};
  if (typeof errors === "object") {
    for (const [key, messages] of Object.entries(errors as Record<string, unknown>)) {
      if (!Array.isArray(messages)) continue;
      const field = key.charAt(0).toLowerCase() + key.slice(1);
      normalized[field as keyof ServiceForm] = messages.filter((message): message is string => typeof message === "string");
    }
  }

  return normalized;
}

function categoryLabel(category: string) {
  return CATEGORY_OPTIONS.find((option) => option.value === normalizeCategory(category))?.label ?? "أخرى";
}

function formatCurrency(value: number | undefined) {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency: "SAR",
    maximumFractionDigits: 0,
  }).format(value ?? 0);
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("ar", { dateStyle: "medium" }).format(date);
}

function ErrorText({ messages }: { messages?: string[] }) {
  if (!messages?.length) return null;
  return <p className="text-xs leading-relaxed text-danger">{messages[0]}</p>;
}

function RequiredLabel({ htmlFor, children }: { htmlFor?: string; children: React.ReactNode }) {
  return (
    <Label htmlFor={htmlFor} className="flex items-center gap-1 text-navy">
      <span>{children}</span>
      <span className="text-danger">*</span>
    </Label>
  );
}

export default function ServicesPage() {
  const [services, setServices] = useState<PaginatedList<ServiceListItem> | null>(null);
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [category, setCategory] = useState<"all" | ServiceCategory>("all");
  const [includeInactive, setIncludeInactive] = useState(true);
  const [pageNumber, setPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [formErrors, setFormErrors] = useState<ValidationErrors>({});
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingService, setEditingService] = useState<Service | null>(null);
  const [selectedService, setSelectedService] = useState<Service | null>(null);
  const [isDetailsLoading, setIsDetailsLoading] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<ServiceListItem | null>(null);
  const [form, setForm] = useState<ServiceForm>(EMPTY_FORM);

  const pageSize = 10;

  const loadServices = async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = buildQuery({
        search: appliedSearch.trim(),
        category: category === "all" ? undefined : category,
        includeInactive,
        pageNumber,
        pageSize,
      });
      const result = await servicesRequest<PaginatedList<ServiceListItem>>(query);
      setServices({
        ...result,
        items: result.items.map(normalizeService),
      });
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل الخدمات."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadServices();
  }, [appliedSearch, category, includeInactive, pageNumber]);

  const stats = useMemo(() => {
    const items = services?.items ?? [];
    return {
      total: services?.totalCount ?? 0,
      active: items.filter((service) => service.isActive).length,
      inactive: items.filter((service) => !service.isActive).length,
    };
  }, [services]);

  const setField = (field: keyof ServiceForm) => (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = field === "isActive" ? event.target.checked : event.target.value;
    setForm((prev) => ({ ...prev, [field]: value }));
    setFormErrors((prev) => ({ ...prev, [field]: undefined, general: undefined }));
  };

  const openCreateForm = () => {
    setEditingService(null);
    setForm(EMPTY_FORM);
    setFormErrors({});
    setMessage("");
    setIsFormOpen(true);
  };

  const openEditForm = async (service: ServiceListItem) => {
    setFormErrors({});
    setMessage("");
    setIsFormOpen(true);

    try {
      const fullService = normalizeService(await servicesRequest<Service>(`/${service.id}`));
      setEditingService(fullService);
      setForm({
        serviceName: fullService.serviceName ?? "",
        category: normalizeCategory(fullService.category),
        defaultHourlyRate: String(fullService.defaultHourlyRate ?? ""),
        defaultRevisions: String(fullService.defaultRevisions ?? 0),
        isActive: !!fullService.isActive,
      });
    } catch (err) {
      setIsFormOpen(false);
      setError(getApiErrorMessage(err, "تعذر تحميل بيانات الخدمة."));
    }
  };

  const openDetails = async (service: ServiceListItem) => {
    setError("");
    setSelectedService(normalizeService(service as Service));
    setIsDetailsLoading(true);

    try {
      const fullService = normalizeService(await servicesRequest<Service>(`/${service.id}`));
      setSelectedService(fullService);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل تفاصيل الخدمة."));
    } finally {
      setIsDetailsLoading(false);
    }
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
      serviceName: form.serviceName.trim(),
      category: form.category,
      defaultHourlyRate: Number(form.defaultHourlyRate),
      defaultRevisions: Number(form.defaultRevisions),
      isActive: form.isActive,
    };

    try {
      if (editingService) {
        await servicesRequest<Service>(`/${editingService.id}`, {
          method: "PUT",
          body: JSON.stringify({
            serviceName: body.serviceName,
            category: body.category,
            defaultHourlyRate: body.defaultHourlyRate,
            defaultRevisions: body.defaultRevisions,
          }),
        });
        await servicesRequest<Service>(`/${editingService.id}/active`, {
          method: "PATCH",
          body: JSON.stringify({ isActive: body.isActive }),
        });
        setMessage("تم تحديث الخدمة بنجاح.");
      } else {
        await servicesRequest<Service>("", {
          method: "POST",
          body: JSON.stringify(body),
        });
        setMessage("تم إنشاء الخدمة بنجاح.");
      }

      setIsFormOpen(false);
      setEditingService(null);
      setForm(EMPTY_FORM);
      await loadServices();
    } catch (err) {
      const validationErrors = normalizeValidationErrors(err);
      setFormErrors(
        Object.keys(validationErrors).length
          ? validationErrors
          : { general: [getApiErrorMessage(err, "تعذر حفظ بيانات الخدمة.")] },
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
      await servicesRequest<object | null>(`/${deleteTarget.id}`, { method: "DELETE" });
      setMessage("تم حذف الخدمة بنجاح.");
      setDeleteTarget(null);
      await loadServices();
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر حذف الخدمة."));
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="خدماتي" desc="إدارة الخدمات التي تقدمها وأسعارها الافتراضية." />
        <Button onClick={openCreateForm} className="w-full rounded-xl bg-teal font-bold text-white hover:bg-teal/90 sm:w-auto">
          <Plus className="ml-2 h-4 w-4" />
          خدمة جديدة
        </Button>
      </div>

      <section className="grid gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.total}</div>
          <div className="mt-1 text-xs text-muted-foreground">إجمالي الخدمات</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.active}</div>
          <div className="mt-1 text-xs text-muted-foreground">نشطة في هذه الصفحة</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.inactive}</div>
          <div className="mt-1 text-xs text-muted-foreground">غير نشطة في هذه الصفحة</div>
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
                placeholder="ابحث باسم الخدمة"
                className="h-11 rounded-xl border-border/70 bg-white pr-10"
              />
            </div>
            <Button type="submit" variant="outline" className="h-11 rounded-xl">
              بحث
            </Button>
          </form>

          <div className="flex flex-wrap gap-2" dir="rtl">
            <Select
              dir="rtl"
              value={category}
              onValueChange={(value) => {
                setCategory(value as "all" | ServiceCategory);
                setPageNumber(1);
              }}
            >
              <SelectTrigger className="h-11 w-full rounded-xl bg-white text-right sm:w-40 [&>span]:w-full [&>span]:text-right">
                <SelectValue placeholder="التصنيف" />
              </SelectTrigger>
              <SelectContent align="end" className="text-right">
                <SelectItem value="all" className="justify-end text-right">
                  كل التصنيفات
                </SelectItem>
                {CATEGORY_OPTIONS.map((option) => (
                  <SelectItem key={option.value} value={option.value} className="justify-end text-right">
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button
              type="button"
              variant="outline"
              className="h-11 w-11 shrink-0 rounded-xl p-0"
              onClick={() => void loadServices()}
            >
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
            <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
            جاري تحميل الخدمات...
          </div>
        ) : services?.items.length ? (
          <>
            <div className="overflow-x-auto rounded-xl border border-border/70">
              <table className="w-full min-w-[900px] text-right text-sm">
                <thead className="bg-muted/50 text-xs text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">الخدمة</th>
                    <th className="px-4 py-3 font-semibold">التصنيف</th>
                    <th className="px-4 py-3 font-semibold">سعر الساعة</th>
                    <th className="px-4 py-3 font-semibold">التعديلات</th>
                    <th className="px-4 py-3 font-semibold">المشاريع</th>
                    <th className="px-4 py-3 font-semibold">الحالة</th>
                    <th className="px-4 py-3 font-semibold">تاريخ الإضافة</th>
                    <th className="px-4 py-3 text-center font-semibold">إجراءات</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/70">
                  {services.items.map((service) => (
                    <tr key={service.id}>
                      <td className="px-4 py-3 font-semibold text-navy">{service.serviceName}</td>
                      <td className="px-4 py-3 text-muted-foreground">{categoryLabel(service.category)}</td>
                      <td className="px-4 py-3 text-muted-foreground">{formatCurrency(service.defaultHourlyRate)}</td>
                      <td className="px-4 py-3 text-muted-foreground">{service.defaultRevisions}</td>
                      <td className="px-4 py-3 text-muted-foreground">{service.projectsCount}</td>
                      <td className="px-4 py-3">
                        <span
                          className={`inline-flex rounded-full px-2 py-1 text-xs font-bold ${
                            service.isActive ? "bg-success-soft text-success" : "bg-muted text-muted-foreground"
                          }`}
                        >
                          {service.isActive ? "نشطة" : "غير نشطة"}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{formatDate(service.createdAtUtc)}</td>
                      <td className="px-4 py-3">
                        <div className="flex justify-center gap-2">
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl"
                            onClick={() => void openDetails(service)}
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl"
                            onClick={() => void openEditForm(service)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl text-danger hover:text-danger"
                            onClick={() => setDeleteTarget(service)}
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
                صفحة {services.pageNumber} من {services.totalPages || 1}
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
                  disabled={pageNumber >= (services.totalPages || 1)}
                  onClick={() => setPageNumber((value) => value + 1)}
                >
                  التالي
                </Button>
              </div>
            </div>
          </>
        ) : (
          <div className="rounded-xl bg-muted/40 p-10 text-center text-sm text-muted-foreground">
            لا توجد خدمات للعرض.
          </div>
        )}
      </section>

      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>{editingService ? "تعديل الخدمة" : "خدمة جديدة"}</DialogTitle>
            <DialogDescription>أدخل بيانات الخدمة الافتراضية المستخدمة في التسعير والمشاريع.</DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmit} className="space-y-4">
            {formErrors.general && (
              <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
                {formErrors.general[0]}
              </div>
            )}

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <RequiredLabel htmlFor="serviceName">اسم الخدمة</RequiredLabel>
                <Input
                  id="serviceName"
                  value={form.serviceName}
                  onChange={setField("serviceName")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.serviceName} />
              </div>

              <div className="space-y-2">
                <RequiredLabel>التصنيف</RequiredLabel>
                <Select
                  dir="rtl"
                  value={form.category}
                  onValueChange={(value) => {
                    setForm((prev) => ({ ...prev, category: value as ServiceCategory }));
                    setFormErrors((prev) => ({ ...prev, category: undefined, general: undefined }));
                  }}
                >
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent align="end" className="text-right">
                    {CATEGORY_OPTIONS.map((option) => (
                      <SelectItem key={option.value} value={option.value} className="justify-end text-right">
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <ErrorText messages={formErrors.category} />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <RequiredLabel htmlFor="defaultHourlyRate">سعر الساعة الافتراضي</RequiredLabel>
                <Input
                  id="defaultHourlyRate"
                  type="number"
                  min="1"
                  step="0.01"
                  value={form.defaultHourlyRate}
                  onChange={setField("defaultHourlyRate")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.defaultHourlyRate} />
              </div>

              <div className="space-y-2">
                <RequiredLabel htmlFor="defaultRevisions">عدد مرات التعديل الافتراضية</RequiredLabel>
                <Input
                  id="defaultRevisions"
                  type="number"
                  min="0"
                  step="1"
                  value={form.defaultRevisions}
                  onChange={setField("defaultRevisions")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.defaultRevisions} />
              </div>
            </div>

            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              <Switch
                checked={form.isActive}
                onCheckedChange={(checked) => {
                  setForm((prev) => ({ ...prev, isActive: checked }));
                  setFormErrors((prev) => ({ ...prev, isActive: undefined, general: undefined }));
                }}
              />
              خدمة نشطة
            </label>

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
        open={!!selectedService || isDetailsLoading}
        onOpenChange={(open) => {
          if (!open) setSelectedService(null);
        }}
      >
        <DialogContent className="max-w-2xl text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>تفاصيل الخدمة</DialogTitle>
            <DialogDescription>كل البيانات المسجلة عن الخدمة المحددة.</DialogDescription>
          </DialogHeader>

          {isDetailsLoading && !selectedService ? (
            <div className="flex items-center justify-center rounded-xl bg-muted/40 p-8 text-sm text-muted-foreground">
              <Loader2 className="ml-2 h-4 w-4 animate-spin" />
              جاري تحميل التفاصيل...
            </div>
          ) : selectedService ? (
            <div className="grid gap-3 text-sm sm:grid-cols-2">
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">اسم الخدمة</div>
                <div className="mt-1 font-semibold text-navy">{selectedService.serviceName}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">التصنيف</div>
                <div className="mt-1 font-semibold text-navy">{categoryLabel(selectedService.category)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">سعر الساعة الافتراضي</div>
                <div className="mt-1 font-semibold text-navy">{formatCurrency(selectedService.defaultHourlyRate)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">عدد مرات التعديل الافتراضية</div>
                <div className="mt-1 font-semibold text-navy">{selectedService.defaultRevisions}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">عدد المشاريع</div>
                <div className="mt-1 font-semibold text-navy">{selectedService.projectsCount}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">الحالة</div>
                <div className="mt-1 font-semibold text-navy">{selectedService.isActive ? "نشطة" : "غير نشطة"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">تاريخ الإضافة</div>
                <div className="mt-1 font-semibold text-navy">{formatDate(selectedService.createdAtUtc)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">آخر تعديل</div>
                <div className="mt-1 font-semibold text-navy">{selectedService.lastModifiedUtc ? formatDate(selectedService.lastModifiedUtc) : "-"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3 sm:col-span-2">
                <div className="text-xs text-muted-foreground">المعرف</div>
                <div className="mt-1 break-all font-mono text-xs text-navy">{selectedService.id}</div>
              </div>
            </div>
          ) : null}
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(open) => !open && setDeleteTarget(null)}>
        <DialogContent className="text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>حذف الخدمة</DialogTitle>
            <DialogDescription>هل تريد حذف {deleteTarget?.serviceName}؟</DialogDescription>
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
