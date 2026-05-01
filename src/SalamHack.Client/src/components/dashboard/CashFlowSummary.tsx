import { useEffect, useState } from "react";
import { Loader2, TrendingUp, TrendingDown, DollarSign } from "lucide-react";
import { fetchCashFlowForecast, CashFlowForecastDto } from "@/lib/reports";

interface Props {
  asOfUtc?: string;
  openingBalance?: number;
}

export default function CashFlowSummary({ asOfUtc, openingBalance = 0 }: Props) {
  const [data, setData] = useState<CashFlowForecastDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setLoading(true);
    setError("");

    fetchCashFlowForecast(asOfUtc, openingBalance)
      .then(setData)
      .catch((e) => setError(e.message))
      .finally(() => setLoading(false));
  }, [asOfUtc, openingBalance]);

  if (loading) {
    return (
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="bg-card rounded-xl p-4 border border-border/70 flex items-center justify-center h-24">
            <Loader2 className="w-5 h-5 animate-spin text-muted-foreground" />
          </div>
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-destructive/10 rounded-xl p-4 border border-destructive/20">
        <p className="text-sm text-destructive">{error}</p>
      </div>
    );
  }

  if (!data) {
    return null;
  }

  const metrics = [
    {
      label: "الرصيد الحالي",
      value: data.currentBalance,
      icon: DollarSign,
      color: "text-blue-600",
      bgColor: "bg-blue-50",
    },
    {
      label: "التدفقات الداخلة (الشهر)",
      value: data.currentMonthInflows,
      icon: TrendingUp,
      color: "text-green-600",
      bgColor: "bg-green-50",
    },
    {
      label: "التدفقات الخارجة (الشهر)",
      value: data.currentMonthOutflows,
      icon: TrendingDown,
      color: "text-red-600",
      bgColor: "bg-red-50",
    },
    {
      label: "صافي التدفق (الشهر)",
      value: data.currentMonthNetFlow,
      icon: data.currentMonthNetFlow >= 0 ? TrendingUp : TrendingDown,
      color: data.currentMonthNetFlow >= 0 ? "text-green-600" : "text-red-600",
      bgColor: data.currentMonthNetFlow >= 0 ? "bg-green-50" : "bg-red-50",
    },
  ];

  return (
    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
      {metrics.map((metric) => {
        const Icon = metric.icon;
        return (
          <div key={metric.label} className={`${metric.bgColor} rounded-xl p-4 border border-border/70`}>
            <div className="flex items-center justify-between mb-3">
              <span className="text-xs text-muted-foreground font-medium">{metric.label}</span>
              <Icon className={`w-5 h-5 ${metric.color}`} />
            </div>
            <p className={`text-lg font-bold ${metric.color}`}>
              {Math.round(metric.value).toLocaleString()} ر.س
            </p>
          </div>
        );
      })}
    </div>
  );
}
