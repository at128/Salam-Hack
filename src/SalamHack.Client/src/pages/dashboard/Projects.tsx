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
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type ProjectStatus = "Planning" | "InProgress" | "Completed" | "Cancelled";
type ProjectHealthStatus = "Healthy" | "AtRisk" | "Critical";

type HealthDto = {
  profit: number;
  marginPercent: number;
  healthStatus: ProjectHealthStatus;
};

type ProjectListItem = {
  id: string;
  projectName: string;
  customerId: string;
  customerName: string;
  serviceId: string;
  serviceName: string;
  suggestedPrice: number;
  profitMargin: number;
  status: ProjectStatus;
  startDate: string;
  endDate: string;
  health: HealthDto;
};

type Project = ProjectListItem & {
  estimatedHours: number;
  actualHours: number;
  toolCost: number;
  revision: number;
  isUrgent: boolean;
};

type CustomerOption = {
  id: string;
  customerName: string;
};

type ServiceOption = {
  id: string;
  serviceName: string;
  isActive: boolean;
};

type PaginatedList<T> = {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  items: T[];
};

type ProjectForm = {
  projectName: string;
  customerId: string;
  serviceId: string;
  estimatedHours: string;
  actualHours: string;
  toolCost: string;
  revision: string;
  isUrgent: boolean;
  suggestedPrice: string;
  startDate: string;
  endDate: string;
  status: ProjectStatus;
};

type ValidationErrors = Partial<Record<keyof ProjectForm | "general", string[]>>;

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const PROJECTS_API_URL = `${API_BASE_URL}/api/v1/projects`;
const CUSTOMERS_API_URL = `${API_BASE_URL}/api/v1/customers`;
const SERVICES_API_URL = `${API_BASE_URL}/api/v1/services`;

const EMPTY_FORM: ProjectForm = {
  projectName: "",
  customerId: "",
  serviceId: "",
  estimatedHours: "",
  actualHours: "0",
  toolCost: "0",
  revision: "0",
  isUrgent: false,
  suggestedPrice: "",
  startDate: "",
  endDate: "",
  status: "Planning",
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

function toDateInput(value: string) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString().slice(0, 10);
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("ar", { dateStyle: "medium" }).format(date);
}

function formatCurrency(value: number | undefined) {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency: "SAR",
    maximumFractionDigits: 0,
  }).format(value ?? 0);
}

function statusLabel(status: ProjectStatus | string) {
  switch (status) {
    case "Planning":
      return "تخطيط";
    case "InProgress":
      return "قيد التنفيذ";
    case "Completed":
      return "مكتمل";
    case "Cancelled":
      return "ملغي";
    default:
      return status;
  }
}

function healthClass(status?: string) {
  switch (status?.toLowerCase()) {
    case "healthy":
      return "bg-success-soft text-success";
    case "atrisk":
      return "bg-warning-soft text-warning";
    case "critical":
      return "bg-danger-soft text-danger";
    default:
      return "bg-muted text-muted-foreground";
  }
}

function healthLabel(status?: string) {
  switch (status?.toLowerCase()) {
    case "healthy":
      return "صحي";
    case "atrisk":
    case "at risk":
      return "معرض للخطر";
    case "critical":
      return "حرج";
    default:
      return status ?? "-";
  }
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
      normalized[field as keyof ProjectForm] = messages.filter((message): message is string => typeof message === "string");
    }
  }
  return normalized;
}

function ErrorText({ messages }: { messages?: string[] }) {
  if (!messages?.length) return null;
  return <p className="text-xs leading-relaxed text-danger">{messages[0]}</p>;
}

export default function ProjectsPage() {
  const [projects, setProjects] = useState<PaginatedList<ProjectListItem> | null>(null);
  const [customers, setCustomers] = useState<CustomerOption[]>([]);
  const [services, setServices] = useState<ServiceOption[]>([]);
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [status, setStatus] = useState<"all" | ProjectStatus>("all");
  const [pageNumber, setPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [formErrors, setFormErrors] = useState<ValidationErrors>({});
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const [selectedProject, setSelectedProject] = useState<Project | null>(null);
  const [isDetailsLoading, setIsDetailsLoading] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<ProjectListItem | null>(null);
  const [form, setForm] = useState<ProjectForm>(EMPTY_FORM);

  const pageSize = 10;

  const loadLookups = async () => {
    const [customerResult, serviceResult] = await Promise.all([
      apiRequest<PaginatedList<CustomerOption>>(`${CUSTOMERS_API_URL}?pageSize=100`),
      apiRequest<PaginatedList<ServiceOption>>(`${SERVICES_API_URL}?includeInactive=false&pageSize=100`),
    ]);

    setCustomers(customerResult.items ?? []);
    setServices(serviceResult.items ?? []);
  };

  const loadProjects = async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = buildQuery({
        search: appliedSearch.trim(),
        status: status === "all" ? undefined : status,
        pageNumber,
        pageSize,
      });
      const result = await apiRequest<PaginatedList<ProjectListItem>>(`${PROJECTS_API_URL}${query}`);
      setProjects(result);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل المشاريع."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadLookups().catch((err) => setError(getApiErrorMessage(err, "تعذر تحميل العملاء أو الخدمات.")));
  }, []);

  useEffect(() => {
    void loadProjects();
  }, [appliedSearch, status, pageNumber]);

  const stats = useMemo(() => {
    const items = projects?.items ?? [];
    return {
      total: projects?.totalCount ?? 0,
      inProgress: items.filter((project) => project.status === "InProgress").length,
      completed: items.filter((project) => project.status === "Completed").length,
    };
  }, [projects]);

  const setField = (field: keyof ProjectForm) => (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = field === "isUrgent" ? event.target.checked : event.target.value;
    setForm((prev) => ({ ...prev, [field]: value }));
    setFormErrors((prev) => ({ ...prev, [field]: undefined, general: undefined }));
  };

  const openCreateForm = () => {
    setEditingProject(null);
    setForm(EMPTY_FORM);
    setFormErrors({});
    setMessage("");
    setIsFormOpen(true);
  };

  const openEditForm = async (project: ProjectListItem) => {
    setFormErrors({});
    setMessage("");
    setIsFormOpen(true);

    try {
      const fullProject = await apiRequest<Project>(`${PROJECTS_API_URL}/${project.id}`);
      setEditingProject(fullProject);
      setForm({
        projectName: fullProject.projectName ?? "",
        customerId: fullProject.customerId ?? "",
        serviceId: fullProject.serviceId ?? "",
        estimatedHours: String(fullProject.estimatedHours ?? ""),
        actualHours: String(fullProject.actualHours ?? 0),
        toolCost: String(fullProject.toolCost ?? 0),
        revision: String(fullProject.revision ?? 0),
        isUrgent: !!fullProject.isUrgent,
        suggestedPrice: String(fullProject.suggestedPrice ?? ""),
        startDate: toDateInput(fullProject.startDate),
        endDate: toDateInput(fullProject.endDate),
        status: fullProject.status ?? "Planning",
      });
    } catch (err) {
      setIsFormOpen(false);
      setError(getApiErrorMessage(err, "تعذر تحميل بيانات المشروع."));
    }
  };

  const openDetails = async (project: ProjectListItem) => {
    setError("");
    setSelectedProject(project as Project);
    setIsDetailsLoading(true);

    try {
      const fullProject = await apiRequest<Project>(`${PROJECTS_API_URL}/${project.id}`);
      setSelectedProject(fullProject);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل تفاصيل المشروع."));
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

    const estimateBody = {
      estimatedHours: Number(form.estimatedHours),
      toolCost: Number(form.toolCost),
      revision: Number(form.revision),
      isUrgent: form.isUrgent,
      suggestedPrice: Number(form.suggestedPrice),
    };

    try {
      if (editingProject) {
        await apiRequest<Project>(`${PROJECTS_API_URL}/${editingProject.id}/name`, {
          method: "PATCH",
          body: JSON.stringify({ projectName: form.projectName.trim() }),
        });
        await apiRequest<Project>(`${PROJECTS_API_URL}/${editingProject.id}/estimate`, {
          method: "PATCH",
          body: JSON.stringify(estimateBody),
        });
        await apiRequest<Project>(`${PROJECTS_API_URL}/${editingProject.id}/schedule`, {
          method: "PATCH",
          body: JSON.stringify({
            startDate: dateInputToIso(form.startDate),
            endDate: dateInputToIso(form.endDate),
          }),
        });
        await apiRequest<Project>(`${PROJECTS_API_URL}/${editingProject.id}/actual-hours`, {
          method: "PATCH",
          body: JSON.stringify({ actualHours: Number(form.actualHours || 0) }),
        });
        await apiRequest<Project>(`${PROJECTS_API_URL}/${editingProject.id}/status`, {
          method: "PATCH",
          body: JSON.stringify({ status: form.status }),
        });
        setMessage("تم تحديث المشروع بنجاح.");
      } else {
        await apiRequest<Project>(PROJECTS_API_URL, {
          method: "POST",
          body: JSON.stringify({
            customerId: form.customerId,
            serviceId: form.serviceId,
            projectName: form.projectName.trim(),
            ...estimateBody,
            startDate: dateInputToIso(form.startDate),
            endDate: dateInputToIso(form.endDate),
          }),
        });
        setMessage("تم إنشاء المشروع بنجاح.");
      }

      setIsFormOpen(false);
      setEditingProject(null);
      setForm(EMPTY_FORM);
      await loadProjects();
    } catch (err) {
      const validationErrors = normalizeValidationErrors(err);
      setFormErrors(
        Object.keys(validationErrors).length
          ? validationErrors
          : { general: [getApiErrorMessage(err, "تعذر حفظ بيانات المشروع.")] },
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
      await apiRequest<object | null>(`${PROJECTS_API_URL}/${deleteTarget.id}`, { method: "DELETE" });
      setMessage("تم حذف المشروع بنجاح.");
      setDeleteTarget(null);
      await loadProjects();
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر حذف المشروع."));
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="إدارة المشاريع" desc="إضافة ومتابعة وتحديث مشاريعك." />
        <Button onClick={openCreateForm} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
          <Plus className="ml-2 h-4 w-4" />
          مشروع جديد
        </Button>
      </div>

      <section className="grid gap-4 sm:grid-cols-3">
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.total}</div>
          <div className="mt-1 text-xs text-muted-foreground">إجمالي المشاريع</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.inProgress}</div>
          <div className="mt-1 text-xs text-muted-foreground">قيد التنفيذ في هذه الصفحة</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.completed}</div>
          <div className="mt-1 text-xs text-muted-foreground">مكتملة في هذه الصفحة</div>
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

      <section className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
        <div className="mb-4 flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <form onSubmit={submitSearch} className="flex flex-1 gap-2">
            <div className="relative flex-1">
              <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="ابحث باسم المشروع أو العميل أو الخدمة"
                className="h-11 rounded-xl border-border/70 bg-white pr-10"
              />
            </div>
            <Button type="submit" variant="outline" className="h-11 rounded-xl">
              بحث
            </Button>
          </form>

          <div className="flex gap-2">
            <Select
              dir="rtl"
              value={status}
              onValueChange={(value) => {
                setStatus(value as "all" | ProjectStatus);
                setPageNumber(1);
              }}
            >
              <SelectTrigger className="h-11 w-44 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                <SelectValue placeholder="حالة المشروع" />
              </SelectTrigger>
              <SelectContent dir="rtl" className="text-right">
                <SelectItem value="all">كل الحالات</SelectItem>
                <SelectItem value="Planning">تخطيط</SelectItem>
                <SelectItem value="InProgress">قيد التنفيذ</SelectItem>
                <SelectItem value="Completed">مكتمل</SelectItem>
                <SelectItem value="Cancelled">ملغي</SelectItem>
              </SelectContent>
            </Select>
            <Button type="button" variant="outline" className="h-11 rounded-xl" onClick={() => void loadProjects()}>
              <RefreshCw className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
            <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
            جاري تحميل المشاريع...
          </div>
        ) : projects?.items.length ? (
          <>
            <div className="overflow-x-auto rounded-xl border border-border/70">
              <table className="w-full min-w-[900px] text-right text-sm">
                <thead className="bg-muted/50 text-xs text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">المشروع</th>
                    <th className="px-4 py-3 font-semibold">العميل</th>
                    <th className="px-4 py-3 font-semibold">الخدمة</th>
                    <th className="px-4 py-3 font-semibold">السعر</th>
                    <th className="px-4 py-3 font-semibold">الصحة</th>
                    <th className="px-4 py-3 font-semibold">الحالة</th>
                    <th className="px-4 py-3 font-semibold">الفترة</th>
                    <th className="px-4 py-3 text-center font-semibold">إجراءات</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/70">
                  {projects.items.map((project) => (
                    <tr key={project.id}>
                      <td className="px-4 py-3 font-semibold text-navy">{project.projectName}</td>
                      <td className="px-4 py-3 text-muted-foreground">{project.customerName}</td>
                      <td className="px-4 py-3 text-muted-foreground">{project.serviceName}</td>
                      <td className="px-4 py-3 text-muted-foreground">{formatCurrency(project.suggestedPrice)}</td>
                      <td className="px-4 py-3">
                        <span className={`inline-flex rounded-full px-2 py-1 text-xs font-bold ${healthClass(project.health?.healthStatus)}`}>
                          {healthLabel(project.health?.healthStatus)} - {project.health?.marginPercent ?? 0}%
                        </span>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{statusLabel(project.status)}</td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatDate(project.startDate)} - {formatDate(project.endDate)}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex justify-center gap-2">
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl"
                            onClick={() => void openDetails(project)}
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl"
                            onClick={() => void openEditForm(project)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="icon"
                            className="h-9 w-9 rounded-xl text-danger hover:text-danger"
                            onClick={() => setDeleteTarget(project)}
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
                صفحة {projects.pageNumber} من {projects.totalPages || 1}
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
                  disabled={pageNumber >= (projects.totalPages || 1)}
                  onClick={() => setPageNumber((value) => value + 1)}
                >
                  التالي
                </Button>
              </div>
            </div>
          </>
        ) : (
          <div className="rounded-xl bg-muted/40 p-10 text-center text-sm text-muted-foreground">
            لا توجد مشاريع للعرض.
          </div>
        )}
      </section>

      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>{editingProject ? "تعديل المشروع" : "مشروع جديد"}</DialogTitle>
            <DialogDescription>املأ بيانات المشروع ثم احفظ التغييرات.</DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmit} className="space-y-4">
            {formErrors.general && (
              <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
                {formErrors.general[0]}
              </div>
            )}

            <div className="space-y-2">
              <Label htmlFor="projectName">اسم المشروع</Label>
              <Input id="projectName" value={form.projectName} onChange={setField("projectName")} required className="rounded-xl bg-white" />
              <ErrorText messages={formErrors.projectName} />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>العميل</Label>
                <Select
                  dir="rtl"
                  value={form.customerId}
                  disabled={!!editingProject}
                  onValueChange={(value) => setForm((prev) => ({ ...prev, customerId: value }))}
                >
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue placeholder="اختر العميل" />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    {customers.map((customer) => (
                      <SelectItem key={customer.id} value={customer.id}>
                        {customer.customerName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <ErrorText messages={formErrors.customerId} />
              </div>

              <div className="space-y-2">
                <Label>الخدمة</Label>
                <Select
                  dir="rtl"
                  value={form.serviceId}
                  disabled={!!editingProject}
                  onValueChange={(value) => setForm((prev) => ({ ...prev, serviceId: value }))}
                >
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue placeholder="اختر الخدمة" />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    {services.map((service) => (
                      <SelectItem key={service.id} value={service.id}>
                        {service.serviceName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <ErrorText messages={formErrors.serviceId} />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-3">
              <div className="space-y-2">
                <Label htmlFor="estimatedHours">الساعات المقدرة</Label>
                <Input id="estimatedHours" type="number" min="0" step="0.25" value={form.estimatedHours} onChange={setField("estimatedHours")} required className="rounded-xl bg-white" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="actualHours">الساعات الفعلية</Label>
                <Input id="actualHours" type="number" min="0" step="0.25" value={form.actualHours} onChange={setField("actualHours")} className="rounded-xl bg-white" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="revision">التعديلات</Label>
                <Input id="revision" type="number" min="0" step="1" value={form.revision} onChange={setField("revision")} required className="rounded-xl bg-white" />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="toolCost">تكلفة الأدوات</Label>
                <Input id="toolCost" type="number" min="0" step="0.01" value={form.toolCost} onChange={setField("toolCost")} required className="rounded-xl bg-white" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="suggestedPrice">السعر المقترح</Label>
                <Input id="suggestedPrice" type="number" min="0" step="0.01" value={form.suggestedPrice} onChange={setField("suggestedPrice")} required className="rounded-xl bg-white" />
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-3">
              <div className="space-y-2">
                <Label htmlFor="startDate">تاريخ البداية</Label>
                <Input id="startDate" type="date" value={form.startDate} onChange={setField("startDate")} required className="rounded-xl bg-white" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="endDate">تاريخ النهاية</Label>
                <Input id="endDate" type="date" value={form.endDate} onChange={setField("endDate")} required className="rounded-xl bg-white" />
              </div>
              <div className="space-y-2">
                <Label>الحالة</Label>
                <Select dir="rtl" value={form.status} onValueChange={(value) => setForm((prev) => ({ ...prev, status: value as ProjectStatus }))}>
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    <SelectItem value="Planning">تخطيط</SelectItem>
                    <SelectItem value="InProgress">قيد التنفيذ</SelectItem>
                    <SelectItem value="Completed">مكتمل</SelectItem>
                    <SelectItem value="Cancelled">ملغي</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              <input type="checkbox" checked={form.isUrgent} onChange={setField("isUrgent")} className="h-4 w-4 rounded border-border accent-teal" />
              مشروع عاجل
            </label>

            <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
              <Button type="submit" disabled={isSaving} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
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
        open={!!selectedProject || isDetailsLoading}
        onOpenChange={(open) => {
          if (!open) setSelectedProject(null);
        }}
      >
        <DialogContent className="max-w-3xl text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>تفاصيل المشروع</DialogTitle>
            <DialogDescription>كل البيانات المسجلة عن المشروع المحدد.</DialogDescription>
          </DialogHeader>

          {isDetailsLoading && !selectedProject ? (
            <div className="flex items-center justify-center rounded-xl bg-muted/40 p-8 text-sm text-muted-foreground">
              <Loader2 className="ml-2 h-4 w-4 animate-spin" />
              جاري تحميل التفاصيل...
            </div>
          ) : selectedProject ? (
            <div className="grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-3">
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">اسم المشروع</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.projectName}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">العميل</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.customerName}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">الخدمة</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.serviceName}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">السعر المقترح</div>
                <div className="mt-1 font-semibold text-navy">{formatCurrency(selectedProject.suggestedPrice)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">هامش الربح</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.profitMargin}%</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">الحالة</div>
                <div className="mt-1 font-semibold text-navy">{statusLabel(selectedProject.status)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">تاريخ البداية</div>
                <div className="mt-1 font-semibold text-navy">{formatDate(selectedProject.startDate)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">تاريخ النهاية</div>
                <div className="mt-1 font-semibold text-navy">{formatDate(selectedProject.endDate)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">عاجل</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.isUrgent ? "نعم" : "لا"}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">الساعات المقدرة</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.estimatedHours ?? 0}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">الساعات الفعلية</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.actualHours ?? 0}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">تكلفة الأدوات</div>
                <div className="mt-1 font-semibold text-navy">{formatCurrency(selectedProject.toolCost)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">التعديلات</div>
                <div className="mt-1 font-semibold text-navy">{selectedProject.revision ?? 0}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">ربح المشروع</div>
                <div className="mt-1 font-semibold text-navy">{formatCurrency(selectedProject.health?.profit)}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">صحة المشروع</div>
                <div className="mt-1 font-semibold text-navy">
                  {healthLabel(selectedProject.health?.healthStatus)} - {selectedProject.health?.marginPercent ?? 0}%
                </div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">معرف العميل</div>
                <div className="mt-1 break-all font-mono text-xs text-navy">{selectedProject.customerId}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3">
                <div className="text-xs text-muted-foreground">معرف الخدمة</div>
                <div className="mt-1 break-all font-mono text-xs text-navy">{selectedProject.serviceId}</div>
              </div>
              <div className="rounded-xl border border-border/70 bg-muted/30 p-3 sm:col-span-2 lg:col-span-3">
                <div className="text-xs text-muted-foreground">معرف المشروع</div>
                <div className="mt-1 break-all font-mono text-xs text-navy">{selectedProject.id}</div>
              </div>
            </div>
          ) : null}
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteTarget} onOpenChange={(open) => !open && setDeleteTarget(null)}>
        <DialogContent className="text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>حذف المشروع</DialogTitle>
            <DialogDescription>هل تريد حذف {deleteTarget?.projectName}؟</DialogDescription>
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
