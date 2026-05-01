import { useEffect, useMemo, useState } from "react";
import { CalendarDays, Edit3, Loader2, Plus, ReceiptText, RefreshCw, Search, Trash2 } from "lucide-react";
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
import { Textarea } from "@/components/ui/textarea";
import { getApiErrorMessage, getValidAccessToken, unwrapApiResponse } from "@/lib/auth";

type ExpenseCategory =
  | "Subscriptions"
  | "Tools"
  | "Marketing"
  | "ProfessionalDevelopment"
  | "Transportation"
  | "Communications"
  | "Other";

type RecurrenceInterval = "Monthly" | "Yearly";

type ExpenseListItem = {
  id: string;
  projectId?: string | null;
  projectName?: string | null;
  category: ExpenseCategory | string;
  description: string;
  amount: number;
  isRecurring: boolean;
  expenseDate: string;
  currency: string;
};

type ProjectListItem = {
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

type ExpenseForm = {
  id?: string;
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

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const EXPENSES_API_URL = `${API_BASE_URL}/api/v1/expenses`;
const PROJECTS_API_URL = `${API_BASE_URL}/api/v1/projects`;

const today = new Date().toISOString().slice(0, 10);

const EMPTY_FORM: ExpenseForm = {
  projectId: "none",
  category: "Subscriptions",
  description: "",
  amount: "",
  isRecurring: false,
  expenseDate: today,
  recurrenceInterval: "Monthly",
  recurrenceEndDate: "",
  currency: "SAR",
};

const CATEGORY_OPTIONS: { value: ExpenseCategory; label: string }[] = [
  { value: "Subscriptions", label: "اشتراكات" },
  { value: "Tools", label: "أدوات" },
  { value: "Marketing", label: "تسويق" },
  { value: "ProfessionalDevelopment", label: "تطوير مهني" },
  { value: "Transportation", label: "مواصلات" },
  { value: "Communications", label: "اتصالات" },
  { value: "Other", label: "أخرى" },
];

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

function buildQuery(params: Record<string, string | number | boolean | undefined>) {
  const searchParams = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === "") continue;
    searchParams.set(key, String(value));
  }

  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function isoToDateInput(value: string | null | undefined) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString().slice(0, 10);
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

function categoryLabel(category: string) {
  return CATEGORY_OPTIONS.find((option) => option.value === category)?.label ?? category;
}

export default function ExpensesPage() {
  const [expenses, setExpenses] = useState<PaginatedList<ExpenseListItem> | null>(null);
  const [projects, setProjects] = useState<ProjectListItem[]>([]);
  const [search, setSearch] = useState("");
  const [category, setCategory] = useState("all");
  const [projectId, setProjectId] = useState("all");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [form, setForm] = useState<ExpenseForm>(EMPTY_FORM);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const pageSize = 10;

  const visibleExpenses = expenses?.items ?? [];
  const pageTotal = useMemo(
    () => visibleExpenses.reduce((sum, expense) => sum + expense.amount, 0),
    [visibleExpenses],
  );
  const recurringCount = visibleExpenses.filter((expense) => expense.isRecurring).length;

  const loadExpenses = async () => {
    setIsLoading(true);
    setError("");

    try {
      const query = buildQuery({
        search: search.trim() || undefined,
        category: category === "all" ? undefined : category,
        projectId: projectId === "all" ? undefined : projectId,
        fromDate: fromDate ? dateInputToIso(fromDate) : undefined,
        toDate: toDate ? dateInputToIso(toDate) : undefined,
        pageNumber,
        pageSize,
      });

      const result = await apiRequest<PaginatedList<ExpenseListItem>>(`${EXPENSES_API_URL}${query}`);
      setExpenses(result);
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر تحميل المصروفات."));
    } finally {
      setIsLoading(false);
    }
  };

  const loadProjects = async () => {
    try {
      const result = await apiRequest<PaginatedList<ProjectListItem>>(`${PROJECTS_API_URL}?pageSize=100`);
      setProjects(result.items ?? []);
    } catch {
      setProjects([]);
    }
  };

  useEffect(() => {
    void loadProjects();
  }, []);

  useEffect(() => {
    void loadExpenses();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [category, projectId, fromDate, toDate, pageNumber]);

  const openCreateDialog = () => {
    setForm(EMPTY_FORM);
    setError("");
    setMessage("");
    setIsDialogOpen(true);
  };

  const openEditDialog = (expense: ExpenseListItem) => {
    setForm({
      id: expense.id,
      projectId: expense.projectId ?? "none",
      category: CATEGORY_OPTIONS.some((option) => option.value === expense.category)
        ? (expense.category as ExpenseCategory)
        : "Other",
      description: expense.description,
      amount: String(expense.amount),
      isRecurring: expense.isRecurring,
      expenseDate: isoToDateInput(expense.expenseDate) || today,
      recurrenceInterval: "Monthly",
      recurrenceEndDate: "",
      currency: expense.currency || "SAR",
    });
    setError("");
    setMessage("");
    setIsDialogOpen(true);
  };

  const submitExpense = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsSaving(true);
    setError("");
    setMessage("");

    const payload = {
      projectId: form.projectId === "none" ? null : form.projectId,
      category: form.category,
      description: form.description.trim(),
      amount: Number(form.amount),
      isRecurring: form.isRecurring,
      expenseDate: dateInputToIso(form.expenseDate),
      recurrenceInterval: form.isRecurring ? form.recurrenceInterval : null,
      recurrenceEndDate: form.isRecurring && form.recurrenceEndDate ? dateInputToIso(form.recurrenceEndDate) : null,
      currency: form.currency.trim().toUpperCase() || "SAR",
    };

    try {
      await apiRequest(form.id ? `${EXPENSES_API_URL}/${form.id}` : EXPENSES_API_URL, {
        method: form.id ? "PUT" : "POST",
        body: JSON.stringify(payload),
      });

      setMessage(form.id ? "تم تحديث المصروف بنجاح." : "تم إضافة المصروف بنجاح.");
      setIsDialogOpen(false);
      setForm(EMPTY_FORM);
      await loadExpenses();
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر حفظ المصروف."));
    } finally {
      setIsSaving(false);
    }
  };

  const deleteExpense = async (expense: ExpenseListItem) => {
    if (!window.confirm(`حذف المصروف "${expense.description}"؟`)) return;

    setError("");
    setMessage("");

    try {
      await apiRequest(`${EXPENSES_API_URL}/${expense.id}`, { method: "DELETE" });
      setMessage("تم حذف المصروف بنجاح.");
      await loadExpenses();
    } catch (err) {
      setError(getApiErrorMessage(err, "تعذر حذف المصروف."));
    }
  };

  return (
    <>
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <PageHeader title="مصروفاتي" desc="إدارة المصروفات، الاشتراكات، والتكاليف المرتبطة بالمشاريع." />
        <Button onClick={openCreateDialog} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
          <Plus className="ml-2 h-4 w-4" />
          إضافة مصروف
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

      <section className="grid gap-4 md:grid-cols-3">
        <SummaryCard icon={ReceiptText} label="مصروفات الصفحة الحالية" value={formatCurrency(pageTotal)} />
        <SummaryCard icon={CalendarDays} label="مصروفات متكررة" value={new Intl.NumberFormat("ar").format(recurringCount)} />
        <SummaryCard icon={Search} label="إجمالي النتائج" value={new Intl.NumberFormat("ar").format(expenses?.totalCount ?? 0)} />
      </section>

      <section className="rounded-2xl border border-border/70 bg-card p-5 shadow-card">
        <div className="mb-4 grid gap-3 lg:grid-cols-[minmax(0,1fr)_180px_180px_150px_150px_auto]">
          <div className="relative">
            <Search className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") {
                  setPageNumber(1);
                  void loadExpenses();
                }
              }}
              placeholder="ابحث بالوصف"
              className="h-11 rounded-xl bg-white pr-10"
            />
          </div>

          <Select
            dir="rtl"
            value={category}
            onValueChange={(value) => {
              setCategory(value);
              setPageNumber(1);
            }}
          >
            <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
              <SelectValue placeholder="التصنيف" />
            </SelectTrigger>
            <SelectContent dir="rtl" className="text-right">
              <SelectItem value="all">كل التصنيفات</SelectItem>
              {CATEGORY_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            dir="rtl"
            value={projectId}
            onValueChange={(value) => {
              setProjectId(value);
              setPageNumber(1);
            }}
          >
            <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
              <SelectValue placeholder="المشروع" />
            </SelectTrigger>
            <SelectContent dir="rtl" className="text-right">
              <SelectItem value="all">كل المشاريع</SelectItem>
              {projects.map((project) => (
                <SelectItem key={project.id} value={project.id}>
                  {project.projectName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Input type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} className="h-11 rounded-xl bg-white" />
          <Input type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} className="h-11 rounded-xl bg-white" />

          <Button type="button" variant="outline" className="h-11 rounded-xl" onClick={() => void loadExpenses()}>
            <RefreshCw className="ml-2 h-4 w-4" />
            تحديث
          </Button>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center rounded-xl bg-muted/40 p-10 text-sm text-muted-foreground">
            <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
            جاري تحميل المصروفات...
          </div>
        ) : visibleExpenses.length ? (
          <>
            <div className="overflow-hidden rounded-xl border border-border/70">
              <table className="w-full text-right text-sm">
                <thead className="bg-muted/50 text-xs text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3 font-semibold">الوصف</th>
                    <th className="px-4 py-3 font-semibold">التصنيف</th>
                    <th className="px-4 py-3 font-semibold">المشروع</th>
                    <th className="px-4 py-3 font-semibold">المبلغ</th>
                    <th className="px-4 py-3 font-semibold">التاريخ</th>
                    <th className="px-4 py-3 font-semibold">إجراءات</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/70">
                  {visibleExpenses.map((expense) => (
                    <tr key={expense.id}>
                      <td className="px-4 py-3">
                        <div className="font-semibold text-navy">{expense.description}</div>
                        {expense.isRecurring ? (
                          <div className="mt-1 text-xs text-muted-foreground">مصروف متكرر</div>
                        ) : null}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{categoryLabel(expense.category)}</td>
                      <td className="px-4 py-3 text-muted-foreground">{expense.projectName ?? "-"}</td>
                      <td className="px-4 py-3 font-bold text-teal">{formatCurrency(expense.amount, expense.currency)}</td>
                      <td className="px-4 py-3 text-muted-foreground">{formatDate(expense.expenseDate)}</td>
                      <td className="px-4 py-3">
                        <div className="flex justify-end gap-2">
                          <Button type="button" variant="outline" size="icon" className="h-9 w-9 rounded-xl" onClick={() => openEditDialog(expense)}>
                            <Edit3 className="h-4 w-4" />
                          </Button>
                          <Button type="button" variant="outline" size="icon" className="h-9 w-9 rounded-xl text-danger hover:text-danger" onClick={() => void deleteExpense(expense)}>
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
                صفحة {expenses?.pageNumber ?? 1} من {expenses?.totalPages || 1}
              </span>
              <div className="flex gap-2">
                <Button type="button" variant="outline" className="rounded-xl" disabled={pageNumber <= 1} onClick={() => setPageNumber((value) => Math.max(1, value - 1))}>
                  السابق
                </Button>
                <Button type="button" variant="outline" className="rounded-xl" disabled={pageNumber >= (expenses?.totalPages || 1)} onClick={() => setPageNumber((value) => value + 1)}>
                  التالي
                </Button>
              </div>
            </div>
          </>
        ) : (
          <div className="rounded-xl bg-muted/40 p-10 text-center text-sm text-muted-foreground">
            لا توجد مصروفات مطابقة.
          </div>
        )}
      </section>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <DialogTitle>{form.id ? "تعديل مصروف" : "إضافة مصروف"}</DialogTitle>
            <DialogDescription>سجل المصروفات العامة أو اربطها بمشروع محدد لاحتساب أثرها على الربح.</DialogDescription>
          </DialogHeader>

          <form onSubmit={submitExpense} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="description">الوصف</Label>
              <Textarea
                id="description"
                value={form.description}
                onChange={(event) => setForm((prev) => ({ ...prev, description: event.target.value }))}
                required
                className="rounded-xl bg-white"
              />
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label>التصنيف</Label>
                <Select dir="rtl" value={form.category} onValueChange={(value) => setForm((prev) => ({ ...prev, category: value as ExpenseCategory }))}>
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    {CATEGORY_OPTIONS.map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>المشروع</Label>
                <Select dir="rtl" value={form.projectId} onValueChange={(value) => setForm((prev) => ({ ...prev, projectId: value }))}>
                  <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent dir="rtl" className="text-right">
                    <SelectItem value="none">بدون مشروع</SelectItem>
                    {projects.map((project) => (
                      <SelectItem key={project.id} value={project.id}>
                        {project.projectName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-3">
              <div className="space-y-2">
                <Label htmlFor="amount">المبلغ</Label>
                <Input
                  id="amount"
                  type="number"
                  min="0.01"
                  step="0.01"
                  value={form.amount}
                  onChange={(event) => setForm((prev) => ({ ...prev, amount: event.target.value }))}
                  required
                  className="rounded-xl bg-white"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="currency">العملة</Label>
                <Input
                  id="currency"
                  value={form.currency}
                  onChange={(event) => setForm((prev) => ({ ...prev, currency: event.target.value.toUpperCase() }))}
                  required
                  className="rounded-xl bg-white"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="expenseDate">التاريخ</Label>
                <Input
                  id="expenseDate"
                  type="date"
                  value={form.expenseDate}
                  onChange={(event) => setForm((prev) => ({ ...prev, expenseDate: event.target.value }))}
                  required
                  className="rounded-xl bg-white"
                />
              </div>
            </div>

            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              <Switch
                checked={form.isRecurring}
                onCheckedChange={(checked) => setForm((prev) => ({ ...prev, isRecurring: checked }))}
              />
              مصروف متكرر
            </label>

            {form.isRecurring && (
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label>التكرار</Label>
                  <Select dir="rtl" value={form.recurrenceInterval} onValueChange={(value) => setForm((prev) => ({ ...prev, recurrenceInterval: value as RecurrenceInterval }))}>
                    <SelectTrigger className="rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent dir="rtl" className="text-right">
                      <SelectItem value="Monthly">شهري</SelectItem>
                      <SelectItem value="Yearly">سنوي</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="recurrenceEndDate">نهاية التكرار</Label>
                  <Input
                    id="recurrenceEndDate"
                    type="date"
                    value={form.recurrenceEndDate}
                    onChange={(event) => setForm((prev) => ({ ...prev, recurrenceEndDate: event.target.value }))}
                    className="rounded-xl bg-white"
                  />
                </div>
              </div>
            )}

            <DialogFooter className="gap-2 sm:justify-start sm:space-x-0">
              <Button type="submit" disabled={isSaving} className="rounded-xl bg-gradient-brand shadow-glow hover:opacity-90">
                {isSaving ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
                حفظ المصروف
              </Button>
              <Button type="button" variant="outline" className="rounded-xl" onClick={() => setIsDialogOpen(false)}>
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
}: {
  icon: typeof ReceiptText;
  label: string;
  value: string;
}) {
  return (
    <div className="rounded-2xl border border-border/70 bg-card p-4 shadow-card">
      <div className="grid h-11 w-11 place-items-center rounded-xl bg-teal-soft text-teal">
        <Icon className="h-5 w-5" />
      </div>
      <div className="mt-4 text-xl font-bold text-navy">{value}</div>
      <div className="mt-1 text-xs text-muted-foreground">{label}</div>
    </div>
  );
}
