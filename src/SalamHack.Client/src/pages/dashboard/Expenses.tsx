import { useEffect, useMemo, useState } from "react";
import { Edit, Loader2, Plus, RefreshCw, Search, Trash2, WalletCards } from "lucide-react";
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

type ExpenseCategory =
  | "subscriptions"
  | "tools"
  | "marketing"
  | "professionalDevelopment"
  | "transportation"
  | "communications"
  | "other";

type RecurrenceInterval = "monthly" | "yearly";

type ExpenseListItem = {
  id: string;
  projectId?: string | null;
  projectName?: string | null;
  category: ExpenseCategory;
  description: string;
  amount: number;
  isRecurring: boolean;
  expenseDate: string;
  currency: string;
};

type Expense = ExpenseListItem & {
  recurrenceInterval?: RecurrenceInterval | null;
  recurrenceEndDate?: string | null;
};

type ProjectOption = {
  id: string;
  projectName: string;
};

type PaginatedList<T> = {
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  items: T[];
};

type ExpenseBreakdown = {
  totalAmount: number;
  expenseCount: number;
  categories: {
    category: ExpenseCategory;
    totalAmount: number;
    expenseCount: number;
    sharePercent: number;
  }[];
};

type ExpenseForm = {
  projectId: string;
  category: ExpenseCategory;
  description: string;
  amount: string;
  isRecurring: boolean;
  expenseDate: string;
  recurrenceInterval: RecurrenceInterval;
  recurrenceEndDate: string;
  currency: string;
};

type ValidationErrors = Partial<Record<keyof ExpenseForm | "general", string[]>>;

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const EXPENSES_API_URL = `${API_BASE_URL}/api/v1/expenses`;
const PROJECTS_API_URL = `${API_BASE_URL}/api/v1/projects`;

const CATEGORIES: { value: ExpenseCategory; label: string }[] = [
  { value: "subscriptions", label: "اشتراكات" },
  { value: "tools", label: "أدوات وبرامج" },
  { value: "marketing", label: "تسويق" },
  { value: "professionalDevelopment", label: "تعلم وتطوير" },
  { value: "transportation", label: "مواصلات" },
  { value: "communications", label: "اتصالات وإنترنت" },
  { value: "other", label: "أخرى" },
];

const CURRENCIES = ["SAR", "USD", "ILS", "JOD", "AED"];

const EMPTY_FORM: ExpenseForm = {
  projectId: "none",
  category: "subscriptions",
  description: "",
  amount: "",
  isRecurring: false,
  expenseDate: new Date().toISOString().slice(0, 10),
  recurrenceInterval: "monthly",
  recurrenceEndDate: "",
  currency: "SAR",
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
      normalized[field as keyof ExpenseForm] = messages.filter((message): message is string => typeof message === "string");
    }
  }

  return normalized;
}

function ErrorText({ messages }: { messages?: string[] }) {
  if (!messages?.length) return null;
  return <p className="text-xs leading-relaxed text-danger">{messages[0]}</p>;
}

function categoryLabel(category: ExpenseCategory | string) {
  const normalized = String(category) as ExpenseCategory;
  return CATEGORIES.find((item) => item.value === normalized)?.label ?? String(category);
}

function recurrenceLabel(expense: Pick<ExpenseListItem, "isRecurring"> & { recurrenceInterval?: RecurrenceInterval | null }) {
  if (!expense.isRecurring) return "مرة واحدة";
  return expense.recurrenceInterval === "yearly" ? "سنوي" : "شهري";
}

function formatDate(value?: string | null) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("ar", { dateStyle: "medium" }).format(date);
}

function toDateInput(value?: string | null) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString().slice(0, 10);
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function formatMoney(amount: number | undefined, currency = "SAR") {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency,
    maximumFractionDigits: 0,
  }).format(amount ?? 0);
}

export default function ExpensesPage() {
  const [expenses, setExpenses] = useState<PaginatedList<ExpenseListItem> | null>(null);
  const [breakdown, setBreakdown] = useState<ExpenseBreakdown | null>(null);
  const [projects, setProjects] = useState<ProjectOption[]>([]);
  const [search, setSearch] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<"all" | ExpenseCategory>("all");
  const [recurringFilter, setRecurringFilter] = useState<"all" | "true" | "false">("all");
  const [projectFilter, setProjectFilter] = useState("all");
  const [pageNumber, setPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingExpense, setEditingExpense] = useState<Expense | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ExpenseListItem | null>(null);
  const [form, setForm] = useState<ExpenseForm>(EMPTY_FORM);
  const [formErrors, setFormErrors] = useState<ValidationErrors>({});
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const pageSize = 10;

  const stats = useMemo(() => {
    const items = expenses?.items ?? [];
    const pageTotal = items.reduce((sum, expense) => sum + expense.amount, 0);
    const recurringCount = items.filter((expense) => expense.isRecurring).length;

    return {
      totalCount: expenses?.totalCount ?? 0,
      monthTotal: breakdown?.totalAmount ?? 0,
      pageTotal,
      recurringCount,
    };
  }, [breakdown, expenses]);

  const topCategory = breakdown?.categories?.[0];

  const loadLookups = async () => {
    const result = await apiRequest<PaginatedList<ProjectOption>>(`${PROJECTS_API_URL}?pageSize=100`);
    setProjects(result.items ?? []);
  };

  const loadExpenses = async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = buildQuery({
        search: appliedSearch.trim(),
        category: categoryFilter === "all" ? undefined : categoryFilter,
        isRecurring: recurringFilter === "all" ? undefined : recurringFilter,
        projectId: projectFilter === "all" ? undefined : projectFilter,
        pageNumber,
        pageSize,
      });

      const [expenseResult, breakdownResult] = await Promise.all([
        apiRequest<PaginatedList<ExpenseListItem>>(`${EXPENSES_API_URL}${query}`),
        apiRequest<ExpenseBreakdown>(`${EXPENSES_API_URL}/category-breakdown`),
      ]);

      setExpenses(expenseResult);
      setBreakdown(breakdownResult);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل المصاريف."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadLookups().catch((err) => setError(getApiErrorMessage(err, "تعذر تحميل المشاريع.")));
  }, []);

  useEffect(() => {
    void loadExpenses();
  }, [appliedSearch, categoryFilter, recurringFilter, projectFilter, pageNumber]);

  const submitSearch = (event: React.FormEvent) => {
    event.preventDefault();
    setPageNumber(1);
    setAppliedSearch(search);
  };

  const setField = (field: keyof ExpenseForm) => (
    event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    const value = field === "isRecurring" ? (event.target as HTMLInputElement).checked : event.target.value;
    setForm((prev) => ({
      ...prev,
      [field]: value,
      ...(field === "isRecurring" && !value ? { recurrenceEndDate: "" } : {}),
    }));
    setFormErrors((prev) => ({ ...prev, [field]: undefined, general: undefined }));
  };

  const openCreateForm = () => {
    setEditingExpense(null);
    setForm(EMPTY_FORM);
    setFormErrors({});
    setMessage("");
    setError("");
    setIsFormOpen(true);
  };

  const openEditForm = async (expense: ExpenseListItem) => {
    setFormErrors({});
    setMessage("");
    setError("");
    setIsFormOpen(true);

    try {
      const fullExpense = await apiRequest<Expense>(`${EXPENSES_API_URL}/${expense.id}`);
      setEditingExpense(fullExpense);
      setForm({
        projectId: fullExpense.projectId ?? "none",
        category: fullExpense.category,
        description: fullExpense.description ?? "",
        amount: String(fullExpense.amount ?? ""),
        isRecurring: !!fullExpense.isRecurring,
        expenseDate: toDateInput(fullExpense.expenseDate),
        recurrenceInterval: fullExpense.recurrenceInterval ?? "monthly",
        recurrenceEndDate: toDateInput(fullExpense.recurrenceEndDate),
        currency: fullExpense.currency ?? "SAR",
      });
    } catch (err) {
      setIsFormOpen(false);
      setError(getApiErrorMessage(err, "تعذر تحميل بيانات المصروف."));
    }
  };

  const buildExpenseBody = () => ({
    projectId: form.projectId === "none" ? null : form.projectId,
    category: form.category,
    description: form.description.trim(),
    amount: Number(form.amount),
    isRecurring: form.isRecurring,
    expenseDate: dateInputToIso(form.expenseDate),
    recurrenceInterval: form.isRecurring ? form.recurrenceInterval : null,
    recurrenceEndDate: form.isRecurring && form.recurrenceEndDate ? dateInputToIso(form.recurrenceEndDate) : null,
    currency: form.currency,
  });

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsSaving(true);
    setFormErrors({});
    setError("");
    setMessage("");

    try {
      if (editingExpense) {
        await apiRequest<Expense>(`${EXPENSES_API_URL}/${editingExpense.id}`, {
          method: "PUT",
          body: JSON.stringify(buildExpenseBody()),
        });
        setMessage("تم تحديث المصروف بنجاح.");
      } else {
        await apiRequest<Expense>(EXPENSES_API_URL, {
          method: "POST",
          body: JSON.stringify(buildExpenseBody()),
        });
        setMessage("تمت إضافة المصروف بنجاح.");
      }

      setIsFormOpen(false);
      setEditingExpense(null);
      setForm(EMPTY_FORM);
      await loadExpenses();
    } catch (err) {
      const validationErrors = normalizeValidationErrors(err);
      setFormErrors(
        Object.keys(validationErrors).length
          ? validationErrors
          : { general: [getApiErrorMessage(err, "تعذر حفظ المصروف.")] },
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
      await apiRequest<object | null>(`${EXPENSES_API_URL}/${deleteTarget.id}`, { method: "DELETE" });
      setMessage("تم حذف المصروف بنجاح.");
      setDeleteTarget(null);
      await loadExpenses();
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر حذف المصروف."));
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="مصاريفي" desc="سجل الاشتراكات، الأدوات، وأي مصروف يؤثر على ربحك الحقيقي." />
        <Button onClick={openCreateForm} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
          <Plus className="ml-2 h-4 w-4" />
          مصروف جديد
        </Button>
      </div>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{formatMoney(stats.monthTotal)}</div>
          <div className="mt-1 text-xs text-muted-foreground">مصاريف هذا الشهر</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{stats.totalCount}</div>
          <div className="mt-1 text-xs text-muted-foreground">إجمالي السجلات</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{formatMoney(stats.pageTotal)}</div>
          <div className="mt-1 text-xs text-muted-foreground">مصاريف الصفحة الحالية</div>
        </div>
        <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
          <div className="text-2xl font-bold text-navy">{topCategory ? categoryLabel(topCategory.category) : "-"}</div>
          <div className="mt-1 text-xs text-muted-foreground">أعلى تصنيف هذا الشهر</div>
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
        <div className="mb-4 flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
          <form onSubmit={submitSearch} className="flex flex-1 gap-2">
            <div className="relative flex-1">
              <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="ابحث باسم الاشتراك أو المصروف"
                className="h-11 rounded-xl border-border/70 bg-white pr-10"
              />
            </div>
            <Button type="submit" variant="outline" className="h-11 rounded-xl">
              بحث
            </Button>
          </form>

          <div className="grid gap-2 sm:grid-cols-3 xl:w-[34rem]">
            <Select
              dir="rtl"
              value={categoryFilter}
              onValueChange={(value) => {
                setPageNumber(1);
                setCategoryFilter(value as "all" | ExpenseCategory);
              }}
            >
              <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                <SelectValue />
              </SelectTrigger>
              <SelectContent dir="rtl" className="text-right">
                <SelectItem value="all">كل التصنيفات</SelectItem>
                {CATEGORIES.map((category) => (
                  <SelectItem key={category.value} value={category.value}>
                    {category.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select
              dir="rtl"
              value={recurringFilter}
              onValueChange={(value) => {
                setPageNumber(1);
                setRecurringFilter(value as "all" | "true" | "false");
              }}
            >
              <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                <SelectValue />
              </SelectTrigger>
              <SelectContent dir="rtl" className="text-right">
                <SelectItem value="all">كل المصاريف</SelectItem>
                <SelectItem value="true">متكررة فقط</SelectItem>
                <SelectItem value="false">مرة واحدة</SelectItem>
              </SelectContent>
            </Select>

            <Button variant="outline" className="h-11 rounded-xl" onClick={() => void loadExpenses()}>
              <RefreshCw className="ml-2 h-4 w-4" />
              تحديث
            </Button>
          </div>
        </div>

        <div className="mb-4 grid gap-2 md:grid-cols-2">
          <Select
            dir="rtl"
            value={projectFilter}
            onValueChange={(value) => {
              setPageNumber(1);
              setProjectFilter(value);
            }}
          >
            <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
              <SelectValue />
            </SelectTrigger>
            <SelectContent dir="rtl" className="text-right">
              <SelectItem value="all">كل المشاريع والمصاريف العامة</SelectItem>
              {projects.map((project) => (
                <SelectItem key={project.id} value={project.id}>
                  {project.projectName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center rounded-xl bg-muted/40 p-8 text-sm text-muted-foreground">
            <Loader2 className="ml-2 h-4 w-4 animate-spin" />
            جاري تحميل المصاريف...
          </div>
        ) : expenses?.items?.length ? (
          <>
            <div className="overflow-hidden rounded-2xl border border-border/70">
              <table className="w-full min-w-[860px] text-right text-sm">
                <thead className="bg-muted/50 text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">المصروف</th>
                    <th className="px-4 py-3 font-semibold">التصنيف</th>
                    <th className="px-4 py-3 font-semibold">المشروع</th>
                    <th className="px-4 py-3 font-semibold">المبلغ</th>
                    <th className="px-4 py-3 font-semibold">التكرار</th>
                    <th className="px-4 py-3 font-semibold">التاريخ</th>
                    <th className="px-4 py-3 font-semibold">إجراءات</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/70">
                  {expenses.items.map((expense) => (
                    <tr key={expense.id} className="bg-card transition-colors hover:bg-muted/30">
                      <td className="px-4 py-3">
                        <div className="font-semibold text-navy">{expense.description}</div>
                        <div className="mt-1 text-xs text-muted-foreground">{expense.currency}</div>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{categoryLabel(expense.category)}</td>
                      <td className="px-4 py-3 text-muted-foreground">{expense.projectName ?? "مصروف عام"}</td>
                      <td className="px-4 py-3 font-semibold text-navy">{formatMoney(expense.amount, expense.currency)}</td>
                      <td className="px-4 py-3">
                        <span className={`rounded-full px-3 py-1 text-xs ${expense.isRecurring ? "bg-warning-soft text-warning" : "bg-muted text-muted-foreground"}`}>
                          {recurrenceLabel(expense)}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{formatDate(expense.expenseDate)}</td>
                      <td className="px-4 py-3">
                        <div className="flex justify-end gap-2">
                          <Button variant="outline" size="icon" className="h-8 w-8 rounded-xl" onClick={() => void openEditForm(expense)}>
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="outline"
                            size="icon"
                            className="h-8 w-8 rounded-xl text-danger hover:text-danger"
                            onClick={() => setDeleteTarget(expense)}
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

            <div className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div className="text-sm text-muted-foreground">
                صفحة {expenses.pageNumber} من {Math.max(expenses.totalPages, 1)} - {expenses.totalCount} سجل
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  className="rounded-xl"
                  disabled={pageNumber <= 1}
                  onClick={() => setPageNumber((page) => Math.max(page - 1, 1))}
                >
                  السابق
                </Button>
                <Button
                  variant="outline"
                  className="rounded-xl"
                  disabled={pageNumber >= (expenses.totalPages || 1)}
                  onClick={() => setPageNumber((page) => page + 1)}
                >
                  التالي
                </Button>
              </div>
            </div>
          </>
        ) : (
          <div className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-border bg-muted/20 p-10 text-center">
            <WalletCards className="mb-3 h-8 w-8 text-muted-foreground" />
            <h3 className="font-semibold text-navy">لا توجد مصاريف بعد</h3>
            <p className="mt-1 text-sm text-muted-foreground">ابدأ بإضافة اشتراك أو أداة أو أي تكلفة مرتبطة بعملك.</p>
            <Button onClick={openCreateForm} className="mt-4 rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
              <Plus className="ml-2 h-4 w-4" />
              إضافة مصروف
            </Button>
          </div>
        )}
      </section>

      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-h-[90vh] max-w-2xl overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>{editingExpense ? "تعديل المصروف" : "مصروف جديد"}</DialogTitle>
            <DialogDescription>سجل تكلفة عامة أو تكلفة مرتبطة بمشروع محدد.</DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmit} className="space-y-4">
            {formErrors.general?.length ? (
              <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
                {formErrors.general[0]}
              </div>
            ) : null}

            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="description">اسم المصروف</Label>
                <Input
                  id="description"
                  value={form.description}
                  onChange={setField("description")}
                  placeholder="مثال: اشتراك Canva Pro"
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.description} />
              </div>

              <div className="space-y-2">
                <Label>التصنيف</Label>
                <Select
                  dir="rtl"
                  value={form.category}
                  onValueChange={(value) => setForm((prev) => ({ ...prev, category: value as ExpenseCategory }))}
                >
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    {CATEGORIES.map((category) => (
                      <SelectItem key={category.value} value={category.value}>
                        {category.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <ErrorText messages={formErrors.category} />
              </div>
            </div>

            <div className="space-y-2">
              <Label>يرتبط بمشروع</Label>
              <Select
                dir="rtl"
                value={form.projectId}
                onValueChange={(value) => setForm((prev) => ({ ...prev, projectId: value }))}
              >
                <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent dir="rtl" className="text-right">
                  <SelectItem value="none">مصروف عام غير مرتبط بمشروع</SelectItem>
                  {projects.map((project) => (
                    <SelectItem key={project.id} value={project.id}>
                      {project.projectName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <ErrorText messages={formErrors.projectId} />
            </div>

            <div className="grid gap-4 md:grid-cols-3">
              <div className="space-y-2">
                <Label htmlFor="amount">المبلغ</Label>
                <Input
                  id="amount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.amount}
                  onChange={setField("amount")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.amount} />
              </div>

              <div className="space-y-2">
                <Label>العملة</Label>
                <Select
                  dir="rtl"
                  value={form.currency}
                  onValueChange={(value) => setForm((prev) => ({ ...prev, currency: value }))}
                >
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    {CURRENCIES.map((currency) => (
                      <SelectItem key={currency} value={currency}>
                        {currency}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="expenseDate">تاريخ المصروف</Label>
                <Input
                  id="expenseDate"
                  type="date"
                  value={form.expenseDate}
                  onChange={setField("expenseDate")}
                  required
                  className="rounded-xl bg-white"
                />
                <ErrorText messages={formErrors.expenseDate} />
              </div>
            </div>

            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              <input
                type="checkbox"
                checked={form.isRecurring}
                onChange={setField("isRecurring")}
                className="h-4 w-4 rounded border-border accent-teal"
              />
              مصروف متكرر مثل اشتراك شهري أو سنوي
            </label>

            {form.isRecurring ? (
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <Label>يتكرر كل</Label>
                  <Select
                    dir="rtl"
                    value={form.recurrenceInterval}
                    onValueChange={(value) => setForm((prev) => ({ ...prev, recurrenceInterval: value as RecurrenceInterval }))}
                  >
                    <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent dir="rtl" className="text-right">
                      <SelectItem value="monthly">شهر</SelectItem>
                      <SelectItem value="yearly">سنة</SelectItem>
                    </SelectContent>
                  </Select>
                  <ErrorText messages={formErrors.recurrenceInterval} />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="recurrenceEndDate">ينتهي في</Label>
                  <Input
                    id="recurrenceEndDate"
                    type="date"
                    value={form.recurrenceEndDate}
                    onChange={setField("recurrenceEndDate")}
                    className="rounded-xl bg-white"
                  />
                  <ErrorText messages={formErrors.recurrenceEndDate} />
                </div>
              </div>
            ) : null}

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

      <Dialog open={!!deleteTarget} onOpenChange={(open) => !open && setDeleteTarget(null)}>
        <DialogContent className="max-w-md text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>حذف المصروف</DialogTitle>
            <DialogDescription>سيتم حذف هذا المصروف من سجلاتك وحسابات الربح.</DialogDescription>
          </DialogHeader>
          <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
            <Button onClick={handleDelete} disabled={isDeleting} className="rounded-xl bg-danger text-white hover:bg-danger/90">
              {isDeleting ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
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
