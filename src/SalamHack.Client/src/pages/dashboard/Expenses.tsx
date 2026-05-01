import { useEffect, useMemo, useState } from "react";
import {
  ArrowRight,
  BarChart3,
  CalendarDays,
  CreditCard,
  Loader2,
  Palette,
  PieChart,
  Plus,
  Receipt,
  Tag,
  Upload,
  X,
} from "lucide-react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
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
import {
  getApiErrorMessage,
  getValidAccessToken,
  unwrapApiResponse,
} from "@/lib/auth";

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
  category?: ExpenseCategory | string | number | Record<string, unknown> | null;
  categoryName?: ExpenseCategory | string | number | null;
  categoryLabel?: ExpenseCategory | string | number | null;
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

type ExpenseCategoryBreakdown = {
  totalAmount: number;
  expenseCount: number;
  categories: {
    category?: ExpenseCategory | string | number | Record<string, unknown> | null;
    categoryName?: ExpenseCategory | string | number | null;
    categoryLabel?: ExpenseCategory | string | number | null;
    totalAmount: number;
    expenseCount: number;
    sharePercent: number;
  }[];
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

type FormErrors = Partial<Record<keyof ExpenseForm | "receipt" | "general", string>>;

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/+$/, "");
const EXPENSES_API_URL = `${API_BASE_URL}/api/v1/expenses`;
const PROJECTS_API_URL = `${API_BASE_URL}/api/v1/projects`;

const today = new Date().toISOString().slice(0, 10);
const monthStart = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);

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

const CATEGORY_OPTIONS: { value: ExpenseCategory; label: string; color: string; icon: typeof Tag }[] = [
  { value: "Subscriptions", label: "اشتراكات", color: "#5b5cf6", icon: CreditCard },
  { value: "Tools", label: "أدوات", color: "#14a89d", icon: Receipt },
  { value: "Marketing", label: "تسويق", color: "#f59f0b", icon: BarChart3 },
  { value: "ProfessionalDevelopment", label: "تطوير مهني", color: "#ec4899", icon: Palette },
  { value: "Transportation", label: "مواصلات", color: "#8b5cf6", icon: CalendarDays },
  { value: "Communications", label: "اتصالات", color: "#0ea5e9", icon: Tag },
  { value: "Other", label: "أخرى", color: "#64748b", icon: Receipt },
];

const CATEGORY_VALUES: ExpenseCategory[] = CATEGORY_OPTIONS.map((category) => category.value);
const CATEGORY_LOOKUP = new Map(
  CATEGORY_OPTIONS.map((category) => [normalizeCategoryKey(category.value), category]),
);

async function apiRequest<T>(url: string, init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing token");

  const res = await fetch(url, {
    ...init,
    headers: {
      Authorization: `Bearer ${token}`,
      Accept: "application/json",
      ...(init?.body && !(init.body instanceof FormData) ? { "Content-Type": "application/json" } : {}),
      ...init?.headers,
    },
  });

  const data = await res.json().catch(() => null);
  if (!res.ok) throw data ?? new Error("API Error");

  return unwrapApiResponse<T>(data);
}

function buildQuery(params: Record<string, string | number | boolean | undefined>) {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== "") search.set(key, String(value));
  });
  return search.toString() ? `?${search}` : "";
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";
  return new Intl.DateTimeFormat("ar", { day: "numeric", month: "long" }).format(date);
}

function formatCurrency(amount: number, currency = "SAR") {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency,
    maximumFractionDigits: 0,
  }).format(amount || 0);
}

function normalizeCategoryKey(category: string) {
  return category.replace(/[\s_-]/g, "").toLowerCase();
}

function readCategoryValue(category: unknown): ExpenseCategory | string | number {
  if (typeof category === "string" || typeof category === "number") {
    return category;
  }

  if (category && typeof category === "object") {
    const candidate = category as Record<string, unknown>;
    return readCategoryValue(candidate.name ?? candidate.value ?? candidate.label ?? candidate.category ?? "Other");
  }

  return "Other";
}

function normalizeCategory(category: unknown): ExpenseCategory | string {
  const categoryValue = readCategoryValue(category);

  if (typeof categoryValue === "number") {
    return CATEGORY_VALUES[categoryValue] ?? "Other";
  }

  if (/^\d+$/.test(categoryValue)) {
    return CATEGORY_VALUES[Number(categoryValue)] ?? "Other";
  }

  return categoryValue;
}

function categoryMeta(category: unknown) {
  const normalizedCategory = normalizeCategory(category);
  return CATEGORY_LOOKUP.get(normalizeCategoryKey(String(normalizedCategory))) ?? CATEGORY_OPTIONS[CATEGORY_OPTIONS.length - 1];
}

function expenseCategory(expense: Pick<ExpenseListItem, "category" | "categoryName" | "categoryLabel">) {
  return expense.category ?? expense.categoryName ?? expense.categoryLabel ?? "Other";
}

function validateForm(form: ExpenseForm): FormErrors {
  const errors: FormErrors = {};
  const amount = Number(form.amount);

  if (!form.description.trim()) errors.description = "اسم المصروف مطلوب.";
  if (!form.category) errors.category = "اختر التصنيف.";
  if (!form.amount || !Number.isFinite(amount) || amount <= 0) errors.amount = "المبلغ مطلوب ويجب أن يكون أكبر من صفر.";
  if (!form.expenseDate) errors.expenseDate = "التاريخ مطلوب.";
  if (form.recurrenceEndDate && form.recurrenceEndDate < form.expenseDate) {
    errors.recurrenceEndDate = "تاريخ نهاية التكرار يجب أن يكون بعد تاريخ المصروف.";
  }

  return errors;
}

function ErrorText({ message }: { message?: string }) {
  if (!message) return null;
  return <p className="text-xs text-danger">{message}</p>;
}

function RequiredMark() {
  return <span className="text-danger">*</span>;
}

export default function ExpensesPage() {
  const [expenses, setExpenses] = useState<PaginatedList<ExpenseListItem> | null>(null);
  const [breakdown, setBreakdown] = useState<ExpenseCategoryBreakdown | null>(null);
  const [projects, setProjects] = useState<ProjectListItem[]>([]);
  const [form, setForm] = useState<ExpenseForm>(EMPTY_FORM);
  const [formErrors, setFormErrors] = useState<FormErrors>({});
  const [receiptFile, setReceiptFile] = useState<File | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<"all" | ExpenseCategory>("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const loadExpenses = async () => {
    setError("");
    setIsLoading(true);

    try {
      const query = buildQuery({
        category: selectedCategory === "all" ? undefined : selectedCategory,
        pageNumber: 1,
        pageSize: 20,
      });
      const res = await apiRequest<PaginatedList<ExpenseListItem>>(`${EXPENSES_API_URL}${query}`);
      setExpenses(res);
    } catch (e) {
      setError(getApiErrorMessage(e, "تعذر تحميل المصروفات."));
    } finally {
      setIsLoading(false);
    }
  };

  const loadBreakdown = async () => {
    try {
      const query = buildQuery({
        fromUtc: dateInputToIso(monthStart),
        toUtc: new Date().toISOString(),
      });
      const res = await apiRequest<ExpenseCategoryBreakdown>(`${EXPENSES_API_URL}/category-breakdown${query}`);
      setBreakdown(res);
    } catch {
      setBreakdown(null);
    }
  };

  const loadProjects = async () => {
    try {
      const res = await apiRequest<PaginatedList<ProjectListItem>>(`${PROJECTS_API_URL}?pageSize=100`);
      setProjects(res.items ?? []);
    } catch {
      setProjects([]);
    }
  };

  useEffect(() => {
    void loadProjects();
    void loadBreakdown();
  }, []);

  useEffect(() => {
    void loadExpenses();
  }, [selectedCategory]);

  const visibleExpenses = expenses?.items ?? [];
  const currentMonthTotal = useMemo(() => {
    const start = new Date(`${monthStart}T00:00:00.000Z`).getTime();
    return visibleExpenses
      .filter((expense) => new Date(expense.expenseDate).getTime() >= start)
      .reduce((total, expense) => total + expense.amount, 0);
  }, [visibleExpenses]);

  const recurringTotal = useMemo(
    () => visibleExpenses.filter((expense) => expense.isRecurring).reduce((total, expense) => total + expense.amount, 0),
    [visibleExpenses],
  );

  const highestCategory = useMemo(() => {
    const categories = breakdown?.categories ?? [];
    return categories.length
      ? [...categories].sort((a, b) => b.totalAmount - a.totalAmount)[0]
      : null;
  }, [breakdown]);

  const chartStops = useMemo(() => {
    const categories = breakdown?.categories?.filter((item) => item.totalAmount > 0) ?? [];
    if (!categories.length) return "#e8eef5 0 100%";

    let cursor = 0;
    return categories
      .map((item) => {
        const start = cursor;
        cursor += item.sharePercent;
                return `${categoryMeta(item.category ?? item.categoryName ?? item.categoryLabel).color} ${start}% ${cursor}%`;
      })
      .join(", ");
  }, [breakdown]);

  const setField = (field: keyof ExpenseForm, value: string | boolean) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    setFormErrors((prev) => ({ ...prev, [field]: undefined, general: undefined }));
  };

  const openCreateDialog = () => {
    setForm(EMPTY_FORM);
    setFormErrors({});
    setReceiptFile(null);
    setMessage("");
    setError("");
    setIsDialogOpen(true);
  };

  const submitExpense = async (event: React.FormEvent) => {
    event.preventDefault();
    const errors = validateForm(form);
    if (Object.keys(errors).length) {
      setFormErrors(errors);
      return;
    }

    setIsSaving(true);
    setError("");
    setMessage("");

    try {
      const created = await apiRequest<ExpenseListItem>(EXPENSES_API_URL, {
        method: "POST",
        body: JSON.stringify({
          projectId: form.projectId === "none" ? null : form.projectId,
          category: form.category,
          description: form.description.trim(),
          amount: Number(form.amount),
          isRecurring: form.isRecurring,
          expenseDate: dateInputToIso(form.expenseDate),
          recurrenceInterval: form.isRecurring ? form.recurrenceInterval : null,
          recurrenceEndDate: form.recurrenceEndDate ? dateInputToIso(form.recurrenceEndDate) : null,
          currency: form.currency,
        }),
      });

      if (receiptFile && created?.id) {
        const receiptData = new FormData();
        receiptData.append("file", receiptFile);
        await apiRequest<object>(`${EXPENSES_API_URL}/${created.id}/receipt`, {
          method: "POST",
          body: receiptData,
        });
      }

      setMessage("تم حفظ المصروف بنجاح.");
      setIsDialogOpen(false);
      setForm(EMPTY_FORM);
      setReceiptFile(null);
      await Promise.all([loadExpenses(), loadBreakdown()]);
    } catch (e) {
      setFormErrors({ general: getApiErrorMessage(e, "تعذر حفظ المصروف.") });
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      <section className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
        <div className="text-right">
          <p className="text-sm font-semibold text-teal">كشف الربح الحقيقي</p>
          <h1 className="mt-2 text-3xl font-extrabold text-navy">كشف الربح الحقيقي</h1>
          <p className="mt-2 text-sm text-muted-foreground">تتبع مصاريفك واكتشف أين تذهب أرباحك فعلا.</p>
        </div>
        <Button onClick={openCreateDialog} className="h-11 w-fit rounded-lg bg-teal px-5 font-bold text-white hover:bg-teal/90">
          <Plus className="ml-2 h-4 w-4" />
          إضافة مصروف
        </Button>
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

      <section className="grid gap-4 lg:grid-cols-3">
        <SummaryCard
          label="مصاريف هذا الشهر"
          value={formatCurrency(breakdown?.totalAmount ?? currentMonthTotal)}
          hint="عن الشهر الحالي"
          tone="danger"
          icon={Receipt}
        />
        <SummaryCard
          label="مصاريف متكررة"
          value={formatCurrency(recurringTotal)}
          hint="تؤثر على التدفق النقدي"
          tone="success"
          icon={BarChart3}
        />
        <SummaryCard
          label="أعلى فئة"
          value={highestCategory ? categoryMeta(highestCategory.category ?? highestCategory.categoryName ?? highestCategory.categoryLabel).label : "-"}
          hint={highestCategory ? formatCurrency(highestCategory.totalAmount) : "لا توجد بيانات بعد"}
          tone="neutral"
          icon={PieChart}
        />
      </section>

      <section dir="rtl" className="flex flex-wrap justify-start gap-2 text-right">
        <FilterChip active={selectedCategory === "all"} onClick={() => setSelectedCategory("all")}>
          الكل
        </FilterChip>
        {CATEGORY_OPTIONS.slice(0, 5).map((category) => (
          <FilterChip
            key={category.value}
            active={selectedCategory === category.value}
            onClick={() => setSelectedCategory(category.value)}
          >
            {category.label}
          </FilterChip>
        ))}
      </section>

      <section dir="rtl" className="grid gap-5 text-right lg:grid-cols-[0.95fr_1.8fr]">
        <div className="rounded-xl border border-border/70 bg-card p-5 shadow-card">
          <div className="mb-6 text-right">
            <h2 className="font-extrabold text-navy">توزيع المصاريف</h2>
          </div>
          <div className="flex flex-col items-center gap-5">
            <div
              className="grid h-44 w-44 place-items-center rounded-full"
              style={{ background: `conic-gradient(${chartStops})` }}
            >
              <div className="grid h-28 w-28 place-items-center rounded-full bg-card p-3 text-center">
                <div className="space-y-2">
                  <div className="text-xl font-extrabold leading-none text-navy">{formatCurrency(breakdown?.totalAmount ?? 0)}</div>
                  <div className="text-xs leading-none text-muted-foreground">هذا الشهر</div>
                </div>
              </div>
            </div>
            <div className="grid w-full gap-2">
              {(breakdown?.categories ?? []).slice(0, 5).map((item) => (
                <div key={String(readCategoryValue(item.category ?? item.categoryName ?? item.categoryLabel))} className="flex items-center justify-between gap-3 text-sm">
                  <span className="flex items-center gap-2 text-muted-foreground">
                    <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: categoryMeta(item.category ?? item.categoryName ?? item.categoryLabel).color }} />
                    {categoryMeta(item.category ?? item.categoryName ?? item.categoryLabel).label}
                  </span>
                  <span className="font-semibold text-navy">{Math.round(item.sharePercent)}%</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="overflow-hidden rounded-xl border border-border/70 bg-card shadow-card">
          <div className="flex items-center justify-between border-b border-border/70 p-5">
            <h2 className="font-extrabold text-navy">آخر المصاريف</h2>
            <Button variant="outline" size="sm" className="rounded-lg" onClick={() => void loadExpenses()}>
              {isLoading ? <Loader2 className="h-4 w-4 animate-spin" /> : "تحديث"}
            </Button>
          </div>

          {isLoading ? (
            <div className="flex items-center justify-start p-10 text-right text-sm text-muted-foreground">
              <Loader2 className="ml-2 h-4 w-4 animate-spin text-teal" />
              جاري تحميل المصاريف...
            </div>
          ) : visibleExpenses.length ? (
            <div className="divide-y divide-border/70">
              {visibleExpenses.map((expense) => {
                const meta = categoryMeta(expenseCategory(expense));
                const Icon = meta.icon;
                return (
                  <div key={expense.id} className="grid gap-3 p-4 sm:grid-cols-[46px_1fr_120px] sm:items-center">
                    <div className="grid h-10 w-10 place-items-center rounded-xl bg-muted/60 justify-self-start">
                      <Icon className="h-4 w-4" style={{ color: meta.color }} />
                    </div>
                    <div className="text-right">
                      <div className="font-bold text-navy">{expense.description}</div>
                      <div className="mt-1 text-xs text-muted-foreground">
                        {meta.label} - {formatDate(expense.expenseDate)}
                        {expense.projectName ? ` - ${expense.projectName}` : ""}
                        {expense.isRecurring ? " - متكرر" : ""}
                      </div>
                    </div>
                    <div className="text-left font-bold text-danger">-{formatCurrency(expense.amount, expense.currency)}</div>
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="p-10 text-right text-sm text-muted-foreground">لا توجد مصاريف للعرض.</div>
          )}
        </div>
      </section>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-h-[92vh] max-w-xl overflow-y-auto text-right" dir="rtl">
          <DialogHeader className="text-right sm:text-right">
            <div className="flex items-center justify-between">
              <DialogTitle className="text-xl font-extrabold text-navy">إضافة مصروف جديد</DialogTitle>
              <Button type="button" variant="ghost" size="icon" className="h-8 w-8" onClick={() => setIsDialogOpen(false)}>
                <X className="h-4 w-4" />
              </Button>
            </div>
            <DialogDescription>أدخل بيانات المصروف واربطه بمشروع عند الحاجة.</DialogDescription>
          </DialogHeader>

          <form onSubmit={submitExpense} className="space-y-4">
            {formErrors.general ? (
              <div className="rounded-xl border border-danger/30 bg-danger-soft p-3 text-sm text-danger">{formErrors.general}</div>
            ) : null}

            <div className="space-y-2">
              <Label htmlFor="expenseDescription" className="inline-flex items-center gap-1">
                اسم المصروف <RequiredMark />
              </Label>
              <Input
                id="expenseDescription"
                value={form.description}
                placeholder="مثال: دفعت 45 توصيل للعميل أحمد"
                onChange={(event) => setField("description", event.target.value)}
                className="h-11 rounded-xl bg-white"
              />
              <ErrorText message={formErrors.description} />
            </div>

            <div className="space-y-2">
              <Label className="inline-flex items-center gap-1">
                التصنيف <RequiredMark />
              </Label>
              <Select dir="rtl" value={form.category} onValueChange={(value) => setField("category", value as ExpenseCategory)}>
                <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent dir="rtl" className="text-right">
                  {CATEGORY_OPTIONS.map((category) => (
                    <SelectItem key={category.value} value={category.value}>
                      {category.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <ErrorText message={formErrors.category} />
            </div>

            <div className="grid gap-3 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="amount" className="inline-flex items-center gap-1">
                  المبلغ (ر.س) <RequiredMark />
                </Label>
                <Input
                  id="amount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.amount}
                  onChange={(event) => setField("amount", event.target.value)}
                  className="h-11 rounded-xl bg-white"
                />
                <ErrorText message={formErrors.amount} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="expenseDate" className="inline-flex items-center gap-1">
                  التاريخ <RequiredMark />
                </Label>
                <Input
                  id="expenseDate"
                  type="date"
                  value={form.expenseDate}
                  onChange={(event) => setField("expenseDate", event.target.value)}
                  className="h-11 rounded-xl bg-white"
                />
                <ErrorText message={formErrors.expenseDate} />
              </div>
            </div>

            <div className="space-y-2">
              <Label>المشروع المرتبط (اختياري)</Label>
              <Select dir="rtl" value={form.projectId} onValueChange={(value) => setField("projectId", value)}>
                <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
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

            <div className="space-y-2">
              <Label>مصروف متكرر؟</Label>
              <div className="grid grid-cols-2 gap-2">
                <Button
                  type="button"
                  variant={form.isRecurring ? "outline" : "default"}
                  className={`h-10 rounded-lg ${!form.isRecurring ? "bg-teal text-white hover:bg-teal/90" : "bg-white"}`}
                  onClick={() => setField("isRecurring", false)}
                >
                  لا
                </Button>
                <Button
                  type="button"
                  variant={form.isRecurring ? "default" : "outline"}
                  className={`h-10 rounded-lg ${form.isRecurring ? "bg-teal text-white hover:bg-teal/90" : "bg-white"}`}
                  onClick={() => setField("isRecurring", true)}
                >
                  نعم
                </Button>
              </div>
            </div>

            {form.isRecurring ? (
              <div className="grid gap-3 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label>التكرار</Label>
                  <Select
                    dir="rtl"
                    value={form.recurrenceInterval}
                    onValueChange={(value) => setField("recurrenceInterval", value as RecurrenceInterval)}
                  >
                    <SelectTrigger className="h-11 rounded-xl bg-white text-right [&>span]:w-full [&>span]:text-right">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent dir="rtl" className="text-right">
                      <SelectItem value="Monthly">شهري</SelectItem>
                      <SelectItem value="Yearly">سنوي</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="recurrenceEndDate">تاريخ انتهاء التكرار</Label>
                  <Input
                    id="recurrenceEndDate"
                    type="date"
                    value={form.recurrenceEndDate}
                    onChange={(event) => setField("recurrenceEndDate", event.target.value)}
                    className="h-11 rounded-xl bg-white"
                  />
                  <ErrorText message={formErrors.recurrenceEndDate} />
                </div>
              </div>
            ) : null}

            <div className="space-y-2">
              <Label htmlFor="receipt">رفع إيصال (اختياري)</Label>
              <label
                htmlFor="receipt"
                className="flex cursor-pointer flex-col items-center justify-center rounded-xl border border-dashed border-border bg-muted/30 p-5 text-center text-sm text-muted-foreground hover:bg-muted/50"
              >
                <Upload className="mb-2 h-5 w-5 text-teal" />
                {receiptFile ? receiptFile.name : "اضغط لرفع إيصال أو صورة"}
              </label>
              <Input
                id="receipt"
                type="file"
                accept="image/*,.pdf"
                className="hidden"
                onChange={(event) => setReceiptFile(event.target.files?.[0] ?? null)}
              />
            </div>

            <Button type="submit" disabled={isSaving} className="h-11 w-full rounded-xl bg-teal font-bold text-white hover:bg-teal/90">
              {isSaving ? <Loader2 className="ml-2 h-4 w-4 animate-spin" /> : null}
              حفظ المصروف
            </Button>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function SummaryCard({
  label,
  value,
  hint,
  tone,
  icon: Icon,
}: {
  label: string;
  value: string;
  hint: string;
  tone: "danger" | "success" | "neutral";
  icon: typeof Receipt;
}) {
  const toneClass =
    tone === "danger"
      ? "bg-danger-soft text-danger"
      : tone === "success"
        ? "bg-success-soft text-success"
        : "bg-muted text-muted-foreground";

  return (
    <div className="rounded-xl border border-border/70 bg-card p-5 shadow-card">
      <div className="flex items-start justify-between">
        <div className={`grid h-10 w-10 place-items-center rounded-xl ${toneClass}`}>
          <Icon className="h-5 w-5" />
        </div>
        <span className="text-sm text-muted-foreground">{label}</span>
      </div>
      <div className="mt-8 text-right">
        <div className="text-2xl font-extrabold text-navy">{value}</div>
        <div className="mt-2 text-xs text-muted-foreground">{hint}</div>
      </div>
    </div>
  );
}

function FilterChip({
  active,
  onClick,
  children,
}: {
  active: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <Button
      type="button"
      variant={active ? "default" : "outline"}
      className={`h-9 rounded-lg px-4 ${active ? "bg-navy text-white hover:bg-navy/90" : "bg-white"}`}
      onClick={onClick}
    >
      {children}
    </Button>
  );
}
