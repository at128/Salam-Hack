import { useEffect, useState } from "react";
import { Loader2, RefreshCw, Calendar } from "lucide-react";
import { fetchCashFlowForecast } from "@/lib/reports";

interface RecurringExpense {
  name?: string | null;
  description?: string | null;
  amount?: number | null;
  monthlyEquivalentAmount?: number | null;
  interval?: string | null;
  recurrenceInterval?: string | null;
  nextOccurrenceUtc?: string | null;
  expenseDate?: string | null;
  recurrenceEndDate?: string | null;
}

interface Props {
  asOfUtc?: string;
  openingBalance?: number;
}

export default function RecurringExpenses({ asOfUtc, openingBalance = 0 }: Props) {
  const [expenses, setExpenses] = useState<RecurringExpense[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setLoading(true);
    setError("");

    fetchCashFlowForecast(asOfUtc, openingBalance)
      .then((data) => {
        const sorted = (data.recurringExpenses || []).sort((a, b) => {
          return getTime(nextOccurrence(a)) - getTime(nextOccurrence(b));
        });
        setExpenses(sorted);
      })
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, [asOfUtc, openingBalance]);

  if (loading) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card flex items-center justify-center h-64">
        <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <h3 className="font-bold text-navy mb-1">المصاريف المتكررة</h3>
        <p className="text-xs text-destructive">{error}</p>
      </div>
    );
  }

  if (expenses.length === 0) {
    return (
      <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
        <h3 className="font-bold text-navy mb-1">المصاريف المتكررة</h3>
        <p className="text-xs text-muted-foreground">لا توجد مصاريف متكررة محسوبة</p>
      </div>
    );
  }

  const totalMonthly = expenses
    .reduce((sum, expense) => sum + toAmount(expense.monthlyEquivalentAmount ?? expense.amount), 0);

  const intervalLabels: Record<string, string> = {
    "Monthly": "شهري",
    "Yearly": "سنوي",
    "Weekly": "أسبوعي",
    "BiWeekly": "كل أسبوعين",
  };

  return (
    <div className="bg-card rounded-2xl p-6 border border-border/70 shadow-card">
      <div className="mb-5">
        <h3 className="font-bold text-navy">المصاريف المتكررة</h3>
        <p className="text-xs text-muted-foreground mt-1">
          {expenses.length} مصروف متكرر • ~{Math.round(totalMonthly).toLocaleString()} ر.س / شهر
        </p>
      </div>

      <div className="space-y-3">
        {expenses.map((expense, idx) => {
          const interval = expenseInterval(expense);
          const nextDate = new Date(nextOccurrence(expense) ?? "");
          const today = new Date();
          const isValidDate = Number.isFinite(nextDate.getTime());
          const isUpcoming = isValidDate && nextDate > today;
          const daysUntil = isValidDate ? Math.ceil((nextDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24)) : 0;

          return (
            <div
              key={`${expense.name ?? "expense"}-${idx}`}
              className="p-3 rounded-lg border border-border/50 bg-muted/20 hover:bg-muted/40 transition-colors"
            >
              <div className="flex items-start justify-between mb-2">
                <div className="flex-1">
                  <p className="text-sm font-medium text-navy">{expense.name || expense.description || "مصروف متكرر"}</p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    {intervalLabels[interval] || interval || "غير محدد"}
                  </p>
                </div>
                <span className="font-bold text-sm text-red-600">-{Math.round(toAmount(expense.amount)).toLocaleString()} ر.س</span>
              </div>
              <div className="flex items-center gap-2 text-xs">
                <Calendar className="w-3.5 h-3.5 text-muted-foreground" />
                <span className="text-muted-foreground">
                  {!isValidDate
                    ? "لا يوجد تاريخ قادم"
                    : isUpcoming
                    ? `خلال ${daysUntil} أيام`
                    : `الآن: ${nextDate.toLocaleDateString("ar-SA")}`}
                </span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function expenseInterval(expense: RecurringExpense) {
  return expense.interval ?? expense.recurrenceInterval ?? "";
}

function getTime(value?: string | null) {
  const time = new Date(value ?? "").getTime();
  return Number.isFinite(time) ? time : Number.MAX_SAFE_INTEGER;
}

function nextOccurrence(expense: RecurringExpense) {
  return expense.nextOccurrenceUtc ?? expense.expenseDate ?? expense.recurrenceEndDate ?? null;
}

function toAmount(value: unknown) {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : 0;
}
