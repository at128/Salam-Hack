import { useEffect, useMemo, useState } from "react";
import {
  CalendarDays,
  Edit3,
  Loader2,
  Plus,
  ReceiptText,
  RefreshCw,
  Search,
  Trash2,
} from "lucide-react";

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

import {
  getApiErrorMessage,
  getValidAccessToken,
  unwrapApiResponse,
} from "@/lib/auth";

/* ================= TYPES ================= */

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

/* ================= CONSTANTS ================= */

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

const CATEGORY_OPTIONS = [
  { value: "Subscriptions", label: "اشتراكات" },
  { value: "Tools", label: "أدوات" },
  { value: "Marketing", label: "تسويق" },
  { value: "ProfessionalDevelopment", label: "تطوير مهني" },
  { value: "Transportation", label: "مواصلات" },
  { value: "Communications", label: "اتصالات" },
  { value: "Other", label: "أخرى" },
];

/* ================= HELPERS ================= */

async function apiRequest<T>(url: string, init?: RequestInit): Promise<T> {
  const token = await getValidAccessToken();
  if (!token) throw new Error("Missing token");

  const res = await fetch(url, {
    ...init,
    headers: {
      Authorization: `Bearer ${token}`,
      Accept: "application/json",
      ...(init?.body ? { "Content-Type": "application/json" } : {}),
    },
  });

  const data = await res.json().catch(() => null);

  if (!res.ok) {
    throw data ?? new Error("API Error");
  }

  return unwrapApiResponse<T>(data);
}

function buildQuery(params: Record<string, any>) {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== "") search.set(k, String(v));
  });
  return search.toString() ? `?${search}` : "";
}

function dateInputToIso(value: string) {
  return new Date(`${value}T00:00:00Z`).toISOString();
}

function isoToDateInput(value?: string) {
  if (!value) return "";
  return new Date(value).toISOString().slice(0, 10);
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("ar", { dateStyle: "medium" }).format(new Date(value));
}

function formatCurrency(amount: number, currency = "SAR") {
  return new Intl.NumberFormat("ar", {
    style: "currency",
    currency,
  }).format(amount);
}

function categoryLabel(cat: string) {
  return CATEGORY_OPTIONS.find((c) => c.value === cat)?.label ?? cat;
}

/* ================= COMPONENT ================= */

export default function ExpensesPage() {
  const [expenses, setExpenses] = useState<PaginatedList<ExpenseListItem> | null>(null);
  const [projects, setProjects] = useState<ProjectListItem[]>([]);
  const [form, setForm] = useState<ExpenseForm>(EMPTY_FORM);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const loadExpenses = async () => {
    try {
      const res = await apiRequest<PaginatedList<ExpenseListItem>>(EXPENSES_API_URL);
      setExpenses(res);
    } catch (e) {
      setError(getApiErrorMessage(e, "تعذر تحميل المصروفات."));
    }
  };

  useEffect(() => {
    loadExpenses();
  }, []);

  const submitExpense = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);

    try {
      await apiRequest(EXPENSES_API_URL, {
        method: "POST",
        body: JSON.stringify({
          ...form,
          amount: Number(form.amount),
          expenseDate: dateInputToIso(form.expenseDate),
        }),
      });

      setMessage("تم الحفظ");
      setIsDialogOpen(false);
      loadExpenses();
    } catch (e) {
      setError(getApiErrorMessage(e, "تعذر حفظ المصروف."));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <>
      <PageHeader title="مصروفاتي" desc="إدارة المصروفات" />

      <Button onClick={() => setIsDialogOpen(true)}>
        <Plus /> إضافة
      </Button>

      <div>
        {expenses?.items.map((e) => (
          <div key={e.id}>
            {e.description} - {formatCurrency(e.amount, e.currency)}
          </div>
        ))}
      </div>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <form onSubmit={submitExpense}>
            <Input
              value={form.description}
              onChange={(e) =>
                setForm((f) => ({ ...f, description: e.target.value }))
              }
            />

            <Button type="submit" disabled={isSaving}>
              {isSaving ? <Loader2 className="animate-spin" /> : "حفظ"}
            </Button>
          </form>
        </DialogContent>
      </Dialog>
    </>
  );
}
